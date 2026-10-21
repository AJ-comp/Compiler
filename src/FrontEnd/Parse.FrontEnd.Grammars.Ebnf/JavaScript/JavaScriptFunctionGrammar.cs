using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Expands ECMAScript's Await grammar parameter inside the JavaScript language
    /// layer. Async parameters/bodies and Module code use +Await; ordinary function
    /// bodies use ~Await. Computed names keep the enclosing parameter. Only rules
    /// whose structure depends on Await are duplicated, leaving the LR engine intact.
    /// <para><see href="../../../../docs/ko/javascript-functions.html#implementation">함수 문맥별 EBNF 연결</see></para>
    /// </summary>
    internal static class JavaScriptFunctionGrammar
    {
        private static readonly Regex Header = new Regex(@"(?m)^[ \t]*(?<name>[A-Za-z_][A-Za-z_0-9]*)[ \t]*(?<assign>:=|:)[ \t]*");
        // Quoted terminals, token patterns and registered predicates are opaque:
        // only nonterminal references are replaced by their context-specific names.
        private static readonly Regex Words = new Regex(@"'[^']*'|""(?:""""|[^""])*""|\{[^}]*\}\?|[A-Za-z_][A-Za-z_0-9]*");

        internal static string Expand(string text, string entry, bool module)
        {
            var rules = new List<Rule>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match match in Header.Matches(text))
            {
                int start = match.Index + match.Length;
                int end = End(text, start);
                var rule = new Rule(match.Groups["name"].Value, match.Groups["assign"].Value,
                    text.Substring(start, end - start).Trim());
                rules.Add(rule);
                if (rule.Assign == ":") names.Add(rule.Name);
            }
            var sensitive = new HashSet<string>(StringComparer.Ordinal)
            { "UnaryExpression", "IdentifierReference", "BindingIdentifier", "LabelIdentifier" };
            // Dependency closure determines which productions really need two forms.
            // Fixed references (e.g. ordinary FunctionBody) do not inherit their caller.
            bool changed;
            do
            {
                changed = false;
                foreach (var rule in rules)
                {
                    if (rule.Assign != ":" || sensitive.Contains(rule.Name)) continue;
                    foreach (Match word in Words.Matches(rule.Body))
                        if (sensitive.Contains(word.Value) && Context(rule.Name, word.Value, false) !=
                            Context(rule.Name, word.Value, true))
                        { changed |= sensitive.Add(rule.Name); break; }
                }
            } while (changed);

            string ExpandBody(Rule rule, bool awaitContext)
            {
                string body = rule.Body;
                if (awaitContext && (rule.Name == "IdentifierReference" || rule.Name == "BindingIdentifier" || rule.Name == "LabelIdentifier"))
                    body = body.Replace(" | 'await'", string.Empty);
                if (awaitContext && rule.Name == "UnaryExpression") body += " | AwaitExpression";
                return Words.Replace(body, match => names.Contains(match.Value) && sensitive.Contains(match.Value) &&
                    Context(rule.Name, match.Value, awaitContext) ? match.Value + "__Await" : match.Value);
            }
            var output = new StringBuilder();
            void Emit(Rule rule, bool awaitContext, bool suffix)
            {
                output.Append(rule.Name).Append(suffix ? "__Await" : string.Empty).Append(' ').Append(rule.Assign)
                    .Append(' ').Append(rule.Assign == ":=" ? rule.Body : ExpandBody(rule, awaitContext)).AppendLine(" ;");
            }
            // The public start symbol remains Expression, Script or Module.
            foreach (var rule in rules) if (rule.Name == entry) Emit(rule, module, false);
            foreach (var rule in rules)
            {
                if (rule.Name == entry)
                {
                    if (entry == "Expression" && sensitive.Contains(entry)) Emit(rule, true, true);
                    continue;
                }
                Emit(rule, false, false);
                if (rule.Assign == ":" && sensitive.Contains(rule.Name)) Emit(rule, true, true);
            }
            return output.ToString();
        }

        private static bool Context(string rule, string reference, bool outer)
        {
            bool parameters = reference == "FormalParameters" || reference == "FunctionBody";
            if (parameters)
            {
                if (rule == "AsyncFunctionDeclaration" || rule == "AsyncFunctionExpression" ||
                    rule == "AsyncGeneratorDeclaration" || rule == "AsyncGeneratorExpression" ||
                    rule == "ClassAsyncMethod" || rule == "ObjectAsyncMethod") return true;
                if (rule == "FunctionDeclaration" || rule == "FunctionExpression" ||
                    rule == "GeneratorDeclaration" || rule == "GeneratorExpression" ||
                    rule == "ClassMethod" || rule == "ClassGeneratorMethod" ||
                    rule == "ObjectMethod" || rule == "ObjectGeneratorMethod") return false;
            }
            if (reference == "BindingIdentifier" && rule.EndsWith("Expression", StringComparison.Ordinal))
            {
                if (rule == "AsyncFunctionExpression" || rule == "AsyncGeneratorExpression") return true;
                if (rule == "FunctionExpression" || rule == "GeneratorExpression") return false;
            }
            if (rule == "ClassField" && reference == "Initializer" ||
                rule == "ClassStaticBlock" && reference == "ClassStaticBody") return false;
            if (rule == "ConciseBody" || rule == "ConciseBodyNoIn" ||
                (rule == "ArrowFunction" || rule == "ArrowFunctionNoIn") && reference.StartsWith("ConciseBody", StringComparison.Ordinal)) return false;
            if (rule == "AsyncConciseBody" || rule == "AsyncConciseBodyNoIn" ||
                (rule == "AsyncArrowFunction" || rule == "AsyncArrowFunctionNoIn") && reference.StartsWith("AsyncConciseBody", StringComparison.Ordinal)) return true;
            return outer;
        }

        private static int End(string text, int start)
        {
            char quote = '\0';
            for (int index = start; index < text.Length; index++)
            {
                char current = text[index];
                if (quote != '\0')
                {
                    if (current != quote) continue;
                    if (index + 1 < text.Length && text[index + 1] == quote) { index++; continue; }
                    quote = '\0';
                }
                else if (current == '\'' || current == '"') quote = current;
                else if (current == ';') return index;
            }
            throw new InvalidOperationException("An embedded JavaScript production has no terminating semicolon.");
        }

        private sealed class Rule
        {
            internal Rule(string name, string assign, string body) { Name = name; Assign = assign; Body = body; }
            internal string Name { get; }
            internal string Assign { get; }
            internal string Body { get; }
        }
    }
}
