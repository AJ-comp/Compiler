using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Checks class element names, private-name scopes and the lexical contexts of
    /// super, new.target, arguments and static initialization blocks. All context
    /// belongs to this validation call; neither parser states nor the tree are changed.
    /// <para><see href="../../../../docs/ko/javascript-classes.html#early-errors">클래스 문법과 파싱 후 검사하는 오류</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html#context">메서드와 일반 함수의 super·new.target 문맥</see></para>
    /// </summary>
    internal static class JavaScriptClassValidation
    {
        internal static void Validate(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            var pending = new Stack<Visit>();
            pending.Push(new Visit(tree, new Context()));
            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                if (visit.Symbol is ParseTreeTerminal terminal)
                {
                    if (terminal.Token.Kind.Caption == "PrivateIdentifier" &&
                        !visit.Context.HasPrivate(PrivateName(terminal)))
                        Add(diagnostics, "undeclared_private_name", "This private name is not declared by an enclosing class.", terminal);
                    continue;
                }
                if (!(visit.Symbol is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                var context = visit.Context;

                if (name == "ClassDeclaration" || name == "ClassExpression")
                {
                    VisitClass(node, context, pending, diagnostics);
                    continue;
                }
                if (JavaScriptSyntax.IsMethod(name))
                {
                    VisitMethod(node, context, pending, diagnostics);
                    continue;
                }
                if (name == "ClassField")
                {
                    // Field grammar inherits Await from the surrounding class.
                    // The initializer's separate parse rule still has to preserve
                    // this identifier restriction, including escaped spellings.
                    bool awaitRestricted = context.AwaitRestricted || JavaScriptSyntax.HasAwaitContext(node);
                    for (int index = node.Count - 1; index >= 0; index--)
                        pending.Push(new Visit(node[index], IsElementName(node[index]) ? context : context.Field(awaitRestricted)));
                    continue;
                }
                if (JavaScriptSyntax.IsPlainFunction(name))
                {
                    var inner = context.Function();
                    for (int index = node.Count - 1; index >= 0; index--)
                        // A declaration's name keeps the enclosing Await context.
                        // A function expression, its parameters and its body use ~Await.
                        pending.Push(new Visit(node[index], JavaScriptSyntax.IsFunctionDeclaration(name) &&
                            Name(node[index]) == "BindingIdentifier" ? context : inner));
                    continue;
                }
                if (JavaScriptSyntax.IsArrow(name))
                {
                    for (int index = node.Count - 1; index >= 0; index--)
                        pending.Push(new Visit(node[index], index == 0 ? context : context.Arrow()));
                    continue;
                }
                if (name == "ClassStaticBlock") context = context.StaticBlock();

                // A private declaration is not a use of the private name. Computed
                // public names are still expressions and must keep their outer context.
                if (name == "ClassElementName" && IsPrivateDeclaration(node)) continue;
                if (name == "SuperCall" && !context.SuperCall)
                    Add(diagnostics, "invalid_super_call", "A super call is only valid in a derived constructor or its lexical arrow functions.", node);
                else if (name == "SuperProperty" && !context.SuperProperty)
                    Add(diagnostics, "invalid_super_property", "A super property requires an enclosing class method, field initializer or static block.", node);
                else if (name == "NewTarget")
                {
                    var tokens = Tokens(node);
                    if (!context.NewTarget || tokens.Count != 3 || tokens[2].Token.Input != "target")
                        Add(diagnostics, "invalid_new_target", "The exact new.target spelling requires a function, field initializer or static block context.", node);
                }
                else if (name == "IdentifierReference" && !context.Arguments && Identifier(node) == "arguments")
                    Add(diagnostics, "invalid_class_arguments", "A class field initializer or static block cannot reference arguments, including from its lexical arrow functions.", node);

                if (context.AwaitRestricted && (name == "IdentifierReference" || name == "BindingIdentifier" || name == "LabelIdentifier") &&
                    Identifier(node) == "await")
                    Add(diagnostics, "invalid_static_block_await", "This enclosing class or static block context cannot use await as an identifier outside a nested function body.", node);
                if (name == "UnaryExpression" && node.Count == 2 && FirstToken(node[0])?.Token.Input == "delete" && IsPrivateReference(node[1]))
                    Add(diagnostics, "invalid_private_delete", "A private class member cannot be deleted.", node[1]);

                PushChildren(pending, node, context);
            }
        }

        private static void VisitClass(ParseTreeNonTerminal node, Context outer, Stack<Visit> pending,
            List<JavaScriptDiagnostic> diagnostics)
        {
            var tail = Child(node, "ClassTail");
            var body = tail == null ? null : Child(tail, "ClassBody");
            if (body == null) { PushChildren(pending, node, outer); return; }
            bool derived = Child(tail, "ClassHeritage") != null;
            var declarations = CheckElements(body, diagnostics);
            var inner = outer.Class(declarations, derived);
            for (int index = node.Count - 1; index >= 0; index--)
            {
                if (!ReferenceEquals(node[index], tail)) pending.Push(new Visit(node[index], outer));
                else for (int part = tail.Count - 1; part >= 0; part--)
                    // A class's private names are visible in its body and computed
                    // keys, but the extends expression sees only enclosing classes.
                    pending.Push(new Visit(tail[part], ReferenceEquals(tail[part], body) ? inner : outer));
            }
        }

        private static HashSet<string> CheckElements(ParseTreeNonTerminal body, List<JavaScriptDiagnostic> diagnostics)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var privateElements = new Dictionary<string, Element>(StringComparer.Ordinal);
            bool constructor = false;
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, body);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string kind = Name(node);
                if (kind == "ClassStaticBlock") continue;
                if (!(JavaScriptSyntax.IsMethod(kind) && kind.StartsWith("Class", StringComparison.Ordinal)) && kind != "ClassField")
                {
                    PushChildren(pending, node);
                    continue;
                }
                var element = new Element(node);
                if (!element.ValidModifiers)
                    Add(diagnostics, "invalid_class_modifier", "A class element modifier must be the exact static prefix, get/set accessor prefix, or static followed by get/set.", node);
                if (element.Name == null) continue; // A computed key has no static PropName.
                if (element.Private)
                {
                    names.Add(element.Name);
                    if (element.Name == "constructor")
                        Add(diagnostics, "invalid_private_name", "A private class element cannot be named #constructor.", element.NameNode);
                    if (privateElements.TryGetValue(element.Name, out var previous))
                    {
                        bool accessorPair = previous.Static == element.Static && !previous.Paired &&
                            (previous.Accessor == "get" && element.Accessor == "set" || previous.Accessor == "set" && element.Accessor == "get");
                        if (!accessorPair)
                            Add(diagnostics, "duplicate_private_name", "A private name may be declared once, except for one getter/setter pair with the same staticness.", element.NameNode);
                        previous.Paired = true;
                    }
                    else privateElements.Add(element.Name, element);
                }
                else
                {
                    if (element.Static && element.Name == "prototype")
                        Add(diagnostics, "invalid_static_prototype", "A static class element cannot be named prototype.", element.NameNode);
                    if (element.Name == "constructor")
                    {
                        if (kind == "ClassField")
                            Add(diagnostics, "invalid_class_field_name", "A class field cannot be named constructor.", element.NameNode);
                        else if (!element.Static)
                        {
                            if (element.Accessor != null || JavaScriptSyntax.IsAsync(node) || JavaScriptSyntax.IsGenerator(node))
                                Add(diagnostics, "invalid_constructor", "The constructor must be an ordinary synchronous non-generator method.", element.NameNode);
                            if (constructor)
                                Add(diagnostics, "duplicate_constructor", "A class cannot have more than one constructor.", element.NameNode);
                            constructor = true;
                        }
                    }
                }
            }
            return names;
        }

        private static void VisitMethod(ParseTreeNonTerminal method, Context context, Stack<Visit> pending,
            List<JavaScriptDiagnostic> diagnostics)
        {
            var element = new Element(method);
            if (!element.ValidModifiers && Name(method).StartsWith("Object", StringComparison.Ordinal))
                Add(diagnostics, "invalid_method_modifier", "An object method modifier must use the exact async, get or set spelling.", method);
            var parameters = Child(method, "FormalParameters");
            if (parameters != null && element.Accessor != null)
            {
                int count = 0;
                bool rest = false;
                var parts = new Stack<ParseTreeSymbol>();
                parts.Push(parameters);
                while (parts.Count != 0)
                {
                    if (!(parts.Pop() is ParseTreeNonTerminal part)) continue;
                    if (Name(part) == "FormalParameter") { count++; continue; }
                    if (Name(part) == "FunctionRestParameter") { count++; rest = true; continue; }
                    PushChildren(parts, part);
                }
                if (element.Accessor == "get" && count != 0)
                    Add(diagnostics, "invalid_getter_parameters", "A getter must not declare parameters.", parameters);
                else if (element.Accessor == "set")
                {
                    var tokens = Tokens(parameters);
                    bool trailingComma = tokens.Count != 0 && tokens[tokens.Count - 1].Token.Input == ",";
                    if (count != 1 || rest || trailingComma)
                        Add(diagnostics, "invalid_setter_parameters", "A setter must declare exactly one non-rest parameter without a trailing comma.", tokens.Count == 0 ? element.NameNode : parameters);
                }
            }
            bool constructor = Name(method).StartsWith("Class", StringComparison.Ordinal) && !element.Private &&
                !element.Static && element.Accessor == null && !JavaScriptSyntax.IsAsync(method) &&
                !JavaScriptSyntax.IsGenerator(method) && element.Name == "constructor";
            var inner = context.Method(constructor && context.Derived);
            for (int index = method.Count - 1; index >= 0; index--)
                pending.Push(new Visit(method[index], IsElementName(method[index]) ? context : inner));
        }

        private static bool IsPrivateReference(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (name == "ParenthesizedExpression" && node.Count == 3) { symbol = node[1]; continue; }
                if ((name == "MemberExpression" || name == "CallExpression") && node.Count > 1)
                    return IsPrivateAccess(node[node.Count - 1]);
                if (name == "OptionalExpression" && node.Count > 1) return IsPrivateAccess(node[node.Count - 1]);
                if (name == "SimpleUpdateExpression") { symbol = node[0]; continue; }
                if (node.Count != 1) return false;
                symbol = node[0];
            }
            return false;
        }

        private static bool IsPrivateAccess(ParseTreeSymbol symbol)
        {
            while (symbol is ParseTreeNonTerminal node)
            {
                string name = Name(node);
                if (name == "PrivateMemberAccess") return true;
                if (name == "OptionalChainStart")
                {
                    var tokens = Tokens(node);
                    return tokens.Count == 2 && tokens[1].Token.Kind.Caption == "PrivateIdentifier";
                }
                if (name == "OptionalChain" && node.Count > 1) { symbol = node[node.Count - 1]; continue; }
                if (node.Count != 1) return false;
                symbol = node[0];
            }
            return false;
        }

        private static bool IsPrivateDeclaration(ParseTreeNonTerminal node)
        {
            var tokens = Tokens(node);
            return tokens.Count == 1 && tokens[0].Token.Kind.Caption == "PrivateIdentifier";
        }

        private static bool IsElementName(ParseTreeSymbol symbol)
            => JavaScriptSyntax.IsElementName(symbol);

        private static ParseTreeNonTerminal Child(ParseTreeNonTerminal node, string name)
        {
            for (int index = 0; index < node.Count; index++)
                if (Name(node[index]) == name) return (ParseTreeNonTerminal)node[index];
            return null;
        }

        private static string Name(ParseTreeSymbol node) => JavaScriptSyntax.Name(node);
        private static string Identifier(ParseTreeSymbol node)
            => JavaScriptTokenPatterns.DecodeIdentifier(FirstToken(node)?.Token.Input ?? string.Empty);
        private static string PrivateName(ParseTreeTerminal token)
            => JavaScriptTokenPatterns.DecodeIdentifier(token.Token.Input.Substring(1));

        private static ParseTreeTerminal FirstToken(ParseTreeSymbol node)
        {
            var tokens = Tokens(node);
            return tokens.Count == 0 ? null : tokens[0];
        }

        private static List<ParseTreeTerminal> Tokens(ParseTreeSymbol symbol)
        {
            var result = new List<ParseTreeTerminal>();
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(symbol);
            while (pending.Count != 0)
            {
                var node = pending.Pop();
                if (node is ParseTreeTerminal terminal) result.Add(terminal);
                else if (node is ParseTreeNonTerminal nonterminal) PushChildren(pending, nonterminal);
            }
            return result;
        }

        private static void PushChildren(Stack<ParseTreeSymbol> pending, ParseTreeNonTerminal node)
        {
            for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]);
        }

        private static void PushChildren(Stack<Visit> pending, ParseTreeNonTerminal node, Context context)
        {
            for (int index = node.Count - 1; index >= 0; index--) pending.Push(new Visit(node[index], context));
        }

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            var tokens = Tokens(node);
            int start = tokens.Count == 0 ? 0 : tokens[0].Token.StartIndex;
            int end = tokens.Count == 0 ? start : tokens[tokens.Count - 1].Token.EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, Math.Max(0, end - start)));
        }

        private sealed class Element
        {
            internal Element(ParseTreeNonTerminal node)
            {
                var names = JavaScriptSyntax.ElementNames(node);
                if (names.Count == 0) return;
                NameNode = names[names.Count - 1];
                ValidModifiers = names.Count == 1;
                string first = RawName(names[0]);
                string kind = JavaScriptClassValidation.Name(node);
                bool classElement = kind.StartsWith("Class", StringComparison.Ordinal);
                if (JavaScriptSyntax.IsAsync(node))
                {
                    if (names.Count == 2) ValidModifiers = first == "async";
                    else if (names.Count == 3 && classElement)
                    {
                        Static = first == "static";
                        ValidModifiers = Static && RawName(names[1]) == "async";
                    }
                    else ValidModifiers = false;
                }
                else if (JavaScriptSyntax.IsGenerator(node))
                {
                    if (names.Count == 2 && classElement) { Static = first == "static"; ValidModifiers = Static; }
                    else ValidModifiers = names.Count == 1;
                }
                else if (names.Count == 2)
                {
                    Static = classElement && first == "static";
                    if (kind == "ClassField") ValidModifiers = Static;
                    else
                    {
                        if (first == "get" || first == "set") Accessor = first;
                        ValidModifiers = Static || Accessor != null;
                    }
                }
                else if (names.Count == 3 && kind == "ClassMethod")
                {
                    string second = RawName(names[1]);
                    Static = first == "static";
                    if (second == "get" || second == "set") Accessor = second;
                    ValidModifiers = Static && Accessor != null;
                }
                var tokens = Tokens(NameNode);
                if (tokens.Count != 1) return;
                var token = tokens[0];
                Private = token.Token.Kind.Caption == "PrivateIdentifier";
                Name = Private ? PrivateName(token) : token.Token.Kind.Caption == "StringLiteral"
                    ? JavaScriptExpressionValidation.DecodeString(token.Token.Input)
                    : token.Token.Kind.Caption == "NumericLiteral" ? token.Token.Input
                    : JavaScriptTokenPatterns.DecodeIdentifier(token.Token.Input);
            }
            private static string RawName(ParseTreeNonTerminal name)
            {
                var tokens = Tokens(name);
                return tokens.Count == 1 ? tokens[0].Token.Input : null;
            }
            internal ParseTreeNonTerminal NameNode { get; }
            internal string Name { get; }
            internal string Accessor { get; }
            internal bool Static { get; }
            internal bool Private { get; }
            internal bool ValidModifiers { get; }
            internal bool Paired { get; set; }
        }

        private sealed class Context
        {
            private readonly Context privateParent;
            private readonly HashSet<string> privateNames;
            internal Context(bool superProperty = false, bool superCall = false, bool newTarget = false,
                bool arguments = true, bool awaitRestricted = false, bool derived = false,
                Context privateParent = null, HashSet<string> privateNames = null)
            {
                SuperProperty = superProperty; SuperCall = superCall; NewTarget = newTarget;
                Arguments = arguments; AwaitRestricted = awaitRestricted; Derived = derived;
                this.privateParent = privateParent; this.privateNames = privateNames;
            }
            internal bool SuperProperty { get; }
            internal bool SuperCall { get; }
            internal bool NewTarget { get; }
            internal bool Arguments { get; }
            internal bool AwaitRestricted { get; }
            internal bool Derived { get; }
            internal bool HasPrivate(string name)
            {
                for (var current = this; current != null; current = current.privateParent)
                    if (current.privateNames != null && current.privateNames.Contains(name)) return true;
                return false;
            }
            internal Context Class(HashSet<string> names, bool derived)
                => new Context(SuperProperty, SuperCall, NewTarget, Arguments, AwaitRestricted, derived, this, names);
            internal Context Function() => new Context(newTarget: true, privateParent: this);
            internal Context Arrow() => new Context(SuperProperty, SuperCall, NewTarget, Arguments, false, Derived, this);
            internal Context Method(bool superCall) => new Context(true, superCall, true, privateParent: this);
            internal Context Field(bool awaitRestricted) => new Context(true, false, true, false, awaitRestricted, privateParent: this);
            internal Context StaticBlock() => new Context(true, false, true, false, true, privateParent: this);
        }

        private sealed class Visit
        {
            internal Visit(ParseTreeSymbol symbol, Context context) { Symbol = symbol; Context = context; }
            internal ParseTreeSymbol Symbol { get; }
            internal Context Context { get; }
        }
    }
}
