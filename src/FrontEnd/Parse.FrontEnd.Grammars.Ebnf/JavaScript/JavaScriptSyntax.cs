using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Identifies JavaScript concrete-tree roles independently of the generated
    /// Await grammar variant. All helpers inspect the tree without changing it.
    /// <para><see href="../../../../docs/ko/javascript-functions.html#implementation">함수 문맥과 구문 트리의 역할</see></para>
    /// </summary>
    internal static class JavaScriptSyntax
    {
        internal static string Name(ParseTreeSymbol symbol)
        {
            string name = (symbol as ParseTreeNonTerminal)?.SignPost?.Name ?? string.Empty;
            while (name.EndsWith("__Await", StringComparison.Ordinal) || name.EndsWith("__Yield", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - 7);
            return name;
        }

        internal static string BaseName(ParseTreeSymbol symbol)
        {
            string name = Name(symbol);
            return name.EndsWith("NoIn", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - 4) : name;
        }

        internal static bool HasAwaitContext(ParseTreeSymbol symbol)
            => ((symbol as ParseTreeNonTerminal)?.SignPost?.Name ?? string.Empty)
                .Contains("__Await");

        internal static bool HasYieldContext(ParseTreeSymbol symbol)
            => ((symbol as ParseTreeNonTerminal)?.SignPost?.Name ?? string.Empty).Contains("__Yield");

        internal static bool IsFunctionDeclaration(string name)
            => name == "FunctionDeclaration" || name == "GeneratorDeclaration" ||
               name == "AsyncFunctionDeclaration" || name == "AsyncGeneratorDeclaration";

        internal static bool IsPlainFunction(string name)
            => IsFunctionDeclaration(name) || name == "FunctionExpression" ||
               name == "GeneratorExpression" || name == "AsyncFunctionExpression" ||
               name == "AsyncGeneratorExpression";

        internal static bool IsArrow(string name)
            => name == "ArrowFunction" || name == "ArrowFunctionNoIn" ||
               name == "AsyncArrowFunction" || name == "AsyncArrowFunctionNoIn";

        internal static bool IsMethod(string name)
            => name == "ClassMethod" || name == "ClassGeneratorMethod" || name == "ClassAsyncMethod" ||
               name == "ObjectMethod" || name == "ObjectGeneratorMethod" || name == "ObjectAsyncMethod" ||
               name == "ObjectAsyncGeneratorMethod" || name == "ClassAsyncGeneratorMethod";

        internal static bool IsFunction(string name)
            => IsPlainFunction(name) || IsArrow(name) || IsMethod(name);

        internal static bool IsAsync(ParseTreeNonTerminal node)
        {
            string name = Name(node);
            return name.StartsWith("Async", StringComparison.Ordinal) ||
                name == "ClassAsyncMethod" || name == "ObjectAsyncMethod" ||
                name == "ClassAsyncGeneratorMethod" || name == "ObjectAsyncGeneratorMethod";
        }

        internal static bool IsGenerator(ParseTreeNonTerminal node)
        {
            string name = Name(node);
            if (name == "GeneratorDeclaration" || name == "GeneratorExpression" ||
                name == "AsyncGeneratorDeclaration" || name == "AsyncGeneratorExpression" ||
                name == "ClassGeneratorMethod" || name == "ObjectGeneratorMethod" ||
                name == "ClassAsyncGeneratorMethod" || name == "ObjectAsyncGeneratorMethod") return true;
            if (name != "ClassAsyncMethod" && name != "ObjectAsyncMethod") return false;
            foreach (var child in node.Items)
                if (child is ParseTreeTerminal terminal && terminal.Token.Input == "*") return true;
            return false;
        }

        internal static bool IsElementName(ParseTreeSymbol symbol)
        {
            string name = Name(symbol);
            return name == "ClassElementName" || name == "ClassModifierName" ||
                name == "PropertyName" || name == "ObjectAccessorName";
        }

        internal static List<ParseTreeNonTerminal> ElementNames(ParseTreeNonTerminal method)
        {
            var result = new List<ParseTreeNonTerminal>();
            foreach (var child in method.Items)
            {
                if (!(child is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ClassElementName" || name == "PropertyName") result.Add(node);
                else if (name == "ClassModifierName" || name == "ObjectAccessorName")
                    foreach (var part in node.Items)
                        if (Name(part) == "ClassElementName" || Name(part) == "PropertyName")
                            result.Add((ParseTreeNonTerminal)part);
            }
            return result;
        }
    }
}
