using System;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Connects non-strict and opt-in web-compatible productions using the existing EBNF
    /// reader. Strictness-dependent early errors are checked on the completed tree.
    /// This transformation and its registered conditions belong to the JavaScript layer.
    /// <para><see href="../../../../docs/ko/javascript-non-strict.html">비엄격 문법과 웹 호환 생성 규칙</see></para>
    /// </summary>
    internal static class JavaScriptNonStrictGrammar
    {
        internal static string Prepare(string text, JavaScriptMode mode, bool webCompatibility)
        {
            if (mode == JavaScriptMode.Strict) return text;
            const string contextualNames = " | 'let' | 'static' | 'implements' | 'interface' | 'package' | 'private' | 'protected' | 'public'";
            foreach (string rule in new[] { "IdentifierReference", "BindingIdentifier", "LabelIdentifier" })
                text = Change(text, rule, body => body + contextualNames);
            text = Change(text, "Statement", body => body + " | WithStatement");
            text += "\nWithStatement : 'with' '(' Expression ')' Statement ;\n";
            // A StatementList may choose a lexical declaration, whereas a lone
            // if/loop/label/with body accepts only Statement. Keep declaration
            // precedence at the list boundary instead of globally preventing an
            // ExpressionStatement such as `if (ok) let\nname;` (ASI after let).
            text = Change(text, "StatementListItem", _ =>
                "StatementListStatementStart Statement | Declaration");
            text += "\nStatementListStatementStart : {StatementListStatementStart()}? ;\n";
            // `let` is an identifier as well as a declaration prefix. Preserve both
            // actions through a registered condition so prediction can distinguish
            // `let = value`, `let name = value`, and binding-pattern declarations.
            text = Change(text, "LetOrConst", _ => "{LetDeclarationStart()}? 'let' | 'const'");
            text = Change(text, "ForInitializer", body => body.Replace("ExpressionNoIn", "ForExpressionStart ExpressionNoIn"));
            foreach (string rule in new[] { "ForInStatement", "ForOfStatement", "ForAwaitOfStatement" })
                text = Change(text, rule, body => body.Replace("'(' LeftHandSideExpression", "'(' ForExpressionStart LeftHandSideExpression"));
            text += "\nForExpressionStart : {ForExpressionStart()}? ;\n";

            // Separate ordinary async methods from async generators so their Yield
            // grammar parameter can be set before reading a contextual `yield` token.
            text = Change(text, "ObjectAsyncMethod", _ =>
                "PropertyName AsyncModifier PropertyName '(' FormalParameters ')' FunctionBody");
            text += "\nObjectAsyncGeneratorMethod : PropertyName AsyncModifier '*' PropertyName '(' FormalParameters ')' FunctionBody ;\n";
            text = Change(text, "ClassAsyncMethod", _ =>
                "ClassElementName AsyncModifier ClassElementName '(' FormalParameters ')' FunctionBody | " +
                "ClassElementName ClassModifierName AsyncModifier ClassElementName '(' FormalParameters ')' FunctionBody");
            text += "\nClassAsyncGeneratorMethod : ClassElementName AsyncModifier '*' ClassElementName '(' FormalParameters ')' FunctionBody | " +
                "ClassElementName ClassModifierName AsyncModifier '*' ClassElementName '(' FormalParameters ')' FunctionBody ;\n";
            text = Change(text, "PropertyDefinition", body => body + " | ObjectAsyncGeneratorMethod");
            text = Change(text, "ClassElement", body => body + " | ClassAsyncGeneratorMethod");

            if (webCompatibility)
            {
                text = Change(text, "IfStatement", _ =>
                    "'if' '(' Expression ')' Statement | 'if' '(' Expression ')' Statement 'else' Statement | " +
                    "'if' '(' Expression ')' LegacyFunctionDeclaration | " +
                    "'if' '(' Expression ')' LegacyFunctionDeclaration 'else' Statement | " +
                    "'if' '(' Expression ')' Statement 'else' LegacyFunctionDeclaration | " +
                    "'if' '(' Expression ')' LegacyFunctionDeclaration 'else' LegacyFunctionDeclaration");
                text = Change(text, "LabelledStatement", _ =>
                    "StatementExpressionStart IdentifierReference ':' LabelledItem");
                text = Change(text, "Statement", body => body + " | WebForInStatement");
                text += "\nLabelledItem : Statement | LegacyFunctionDeclaration ;\n" +
                    "LegacyFunctionDeclaration : FunctionDeclaration ;\n" +
                    "WebForInStatement : 'for' '(' 'var' BindingIdentifier '=' AssignmentExpressionNoIn 'in' Expression ')' Statement ;\n";
            }
            return text;
        }

        private static string Change(string text, string name, Func<string, string> replace)
        {
            var match = Regex.Match(text, @"(?m)^" + Regex.Escape(name) + @"[ \t]*:[ \t]*");
            if (!match.Success) throw new InvalidOperationException("Missing JavaScript production: " + name);
            int start = match.Index + match.Length;
            int end = JavaScriptFunctionGrammar.ProductionEnd(text, start);
            return text.Substring(0, start) + replace(text.Substring(start, end - start).Trim()) + " " + text.Substring(end);
        }
    }
}
