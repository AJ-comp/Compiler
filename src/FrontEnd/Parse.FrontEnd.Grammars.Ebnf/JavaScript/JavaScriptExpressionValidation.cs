using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// ECMAScript static early errors for strict Script expressions and statements.
    /// These checks inspect the concrete tree; they do not evaluate JavaScript values.
    /// <para><see href="../../../../docs/ko/javascript-expressions.html#early-errors">표현식 파싱 후 검사하는 오류와 이유</see></para>
    /// </summary>
    public static class JavaScriptExpressionValidation
    {
        private static readonly HashSet<string> Reserved = new HashSet<string>(
            ("break case catch class const continue debugger default delete do else enum export extends false " +
             "finally for function if import in instanceof new null return super switch this throw true try typeof " +
             "var void while with yield let static implements interface package private protected public").Split(' '),
            StringComparer.Ordinal);

        public static IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            var diagnostics = new List<JavaScriptDiagnostic>();
            var arrows = JavaScriptArrowFunctions.Analyze(tree, diagnostics);
            var patterns = JavaScriptAssignmentPatterns.Analyze(tree, IsSimpleTarget, diagnostics);
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal)
                {
                    if (terminal.Token.Kind.Caption == "RegularExpressionLiteral")
                        JavaScriptRegExpValidation.Validate(terminal.Token, diagnostics);
                    continue;
                }
                if (!(symbol is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "IdentifierReference" || name == "LabelIdentifier")
                {
                    string identifier = JavaScriptTokenPatterns.DecodeIdentifier(node.AllTokens[0].Input);
                    if (Reserved.Contains(identifier))
                        Add(diagnostics, "reserved_identifier", "This identifier is reserved in a strict Script expression.", node);
                    else if (identifier == "await" && JavaScriptSyntax.HasAwaitContext(node))
                        Add(diagnostics, "invalid_await_identifier", "An Await grammar context cannot use await as an identifier.", node);
                }
                else if (name == "AssignmentExpression" && node.Count == 3 && Name(node[1]) == "AssignmentOperator")
                {
                    if (!JavaScriptAssignmentPatterns.IsPatternAssignment(node) && !IsSimpleTarget(node[0]))
                        Add(diagnostics, "invalid_assignment_target", "The left side is not an assignable identifier or property.", node[0]);
                }
                else if (name == "UpdateExpression" && node.Count > 1)
                {
                    var operand = Name(node[0]) == "PrefixOperator" ? node[node.Count - 1] : node[0];
                    if (!IsSimpleTarget(operand))
                        Add(diagnostics, "invalid_update_target", "An update operator requires an assignable identifier or property.", operand);
                }
                else if (name == "UnaryExpression" && node.Count == 2 && node[0].AllTokens[0].Input == "delete"
                         && IsIdentifierReference(node[1]))
                {
                    Add(diagnostics, "strict_delete_identifier", "A strict expression cannot delete an identifier reference.", node[1]);
                }
                else if (name == "ObjectLiteral" && !patterns.IsPattern(node) && !arrows.IsPattern(node)) CheckProtoProperties(node, diagnostics);
                else if (name == "CoverInitializedName" && !patterns.IsPatternInitializer(node) && !arrows.IsPatternInitializer(node))
                    Add(diagnostics, "invalid_cover_initializer", "A shorthand initializer is only valid in an assignment pattern.", node);
                else if (name == "TemplateLiteral") CheckTemplateEscapes(node, diagnostics);

                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            JavaScriptStatementValidation.Validate(tree, diagnostics);
            JavaScriptClassValidation.Validate(tree, diagnostics);
            JavaScriptModuleValidation.Validate(tree, diagnostics);
            return diagnostics.AsReadOnly();
        }

        internal static bool IsSimpleTarget(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (name == "SimpleUpdateExpression") { symbol = node[0]; continue; }
                if (name == "IdentifierReference")
                {
                    string identifier = JavaScriptTokenPatterns.DecodeIdentifier(node.AllTokens[0].Input);
                    return identifier != "eval" && identifier != "arguments" && !Reserved.Contains(identifier);
                }
                if (name == "OptionalExpression") return false;
                if (name == "SuperProperty") return true;
                if (name == "ImportMeta") return false;
                if ((name == "MemberExpression" || name == "CallExpression") && node.Count > 1)
                    return Name(node[node.Count - 1]) == "MemberAccess";
                if (name == "ParenthesizedExpression") { symbol = node[1]; continue; }
                if (node.Count != 1) return false;
                symbol = node[0];
            }
            return false;
        }

        private static bool IsIdentifierReference(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                if (Name(node) == "IdentifierReference") return true;
                if (Name(node) == "SimpleUpdateExpression") { symbol = node[0]; continue; }
                if (Name(node) == "ParenthesizedExpression") { symbol = node[1]; continue; }
                if (node.Count != 1) return false;
                symbol = node[0];
            }
            return false;
        }

        private static void CheckTemplateEscapes(ParseTreeNonTerminal template, List<JavaScriptDiagnostic> diagnostics)
        {
            // The caller selects an untagged template. Its substitutions remain ordinary
            // expressions, which the main traversal checks independently. For example,
            // tag`outer ${`\u{invalid}`} text` still contains an invalid untagged template.
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(template);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal)
                {
                    string caption = terminal.Token.Kind.Caption;
                    if (caption == "NoSubstitutionTemplate" || caption == "TemplateHead" ||
                        caption == "TemplateMiddle" || caption == "TemplateTail")
                    {
                        var diagnostic = JavaScriptTemplateTokens.GetInvalidEscape(terminal.Token);
                        if (diagnostic != null) diagnostics.Add(diagnostic);
                    }
                }
                else if (symbol is ParseTreeNonTerminal node && Name(node) != "Expression")
                {
                    for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
                }
            }
        }

        private static void CheckProtoProperties(ParseTreeNonTerminal objectLiteral, List<JavaScriptDiagnostic> diagnostics)
        {
            bool found = false;
            var pending = new Stack<ParseTreeSymbol>();
            foreach (var child in objectLiteral.Items) pending.Push(child);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                if (Name(node) == "PropertyDefinitionList")
                {
                    for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
                    continue;
                }
                if (Name(node) != "PropertyDefinition" || node.Count != 3 || Name(node[0]) != "PropertyName") continue;
                var tokens = node[0].AllTokens;
                if (tokens.Count != 1) continue; // Computed keys do not set the literal's prototype.
                string text = tokens[0].Input;
                string property = tokens[0].Kind.Caption == "StringLiteral" ? DecodeString(text)
                    : tokens[0].Kind.Caption == "NumericLiteral" ? text : JavaScriptTokenPatterns.DecodeIdentifier(text);
                if (property != "__proto__") continue;
                if (found) Add(diagnostics, "duplicate_proto", "An object literal cannot contain two __proto__ prototype setters.", node[0]);
                found = true;
            }
        }

        internal static string DecodeString(string text)
        {
            var result = new StringBuilder();
            for (int i = 1; i < text.Length - 1; i++)
            {
                char ch = text[i];
                if (ch != '\\') { result.Append(ch); continue; }
                ch = text[++i];
                if (ch == '\r' || ch == '\n' || ch == '\u2028' || ch == '\u2029')
                {
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    continue;
                }
                if (ch == 'u' || ch == 'x')
                {
                    bool braces = ch == 'u' && text[i + 1] == '{';
                    int start = i + (braces ? 2 : 1);
                    int length = braces ? text.IndexOf('}', start) - start : ch == 'u' ? 4 : 2;
                    string digits = text.Substring(start, length).TrimStart('0');
                    int codePoint = digits.Length == 0 ? 0 : int.Parse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    result.Append(codePoint <= 0xffff ? ((char)codePoint).ToString() : char.ConvertFromUtf32(codePoint));
                    i = start + length - (braces ? 0 : 1);
                    continue;
                }
                switch (ch)
                {
                    case 'b': result.Append('\b'); break;
                    case 'f': result.Append('\f'); break;
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case 'v': result.Append('\v'); break;
                    case '0': result.Append('\0'); break;
                    default: result.Append(ch); break;
                }
            }
            return result.ToString();
        }

        private static string Name(ParseTreeSymbol symbol)
        {
            return JavaScriptSyntax.BaseName(symbol);
        }

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            var tokens = node.AllTokens;
            int start = tokens.Count == 0 ? 0 : tokens[0].StartIndex;
            int end = tokens.Count == 0 ? start : tokens[tokens.Count - 1].EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, end - start));
        }
    }
}
