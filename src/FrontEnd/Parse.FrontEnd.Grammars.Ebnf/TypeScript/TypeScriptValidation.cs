using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Checks TypeScript grammar restrictions and reuses the JavaScript early-error
    /// checks through a temporary JavaScript view of the original concrete tree.
    /// This layer does not resolve type names or check value/type compatibility.
    /// </summary>
    internal static class TypeScriptValidation
    {
        internal static IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree,
            JavaScriptMode mode = JavaScriptMode.Strict, bool webCompatibility = false)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            var diagnostics = new List<JavaScriptDiagnostic>();
            CheckGrammar(tree, diagnostics);
            TypeScriptClassValidation.Validate(tree, diagnostics);
            var projection = TypeScriptProjection.Create(tree);
            if (!ReferenceEquals(projection, null))
            {
                var typeScriptNames = TypeScriptModuleNames(tree);
                foreach (var diagnostic in JavaScriptExpressionValidation.Validate(projection, mode, webCompatibility))
                {
                    // Erased type declarations, ambient bindings and namespace/
                    // enum declarations are legitimate TypeScript export names.
                    // All other JavaScript module diagnostics remain unchanged.
                    if (diagnostic.Code == "undefined_export" && IsDeclaredTypeScriptExport(tree, diagnostic, typeScriptNames)) continue;
                    diagnostics.Add(diagnostic);
                }
            }
            return diagnostics.AsReadOnly();
        }

        private static HashSet<string> TypeScriptModuleNames(ParseTreeSymbol tree)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (TypeScriptProjection.Name(tree) != "Module") return names;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == "TSInterfaceDeclaration" || name == "TSTypeAliasDeclaration" || name == "TSEnumDeclaration" ||
                    name == "TSNamespaceDeclaration" || name == "TSFunctionOverload" || name == "TSAmbientFunctionDeclaration" ||
                    name == "ClassDeclaration" || JavaScriptSyntax.IsFunctionDeclaration(name))
                {
                    var identifier = Child(node, "BindingIdentifier");
                    if (!ReferenceEquals(identifier, null)) AddName(identifier, names);
                    continue;
                }
                if (name == "VariableStatement" || name == "LexicalDeclaration")
                {
                    CollectBindings(node, names);
                    continue;
                }
                if (name == "Block" || name == "FunctionBody" || name == "ClassStaticBlock" || JavaScriptSyntax.IsFunction(name)) continue;
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            return names;
        }

        private static void CollectBindings(ParseTreeSymbol root, HashSet<string> names)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == "BindingIdentifier") { AddName(node, names); continue; }
                if (name.StartsWith("TSType", StringComparison.Ordinal) || name == "TSAnnotation" ||
                    name == "Initializer" || name == "InitializerNoIn" || name == "PropertyName" ||
                    JavaScriptSyntax.IsFunction(name)) continue;
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static void AddName(ParseTreeSymbol node, HashSet<string> names)
        {
            var token = FirstToken(node);
            if (token != null) names.Add(JavaScriptTokenPatterns.DecodeIdentifier(token.Input));
        }

        private static bool IsDeclaredTypeScriptExport(ParseTreeSymbol tree, JavaScriptDiagnostic diagnostic, HashSet<string> names)
        {
            if (names.Count == 0) return false;
            foreach (var token in Tokens(tree))
                if (token.StartIndex == diagnostic.SourceOffset)
                    return names.Contains(JavaScriptTokenPatterns.DecodeIdentifier(token.Input));
            return false;
        }

        private static void CheckGrammar(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                TypeScriptExpressions.Validate(node, diagnostics);
                TypeScriptNonNullExpressions.Validate(node, diagnostics);
                if (name == "FormalParameters" || name == "TSFunctionParameters") CheckParameters(node, diagnostics);
                if (name == "TSTypeParameterList") CheckTypeParameters(node, diagnostics);
                if (name == "TSTupleType") CheckTuple(node, diagnostics);
                if (name == "TSGetterSignature" || name == "TSSetterSignature") CheckAccessorSignature(node, diagnostics);
                if (name == "TSIndexSignature" || name == "TSClassIndexSignature") CheckIndexSignature(node, diagnostics);
                if (name == "TSNamespaceDeclaration") CheckNamespace(node, diagnostics);
                if (name == "TSAmbientModuleDeclaration" && !HasAncestor(node, "TSAmbientDeclaration"))
                    Add(diagnostics, "typescript_ambient_module_declare", "An ambient external module declaration in a source file requires declare.", node);
                if (name == "TSEnumMember")
                {
                    var property = Child(node, "PropertyName") as ParseTreeNonTerminal;
                    if (!ReferenceEquals(property, null) && HasDirectToken(property, "["))
                        Add(diagnostics, "typescript_computed_enum_name", "An enum member cannot have a computed property name.", property);
                }
                if ((name == "ForInStatement" || name == "ForOfStatement" || name == "ForAwaitOfStatement") &&
                    Child(node, "ForDeclaration") is ParseTreeNonTerminal declaration &&
                    (Contains(declaration, "TSAnnotation", stopAtExpressions: true) || Contains(declaration, "TSTypeAnnotation", stopAtExpressions: true)))
                    Add(diagnostics, "typescript_for_type_annotation", "A for-in or for-of declaration cannot have a type annotation.", declaration);
                if (name == "TSAmbientDeclaration") CheckAmbient(node, diagnostics);
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static void CheckParameters(ParseTreeNonTerminal list, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameters = Parameters(list);
            bool typeSignature = TypeScriptProjection.Name(list) == "TSFunctionParameters" ||
                list.Parent is ParseTreeNonTerminal owner && TypeScriptProjection.IsFunctionSignature(owner);
            bool optionalSeen = false;
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                string name = TypeScriptProjection.Name(parameter);
                bool rest = name == "FunctionRestParameter" || HasDirectToken(parameter, "...");
                bool optional = Contains(parameter, "TSOptional", stopAtTypes: true) || Contains(parameter, "TSParameterOptional", stopAtTypes: true);
                bool initializer = Contains(parameter, "Initializer", stopAtTypes: true) || Contains(parameter, "TSParameterInitializer", stopAtTypes: true);
                bool pseudoThis = name == "TSThisParameter" || IsThisParameter(parameter);
                if (pseudoThis)
                {
                    if (i != 0)
                        Add(diagnostics, "typescript_this_parameter_position", "A this parameter must be the first parameter.", parameter);
                    if (optional || initializer || rest)
                        Add(diagnostics, "typescript_invalid_this_parameter", "A this parameter cannot be optional, rest, or initialized.", parameter);
                    continue;
                }
                if (typeSignature && initializer)
                    Add(diagnostics, "typescript_signature_initializer", "A parameter initializer is only allowed in a function or constructor implementation.", parameter);
                if (rest)
                {
                    if (i != parameters.Count - 1)
                        Add(diagnostics, "typescript_rest_position", "A rest parameter must be last in the parameter list.", parameter);
                    if (optional)
                        Add(diagnostics, "typescript_optional_rest", "A rest parameter cannot be optional.", parameter);
                    if (initializer)
                        Add(diagnostics, "typescript_rest_initializer", "A rest parameter cannot have an initializer.", parameter);
                    if (HasTrailingComma(list))
                        Add(diagnostics, "typescript_rest_trailing_comma", "A rest parameter cannot have a trailing comma.", parameter);
                }
                else if (optional)
                {
                    optionalSeen = true;
                    if (initializer)
                        Add(diagnostics, "typescript_optional_initializer", "A parameter cannot have both a question mark and an initializer.", parameter);
                }
                else if (optionalSeen && !initializer)
                    Add(diagnostics, "typescript_required_after_optional", "A required parameter cannot follow an optional parameter.", parameter);
            }
        }

        private static List<ParseTreeNonTerminal> Parameters(ParseTreeSymbol list)
        {
            var parameters = new List<ParseTreeNonTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(list);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == "FormalParameter" || name == "FunctionRestParameter" || name == "TSThisParameter" || name == "TSParameter")
                { parameters.Add(node); continue; }
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            return parameters;
        }

        private static void CheckTypeParameters(ParseTreeNonTerminal list, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameters = Roles(list, "TSTypeParameter");
            bool defaultSeen = false;
            var names = new HashSet<string>(StringComparer.Ordinal);
            string owner = TypeParameterOwner(list);
            bool varianceAllowed = owner == "ClassDeclaration" || owner == "ClassExpression" ||
                owner == "TSInterfaceDeclaration" || owner == "TSTypeAliasDeclaration";
            bool constAllowed = owner == "ClassDeclaration" || owner == "ClassExpression" ||
                JavaScriptSyntax.IsFunction(owner) || owner == "TSFunctionType" || owner == "TSMethodSignature" ||
                owner == "TSCallSignature" || owner == "TSConstructSignature" || owner == "TSFunctionOverload" || owner == "TSAmbientFunctionDeclaration";
            foreach (var parameter in parameters)
            {
                var binding = Child(parameter, "TSBindingIdentifier");
                if (!ReferenceEquals(binding, null))
                {
                    string identifier = JavaScriptTokenPatterns.DecodeIdentifier(FirstToken(binding)?.Input ?? string.Empty);
                    if (!names.Add(identifier))
                        Add(diagnostics, "typescript_duplicate_type_parameter", "A type parameter name cannot appear twice in the same list.", binding);
                }
                bool hasDefault = !ReferenceEquals(Child(parameter, "TSTypeDefault"), null);
                if (hasDefault) defaultSeen = true;
                else if (defaultSeen)
                    Add(diagnostics, "typescript_required_type_parameter", "A required type parameter cannot follow a type parameter with a default.", parameter);
                var modifiers = Child(parameter, "TSTypeParameterModifiers");
                if (ReferenceEquals(modifiers, null)) continue;
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var token in Tokens(modifiers))
                {
                    string value = token.Input;
                    if (!seen.Add(value))
                        diagnostics.Add(new JavaScriptDiagnostic("typescript_duplicate_type_modifier", "A type parameter modifier cannot appear twice.", token.StartIndex, token.Input.Length));
                    if (value == "in" && seen.Contains("out"))
                        diagnostics.Add(new JavaScriptDiagnostic("typescript_type_modifier_order", "The in modifier must precede the out modifier.", token.StartIndex, token.Input.Length));
                    if ((value == "in" || value == "out") && !varianceAllowed)
                        diagnostics.Add(new JavaScriptDiagnostic("typescript_variance_context", "Variance modifiers require a class, interface, or type alias type parameter.", token.StartIndex, token.Input.Length));
                    if (value == "const" && !constAllowed)
                        diagnostics.Add(new JavaScriptDiagnostic("typescript_const_type_parameter_context", "A const type parameter requires a function, method, or class.", token.StartIndex, token.Input.Length));
                }
            }
        }

        private static void CheckTuple(ParseTreeNonTerminal tuple, List<JavaScriptDiagnostic> diagnostics)
        {
            bool optionalSeen = false, arrayRestSeen = false;
            foreach (var element in Roles(tuple, "TSTupleElement"))
            {
                var member = Child(element, "TSNamedTupleMember") ?? Child(element, "TSUnnamedTupleMember");
                if (!(member is ParseTreeNonTerminal node)) continue;
                bool rest = HasDirectToken(node, "...");
                bool optional = HasDirectToken(node, "?");
                if (rest && optional)
                    Add(diagnostics, "typescript_optional_tuple_rest", "A tuple member cannot be both optional and rest.", node);
                if (optional)
                {
                    if (arrayRestSeen)
                        Add(diagnostics, "typescript_tuple_optional_after_rest", "An optional tuple element cannot follow a rest array element.", node);
                    optionalSeen = true;
                }
                else if (!rest && optionalSeen)
                    Add(diagnostics, "typescript_tuple_required_after_optional", "A required tuple element cannot follow an optional element.", node);
                if (rest && IsArrayType(Child(node, "TSType")))
                {
                    if (arrayRestSeen)
                        Add(diagnostics, "typescript_tuple_duplicate_array_rest", "A rest array element cannot follow another rest array element.", node);
                    arrayRestSeen = true;
                }
            }
        }

        private static void CheckAccessorSignature(ParseTreeNonTerminal signature, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameters = Child(signature, "TSFunctionParameters");
            if (ReferenceEquals(parameters, null)) return;
            var runtime = new List<ParseTreeNonTerminal>();
            foreach (var parameter in Parameters(parameters)) if (!IsThisParameter(parameter)) runtime.Add(parameter);
            bool getter = TypeScriptProjection.Name(signature) == "TSGetterSignature";
            if (runtime.Count != (getter ? 0 : 1))
                Add(diagnostics, "typescript_accessor_parameters", getter ? "A getter signature cannot have runtime parameters." : "A setter signature requires one runtime parameter.", parameters);
            if (!getter && runtime.Count == 1 && (HasDirectToken(runtime[0], "...") ||
                Contains(runtime[0], "TSParameterOptional", stopAtTypes: true)))
                Add(diagnostics, "typescript_setter_parameter", "A setter parameter cannot be rest or optional.", runtime[0]);
            if (!getter && !ReferenceEquals(Child(signature, "TSReturnTypeAnnotation"), null))
                Add(diagnostics, "typescript_setter_return_type", "A setter cannot have a return type annotation.", signature);
        }

        private static void CheckIndexSignature(ParseTreeNonTerminal signature, List<JavaScriptDiagnostic> diagnostics)
        {
            var parameters = Roles(signature, TypeScriptProjection.Name(signature) == "TSClassIndexSignature"
                ? "TSClassIndexParameter" : "TSIndexParameter");
            if (parameters.Count != 1)
                Add(diagnostics, "typescript_index_parameter_count", "An index signature must have exactly one parameter.", signature);
            foreach (var parameter in parameters)
                if (ReferenceEquals(Child(parameter, "TSTypeAnnotation"), null))
                    Add(diagnostics, "typescript_index_parameter_type", "An index signature parameter must have a type annotation.", parameter);
            if (ReferenceEquals(Child(signature, "TSTypeAnnotation"), null))
                Add(diagnostics, "typescript_index_result_type", "An index signature must have a result type annotation.", signature);
        }

        private static void CheckNamespace(ParseTreeNonTerminal declaration, List<JavaScriptDiagnostic> diagnostics)
        {
            var enclosing = declaration.Parent;
            while (!ReferenceEquals(enclosing, null))
            {
                string owner = TypeScriptProjection.Name(enclosing);
                if (owner == "TSNamespaceBody" || owner == "Script" || owner == "Module") break;
                if (owner == "FunctionBody" || owner == "Block" || owner == "ClassStaticBlock" || JavaScriptSyntax.IsFunction(owner))
                {
                    Add(diagnostics, "typescript_namespace_position", "A namespace declaration must be at the top level of a source file or namespace.", declaration);
                    break;
                }
                enclosing = enclosing.Parent;
            }
            // The projection supplies a scope boundary, not a callable function.
            // Preserve the source-level restrictions that differ from that view.
            var pending = new Stack<NamespaceVisit>();
            foreach (var body in Roles(declaration, "TSNamespaceBody")) pending.Push(new NamespaceVisit(body, false));
            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                if (!(visit.Symbol is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == "TSNamespaceDeclaration" || name == "TSAmbientDeclaration" || name == "ClassStaticBlock") continue;
                bool returnAllowed = visit.ReturnAllowed;
                if (JavaScriptSyntax.IsFunction(name))
                {
                    if (JavaScriptSyntax.IsMethod(name))
                    {
                        foreach (var key in JavaScriptSyntax.ElementNames(node)) pending.Push(new NamespaceVisit(key, returnAllowed));
                        continue;
                    }
                    if (!JavaScriptSyntax.IsArrow(name)) continue;
                    returnAllowed = true; // An arrow has return, but inherits new.target.
                }
                if (name == "ClassField")
                {
                    foreach (var key in JavaScriptSyntax.ElementNames(node)) pending.Push(new NamespaceVisit(key, returnAllowed));
                    continue;
                }
                if (name == "ReturnStatement" && !returnAllowed)
                    Add(diagnostics, "typescript_namespace_return", "A namespace body cannot contain a top-level return statement.", node);
                if (name == "NewTarget")
                    Add(diagnostics, "typescript_namespace_new_target", "A namespace body does not establish a new.target context.", node);
                if (name == "ImportDeclaration" || name == "ExportDeclaration" &&
                    !ReferenceEquals(Child(node, "FromClause"), null))
                    Add(diagnostics, "typescript_namespace_external_import", "An ordinary namespace cannot contain an external-module import or re-export.", node);
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(new NamespaceVisit(node[i], returnAllowed));
            }
        }

        private readonly struct NamespaceVisit
        {
            internal NamespaceVisit(ParseTreeSymbol symbol, bool returnAllowed) { Symbol = symbol; ReturnAllowed = returnAllowed; }
            internal ParseTreeSymbol Symbol { get; }
            internal bool ReturnAllowed { get; }
        }

        private static void CheckAmbient(ParseTreeNonTerminal declaration, List<JavaScriptDiagnostic> diagnostics)
        {
            if (Contains(declaration, "FunctionBody"))
                Add(diagnostics, "typescript_ambient_implementation", "An implementation cannot be declared in an ambient context.", declaration);
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(declaration);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == "ExpressionStatement" || name == "IfStatement" || name == "ForStatement" ||
                    name == "ForInStatement" || name == "ForOfStatement" || name == "ForAwaitOfStatement" ||
                    name == "WhileStatement" || name == "DoWhileStatement" || name == "ReturnStatement" ||
                    name == "ThrowStatement" || name == "TryStatement" || name == "SwitchStatement" ||
                    name == "WithStatement" || name == "DebuggerStatement" || name == "LabelledStatement")
                    Add(diagnostics, "typescript_ambient_statement", "Statements are not allowed in an ambient declaration body.", node);
                if (name == "VariableStatement" || name == "LexicalDeclaration")
                {
                    bool constant = FirstToken(node)?.Input == "const";
                    foreach (var binding in Roles(node, name == "VariableStatement" ? "InitializedVariableDeclaration" : "InitializedLexicalBinding"))
                        CheckAmbientInitializer(binding, constant, diagnostics);
                }
                if (name == "ClassField")
                {
                    bool readOnly = TypeScriptClassValidation.HasModifier(node, "readonly");
                    CheckAmbientInitializer(node, readOnly, diagnostics);
                }
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static void CheckAmbientInitializer(ParseTreeNonTerminal declaration, bool constantOrReadonly,
            List<JavaScriptDiagnostic> diagnostics)
        {
            var initializer = Child(declaration, "Initializer");
            if (ReferenceEquals(initializer, null)) return;
            bool annotation = !ReferenceEquals(Child(declaration, "TSAnnotation"), null) ||
                !ReferenceEquals(Child(declaration, "TSTypeAnnotation"), null);
            if (!constantOrReadonly || annotation || !IsAmbientLiteral(initializer))
                Add(diagnostics, "typescript_ambient_initializer", "An ambient initializer requires an unannotated const or readonly declaration and a literal or enum reference.", initializer);
            foreach (var token in Tokens(initializer))
                if (token.Kind.Caption == "NoSubstitutionTemplate")
                {
                    var invalidEscape = JavaScriptTemplateTokens.GetInvalidEscape(token);
                    if (invalidEscape != null) diagnostics.Add(invalidEscape);
                }
        }

        private static bool IsAmbientLiteral(ParseTreeSymbol initializer)
        {
            var tokens = new List<TokenData>(Tokens(initializer));
            if (tokens.Count != 0 && tokens[0].Input == "=") tokens.RemoveAt(0);
            if (tokens.Count == 1)
            {
                var token = tokens[0];
                return token.Kind.Caption == "StringLiteral" || token.Kind.Caption == "NoSubstitutionTemplate" || token.Kind.Caption == "NumericLiteral" ||
                    token.Input == "true" || token.Input == "false";
            }
            if (tokens.Count == 2 && tokens[0].Input == "-" && tokens[1].Kind.Caption == "NumericLiteral") return true;
            // Whether a qualified name denotes an enum is a type/name-resolution
            // question. Retain syntactically simple references for that later layer.
            if (tokens.Count < 3 || tokens.Count % 2 == 0) return false;
            for (int i = 0; i < tokens.Count; i++)
                if (i % 2 == 1 ? tokens[i].Input != "." : !TypeScriptLookahead.IsName(tokens[i].Input)) return false;
            return true;
        }

        private static ParseTreeSymbol Child(ParseTreeNonTerminal node, string name)
        {
            var pending = new Stack<ParseTreeSymbol>();
            for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            while (pending.Count != 0)
            {
                var child = pending.Pop();
                string role = TypeScriptProjection.Name(child);
                if (role == name) return child;
                if (child is ParseTreeNonTerminal wrapper && role.StartsWith("__", StringComparison.Ordinal))
                    for (int i = wrapper.Count - 1; i >= 0; i--) pending.Push(wrapper[i]);
            }
            return null;
        }

        private static bool HasAncestor(ParseTreeSymbol node, string role)
        {
            var parent = node.Parent;
            while (!ReferenceEquals(parent, null))
            {
                if (TypeScriptProjection.Name(parent) == role) return true;
                parent = parent.Parent;
            }
            return false;
        }

        private static List<ParseTreeNonTerminal> Roles(ParseTreeSymbol root, string role)
        {
            var result = new List<ParseTreeNonTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                if (TypeScriptProjection.Name(node) == role) { result.Add(node); continue; }
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            return result;
        }

        private static bool HasDirectToken(ParseTreeNonTerminal node, string text)
        {
            foreach (var child in node.Items)
                if (child is ParseTreeTerminal terminal && terminal.Token.Input == text) return true;
            return false;
        }

        private static bool HasTrailingComma(ParseTreeSymbol list)
        {
            TokenData previous = null, last = null;
            foreach (var token in Tokens(list)) { previous = last; last = token; }
            return last?.Input == "," || last?.Input == ")" && previous?.Input == ",";
        }

        private static bool IsThisParameter(ParseTreeNonTerminal parameter)
        {
            if (TypeScriptProjection.Name(parameter) == "TSThisParameter") return true;
            var binding = Child(parameter, "TSParameterBinding") as ParseTreeNonTerminal;
            return !ReferenceEquals(binding, null) && binding.Count == 1 &&
                binding[0] is ParseTreeTerminal terminal && terminal.Token.Input == "this";
        }

        private static string TypeParameterOwner(ParseTreeNonTerminal list)
        {
            var parent = list.Parent;
            while (!ReferenceEquals(parent, null))
            {
                string name = TypeScriptProjection.Name(parent);
                if (name == "ClassDeclaration" || name == "ClassExpression" || JavaScriptSyntax.IsFunction(name) ||
                    name == "TSInterfaceDeclaration" || name == "TSTypeAliasDeclaration" || name == "TSFunctionType" ||
                    name == "TSMethodSignature" || name == "TSCallSignature" || name == "TSConstructSignature" ||
                    name == "TSFunctionOverload" || name == "TSAmbientFunctionDeclaration") return name;
                parent = parent.Parent;
            }
            return string.Empty;
        }

        private static bool IsArrayType(ParseTreeSymbol type)
        {
            while (type is ParseTreeNonTerminal node)
            {
                string name = TypeScriptProjection.Name(node);
                if (name == "TSPostfixType" && node.Count == 3 && HasDirectToken(node, "[") && HasDirectToken(node, "]")) return true;
                if (name == "TSTypeOperator" && node.Count == 2 && HasDirectToken(node, "readonly")) { type = node[1]; continue; }
                if (node.Count != 1) return false;
                type = node[0];
            }
            return false;
        }

        private static TokenData FirstToken(ParseTreeSymbol node)
        {
            foreach (var token in Tokens(node)) return token;
            return null;
        }

        private static IEnumerable<TokenData> Tokens(ParseTreeSymbol root)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal) yield return terminal.Token;
                else if (symbol is ParseTreeNonTerminal node)
                    for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static bool Contains(ParseTreeSymbol root, string role, bool stopAtTypes = false, bool stopAtExpressions = false)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = TypeScriptProjection.Name(node);
                if (name == role) return true;
                if (stopAtTypes && (name.StartsWith("TSType", StringComparison.Ordinal) || name == "TSAnnotation" || name == "TSReturnType")) continue;
                if (stopAtExpressions && (name == "Initializer" || name == "InitializerNoIn" ||
                    name == "AssignmentExpression" || name == "AssignmentExpressionNoIn")) continue;
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            return false;
        }

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            // Do not flatten an entire declaration recursively: large nested type
            // syntax must not consume the C# call stack merely to report a span.
            int start = int.MaxValue, end = 0;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(node);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal)
                {
                    if (terminal.Token.StartIndex < 0) continue;
                    start = Math.Min(start, terminal.Token.StartIndex);
                    end = Math.Max(end, terminal.Token.EndIndex + 1);
                }
                else if (symbol is ParseTreeNonTerminal nonterminal)
                    for (int i = nonterminal.Count - 1; i >= 0; i--) pending.Push(nonterminal[i]);
            }
            if (start == int.MaxValue) start = 0;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, Math.Max(0, end - start)));
        }
    }
}
