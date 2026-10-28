using System;
using System.Collections.Generic;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>Connects TypeScript's runtime assertions to the shared expression grammar.</summary>
    internal static class TypeScriptExpressions
    {
        internal static string Prepare(string text)
        {
            text = AddAssertions(AddAssertions(text, string.Empty), "NoIn");
            foreach (string rule in new[] { "ForInStatement", "ForOfStatement", "ForAwaitOfStatement" })
                text = TypeScriptDialect.Edit(text, rule, body =>
                    body.Replace("LeftHandSideExpression", "TSForAssertionTarget"));
            return text +
                "\nTSAssertionEnd : {TSNoAssertionOperatorStart()}? ;\n" +
                "TSAssertionTypeEnd : {TSAssertionTypeEnd()}? ;\n" +
                "TSForAssertionTarget : LeftHandSideExpression | TSAsExpressionNoIn | TSSatisfiesExpressionNoIn ;\n";
        }

        private static string AddAssertions(string text, string suffix)
        {
            string relational = "RelationalExpression" + suffix;
            string continuation = "TSAssertionContinuation" + suffix;
            string asExpression = "TSAsExpression" + suffix;
            string satisfiesExpression = "TSSatisfiesExpression" + suffix;
            text = TypeScriptDialect.Edit(text, relational, body => body + " | " + continuation);
            // A plain Reduce from relational to equality must not commit before
            // an assertion's epsilon guard. The two exits are complementary.
            text = TypeScriptDialect.Edit(text, "EqualityExpression" + suffix,
                body => body.Replace(relational, relational + " TSAssertionEnd"));

            // The ordinary higher-precedence tiers contain no assertions. In particular,
            // the right operand of a+b as T cannot swallow the assertion as b as T.
            // Once an assertion has been read, binary parsing may continue from its
            // result: x as T + y is valid. Each right operand still uses its ordinary
            // precedence tier, preserving multiplication, shifts and right-associative **.
            return text + "\n" + asExpression + " : " + relational + " {TSAsOperatorStart()}? 'as' TSType TSAssertionTypeEnd ;\n" +
                satisfiesExpression + " : " + relational + " {TSSatisfiesOperatorStart()}? 'satisfies' TSType TSAssertionTypeEnd ;\n" +
                continuation + " : " + asExpression + " | " + satisfiesExpression +
                " | " + continuation + " '**' ExponentiationExpression" +
                " | " + continuation + " MultiplicativeOperator ExponentiationExpression" +
                " | " + continuation + " AdditiveOperator MultiplicativeExpression" +
                " | " + continuation + " ShiftOperator AdditiveExpression ;\n";
        }

        internal static void ConfigurePredicates(EbnfReaderOptions options)
        {
            options.RegisterPredicate("TSNoAssertionOperatorStart", context =>
                context.NextToken.HasLineBreakBefore ||
                context.NextToken.Input != "as" && context.NextToken.Input != "satisfies");
            // A type union/intersection belongs to the assertion's type, rather
            // than a second interpretation as a runtime bitwise expression.
            options.RegisterPredicate("TSAssertionTypeEnd", context =>
                context.NextToken.Input != "|" && context.NextToken.Input != "&");
            options.RegisterPredicate("TSAsOperatorStart", context =>
                context.NextToken.Input == "as" && !context.NextToken.HasLineBreakBefore);
            options.RegisterPredicate("TSSatisfiesOperatorStart", context =>
                context.NextToken.Input == "satisfies" && !context.NextToken.HasLineBreakBefore);
        }

        // Continuations must have the normal JavaScript role in the validation view.
        // Their original TypeScript nodes remain available in the caller's concrete tree.
        internal static string ProjectionRole(ParseTreeNonTerminal node)
        {
            if (Name(node) == "TSForAssertionTarget") return "LeftHandSideExpression";
            if (Name(node) != "TSAssertionContinuation" || node.Count != 3) return null;
            switch (Operator(node)?.Input)
            {
                case "**": return "ExponentiationExpression";
                case "*": case "/": case "%": return "MultiplicativeExpression";
                case "+": case "-": return "AdditiveExpression";
                case "<<": case ">>": case ">>>": return "ShiftExpression";
                default: return null;
            }
        }

        internal static void Validate(ParseTreeNonTerminal node, List<JavaScriptDiagnostic> diagnostics)
        {
            if (Name(node) != "TSAssertionContinuation" || node.Count != 3) return;
            var token = Operator(node);
            if (token == null || Precedence(token.Input) <= OperandLimit(node[0])) return;

            // TypeScript 7 stops before a higher-precedence operator following an
            // assertion on a binary expression. Otherwise erasing `as T` could
            // silently change a+b as T*c into a+b*c. Parentheses reset this limit.
            diagnostics.Add(new JavaScriptDiagnostic("typescript_assertion_precedence",
                "Parenthesize the asserted expression before this higher-precedence operator.",
                token.StartIndex, token.Input.Length));
        }

        private static int OperandLimit(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (name == "ParenthesizedExpression") return int.MaxValue;
                if (name == "TSAsExpression" || name == "TSSatisfiesExpression")
                {
                    symbol = node[0];
                    continue;
                }
                if (node.Count == 1) { symbol = node[0]; continue; }
                if (node.Count != 3) return int.MaxValue;
                switch (name)
                {
                    case "TSAssertionContinuation": case "RelationalExpression": case "PrivateInExpression":
                    case "ShiftExpression": case "AdditiveExpression": case "MultiplicativeExpression":
                    case "ExponentiationExpression":
                        return Precedence(Operator(node)?.Input);
                    default: return int.MaxValue;
                }
            }
            return int.MaxValue;
        }

        private static TokenData Operator(ParseTreeNonTerminal node)
            => node[1].AllTokens.Count == 1 ? node[1].AllTokens[0] : null;

        private static int Precedence(string spelling)
        {
            switch (spelling)
            {
                case "**": return 15;
                case "*": case "/": case "%": return 13;
                case "+": case "-": return 12;
                case "<<": case ">>": case ">>>": return 11;
                case "<": case ">": case "<=": case ">=": case "in": case "instanceof": return 9;
                default: return int.MaxValue;
            }
        }

        private static string Name(ParseTreeSymbol node)
        {
            string name = TypeScriptProjection.Name(node);
            return name.EndsWith("NoIn", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - 4) : name;
        }
    }
}
