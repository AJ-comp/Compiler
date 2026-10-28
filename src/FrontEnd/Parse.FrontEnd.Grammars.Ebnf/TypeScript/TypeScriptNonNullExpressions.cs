using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Adds TypeScript postfix non-null assertions to member, call and optional
    /// chains without changing the shared JavaScript grammar or parser engine.
    /// Keeping those categories separate preserves assignment targets and the
    /// restriction on an unparenthesized optional constructor expression.
    /// </summary>
    internal static class TypeScriptNonNullExpressions
    {
        internal static string Prepare(string text)
        {
            text = TypeScriptDialect.Edit(text, "MemberExpression", body => body +
                " | TSNonNullExpressionMember | TSNonNullExpressionOptional TaggedTemplate");
            text = TypeScriptDialect.Edit(text, "CallExpression", body => body + " | TSNonNullExpressionCall");
            text = TypeScriptDialect.Edit(text, "OptionalExpression", body => body +
                " | TSNonNullExpressionOptional | OptionalExpression MemberAccess | OptionalExpression Arguments");
            text = TypeScriptDialect.Edit(text, "SimpleUpdateExpression", body =>
                body.Replace("{NoPostfix()}?", "{TSNoPostfix()}?"));
            return text + @"
# A postfix assertion cannot cross a line break. Its distinct token kind lets
# the lexical policy leave prefix ! and the != / !== operators unchanged.
TSNonNullExpressionMember : MemberExpression NoLineTerminator TSNonNullToken ;
TSNonNullExpressionCall : CallExpression NoLineTerminator TSNonNullToken ;
TSNonNullExpressionOptional : OptionalExpression NoLineTerminator TSNonNullToken ;
TSNonNullToken := Identifier ""!(?!=)"" ;
";
        }

        internal static void ConfigurePredicates(EbnfReaderOptions options)
        {
            var noPostfix = options.SnapshotPredicates()["NoPostfix"];
            // Keep JavaScript's existing boundary checks, but do not finish an
            // update expression while the next token is a TypeScript postfix !.
            // A prefix ! after a line break retains its ordinary token kind.
            options.RegisterPredicate("TSNoPostfix", context =>
                context.NextToken.Kind.Caption != "TSNonNullToken" && noPostfix.Evaluate(context));
        }

        internal static void Validate(ParseTreeNonTerminal node, List<JavaScriptDiagnostic> diagnostics)
        {
            string name = TypeScriptProjection.Name(node);
            if (name != "MemberExpression" && name != "NewExpression") return;
            if (node.Count < 2 || !(node[0] is ParseTreeTerminal keyword) || keyword.Token.Input != "new") return;
            if (!HasOptionalConstructorTarget(node[1])) return;
            var tokens = node[1].AllTokens;
            if (tokens.Count == 0) return;
            int start = tokens[0].StartIndex;
            int length = tokens[tokens.Count - 1].EndIndex - start + 1;
            diagnostics.Add(new JavaScriptDiagnostic("typescript_optional_chain_constructor",
                "An unparenthesized optional chain cannot be the target of a new expression.", start, length));
        }

        private static bool HasOptionalConstructorTarget(ParseTreeSymbol target)
        {
            // An asserted optional tag can resume ordinary member syntax. It
            // still cannot occur inside an unparenthesized constructor target.
            // Follow only that target's base; do not inspect its arguments or
            // computed-property expressions, where optional chains are valid.
            while (target is ParseTreeNonTerminal node)
            {
                string name = TypeScriptProjection.Name(node);
                if (name == "ParenthesizedExpression") return false;
                if (name == "TSNonNullExpressionOptional") return true;
                if (node.Count == 0 || node[0] is ParseTreeTerminal) return false;
                if (node.Count != 1 && name != "MemberExpression" && name != "CallExpression" &&
                    name != "TSNonNullExpressionMember" && name != "TSNonNullExpressionCall") return false;
                target = node[0];
            }
            return false;
        }
    }
}
