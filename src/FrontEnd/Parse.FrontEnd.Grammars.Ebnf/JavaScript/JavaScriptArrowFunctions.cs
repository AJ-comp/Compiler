using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Refines arrow parameter cover trees into context-appropriate binding identifiers and patterns.
    /// Exact-node sets distinguish parameters from ordinary object/array expressions;
    /// defaults and computed property names keep their expression validation rules.
    /// The original concrete tree is preserved and analysis state belongs to one call.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#arrows">화살표 함수의 매개변수와 본문 검사</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html#parameters">async 화살표 함수의 매개변수 검사</see></para>
    /// </summary>
    internal sealed class JavaScriptArrowFunctions
    {
        private readonly HashSet<ParseTreeNonTerminal> parameterLists = new HashSet<ParseTreeNonTerminal>(NodeIdentity.Instance);
        private readonly HashSet<ParseTreeNonTerminal> patterns = new HashSet<ParseTreeNonTerminal>(NodeIdentity.Instance);
        private readonly HashSet<ParseTreeNonTerminal> initializers = new HashSet<ParseTreeNonTerminal>(NodeIdentity.Instance);
        private readonly List<JavaScriptDiagnostic> diagnostics;
        private readonly JavaScriptStrictness strictness;

        private JavaScriptArrowFunctions(List<JavaScriptDiagnostic> diagnostics, JavaScriptStrictness strictness)
        {
            this.diagnostics = diagnostics;
            this.strictness = strictness;
        }

        internal static JavaScriptArrowFunctions Analyze(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
            => Analyze(tree, diagnostics, JavaScriptStrictness.Analyze(tree, JavaScriptMode.Strict));

        internal static JavaScriptArrowFunctions Analyze(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics,
            JavaScriptStrictness strictness)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            if (strictness == null) throw new ArgumentNullException(nameof(strictness));
            var result = new JavaScriptArrowFunctions(diagnostics, strictness);
            var parentheses = new List<ParseTreeNonTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ArrowFunction" || name == "AsyncArrowFunction") result.CheckArrow(node);
                else if (name == "ParenthesizedExpression") parentheses.Add(node);
                PushChildren(pending, node);
            }
            // The cover grammar also accepts (), (a,), and (...a), but those forms
            // only have meaning when this exact pair of parentheses is an arrow head.
            // An inner pair such as ((a)) => a remains an invalid binding target.
            foreach (var node in parentheses)
                if (!result.IsParameterList(node) && (node.Count != 3 || Name(node[1]) != "Expression"))
                    result.Add("invalid_parenthesized_expression", "An ordinary parenthesized expression cannot be empty or contain a trailing comma or rest parameter.", node);
            return result;
        }

        internal bool IsParameterList(ParseTreeNonTerminal node) => parameterLists.Contains(node);
        internal bool IsPattern(ParseTreeNonTerminal node) => patterns.Contains(node);
        internal bool IsPatternInitializer(ParseTreeNonTerminal node) => initializers.Contains(node);

        private void CheckArrow(ParseTreeNonTerminal function)
        {
            var names = new List<ParseTreeNonTerminal>();
            var work = new Stack<BindingWork>();
            bool async = Name(function) == "AsyncArrowFunction";
            ParseTreeSymbol headSource = function[0];
            bool asyncArguments = false;
            if (async)
            {
                if (Name(headSource) == "CallExpression")
                {
                    var call = (ParseTreeNonTerminal)headSource;
                    var callee = call.Count == 2 ? CoverRoot(call[0]) : null;
                    if (call.Count != 2 || !IsRawAsync(callee) || Name(call[1]) != "Arguments" ||
                        call[1].AllTokens[0].HasLineBreakBefore)
                    {
                        Add("invalid_async_arrow_head", "An async arrow head must be the exact async spelling followed by a parameter list without a line break.", headSource);
                        return;
                    }
                    headSource = call[1];
                    asyncArguments = true;
                }
                else
                {
                    ParseTreeSymbol parameter = null;
                    for (int index = 1; index < function.Count; index++)
                        if (Name(function[index]) == "IdentifierReference") { parameter = function[index]; break; }
                    if (!IsRawAsync(headSource) || parameter == null)
                    {
                        Add("invalid_async_arrow_head", "An async arrow head must start with the exact async spelling and a binding identifier.", headSource);
                        return;
                    }
                    headSource = parameter;
                }
            }
            var head = CoverRoot(headSource);
            JavaScriptStatementValidation.CheckParameterExpressions(headSource, diagnostics,
                forbidAwaitIdentifier: async);
            bool simple = true;
            if (head is ParseTreeNonTerminal list && (Name(list) == "ParenthesizedExpression" || asyncArguments))
            {
                parameterLists.Add(list);
                var entries = new List<ParseTreeSymbol>();
                for (int index = 1; index < list.Count - 1; index++)
                    entries.AddRange(Flatten(list[index], asyncArguments ? "ArgumentList" : "Expression"));
                for (int index = 0; index < entries.Count; index++)
                {
                    var entry = entries[index];
                    if (entry is ParseTreeTerminal) continue; // List commas carry no names.
                    if (Name(entry) == "Argument") entry = ((ParseTreeNonTerminal)entry)[0];
                    if (Name(entry) == "SpreadElement")
                    {
                        simple = false;
                        CheckRest((ParseTreeNonTerminal)entry, index == entries.Count - 1,
                            allowPattern: true, work);
                    }
                    else
                    {
                        if (!IsIdentifier(CoverRoot(entry))) simple = false;
                        work.Push(new BindingWork(entry, allowDefault: true, allowPattern: true));
                    }
                }
            }
            else if (IsIdentifier(head))
                work.Push(new BindingWork(head, allowDefault: false, allowPattern: false));
            else
                Add("invalid_arrow_parameters", "An unparenthesized arrow parameter must be a binding identifier.", headSource);

            // Pattern descent is iterative, so nested destructuring does not consume
            // the C# call stack. Initializers and computed names are never descended
            // here: identifiers in those expressions are not parameter bindings.
            while (work.Count != 0)
            {
                var item = work.Pop();
                var value = CoverRoot(item.Symbol);
                if (value is ParseTreeNonTerminal node)
                {
                    string name = Name(node);
                    if (item.AllowDefault && IsAssignment(node) && Operator(node) == "=")
                    {
                        work.Push(new BindingWork(node[0], allowDefault: false, allowPattern: item.AllowPattern));
                        continue;
                    }
                    if (IsIdentifier(node))
                    {
                        names.Add(node);
                        string identifier = JavaScriptTokenPatterns.DecodeIdentifier(node.AllTokens[0].Input);
                        if (strictness.IsStrict(node) && (identifier == "eval" || identifier == "arguments") ||
                            strictness.IsReserved(identifier, node) || identifier == "await" &&
                            (async || JavaScriptSyntax.HasAwaitContext(node)))
                            Add("invalid_binding_identifier", "This name cannot be bound in this JavaScript context.", node);
                        continue;
                    }
                    if (item.AllowPattern && (name == "ArrayLiteral" || name == "ObjectLiteral"))
                    {
                        // Only this literal node becomes a binding pattern. A literal
                        // inside its default/computed key remains an ordinary literal.
                        patterns.Add(node);
                        if (name == "ArrayLiteral") CheckArray(node, work);
                        else CheckObject(node, work);
                        continue;
                    }
                }
                Add("invalid_binding_target", "An arrow parameter must bind an identifier or a permitted destructuring pattern.", item.Symbol);
            }

            names.Sort((left, right) => left.AllTokens[0].StartIndex.CompareTo(right.AllTokens[0].StartIndex));
            var body = FindBody(function[function.Count - 1]);
            JavaScriptStatementValidation.CheckArrowFunction(body, names, simple, headSource, diagnostics);
        }

        private static bool IsRawAsync(ParseTreeSymbol symbol)
        {
            symbol = CoverRoot(symbol);
            return IsIdentifier(symbol) && symbol.AllTokens.Count == 1 && symbol.AllTokens[0].Input == "async";
        }

        private void CheckArray(ParseTreeNonTerminal array, Stack<BindingWork> work)
        {
            if (array.Count == 2) return;
            var entries = Flatten(array[1], "ArrayElements");
            for (int index = 0; index < entries.Count; index++)
            {
                if (!(entries[index] is ParseTreeNonTerminal element) || Name(element) != "ArrayElement") continue;
                var value = element[0];
                if (Name(value) == "SpreadElement")
                    CheckRest((ParseTreeNonTerminal)value, index == entries.Count - 1,
                        allowPattern: true, work);
                else work.Push(new BindingWork(value, allowDefault: true, allowPattern: true));
            }
        }

        private void CheckObject(ParseTreeNonTerminal obj, Stack<BindingWork> work)
        {
            if (obj.Count == 2) return;
            var entries = Flatten(obj[1], "PropertyDefinitionList");
            for (int index = 0; index < entries.Count; index++)
            {
                if (!(entries[index] is ParseTreeNonTerminal property) || Name(property) != "PropertyDefinition") continue;
                if (property.Count == 3)
                {
                    // PropertyName (including [computed expressions]) contributes no
                    // bound name. Only the value becomes a binding element.
                    work.Push(new BindingWork(property[2], allowDefault: true, allowPattern: true));
                    continue;
                }
                var value = property[0];
                if (Name(value) == "SpreadElement")
                    CheckRest((ParseTreeNonTerminal)value, index == entries.Count - 1 && obj.Count == 3,
                        allowPattern: false, work);
                else if (value is ParseTreeNonTerminal initializer && Name(initializer) == "CoverInitializedName")
                {
                    initializers.Add(initializer);
                    work.Push(new BindingWork(initializer[0], allowDefault: false, allowPattern: false));
                }
                else work.Push(new BindingWork(value, allowDefault: false, allowPattern: false));
            }
        }

        private void CheckRest(ParseTreeNonTerminal spread, bool isLast, bool allowPattern,
            Stack<BindingWork> work)
        {
            if (!isLast)
                Add("invalid_rest_element", "A rest parameter or binding must be last and cannot have a trailing comma.", spread);
            var value = CoverRoot(spread[1]);
            if (value is ParseTreeNonTerminal assignment && IsAssignment(assignment))
            {
                Add("invalid_rest_element", "A rest parameter or binding cannot have a default initializer.", spread[1]);
                return;
            }
            work.Push(new BindingWork(spread[1], allowDefault: false, allowPattern));
        }

        private static ParseTreeNonTerminal FindBody(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                if (Name(node) == "FunctionBody") return node;
                // ConciseBody may also start with an empty predicate node. Choose its
                // final expression/body child, without entering a returned expression.
                if (Name(node) == "ConciseBody" || Name(node) == "AsyncConciseBody") { symbol = node[node.Count - 1]; continue; }
                return null;
            }
            return null;
        }

        private static ParseTreeSymbol CoverRoot(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (name == "IdentifierReference" || name == "ParenthesizedExpression" ||
                    name == "ArrayLiteral" || name == "ObjectLiteral") return node;
                if (name == "SimpleUpdateExpression") { symbol = node[0]; continue; }
                if (node.Count != 1) return node;
                symbol = node[0];
            }
            return symbol;
        }

        private static bool IsIdentifier(ParseTreeSymbol symbol) => Name(symbol) == "IdentifierReference";
        private static bool IsAssignment(ParseTreeNonTerminal node)
            => Name(node) == "AssignmentExpression" && node.Count == 3 && Name(node[1]) == "AssignmentOperator";
        private static string Operator(ParseTreeNonTerminal node) => node[1].AllTokens[0].Input;

        private static List<ParseTreeSymbol> Flatten(ParseTreeSymbol root, string listName)
        {
            var result = new List<ParseTreeSymbol>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeNonTerminal node && (Name(node) == listName ||
                    listName == "ArrayElements" && (Name(node) == "ArrayElementList" || Name(node) == "ArrayElision")))
                    PushChildren(pending, node);
                else result.Add(symbol);
            }
            return result;
        }

        private static void PushChildren(Stack<ParseTreeSymbol> pending, ParseTreeNonTerminal node)
        {
            for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]);
        }

        private static string Name(ParseTreeSymbol symbol)
        {
            return JavaScriptSyntax.BaseName(symbol);
        }

        private void Add(string code, string message, ParseTreeSymbol symbol)
        {
            ParseTreeTerminal first = null;
            ParseTreeTerminal last = null;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(symbol);
            while (pending.Count != 0)
            {
                var item = pending.Pop();
                if (item is ParseTreeTerminal terminal)
                {
                    if (first == null) first = terminal;
                    last = terminal;
                }
                else if (item is ParseTreeNonTerminal node) PushChildren(pending, node);
            }
            int start = first == null ? 0 : first.Token.StartIndex;
            int end = last == null ? start : last.Token.EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, Math.Max(0, end - start)));
        }

        private sealed class BindingWork
        {
            internal BindingWork(ParseTreeSymbol symbol, bool allowDefault, bool allowPattern)
            {
                Symbol = symbol;
                AllowDefault = allowDefault;
                AllowPattern = allowPattern;
            }
            internal ParseTreeSymbol Symbol { get; }
            internal bool AllowDefault { get; }
            internal bool AllowPattern { get; }
        }

        private sealed class NodeIdentity : IEqualityComparer<ParseTreeNonTerminal>
        {
            internal static readonly NodeIdentity Instance = new NodeIdentity();
            public bool Equals(ParseTreeNonTerminal left, ParseTreeNonTerminal right) => ReferenceEquals(left, right);
            public int GetHashCode(ParseTreeNonTerminal node) => RuntimeHelpers.GetHashCode(node);
        }
    }
}
