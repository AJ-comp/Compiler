using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Determines strictness for each concrete-tree node before early-error checks run.
    /// Script/function directive prologues apply to the entire scope, including parameters
    /// and strings before the directive. Classes and Modules are always strict. Method
    /// computed names retain their surrounding context, separately from their own body.
    /// All state belongs to one validation call; the source tree is never modified.
    /// <para><see href="../../../../docs/ko/javascript-non-strict.html#strict-scopes">코드 구간별 엄격 모드 판정</see></para>
    /// </summary>
    internal sealed class JavaScriptStrictness
    {
        private static readonly HashSet<string> AlwaysReserved = new HashSet<string>(
            ("break case catch class const continue debugger default delete do else enum export extends false " +
             "finally for function if import in instanceof new null return super switch this throw true try typeof " +
             "var void while with").Split(' '), StringComparer.Ordinal);
        private static readonly HashSet<string> StrictReserved = new HashSet<string>(
            "yield let static implements interface package private protected public".Split(' '), StringComparer.Ordinal);
        private readonly Dictionary<ParseTreeSymbol, bool> strict = new Dictionary<ParseTreeSymbol, bool>(Identity.Instance);

        private JavaScriptStrictness(JavaScriptMode mode, bool webCompatibility)
        { Mode = mode; WebCompatibility = webCompatibility; }

        internal JavaScriptMode Mode { get; }
        internal bool WebCompatibility { get; }
        internal bool IsStrict(ParseTreeSymbol symbol) => strict.TryGetValue(symbol, out bool result)
            ? result : Mode == JavaScriptMode.Strict;
        internal bool IsReserved(string identifier, ParseTreeSymbol symbol)
            => AlwaysReserved.Contains(identifier) || IsStrict(symbol) && StrictReserved.Contains(identifier) ||
               identifier == "yield" && JavaScriptSyntax.HasYieldContext(symbol);

        internal static JavaScriptStrictness Analyze(ParseTreeSymbol tree, JavaScriptMode mode, bool webCompatibility = false)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            if (mode != JavaScriptMode.Strict && mode != JavaScriptMode.NonStrict)
                throw new ArgumentOutOfRangeException(nameof(mode));
            var result = new JavaScriptStrictness(mode, webCompatibility);
            var pending = new Stack<Visit>();
            pending.Push(new Visit(tree, mode == JavaScriptMode.Strict));
            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                bool outer = visit.Strict;
                if (!(visit.Symbol is ParseTreeNonTerminal node))
                { result.strict[visit.Symbol] = outer; continue; }
                string name = JavaScriptSyntax.Name(node);
                bool current = outer || name == "Module" || name == "ClassDeclaration" || name == "ClassExpression";
                if (name == "Script") current |= HasUseStrictDirective(node);
                if (JavaScriptSyntax.IsFunction(name))
                {
                    var body = FunctionBody(node);
                    current |= body != null && HasUseStrictDirective(body);
                }
                result.strict[node] = current;
                for (int index = node.Count - 1; index >= 0; index--)
                {
                    // An object's computed key executes in the enclosing code, even when
                    // the method's own use-strict directive makes its parameters/body strict.
                    bool childStrict = JavaScriptSyntax.IsMethod(name) && JavaScriptSyntax.IsElementName(node[index])
                        ? outer : current;
                    pending.Push(new Visit(node[index], childStrict));
                }
            }
            return result;
        }

        internal static ParseTreeNonTerminal FunctionBody(ParseTreeNonTerminal function)
        {
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, function);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = JavaScriptSyntax.BaseName(node);
                if (name == "FunctionBody") return node;
                if (name == "ConciseBody" || name == "AsyncConciseBody") PushChildren(pending, node);
            }
            return null;
        }

        internal static bool HasUseStrictDirective(ParseTreeNonTerminal body)
        {
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, body);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = JavaScriptSyntax.Name(node);
                if (name != "StatementListItem")
                {
                    if (name == "StatementList" || name.StartsWith("__", StringComparison.Ordinal)) PushChildren(pending, node);
                    continue;
                }
                // Large initial expression statements may contain thousands of
                // array elements. Stop as soon as this cannot be a directive,
                // without recursively collecting every token from the whole tree.
                if (!TryStringDirective(node, out string text)) return false;
                if (text == "\"use strict\"" || text == "'use strict'") return true;
            }
            return false;
        }

        private static bool TryStringDirective(ParseTreeNonTerminal statement, out string text)
        {
            text = null;
            int count = 0;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(statement);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal)
                {
                    if (count == 0)
                    {
                        if (terminal.Token.Kind.Caption != "StringLiteral") return false;
                        text = terminal.Token.Input;
                    }
                    else if (count != 1 || terminal.Token.Input != ";") return false;
                    count++;
                }
                else if (symbol is ParseTreeNonTerminal node) PushChildren(pending, node);
            }
            return count != 0;
        }

        private static void PushChildren(Stack<ParseTreeSymbol> pending, ParseTreeNonTerminal node)
        { for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]); }

        private sealed class Visit
        {
            internal Visit(ParseTreeSymbol symbol, bool strict) { Symbol = symbol; Strict = strict; }
            internal ParseTreeSymbol Symbol { get; }
            internal bool Strict { get; }
        }
        private sealed class Identity : IEqualityComparer<ParseTreeSymbol>
        {
            internal static readonly Identity Instance = new Identity();
            public bool Equals(ParseTreeSymbol left, ParseTreeSymbol right) => ReferenceEquals(left, right);
            public int GetHashCode(ParseTreeSymbol symbol) => RuntimeHelpers.GetHashCode(symbol);
        }
    }
}
