using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Checks the grammatical restrictions on TypeScript class modifiers, member
    /// signatures and constructor parameter properties. All checks use the original
    /// concrete tree; resolving a base type or checking an override's type is left
    /// to a type checker. The JavaScript validation view keeps its existing checks.
    /// </summary>
    internal static class TypeScriptClassValidation
    {
        internal static bool HasModifier(ParseTreeNonTerminal member, string modifier)
            => new Member(member).Has(modifier);

        internal static void Validate(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(tree);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (IsClassMember(name)) CheckMember(new Member(node), diagnostics);
                if (name == "FormalParameter" && Part(node, "TSParameterModifiers") != null)
                    CheckParameterProperty(node, diagnostics);
                if (name == "TSClassIndexParameter") CheckIndexBinding(node, diagnostics);
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static bool IsClassMember(string name)
            => name == "ClassField" || name == "ClassStaticBlock" || name == "TSClassIndexSignature" ||
               name.StartsWith("Class", StringComparison.Ordinal) && JavaScriptSyntax.IsMethod(name);

        private static void CheckIndexBinding(ParseTreeNonTerminal parameter, List<JavaScriptDiagnostic> diagnostics)
        {
            var binding = Part(parameter, "TSClassIndexBinding");
            var token = FirstToken(binding);
            if (token == null) return;
            string identifier = JavaScriptTokenPatterns.DecodeIdentifier(token.Input);
            // Index signatures disappear from the JavaScript validation view.
            // Preserve the same strict binding-name rules on their original names.
            var strictness = JavaScriptStrictness.Analyze(binding, JavaScriptMode.Strict);
            if (strictness.IsReserved(identifier, binding) || identifier == "eval" || identifier == "arguments" ||
                identifier == "await" && JavaScriptSyntax.HasAwaitContext(parameter))
                Add(diagnostics, "typescript_class_index_binding", "This name cannot be used for an index-signature parameter in the current class context.", binding);
        }

        private static void CheckMember(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (Name(member.Node) == "ClassStaticBlock")
            {
                if (member.Modifiers.Count != 0)
                    Add(diagnostics, "typescript_static_block_modifier", "A static initialization block cannot have declaration modifiers.", member.Node);
                if (member.Ambient)
                    Add(diagnostics, "typescript_ambient_static_block", "A static initialization block cannot appear in an ambient context.", member.Node);
                return;
            }
            CheckModifiers(member.Modifiers, member.Field, member.Index, false, diagnostics);
            CheckPrivateModifiers(member, diagnostics);
            CheckAbstractMember(member, diagnostics);
            CheckAmbientMember(member, diagnostics);
            if (member.Field) CheckField(member, diagnostics);
            else if (!member.Index) CheckMethod(member, diagnostics);
        }

        private static void CheckModifiers(List<TokenData> modifiers, bool field, bool index,
            bool parameter, List<JavaScriptDiagnostic> diagnostics)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            bool accessibility = false;
            foreach (var token in modifiers)
            {
                string current = token.Input;
                if (current == "get" || current == "set") continue;
                bool visibility = IsAccessibility(current);
                if (!seen.Add(current) || visibility && accessibility)
                {
                    Add(diagnostics, "typescript_duplicate_class_modifier", "A modifier or accessibility modifier cannot be repeated.", token);
                    continue;
                }
                if (visibility) accessibility = true;
                if (parameter && !visibility && current != "readonly" && current != "override")
                    Add(diagnostics, "typescript_parameter_modifier", "A parameter property only permits accessibility, readonly and override modifiers.", token);
                if (index && current != "readonly" && current != "static")
                    Add(diagnostics, "typescript_index_modifier", "A class index signature only permits readonly and static modifiers.", token);
                if (current == "readonly" && !field && !index && !parameter)
                    Add(diagnostics, "typescript_readonly_member", "The readonly modifier requires a property or index signature.", token);
                if (current == "accessor" && !field)
                    Add(diagnostics, "typescript_accessor_modifier", "The accessor modifier requires a property declaration.", token);
                if (current == "declare" && !field)
                    Add(diagnostics, "typescript_declare_member", "The declare modifier can only appear on a class property.", token);
                if (ComesBeforeSeen(current, seen))
                    Add(diagnostics, "typescript_class_modifier_order", "This modifier must precede an earlier modifier on the declaration.", token);
            }
            CheckIncompatibleModifiers(seen, modifiers, diagnostics);
        }

        private static bool ComesBeforeSeen(string current, HashSet<string> seen)
        {
            if (IsAccessibility(current))
                return HasAny(seen, "static", "abstract", "override", "readonly", "accessor", "async");
            if (current == "static") return HasAny(seen, "override", "readonly", "accessor", "async");
            if (current == "abstract") return HasAny(seen, "override", "accessor");
            if (current == "override") return HasAny(seen, "readonly", "accessor", "async");
            return false;
        }

        private static void CheckIncompatibleModifiers(HashSet<string> seen, List<TokenData> modifiers,
            List<JavaScriptDiagnostic> diagnostics)
        {
            bool invalid = seen.Contains("abstract") && HasAny(seen, "private", "static", "async") ||
                seen.Contains("declare") && HasAny(seen, "override", "async", "accessor") ||
                seen.Contains("accessor") && seen.Contains("readonly");
            if (!invalid || modifiers.Count == 0) return;
            Add(diagnostics, "typescript_incompatible_class_modifiers", "These class modifiers cannot be used together.", modifiers[modifiers.Count - 1]);
        }

        private static void CheckPrivateModifiers(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (!member.Private) return;
            foreach (var token in member.Modifiers)
                if (IsAccessibility(token.Input) || token.Input == "declare" || token.Input == "abstract")
                    Add(diagnostics, "typescript_private_identifier_modifier", "A private identifier cannot have an accessibility, declare or abstract modifier.", token);
        }

        private static void CheckAbstractMember(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (!member.Has("abstract")) return;
            var owner = EnclosingClass(member.Node);
            if (owner == null || !IsAbstractClass(owner))
                Add(diagnostics, "typescript_abstract_member_class", "An abstract member requires an abstract class declaration.", member.Node);
            if (member.Body != null)
                Add(diagnostics, "typescript_abstract_implementation", "An abstract method or accessor cannot have an implementation.", member.Body);
            if (member.Initializer != null)
                Add(diagnostics, "typescript_abstract_initializer", "An abstract property cannot have an initializer.", member.Initializer);
            if (member.Constructor)
                Add(diagnostics, "typescript_constructor_modifier", "A constructor cannot have an abstract modifier.", member.Node);
        }

        private static void CheckAmbientMember(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (!member.Ambient) return;
            if (member.Has("async") || JavaScriptSyntax.IsAsync(member.Node))
                Add(diagnostics, "typescript_ambient_class_async", "An ambient class member cannot have an async modifier.", member.Node);
            // The enclosing ambient-declaration checker also handles implementations
            // and initializer forms. A member's own declare modifier needs the same
            // initializer rule even when its containing class is not ambient.
            if (!member.Has("declare") || member.Initializer == null) return;
            if (!member.Has("readonly") || member.Annotation != null || !IsAmbientLiteral(member.Initializer))
                Add(diagnostics, "typescript_ambient_initializer", "An ambient property initializer requires an unannotated readonly property and a literal or enum reference.", member.Initializer);
            foreach (var token in Tokens(member.Initializer))
                if (token.Kind.Caption == "NoSubstitutionTemplate")
                {
                    var invalidEscape = JavaScriptTemplateTokens.GetInvalidEscape(token);
                    if (invalidEscape != null) diagnostics.Add(invalidEscape);
                }
        }

        private static void CheckField(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (member.Has("accessor") && Marker(member.Node, "TSOptional") != null)
                Add(diagnostics, "typescript_optional_accessor", "An auto-accessor property cannot be optional.", member.Node);
            var definite = Marker(member.Node, "TSDefinite");
            if (definite == null) return;
            if (member.Annotation == null || member.Initializer != null || member.Static || member.Ambient || member.Has("abstract"))
                Add(diagnostics, "typescript_definite_assignment", "A definite assignment assertion requires a typed, non-static, non-ambient property without an initializer.", definite);
        }

        private static void CheckMethod(Member member, List<JavaScriptDiagnostic> diagnostics)
        {
            if (member.Body == null && JavaScriptSyntax.IsGenerator(member.Node))
                Add(diagnostics, "typescript_generator_signature", "An overload signature cannot be declared as a generator.", member.Node);
            var parameters = Part(member.Node, "FormalParameters");
            bool typeParameters = HasTokens(Part(member.Node, "TSOptionalTypeParameters")) ||
                HasTokens(Part(member.Node, "TSTypeParameters"));
            bool returnType = HasTokens(Part(member.Node, "TSOptionalReturnType")) ||
                HasTokens(Part(member.Node, "TSReturnTypeAnnotation"));
            if (Marker(member.Node, "TSDefinite") != null)
                Add(diagnostics, "typescript_method_definite_assignment", "A method cannot have a definite assignment assertion.", member.Node);
            if (member.Constructor)
            {
                if (typeParameters || returnType || Marker(member.Node, "TSOptional") != null)
                    Add(diagnostics, "typescript_constructor_signature", "A constructor cannot be optional or declare type parameters or a return type.", member.Node);
                if (member.Has("override"))
                    Add(diagnostics, "typescript_constructor_modifier", "A constructor cannot have an override modifier.", member.Node);
                if (Contains(parameters, "TSThisParameter"))
                    Add(diagnostics, "typescript_constructor_this", "A constructor cannot declare a this parameter.", parameters);
            }
            if (member.Accessor == null) return;
            if (typeParameters || Marker(member.Node, "TSOptional") != null)
                Add(diagnostics, "typescript_accessor_signature", "An accessor cannot be optional or declare type parameters.", member.Node);
            if (Contains(parameters, "TSThisParameter"))
                Add(diagnostics, "typescript_accessor_this", "An accessor cannot declare a this parameter.", parameters);
            if (member.Body == null && !member.Ambient && !member.Has("abstract"))
                Add(diagnostics, "typescript_accessor_implementation", "A non-ambient, non-abstract accessor requires an implementation.", member.Node);
            CheckAccessorParameters(parameters, member.Accessor == "get", member.Body == null, diagnostics);
            if (member.Accessor != "set") return;
            if (returnType)
                Add(diagnostics, "typescript_setter_return_type", "A setter cannot have a return type annotation.", member.Node);
        }

        private static void CheckAccessorParameters(ParseTreeSymbol parameters, bool getter, bool erasedSignature,
            List<JavaScriptDiagnostic> diagnostics)
        {
            if (parameters == null) return;
            var runtime = new List<ParseTreeNonTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(parameters);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "FormalParameter" || name == "FunctionRestParameter")
                {
                    if (!Contains(node, "TSThisParameter")) runtime.Add(node);
                    continue;
                }
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            // Methods with bodies keep their JavaScript arity diagnostics. Only
            // erased signatures need a replacement arity/rest check in this layer.
            if (erasedSignature && runtime.Count != (getter ? 0 : 1))
                Add(diagnostics, "typescript_accessor_parameters", getter ?
                    "A getter cannot declare runtime parameters." : "A setter requires exactly one runtime parameter.", parameters);
            if (getter) return;
            foreach (var parameter in runtime)
            {
                if (erasedSignature && Name(parameter) == "FunctionRestParameter")
                    Add(diagnostics, "typescript_setter_parameter", "A setter cannot declare a rest parameter.", parameter);
                if (Part(parameter, "TSOptional") != null || Part(parameter, "Initializer") != null)
                    Add(diagnostics, "typescript_setter_parameter", "A setter parameter cannot be optional or have an initializer.", parameter);
            }
            // Unlike ECMAScript setters, the TypeScript parser/checker permits
            // a trailing comma after the setter's sole parameter.
        }

        private static void CheckParameterProperty(ParseTreeNonTerminal parameter,
            List<JavaScriptDiagnostic> diagnostics)
        {
            var modifiers = new List<TokenData>(Tokens(Part(parameter, "TSParameterModifiers")));
            CheckModifiers(modifiers, false, false, true, diagnostics);
            var owner = EnclosingFunction(parameter);
            var member = owner != null && IsClassMember(Name(owner)) ? new Member(owner) : null;
            if (member == null || !member.Constructor || member.Body == null || member.Ambient)
                Add(diagnostics, "typescript_parameter_property_context", "A parameter property is only allowed in a constructor implementation.", parameter);
            if (Contains(parameter, "BindingPattern") || Contains(parameter, "BindingRestElement") ||
                Contains(parameter, "FunctionRestParameter"))
                Add(diagnostics, "typescript_parameter_property_binding", "A parameter property cannot use a binding pattern or a rest parameter.", parameter);
            var binding = Part(parameter, "BindingIdentifier");
            var token = FirstToken(binding);
            if (token != null && JavaScriptTokenPatterns.DecodeIdentifier(token.Input) == "constructor")
                Add(diagnostics, "typescript_parameter_property_name", "A parameter property cannot be named constructor.", binding);
        }

        private static bool IsAbstractClass(ParseTreeNonTerminal node)
        {
            // Only a modifier on this class counts. Looking through an enclosing
            // field initializer or function would borrow another declaration's
            // abstract modifier for a nested class expression.
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
            {
                string name = Name(parent);
                if (name == "TSAbstractClassDeclaration" || name == "TSAbstractClassExpression") return true;
                if (name == "TSAmbientTarget" && parent is ParseTreeNonTerminal target)
                {
                    foreach (var child in target.Items)
                        if (child is ParseTreeTerminal token && token.Token.Input == "abstract") return true;
                }
                if (!name.StartsWith("__", StringComparison.Ordinal)) return false;
            }
            return false;
        }

        private static bool IsAmbient(ParseTreeSymbol node)
        {
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
                if (Name(parent) == "TSAmbientDeclaration") return true;
            return false;
        }

        private static ParseTreeNonTerminal EnclosingClass(ParseTreeSymbol node)
        {
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
                if (parent is ParseTreeNonTerminal candidate &&
                    (Name(candidate) == "ClassDeclaration" || Name(candidate) == "ClassExpression")) return candidate;
            return null;
        }

        private static ParseTreeNonTerminal EnclosingFunction(ParseTreeSymbol node)
        {
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
                if (parent is ParseTreeNonTerminal candidate && JavaScriptSyntax.IsFunction(Name(candidate))) return candidate;
            return null;
        }

        private static bool IsAmbientLiteral(ParseTreeSymbol initializer)
        {
            var tokens = new List<TokenData>(Tokens(initializer));
            if (tokens.Count != 0 && tokens[0].Input == "=") tokens.RemoveAt(0);
            if (tokens.Count == 1)
                return tokens[0].Kind.Caption == "StringLiteral" || tokens[0].Kind.Caption == "NumericLiteral" ||
                    tokens[0].Kind.Caption == "NoSubstitutionTemplate" || tokens[0].Input == "true" || tokens[0].Input == "false";
            if (tokens.Count == 2 && tokens[0].Input == "-" && tokens[1].Kind.Caption == "NumericLiteral") return true;
            if (tokens.Count < 3 || tokens.Count % 2 == 0) return false;
            for (int i = 0; i < tokens.Count; i++)
                if (i % 2 == 1 ? tokens[i].Input != "." : !TypeScriptLookahead.IsName(tokens[i].Input)) return false;
            return true;
        }

        private static bool IsAccessibility(string value)
            => value == "public" || value == "private" || value == "protected";

        private static bool HasAny(HashSet<string> values, params string[] choices)
        {
            foreach (string choice in choices) if (values.Contains(choice)) return true;
            return false;
        }

        private static ParseTreeSymbol Marker(ParseTreeNonTerminal node, string name)
        {
            var direct = Part(node, name);
            if (direct != null) return direct;
            var marker = Part(node, "TSClassFieldMarker") as ParseTreeNonTerminal;
            if (marker != null) return Part(marker, name);
            var optional = Part(node, "TSClassMethodOptional") as ParseTreeNonTerminal;
            return optional == null ? null : Part(optional, name);
        }

        private static ParseTreeSymbol Part(ParseTreeNonTerminal node, string role)
        {
            var pending = new Stack<ParseTreeSymbol>();
            for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            while (pending.Count != 0)
            {
                var part = pending.Pop();
                string name = Name(part);
                if (name == role) return part;
                if (part is ParseTreeNonTerminal wrapper && name.StartsWith("__", StringComparison.Ordinal))
                    for (int i = wrapper.Count - 1; i >= 0; i--) pending.Push(wrapper[i]);
            }
            return null;
        }

        private static bool Contains(ParseTreeSymbol root, string role)
        {
            if (root == null) return false;
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var current = pending.Pop();
                string name = Name(current);
                if (name == role) return true;
                if (name.StartsWith("TSType", StringComparison.Ordinal) || name == "TSAnnotation" ||
                    name == "FunctionBody" || name == "Initializer" && role != "Initializer") continue;
                if (current is ParseTreeNonTerminal node)
                    for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
            return false;
        }

        private static bool HasTokens(ParseTreeSymbol node) => FirstToken(node) != null;

        private static TokenData FirstToken(ParseTreeSymbol node)
        {
            foreach (var token in Tokens(node)) return token;
            return null;
        }

        private static IEnumerable<TokenData> Tokens(ParseTreeSymbol root)
        {
            if (root == null) yield break;
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

        private static string Name(ParseTreeSymbol node) => TypeScriptProjection.Name(node);

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, TokenData token)
            => diagnostics.Add(new JavaScriptDiagnostic(code, message, token.StartIndex, token.EndIndex - token.StartIndex + 1));

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            int start = int.MaxValue, end = 0;
            foreach (var token in Tokens(node))
            {
                if (token.StartIndex < 0) continue;
                start = Math.Min(start, token.StartIndex);
                end = Math.Max(end, token.EndIndex + 1);
            }
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start == int.MaxValue ? 0 : start,
                start == int.MaxValue ? 0 : end - start));
        }

        private sealed class Member
        {
            internal Member(ParseTreeNonTerminal node)
            {
                Node = node;
                Field = Name(node) == "ClassField";
                Index = Name(node) == "TSClassIndexSignature";
                Modifiers = new List<TokenData>();
                for (var parent = node.Parent; parent is ParseTreeNonTerminal wrapper; parent = parent.Parent)
                {
                    string role = Name(wrapper);
                    if (role == "TSClassModifiedElement")
                    {
                        Modifiers.AddRange(Tokens(Part(wrapper, "TSModifiers")));
                        break;
                    }
                    if (role != "TSClassModifiedMember" && role != "TSClassMember" &&
                        !role.StartsWith("__", StringComparison.Ordinal)) break;
                }
                var names = JavaScriptSyntax.ElementNames(node);
                for (int i = 0; i + 1 < names.Count; i++)
                {
                    var token = FirstToken(names[i]);
                    if (token != null) Modifiers.Add(token);
                }
                if (Index)
                {
                    var signature = Part(node, "TSIndexSignature");
                    var token = FirstToken(signature);
                    if (token?.Input == "readonly") Modifiers.Add(token);
                }
                if (names.Count != 0)
                {
                    var nameTokens = new List<TokenData>(Tokens(names[names.Count - 1]));
                    if (nameTokens.Count == 1)
                    {
                        var token = nameTokens[0];
                        Private = token.Kind.Caption == "PrivateIdentifier";
                        if (token.Kind.Caption == "StringLiteral") Spelling = JavaScriptExpressionValidation.DecodeString(token.Input);
                        else if (token.Kind.Caption == "NumericLiteral") Spelling = token.Input;
                        else if (!Private) Spelling = JavaScriptTokenPatterns.DecodeIdentifier(token.Input);
                    }
                }
                foreach (var token in Modifiers)
                {
                    if (token.Input == "static") Static = true;
                    if (token.Input == "get" || token.Input == "set") Accessor = token.Input;
                }
                Constructor = !Field && !Index && !Static && !Private && Accessor == null &&
                    !JavaScriptSyntax.IsAsync(node) && !JavaScriptSyntax.IsGenerator(node) && Spelling == "constructor";
                Body = Part(node, "FunctionBody");
                Initializer = Part(node, "Initializer");
                Annotation = Part(node, "TSAnnotation");
                Ambient = Has("declare") || IsAmbient(node);
            }

            internal ParseTreeNonTerminal Node { get; }
            internal List<TokenData> Modifiers { get; }
            internal bool Field { get; }
            internal bool Index { get; }
            internal bool Private { get; }
            internal bool Static { get; }
            internal bool Constructor { get; }
            internal string Spelling { get; }
            internal string Accessor { get; }
            internal bool Ambient { get; }
            internal ParseTreeSymbol Body { get; }
            internal ParseTreeSymbol Initializer { get; }
            internal ParseTreeSymbol Annotation { get; }
            internal bool Has(string value)
            {
                foreach (var token in Modifiers) if (token.Input == value) return true;
                return false;
            }
        }
    }
}
