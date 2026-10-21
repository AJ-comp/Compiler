using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Checks strict Script/Module/function bindings, declaration scopes and control-flow targets.
    /// Binding-name collection stops at initializers and property names, so expressions
    /// inside defaults and computed keys keep their ordinary expression meaning.
    /// All traversal and diagnostic state belongs to a single validation call.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#early-errors">함수·문장과 선언 패턴의 검사</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html#parameters">함수 종류와 매개변수 오류 검사</see></para>
    /// </summary>
    internal static class JavaScriptStatementValidation
    {
        private static readonly HashSet<string> Reserved = new HashSet<string>(
            ("break case catch class const continue debugger default delete do else enum export extends false " +
             "finally for function if import in instanceof new null return super switch this throw true try typeof " +
             "var void while with yield let static implements interface package private protected public").Split(' '),
            StringComparer.Ordinal);

        internal static void Validate(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            var pending = new Stack<Visit>();
            pending.Push(new Visit(tree, new Flow()));
            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                if (!(visit.Symbol is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                var flow = visit.Flow;

                if (name == "BindingIdentifier")
                {
                    string identifier = Identifier(node);
                    if (identifier == "eval" || identifier == "arguments" || Reserved.Contains(identifier) ||
                        identifier == "await" && JavaScriptSyntax.HasAwaitContext(node))
                        Add(diagnostics, "invalid_binding_identifier", "This name cannot be bound in strict JavaScript code.", node);
                }
                if (IsFunction(name))
                {
                    CheckFunction(node, diagnostics);
                    // A function establishes its own return, loop and label context.
                    // A loop/label outside this function cannot receive its break/continue.
                    var inner = new Flow(inFunction: true, yieldAllowed: JavaScriptSyntax.IsGenerator(node));
                    for (int index = node.Count - 1; index >= 0; index--)
                        pending.Push(new Visit(node[index], JavaScriptSyntax.IsMethod(name) &&
                            JavaScriptSyntax.IsElementName(node[index]) ? flow : inner));
                    continue;
                }
                else if (name == "ClassField")
                {
                    for (int index = node.Count - 1; index >= 0; index--)
                        pending.Push(new Visit(node[index], JavaScriptSyntax.IsElementName(node[index]) ? flow : new Flow()));
                    continue;
                }
                else if (name == "ClassStaticBlock")
                {
                    // Static initialization blocks have their own var/lexical scope,
                    // but they are not functions and therefore cannot return.
                    CheckScope(node, diagnostics);
                    flow = new Flow();
                }
                else if (IsScope(name)) CheckScope(node, diagnostics);

                if (name == "LexicalDeclaration") CheckLexicalInitializers(node, diagnostics);
                else if (name == "ForStatement" || name == "ForInStatement" || name == "ForOfStatement" || name == "ForInOfStatement" || name == "ForAwaitOfStatement")
                    CheckFor(node, diagnostics);
                else if (name == "CatchClause" || name == "Catch") CheckCatch(node, diagnostics);
                else if (name == "CaseBlock") CheckDefaultClauses(node, diagnostics);
                else if (name == "ReturnStatement" && !flow.InFunction)
                    Add(diagnostics, "invalid_return", "A return statement must be inside a function body.", node);
                else if ((name == "YieldExpression" || name == "YieldExpressionNoIn") && !flow.YieldAllowed)
                    Add(diagnostics, "invalid_yield", "A yield expression requires a generator function body.", node);
                else if (name == "BreakStatement") CheckTransfer(node, flow, isContinue: false, diagnostics);
                else if (name == "ContinueStatement") CheckTransfer(node, flow, isContinue: true, diagnostics);
                else if (name == "WithStatement")
                    Add(diagnostics, "strict_with", "A with statement is not allowed in strict JavaScript code.", node);

                if (name == "LabelledStatement")
                {
                    var label = FirstChild(node, "IdentifierReference", "LabelIdentifier");
                    if (label != null)
                    {
                        string identifier = Identifier(label);
                        if (flow.Labels.ContainsKey(identifier))
                            Add(diagnostics, "duplicate_label", "An active label cannot be declared again.", label);
                        var labels = new Dictionary<string, bool>(flow.Labels, StringComparer.Ordinal)
                        {
                            [identifier] = LabelsIteration(node)
                        };
                        flow = new Flow(flow.InFunction, flow.Loops, flow.Breakable, labels, flow.YieldAllowed);
                    }
                }
                else if (IsIteration(name))
                    flow = new Flow(flow.InFunction, flow.Loops + 1, flow.Breakable + 1, flow.Labels, flow.YieldAllowed);
                else if (name == "SwitchStatement")
                    flow = new Flow(flow.InFunction, flow.Loops, flow.Breakable + 1, flow.Labels, flow.YieldAllowed);

                for (int index = node.Count - 1; index >= 0; index--)
                    pending.Push(new Visit(node[index], flow));
            }
        }

        private static void CheckFunction(ParseTreeNonTerminal function, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameters = FirstChild(function, "FormalParameters");
            var body = FirstChild(function, "FunctionBody");
            if (parameters == null || body == null) return;
            CheckParameterExpressions(parameters, diagnostics, JavaScriptSyntax.IsAsync(function), JavaScriptSyntax.IsGenerator(function));
            var names = Bindings(parameters);
            CheckDuplicates(names, "duplicate_parameter", "A strict function cannot bind the same parameter name twice.", diagnostics);
            var declarations = DirectDeclarations(body);
            CheckIntersection(names, declarations.Lexical, "A parameter name cannot also be a lexical declaration in the function body.", diagnostics);
            if (!IsSimpleParameters(parameters) && HasUseStrictDirective(body))
                Add(diagnostics, "non_simple_strict_directive", "A function with default, rest or destructured parameters cannot contain a use strict directive.", parameters);
        }

        internal static void CheckParameterExpressions(ParseTreeSymbol parameters, List<JavaScriptDiagnostic> diagnostics,
            bool forbidAwait = true, bool forbidYield = true, bool forbidAwaitIdentifier = false)
        {
            if (!forbidAwait && !forbidYield) return;
            // Contains does not descend into nested function or arrow definitions.
            // Method keys and class heritage/computed keys still execute while the
            // outer parameter is initialized, so those expressions remain visible.
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(parameters);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "AwaitExpression" && forbidAwait)
                    Add(diagnostics, "invalid_parameter_await", "A parameter initializer cannot contain an await expression.", node);
                else if ((name == "YieldExpression" || name == "YieldExpressionNoIn") && forbidYield)
                    Add(diagnostics, "invalid_parameter_yield", "A parameter initializer cannot contain a yield expression.", node);
                else if (forbidAwaitIdentifier && (name == "IdentifierReference" || name == "BindingIdentifier" ||
                    name == "LabelIdentifier") && Identifier(node) == "await")
                    Add(diagnostics, "invalid_parameter_await", "An async arrow parameter cannot use await as an identifier.", node);
                if (JavaScriptSyntax.IsFunction(name))
                {
                    if (JavaScriptSyntax.IsMethod(name))
                        foreach (var key in JavaScriptSyntax.ElementNames(node)) pending.Push(key);
                    else if (forbidAwaitIdentifier && JavaScriptSyntax.IsArrow(name) && !JavaScriptSyntax.IsAsync(node))
                        // Reinterpreting an async cover head gives nested ordinary
                        // arrow parameters +Await, but their concise body resets it.
                        pending.Push(node[0]);
                    continue;
                }
                if (name == "ClassField")
                {
                    if (forbidAwaitIdentifier)
                        // Async-arrow cover reparsing gives the complete field
                        // grammar +Await as well. A nested function still resets it.
                        PushChildren(pending, node);
                    else foreach (var key in JavaScriptSyntax.ElementNames(node)) pending.Push(key);
                    continue;
                }
                if (name == "ClassStaticBlock") continue;
                PushChildren(pending, node);
            }
        }

        internal static void CheckArrowFunction(ParseTreeNonTerminal body, IReadOnlyList<ParseTreeNonTerminal> parameters,
            bool simple, ParseTreeSymbol parameterList, List<JavaScriptDiagnostic> diagnostics)
        {
            // Arrow cover parameters are represented by IdentifierReference nodes.
            // Their binding interpretation has already been checked by the arrow
            // analyzer, so only the shared name/scope rules belong in this method.
            var names = new List<BoundName>();
            foreach (var parameter in parameters) names.Add(new BoundName(parameter));
            CheckDuplicates(names, "duplicate_parameter", "An arrow function cannot bind the same parameter name twice.", diagnostics);
            if (body == null) return; // An expression body declares no local lexical names.
            CheckIntersection(names, DirectDeclarations(body).Lexical,
                "A parameter name cannot also be a lexical declaration in the function body.", diagnostics);
            if (!simple && HasUseStrictDirective(body))
                Add(diagnostics, "non_simple_strict_directive", "A function with default, rest or destructured parameters cannot contain a use strict directive.", parameterList);
        }

        private static void CheckScope(ParseTreeNonTerminal scope, List<JavaScriptDiagnostic> diagnostics)
        {
            var declared = DirectDeclarations(scope);
            CheckDuplicates(declared.Lexical, "duplicate_binding", "A lexical scope cannot bind the same name twice.", diagnostics);
            var variables = VarBindings(scope);
            variables.AddRange(declared.Functions);
            CheckIntersection(declared.Lexical, variables, "A lexical declaration conflicts with a var or function declaration in this scope.", diagnostics);
        }

        private static Declarations DirectDeclarations(ParseTreeNonTerminal scope)
        {
            var result = new Declarations();
            bool functionsAreVar = Name(scope) == "Script" || Name(scope) == "ScriptBody" || Name(scope) == "FunctionBody" || Name(scope) == "ClassStaticBlock";
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, scope);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ImportedBinding")
                {
                    // Imports are lexical module bindings. They therefore share the
                    // same duplicate-name and lexical-versus-var checks as let/class.
                    result.Lexical.AddRange(Bindings(node));
                    continue;
                }
                if (name == "ImportSpecifier" && node.Count == 1)
                {
                    // The unaliased import shares its name syntax with the exported
                    // name until `as` distinguishes it. A string is diagnosed by the
                    // module checker and never becomes a local identifier here.
                    if (FirstToken(node)?.Token.Kind.Caption != "StringLiteral")
                        result.Lexical.Add(new BoundName(node));
                    continue;
                }
                if (name == "DefaultExportFunction" || name == "DefaultExportClass")
                {
                    var declaration = FirstChild(node, "FunctionExpression", "GeneratorExpression", "AsyncFunctionExpression", "AsyncGeneratorExpression", "ClassExpression");
                    var identifier = declaration == null ? null : FirstChild(declaration, "BindingIdentifier");
                    if (identifier != null) result.Lexical.Add(new BoundName(identifier));
                    continue;
                }
                if (name == "LexicalDeclaration")
                {
                    result.Lexical.AddRange(Bindings(node));
                    continue;
                }
                if (JavaScriptSyntax.IsFunctionDeclaration(name))
                {
                    var identifier = FirstChild(node, "BindingIdentifier");
                    if (identifier != null)
                        (functionsAreVar ? result.Functions : result.Lexical).Add(new BoundName(identifier));
                    continue;
                }
                if (name == "ClassDeclaration")
                {
                    var identifier = FirstChild(node, "BindingIdentifier");
                    if (identifier != null) result.Lexical.Add(new BoundName(identifier));
                    continue;
                }
                // Only declarations directly contained by this StatementList belong to
                // this lexical scope. Blocks and statement bodies establish other paths.
                if (name == "Statement" || IsScope(name) || IsFunction(name) || IsClass(name)) continue;
                PushChildren(pending, node);
            }
            return result;
        }

        internal static HashSet<string> ModuleDeclaredNames(ParseTreeNonTerminal module)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            var direct = DirectDeclarations(FirstChild(module, "ModuleBody") ?? module);
            foreach (var binding in direct.Lexical) result.Add(binding.Name);
            foreach (var binding in direct.Functions) result.Add(binding.Name);
            foreach (var binding in VarBindings(module)) result.Add(binding.Name);
            return result;
        }

        internal static List<ParseTreeNonTerminal> ExportedDeclarationBindings(ParseTreeNonTerminal declaration)
        {
            var result = new List<ParseTreeNonTerminal>();
            string name = Name(declaration);
            if (JavaScriptSyntax.IsFunctionDeclaration(name) || name == "ClassDeclaration")
            {
                var identifier = FirstChild(declaration, "BindingIdentifier");
                if (identifier != null) result.Add(identifier);
            }
            else foreach (var binding in Bindings(declaration)) result.Add(binding.Node);
            return result;
        }

        private static List<BoundName> VarBindings(ParseTreeNonTerminal root)
        {
            var result = new List<BoundName>();
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, root);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                // A nested function's variables never belong to the surrounding scope.
                // Its declaration name is handled by DirectDeclarations where appropriate.
                if (IsFunction(name) || IsClass(name) || name == "ClassStaticBlock") continue;
                if (IsVariableDeclaration(name))
                {
                    result.AddRange(Bindings(node));
                    continue;
                }
                if (name == "ForDeclaration")
                {
                    if (FirstToken(node)?.Token.Input == "var") result.AddRange(Bindings(node));
                    continue;
                }
                PushChildren(pending, node);
            }
            return result;
        }

        private static bool IsVariableDeclaration(string name)
            => name == "VariableDeclaration" || name == "UninitializedVariableDeclaration" ||
               name == "InitializedVariableDeclaration" || name == "ForVariableDeclaration" ||
               name == "ForVariableDeclarationNoIn";

        private static void CheckLexicalInitializers(ParseTreeNonTerminal declaration, List<JavaScriptDiagnostic> diagnostics)
        {
            if (FirstToken(declaration)?.Token.Input != "const") return;
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, declaration);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "UninitializedLexicalBinding")
                {
                    Add(diagnostics, "missing_const_initializer", "A const declaration requires an initializer.", node);
                    continue;
                }
                if (name == "LexicalBinding" && FirstChild(node, "Initializer", "InitializerNoIn") == null &&
                    FirstChild(node, "BindingIdentifier") != null)
                {
                    Add(diagnostics, "missing_const_initializer", "A const declaration requires an initializer.", node);
                    continue;
                }
                if (IsExpressionBoundary(name)) continue;
                PushChildren(pending, node);
            }
        }

        private static void CheckFor(ParseTreeNonTerminal loop, List<JavaScriptDiagnostic> diagnostics)
        {
            var declaration = FirstChild(loop, "ForDeclaration", "ForInitializer");
            if (declaration == null) return;
            string keyword = FirstToken(declaration)?.Token.Input;
            if (keyword != "let" && keyword != "const") return;
            var names = Bindings(declaration);
            CheckDuplicates(names, "duplicate_binding", "A lexical for declaration cannot bind the same name twice.", diagnostics);
            var statement = LastChild(loop, "Statement");
            if (statement != null)
                CheckIntersection(names, VarBindings(statement), "A lexical for binding conflicts with a var declaration in the loop body.", diagnostics);
            if (Name(loop) == "ForStatement" && keyword == "const")
                CheckForConstInitializers(declaration, diagnostics);
        }

        private static void CheckForConstInitializers(ParseTreeNonTerminal declaration, List<JavaScriptDiagnostic> diagnostics)
        {
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, declaration);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ForLexicalBinding" || name == "ForLexicalBindingNoIn")
                {
                    if (FirstChild(node, "Initializer", "InitializerNoIn") == null)
                        Add(diagnostics, "missing_const_initializer", "A const declaration requires an initializer.", node);
                    continue;
                }
                if (IsExpressionBoundary(name)) continue;
                PushChildren(pending, node);
            }
        }

        private static void CheckCatch(ParseTreeNonTerminal clause, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameter = FirstChild(clause, "CatchParameter");
            var block = FirstChild(clause, "Block");
            if (parameter == null || block == null) return;
            var names = Bindings(parameter);
            CheckDuplicates(names, "duplicate_binding", "A catch pattern cannot bind the same name twice.", diagnostics);
            CheckIntersection(names, DirectDeclarations(block).Lexical, "A catch parameter conflicts with a lexical declaration in the catch block.", diagnostics);
            // This grammar follows the main specification. The Annex B host extension
            // for `catch (name) { var name; }` is deliberately not enabled here.
            CheckIntersection(names, VarBindings(block), "A catch parameter conflicts with a var declaration in the catch block.", diagnostics);
        }

        private static List<BoundName> Bindings(ParseTreeSymbol root)
        {
            var result = new List<BoundName>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "BindingIdentifier")
                {
                    result.Add(new BoundName(node));
                    continue;
                }
                // A computed key/default value is evaluated as an expression. Identifiers
                // or nested function parameters in it are not names bound by this pattern.
                if (IsExpressionBoundary(name) || IsFunction(name)) continue;
                PushChildren(pending, node);
            }
            return result;
        }

        private static bool IsExpressionBoundary(string name)
            => name == "Initializer" || name == "InitializerNoIn" || name == "PropertyName" ||
               name == "ComputedPropertyName" || name == "Expression" || name == "ExpressionNoIn" ||
               name == "AssignmentExpression" || name == "AssignmentExpressionNoIn";

        private static bool IsSimpleParameters(ParseTreeNonTerminal parameters)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(parameters);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "BindingPattern" || name == "Initializer" || name == "FunctionRestParameter" || name == "BindingRestElement")
                    return false;
                if (IsExpressionBoundary(name) || IsFunction(name)) continue;
                PushChildren(pending, node);
            }
            return true;
        }

        private static bool HasUseStrictDirective(ParseTreeNonTerminal body)
        {
            // Read only the body's leading StatementListItems, in source order. A string
            // inside parentheses, another block or a nested function is not a directive.
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, body);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name != "StatementListItem")
                {
                    if (name == "StatementList" || name.StartsWith("__", StringComparison.Ordinal)) PushChildren(pending, node);
                    continue;
                }
                if (FirstToken(node)?.Token.Kind.Caption != "StringLiteral") return false;
                var tokens = Tokens(node);
                if (tokens.Count == 0 || tokens[0].Token.Kind.Caption != "StringLiteral" ||
                    tokens.Count > 2 || tokens.Count == 2 && tokens[1].Token.Input != ";") return false;
                string text = tokens[0].Token.Input;
                if (text == "\"use strict\"" || text == "'use strict'") return true;
            }
            return false;
        }

        private static void CheckDefaultClauses(ParseTreeNonTerminal block, List<JavaScriptDiagnostic> diagnostics)
        {
            bool found = false;
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, block);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "CaseClause" || name == "DefaultClause")
                {
                    if (FirstToken(node)?.Token.Input == "default")
                    {
                        if (found) Add(diagnostics, "duplicate_default", "A switch statement cannot contain more than one default clause.", node);
                        found = true;
                    }
                    continue;
                }
                if (name == "CaseBlock" || IsFunction(name) || name == "Statement") continue;
                PushChildren(pending, node);
            }
        }

        private static void CheckTransfer(ParseTreeNonTerminal statement, Flow flow, bool isContinue, List<JavaScriptDiagnostic> diagnostics)
        {
            var label = FirstChild(statement, "IdentifierReference", "LabelIdentifier");
            if (label != null)
            {
                if (!flow.Labels.TryGetValue(Identifier(label), out bool iteration))
                    Add(diagnostics, "unknown_label", "The statement refers to a label that is not active in this function.", label);
                else if (isContinue && !iteration)
                    Add(diagnostics, "invalid_continue_target", "A continue label must refer to an iteration statement.", label);
            }
            else if (isContinue ? flow.Loops == 0 : flow.Breakable == 0)
                Add(diagnostics, isContinue ? "invalid_continue" : "invalid_break",
                    isContinue ? "A continue statement must be inside an iteration statement." : "An unlabelled break must be inside an iteration or switch statement.", statement);
        }

        private static bool LabelsIteration(ParseTreeNonTerminal labelled)
        {
            ParseTreeSymbol symbol = labelled;
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (IsIteration(name)) return true;
                if (name == "LabelledStatement") { symbol = node[node.Count - 1]; continue; }
                if (node.Count != 1) return false;
                symbol = node[0];
            }
            return false;
        }

        private static bool IsFunction(string name)
            => JavaScriptSyntax.IsFunction(name);
        private static bool IsClass(string name)
            => name == "ClassDeclaration" || name == "ClassExpression";
        private static bool IsScope(string name)
            => name == "Script" || name == "ScriptBody" || name == "Module" || name == "ModuleBody" ||
               name == "FunctionBody" || name == "Block" || name == "CaseBlock" || name == "ClassStaticBlock";
        private static bool IsIteration(string name)
            => name == "ForStatement" || name == "ForInStatement" || name == "ForOfStatement" || name == "ForInOfStatement" ||
               name == "ForAwaitOfStatement" || name == "WhileStatement" || name == "DoWhileStatement";

        private static void CheckDuplicates(List<BoundName> names, string code, string message, List<JavaScriptDiagnostic> diagnostics)
        {
            var found = new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in names)
                if (!found.Add(name.Name)) Add(diagnostics, code, message, name.Node);
        }

        private static void CheckIntersection(List<BoundName> left, List<BoundName> right, string message, List<JavaScriptDiagnostic> diagnostics)
        {
            var found = new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in right) found.Add(name.Name);
            foreach (var name in left)
                if (found.Contains(name.Name)) Add(diagnostics, "conflicting_declaration", message, name.Node);
        }

        private static ParseTreeNonTerminal FirstChild(ParseTreeNonTerminal node, params string[] names)
        {
            for (int index = 0; index < node.Count; index++)
                foreach (string name in names)
                    if (Name(node[index]) == name) return (ParseTreeNonTerminal)node[index];
            return null;
        }

        private static ParseTreeNonTerminal LastChild(ParseTreeNonTerminal node, string name)
        {
            for (int index = node.Count - 1; index >= 0; index--)
                if (Name(node[index]) == name) return (ParseTreeNonTerminal)node[index];
            return null;
        }

        private static string Name(ParseTreeSymbol symbol) => JavaScriptSyntax.Name(symbol);
        private static string Identifier(ParseTreeSymbol symbol)
            => JavaScriptTokenPatterns.DecodeIdentifier(FirstToken(symbol)?.Token.Input ?? string.Empty);

        private static ParseTreeTerminal FirstToken(ParseTreeSymbol symbol)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(symbol);
            while (pending.Count != 0)
            {
                var current = pending.Pop();
                if (current is ParseTreeTerminal terminal) return terminal;
                if (current is ParseTreeNonTerminal node) PushChildren(pending, node);
            }
            return null;
        }

        private static List<ParseTreeTerminal> Tokens(ParseTreeSymbol symbol)
        {
            var result = new List<ParseTreeTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(symbol);
            while (pending.Count != 0)
            {
                var current = pending.Pop();
                if (current is ParseTreeTerminal terminal) result.Add(terminal);
                else if (current is ParseTreeNonTerminal node) PushChildren(pending, node);
            }
            return result;
        }

        private static void PushChildren(Stack<ParseTreeSymbol> pending, ParseTreeNonTerminal node)
        {
            for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]);
        }

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            var tokens = Tokens(node);
            int start = tokens.Count == 0 ? 0 : tokens[0].Token.StartIndex;
            int end = tokens.Count == 0 ? start : tokens[tokens.Count - 1].Token.EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, Math.Max(0, end - start)));
        }

        private sealed class BoundName
        {
            internal BoundName(ParseTreeNonTerminal node) { Node = node; Name = Identifier(node); }
            internal string Name { get; }
            internal ParseTreeNonTerminal Node { get; }
        }

        private sealed class Declarations
        {
            internal List<BoundName> Lexical { get; } = new List<BoundName>();
            internal List<BoundName> Functions { get; } = new List<BoundName>();
        }

        private sealed class Flow
        {
            internal Flow(bool inFunction = false, int loops = 0, int breakable = 0, Dictionary<string, bool> labels = null,
                bool yieldAllowed = false)
            {
                InFunction = inFunction;
                Loops = loops;
                Breakable = breakable;
                Labels = labels ?? new Dictionary<string, bool>(StringComparer.Ordinal);
                YieldAllowed = yieldAllowed;
            }
            internal bool InFunction { get; }
            internal int Loops { get; }
            internal int Breakable { get; }
            internal Dictionary<string, bool> Labels { get; }
            internal bool YieldAllowed { get; }
        }

        private sealed class Visit
        {
            internal Visit(ParseTreeSymbol symbol, Flow flow) { Symbol = symbol; Flow = flow; }
            internal ParseTreeSymbol Symbol { get; }
            internal Flow Flow { get; }
        }
    }
}
