using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Removes TypeScript class-only wrappers from the temporary JavaScript
    /// validation tree. The original concrete tree and source tokens stay intact.
    /// Runtime staticness remains visible to the existing JavaScript class checks.
    /// </summary>
    internal static class TypeScriptClassProjection
    {
        internal static bool TryCreate(ParseTreeNonTerminal node,
            IDictionary<ParseTreeSymbol, ParseTreeSymbol> projected, out ParseTreeSymbol result)
        {
            result = null;
            string name = TypeScriptProjection.Name(node);
            if (name == "FormalParameters" && IsSetterParameters(node) && HasTrailingComma(node))
            {
                // TypeScript accepts a trailing comma in a setter signature.
                // Keep it in the original tree, but omit it from the JavaScript
                // validation view, whose ECMAScript setter rules forbid it.
                var parameters = new ParseTreeNonTerminal(node.SignPost);
                for (int i = 0; i < node.Count; i++)
                {
                    var child = projected[node[i]];
                    if (child != null) parameters.Add(child);
                }
                var tokens = node.AllTokens;
                result = WithoutTrailingComma(parameters, tokens[tokens.Count - 1]);
                return true;
            }
            if (name == "TSAbstractClassDeclaration" || name == "TSAbstractClassExpression")
            {
                result = ProjectedClass(node, projected);
                return true;
            }
            if (name == "TSClassModifiedMember" || name == "TSClassMember")
            {
                foreach (var child in node.Items)
                    if (IsMember(TypeScriptProjection.Name(child)) || TypeScriptProjection.Name(child) == "TSClassMember")
                    {
                        result = projected[child];
                        return true;
                    }
                throw new InvalidOperationException("A modified member wrapper must contain a class member.");
            }
            if (name != "TSClassModifiedElement") return false;

            ParseTreeNonTerminal member = null;
            foreach (var child in node.Items)
                if (child is ParseTreeNonTerminal part && TypeScriptProjection.Name(part) == "TSClassModifiedMember")
                    member = part;
            if (member == null)
                throw new InvalidOperationException("A modified class element must contain a class member.");
            result = projected[member];
            if (result == null || !(result is ParseTreeNonTerminal body)) return true;

            // Accessibility, readonly and other TypeScript modifiers are erased.
            // A static modifier is runtime information: present it in the exact
            // ClassElementName shape already understood by JavaScript validation.
            var staticToken = StaticModifier(node);
            if (staticToken == null) return true;
            var existingNames = JavaScriptSyntax.ElementNames(body);
            for (int i = 0; i + 1 < existingNames.Count; i++)
                if (existingNames[i].AllTokens.Count == 1 && existingNames[i].AllTokens[0].Input == "static") return true;
            var copy = new ParseTreeNonTerminal(body.SignPost);
            var staticName = Named("ClassElementName");
            staticName.Add(new ParseTreeTerminal(staticToken.Token));
            copy.Add(staticName);
            foreach (var child in body.Items) copy.Add(child);
            result = copy;
            return true;
        }

        private static ParseTreeSymbol ProjectedClass(ParseTreeNonTerminal node,
            IDictionary<ParseTreeSymbol, ParseTreeSymbol> projected)
        {
            foreach (var child in node.Items)
            {
                string name = TypeScriptProjection.Name(child);
                if (name == "ClassDeclaration" || name == "ClassExpression") return projected[child];
            }
            throw new InvalidOperationException("An abstract class wrapper must contain a class declaration.");
        }

        private static ParseTreeTerminal StaticModifier(ParseTreeNonTerminal node)
        {
            var pending = new Stack<ParseTreeSymbol>();
            foreach (var child in node.Items)
                if (TypeScriptProjection.Name(child) == "TSModifiers") pending.Push(child);
            while (pending.Count != 0)
            {
                var symbol = pending.Pop();
                if (symbol is ParseTreeTerminal terminal && terminal.Token.Input == "static") return terminal;
                if (symbol is ParseTreeNonTerminal part)
                    foreach (var child in part.Items) pending.Push(child);
            }
            return null;
        }

        private static bool IsMember(string name)
            => name == "ClassField" || name == "ClassMethod" || name == "ClassAsyncMethod" ||
               name == "ClassGeneratorMethod" || name == "ClassAsyncGeneratorMethod" ||
               name == "ClassStaticBlock" || name == "TSClassIndexSignature";

        private static bool IsSetterParameters(ParseTreeNonTerminal parameters)
        {
            if (!(parameters.Parent is ParseTreeNonTerminal method) || TypeScriptProjection.Name(method) != "ClassMethod") return false;
            var names = JavaScriptSyntax.ElementNames(method);
            for (int i = 0; i + 1 < names.Count; i++)
                if (names[i].AllTokens.Count == 1 && names[i].AllTokens[0].Input == "set") return true;
            return false;
        }

        private static bool HasTrailingComma(ParseTreeNonTerminal parameters)
        {
            var tokens = parameters.AllTokens;
            return tokens.Count != 0 && tokens[tokens.Count - 1].Input == ",";
        }

        private static ParseTreeSymbol WithoutTrailingComma(ParseTreeSymbol symbol, Janglim.FrontEnd.TokenData comma)
        {
            if (symbol is ParseTreeTerminal terminal) return ReferenceEquals(terminal.Token, comma) ? null : symbol;
            var node = (ParseTreeNonTerminal)symbol;
            var tokens = node.AllTokens;
            if (tokens.Count == 0 || !ReferenceEquals(tokens[tokens.Count - 1], comma)) return node;
            var copy = new ParseTreeNonTerminal(node.SignPost);
            foreach (var child in node.Items)
            {
                var value = WithoutTrailingComma(child, comma);
                if (value != null) copy.Add(value);
            }
            return copy;
        }

        private static ParseTreeNonTerminal Named(string name)
        {
            var rule = new NonTerminal(name);
            rule.Add(new Epsilon());
            return new ParseTreeNonTerminal(rule[0]);
        }
    }
}
