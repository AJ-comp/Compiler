using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Retains a private method signature's lexical binding in the JavaScript
    /// validation view. Overload signatures with a real implementation disappear;
    /// otherwise one validation-only field supplies the original private name.
    /// The original tree and its token positions are never changed.
    /// </summary>
    internal static class TypeScriptClassSignatureProjection
    {
        internal static ParseTreeSymbol CreateBinding(ParseTreeNonTerminal node)
        {
            if (!IsMethodSignature(node)) return null;
            var name = PrivateName(node);
            if (name == null) return null;
            var body = EnclosingBody(node);
            if (body == null) return null;

            string identifier = Identifier(name);
            bool isStatic = TypeScriptClassValidation.HasModifier(node, "static");
            ParseTreeNonTerminal first = null;
            foreach (var member in Members(body))
            {
                if (!IsMethod(member) || Accessor(member)) continue;
                var otherName = PrivateName(member);
                if (otherName == null || Identifier(otherName) != identifier ||
                    TypeScriptClassValidation.HasModifier(member, "static") != isStatic) continue;
                if (!IsMethodSignature(member)) return null;
                if (first == null) first = member;
            }
            if (!ReferenceEquals(first, node)) return null;

            // Only the binding is needed. A field with no initializer introduces
            // the same private name without inventing a method implementation or
            // changing parameter/function-context checks already done in TS.
            var field = Named("ClassField");
            var staticToken = isStatic ? StaticToken(node) : null;
            if (staticToken != null) field.Add(ElementName(staticToken));
            field.Add(ElementName(name));
            return field;
        }

        private static bool IsMethod(ParseTreeNonTerminal node)
            => Name(node).StartsWith("Class", StringComparison.Ordinal) && JavaScriptSyntax.IsMethod(Name(node));

        private static bool IsMethodSignature(ParseTreeNonTerminal node)
        {
            if (!IsMethod(node) || Accessor(node)) return false;
            foreach (var child in node.Items)
                if (Name(child) == "TSMethodOverload") return true;
            return false;
        }

        private static bool Accessor(ParseTreeNonTerminal node)
        {
            var names = JavaScriptSyntax.ElementNames(node);
            for (int i = 0; i + 1 < names.Count; i++)
            {
                var tokens = names[i].AllTokens;
                if (tokens.Count == 1 && (tokens[0].Input == "get" || tokens[0].Input == "set")) return true;
            }
            return false;
        }

        private static TokenData PrivateName(ParseTreeNonTerminal node)
        {
            var names = JavaScriptSyntax.ElementNames(node);
            if (names.Count == 0) return null;
            var tokens = names[names.Count - 1].AllTokens;
            return tokens.Count == 1 && tokens[0].Kind.Caption == "PrivateIdentifier" ? tokens[0] : null;
        }

        private static string Identifier(TokenData token)
            => JavaScriptTokenPatterns.DecodeIdentifier(token.Input.Substring(1));

        private static ParseTreeNonTerminal EnclosingBody(ParseTreeSymbol node)
        {
            for (var parent = node.Parent; parent != null; parent = parent.Parent)
            {
                string name = Name(parent);
                if (name == "ClassBody") return parent as ParseTreeNonTerminal;
                if (name == "ClassDeclaration" || name == "ClassExpression") return null;
            }
            return null;
        }

        private static IEnumerable<ParseTreeNonTerminal> Members(ParseTreeNonTerminal body)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(body);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (IsMethod(node) || name == "ClassField" || name == "ClassStaticBlock" || name == "TSClassIndexSignature")
                {
                    yield return node;
                    continue;
                }
                for (int i = node.Count - 1; i >= 0; i--) pending.Push(node[i]);
            }
        }

        private static TokenData StaticToken(ParseTreeNonTerminal node)
        {
            for (var parent = node.Parent; parent is ParseTreeNonTerminal wrapper; parent = parent.Parent)
            {
                string name = Name(wrapper);
                if (name == "TSClassModifiedElement")
                {
                    foreach (var child in wrapper.Items)
                        if (Name(child) == "TSModifiers")
                            foreach (var token in child.AllTokens)
                                if (token.Input == "static") return token;
                    break;
                }
                if (name != "TSClassModifiedMember" && name != "TSClassMember" &&
                    !name.StartsWith("__", StringComparison.Ordinal)) break;
            }
            var names = JavaScriptSyntax.ElementNames(node);
            for (int i = 0; i + 1 < names.Count; i++)
            {
                var tokens = names[i].AllTokens;
                if (tokens.Count == 1 && tokens[0].Input == "static") return tokens[0];
            }
            return null;
        }

        private static ParseTreeNonTerminal ElementName(TokenData token)
        {
            var name = Named("ClassElementName");
            name.Add(new ParseTreeTerminal(token));
            return name;
        }

        private static ParseTreeNonTerminal Named(string name)
        {
            var rule = new NonTerminal(name);
            rule.Add(new Epsilon());
            return new ParseTreeNonTerminal(rule[0]);
        }

        private static string Name(ParseTreeSymbol node) => TypeScriptProjection.Name(node);
    }
}
