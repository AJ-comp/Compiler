using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Validates array/object cover trees as destructuring assignment patterns for plain '='.
    /// Only actual pattern nodes receive pattern-specific rules; default values, computed
    /// property keys and member-access bases remain ordinary expressions for the main validator.
    /// The concrete tree is preserved and all interpretation state belongs to one validation.
    /// <para><see href="../../../../docs/ko/javascript-expressions.html#destructuring">구조 분해 패턴과 일반 표현식의 검사 경계</see></para>
    /// </summary>
    internal sealed class JavaScriptAssignmentPatterns
    {
        private readonly HashSet<ParseTreeNonTerminal> patterns = new HashSet<ParseTreeNonTerminal>(NodeIdentity.Instance);
        private readonly HashSet<ParseTreeNonTerminal> initializers = new HashSet<ParseTreeNonTerminal>(NodeIdentity.Instance);
        private readonly Stack<ParseTreeNonTerminal> pendingPatterns = new Stack<ParseTreeNonTerminal>();
        private readonly Func<ParseTreeSymbol, bool> isSimpleTarget;
        private readonly List<JavaScriptDiagnostic> diagnostics;

        private JavaScriptAssignmentPatterns(Func<ParseTreeSymbol, bool> isSimpleTarget, List<JavaScriptDiagnostic> diagnostics)
        {
            this.isSimpleTarget = isSimpleTarget;
            this.diagnostics = diagnostics;
        }

        internal static JavaScriptAssignmentPatterns Analyze(ParseTreeSymbol tree,
            Func<ParseTreeSymbol, bool> isSimpleTarget, List<JavaScriptDiagnostic> diagnostics)
        {
            var result = new JavaScriptAssignmentPatterns(isSimpleTarget, diagnostics);
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                if (IsPatternAssignment(node)) result.Schedule(PatternRoot(node[0]));
                string name = Name(node);
                int targetIndex = name == "ForAwaitOfStatement" ? 3 : 2;
                if ((name == "ForInStatement" || name == "ForOfStatement" || name == "ForAwaitOfStatement") && Name(node[targetIndex]) == "LeftHandSideExpression")
                {
                    result.CheckTarget(node[targetIndex], allowPattern: true);
                    if (name == "ForOfStatement" && IsRawAsync(node[targetIndex]))
                        result.InvalidTarget(node[targetIndex]);
                }
                for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]);
            }
            // Iteration also covers deeply nested rest/default patterns without using the
            // C# call stack. Scheduling once prevents repeated checks of overlapping trees.
            while (result.pendingPatterns.Count != 0)
            {
                var pattern = result.pendingPatterns.Pop();
                if (Name(pattern) == "ArrayLiteral") result.CheckArray(pattern);
                else result.CheckObject(pattern);
            }
            return result;
        }

        internal bool IsPattern(ParseTreeNonTerminal node) => patterns.Contains(node);
        internal bool IsPatternInitializer(ParseTreeNonTerminal node) => initializers.Contains(node);

        internal static bool IsPatternAssignment(ParseTreeNonTerminal node)
            => IsAssignment(node) && Operator(node) == "=" && PatternRoot(node[0]) != null;

        private void Schedule(ParseTreeNonTerminal node)
        {
            if (patterns.Add(node)) pendingPatterns.Push(node);
        }

        private void CheckArray(ParseTreeNonTerminal array)
        {
            if (array.Count == 2) return;
            var entries = Flatten(array[1], "ArrayElements");
            for (int index = 0; index < entries.Count; index++)
            {
                if (!(entries[index] is ParseTreeNonTerminal element) || Name(element) != "ArrayElement") continue;
                var value = element[0];
                if (Name(value) == "SpreadElement")
                    CheckRest((ParseTreeNonTerminal)value, index == entries.Count - 1, allowPattern: true);
                else CheckElement(value);
            }
        }

        private void CheckObject(ParseTreeNonTerminal obj)
        {
            if (obj.Count == 2) return;
            var entries = Flatten(obj[1], "PropertyDefinitionList");
            for (int index = 0; index < entries.Count; index++)
            {
                if (!(entries[index] is ParseTreeNonTerminal property) || Name(property) != "PropertyDefinition") continue;
                if (property.Count == 3)
                {
                    // PropertyName is an expression context (including computed keys).
                    // Only its value is reinterpreted as an assignment element.
                    CheckElement(property[2]);
                    continue;
                }
                var value = property[0];
                if (Name(value) == "SpreadElement")
                    CheckRest((ParseTreeNonTerminal)value, index == entries.Count - 1 && obj.Count == 3, allowPattern: false);
                else if (value is ParseTreeNonTerminal initializer && Name(initializer) == "CoverInitializedName")
                {
                    initializers.Add(initializer);
                    CheckTarget(initializer[0], allowPattern: false);
                }
                else CheckTarget(value, allowPattern: false);
            }
        }

        private void CheckElement(ParseTreeSymbol value)
        {
            if (value is ParseTreeNonTerminal assignment && IsAssignment(assignment))
            {
                if (Operator(assignment) == "=") CheckTarget(assignment[0], allowPattern: true);
                else InvalidTarget(value);
                // A default's right-hand side remains an ordinary AssignmentExpression.
                // The main traversal checks it separately, including nested real assignments.
            }
            else CheckTarget(value, allowPattern: true);
        }

        private void CheckRest(ParseTreeNonTerminal spread, bool isLast, bool allowPattern)
        {
            if (!isLast)
                Add("invalid_rest_element", "A rest element or property must be last and cannot have a trailing comma.", spread);
            var value = spread[1];
            if (value is ParseTreeNonTerminal assignment && IsAssignment(assignment))
            {
                Add("invalid_rest_element", "A rest element or property cannot have a default initializer.", value);
                return;
            }
            CheckTarget(value, allowPattern);
        }

        private void CheckTarget(ParseTreeSymbol target, bool allowPattern)
        {
            var nested = PatternRoot(target);
            if (nested != null && allowPattern) Schedule(nested);
            else if (nested != null || !isSimpleTarget(target)) InvalidTarget(target);
        }

        private void InvalidTarget(ParseTreeSymbol target)
            => Add("invalid_assignment_target", "This destructuring target is not an assignable identifier, property, or permitted nested pattern.", target);

        private void Add(string code, string message, ParseTreeSymbol node)
        {
            // Invalid rest initializers can contain large expression trees. Find their
            // source bounds without recursively materializing AllTokens at each level.
            ParseTreeTerminal first = null;
            ParseTreeTerminal last = null;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(node);
            while (pending.Count != 0)
            {
                var current = pending.Pop();
                if (current is ParseTreeTerminal terminal)
                {
                    if (first == null) first = terminal;
                    last = terminal;
                }
                else if (current is ParseTreeNonTerminal nonTerminal)
                    for (int index = nonTerminal.Count - 1; index >= 0; index--) pending.Push(nonTerminal[index]);
            }
            int begin = first == null ? 0 : first.Token.StartIndex;
            int end = last == null ? begin : last.Token.EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, begin, end - begin));
        }

        private static bool IsAssignment(ParseTreeNonTerminal node)
            => Name(node) == "AssignmentExpression" && node.Count == 3 && Name(node[1]) == "AssignmentOperator";

        private static bool IsRawAsync(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node && node.Count == 1) symbol = node[0];
            return symbol is ParseTreeTerminal terminal && terminal.Token.Input == "async";
        }

        private static string Operator(ParseTreeNonTerminal node) => node[1].AllTokens[0].Input;
        private static string Name(ParseTreeSymbol node)
            => JavaScriptSyntax.BaseName(node);

        private static ParseTreeNonTerminal PatternRoot(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                if (Name(node) == "ArrayLiteral" || Name(node) == "ObjectLiteral") return node;
                if (Name(node) == "SimpleUpdateExpression") { symbol = node[0]; continue; }
                // Parentheses around a whole pattern block cover reinterpretation. Simple
                // targets such as (a) are handled by isSimpleTarget, which may unwrap them.
                if (node.Count != 1) return null;
                symbol = node[0];
            }
            return null;
        }

        private static List<ParseTreeSymbol> Flatten(ParseTreeSymbol root, string listName)
        {
            var result = new List<ParseTreeSymbol>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var item = pending.Pop();
                if (item is ParseTreeNonTerminal list && Name(list) == listName)
                    for (int index = list.Count - 1; index >= 0; index--) pending.Push(list[index]);
                else result.Add(item);
            }
            return result;
        }

        private sealed class NodeIdentity : IEqualityComparer<ParseTreeNonTerminal>
        {
            internal static readonly NodeIdentity Instance = new NodeIdentity();
            public bool Equals(ParseTreeNonTerminal left, ParseTreeNonTerminal right) => ReferenceEquals(left, right);
            public int GetHashCode(ParseTreeNonTerminal node) => RuntimeHelpers.GetHashCode(node);
        }
    }
}
