using Janglim.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Checks module-local imports/exports, decoded export and attribute names, and
    /// the source-goal/context restrictions of import.meta and top-level await.
    /// The checker reads the completed concrete tree; it neither loads dependencies
    /// nor changes the parser, lexer, tree, or JavaScript runtime state.
    /// <para><see href="../../../../docs/ko/javascript-modules.html#early-errors">모듈의 이름과 문맥을 검사하는 이유</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html#context">모듈과 async 함수의 await 문맥</see></para>
    /// </summary>
    internal static class JavaScriptModuleValidation
    {
        private static readonly HashSet<string> ReservedBindings = new HashSet<string>(
            ("await break case catch class const continue debugger default delete do else enum export extends false " +
             "finally for function if import in instanceof new null return super switch this throw true try typeof " +
             "var void while with yield let static implements interface package private protected public eval arguments").Split(' '),
            StringComparer.Ordinal);

        internal static void Validate(ParseTreeSymbol tree, List<JavaScriptDiagnostic> diagnostics)
        {
            if (ReferenceEquals(tree, null)) throw new ArgumentNullException(nameof(tree));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            bool module = Name(tree) == "Module";
            if (module) CheckExports((ParseTreeNonTerminal)tree, diagnostics);

            var pending = new Stack<Visit>();
            pending.Push(new Visit(tree, module));
            while (pending.Count != 0)
            {
                var visit = pending.Pop();
                if (!(visit.Symbol is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                bool awaitAllowed = visit.AwaitAllowed;

                if (module && (name == "IdentifierReference" || name == "BindingIdentifier" || name == "LabelIdentifier") &&
                    Identifier(node) == "await")
                    Add(diagnostics, "reserved_module_await", "A module cannot use await as an identifier, including inside nested ordinary functions.", node);
                else if (name == "ImportMeta")
                {
                    var tokens = Tokens(node);
                    if (!module || tokens.Count != 3 || tokens[2].Token.Input != "meta")
                        Add(diagnostics, "invalid_import_meta", "The exact import.meta spelling requires the Module source goal.", node);
                }
                else if ((name == "AwaitExpression" || name == "ForAwaitOfStatement") && !awaitAllowed)
                    Add(diagnostics, "invalid_await", "An await expression or for-await-of loop requires an async function or the module's top-level evaluation context.", node);
                else if (name == "ModuleExportName") CheckExportName(node, diagnostics);
                else if (name == "ImportSpecifier" && node.Count == 1) CheckUnaliasedImport(node, diagnostics);
                else if (name == "ImportAttributes") CheckAttributes(node, diagnostics);

                // Function parameters and bodies have their own Await grammar context.
                // Ordinary arrows also forbid AwaitExpression in their cover parameters.
                if (JavaScriptSyntax.IsPlainFunction(name) || JavaScriptSyntax.IsArrow(name))
                    awaitAllowed = JavaScriptSyntax.IsAsync(node);
                else if (name == "ClassStaticBlock") awaitAllowed = false;
                else if (JavaScriptSyntax.IsMethod(name) || name == "ClassField")
                {
                    for (int index = node.Count - 1; index >= 0; index--)
                        // A computed key is evaluated in the enclosing context. The
                        // method parameters/body and field initializer are separate.
                        pending.Push(new Visit(node[index], IsElementName(node[index])
                            ? awaitAllowed : JavaScriptSyntax.IsMethod(name) && JavaScriptSyntax.IsAsync(node)));
                    continue;
                }
                for (int index = node.Count - 1; index >= 0; index--)
                    pending.Push(new Visit(node[index], awaitAllowed));
            }
        }

        private static void CheckExports(ParseTreeNonTerminal module, List<JavaScriptDiagnostic> diagnostics)
        {
            var declarations = JavaScriptStatementValidation.ModuleDeclaredNames(module);
            var exported = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(module);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ExportDeclaration")
                {
                    CheckExportDeclaration(node, declarations, exported, diagnostics);
                    continue;
                }
                // Static declarations only belong to ModuleItemList. These boundaries
                // also keep locally constructed/subtree analyses from exporting nested names.
                if (name == "StatementListItem" || name == "ImportDeclaration") continue;
                PushChildren(pending, node);
            }
        }

        private static void CheckExportDeclaration(ParseTreeNonTerminal declaration, HashSet<string> declared,
            HashSet<string> exported, List<JavaScriptDiagnostic> diagnostics)
        {
            var tokens = Tokens(declaration);
            if (tokens.Count > 1 && tokens[1].Token.Input == "default")
            {
                Export("default", tokens[1], exported, diagnostics);
                return;
            }
            bool reexport = Child(declaration, "FromClause") != null;
            var pending = new Stack<ParseTreeSymbol>();
            PushChildren(pending, declaration);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                string name = Name(node);
                if (name == "ExportSpecifier")
                {
                    var local = Child(node, "ModuleExportName");
                    var output = LastChild(node, "ModuleExportName");
                    if (output != null) Export(ExportName(output), output, exported, diagnostics);
                    if (!reexport && local != null)
                    {
                        var first = FirstToken(local);
                        if (first?.Token.Kind.Caption == "StringLiteral" || ReservedBindings.Contains(ExportName(local)))
                            Add(diagnostics, "invalid_local_export", "A local export must refer to a valid module binding identifier.", local);
                        else if (!declared.Contains(ExportName(local)))
                            Add(diagnostics, "undefined_export", "The local name is not declared in this module's lexical or var scope.", local);
                    }
                    continue;
                }
                if (name == "VariableStatement" || name == "LexicalDeclaration" ||
                    JavaScriptSyntax.IsFunctionDeclaration(name) || name == "ClassDeclaration")
                {
                    foreach (var identifier in JavaScriptStatementValidation.ExportedDeclarationBindings(node))
                        Export(Identifier(identifier), identifier, exported, diagnostics);
                    continue;
                }
                // The namespace form exports its final ModuleExportName. A bare
                // `export *` contributes no name and may appear more than once.
                if (name == "ModuleExportName")
                {
                    Export(ExportName(node), node, exported, diagnostics);
                    continue;
                }
                if (name == "FromClause" || name == "ImportAttributes") continue;
                PushChildren(pending, node);
            }
        }

        private static void Export(string name, ParseTreeSymbol node, HashSet<string> exported,
            List<JavaScriptDiagnostic> diagnostics)
        {
            if (!exported.Add(name))
                Add(diagnostics, "duplicate_export", "A module cannot export the same decoded name more than once.", node);
        }

        private static void CheckUnaliasedImport(ParseTreeNonTerminal specifier, List<JavaScriptDiagnostic> diagnostics)
        {
            var token = FirstToken(specifier);
            if (token?.Token.Kind.Caption == "StringLiteral" || ReservedBindings.Contains(ExportName(specifier)))
                Add(diagnostics, "invalid_import_binding", "An unaliased named import must also be a valid local binding identifier; other names require an as binding.", specifier);
        }

        private static void CheckAttributes(ParseTreeNonTerminal attributes, List<JavaScriptDiagnostic> diagnostics)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(attributes);
            while (pending.Count != 0)
            {
                if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
                if (Name(node) == "AttributeKey")
                {
                    if (!names.Add(ExportName(node)))
                        Add(diagnostics, "duplicate_import_attribute", "An import attribute key cannot appear twice after string and identifier escapes are decoded.", node);
                    continue;
                }
                PushChildren(pending, node);
            }
        }

        private static void CheckExportName(ParseTreeNonTerminal name, List<JavaScriptDiagnostic> diagnostics)
        {
            if (FirstToken(name)?.Token.Kind.Caption != "StringLiteral") return;
            string value = ExportName(name);
            for (int index = 0; index < value.Length; index++)
            {
                if (char.IsHighSurrogate(value[index]) && index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
                {
                    index++;
                    continue;
                }
                if (char.IsSurrogate(value[index]))
                {
                    Add(diagnostics, "invalid_module_export_name", "A string import/export name must contain well-formed Unicode scalar values.", name);
                    return;
                }
            }
        }

        private static bool IsElementName(ParseTreeSymbol node)
            => JavaScriptSyntax.IsElementName(node);

        private static string ExportName(ParseTreeSymbol name)
        {
            var token = FirstToken(name);
            if (token == null) return string.Empty;
            return token.Token.Kind.Caption == "StringLiteral"
                ? JavaScriptExpressionValidation.DecodeString(token.Token.Input)
                : JavaScriptTokenPatterns.DecodeIdentifier(token.Token.Input);
        }

        private static string Identifier(ParseTreeSymbol node)
            => JavaScriptTokenPatterns.DecodeIdentifier(FirstToken(node)?.Token.Input ?? string.Empty);
        private static string Name(ParseTreeSymbol node) => JavaScriptSyntax.Name(node);

        private static ParseTreeNonTerminal Child(ParseTreeNonTerminal node, string name)
        {
            for (int index = 0; index < node.Count; index++)
                if (Name(node[index]) == name) return (ParseTreeNonTerminal)node[index];
            return null;
        }

        private static ParseTreeNonTerminal LastChild(ParseTreeNonTerminal node, string name)
        {
            for (int index = node.Count - 1; index >= 0; index--)
                if (Name(node[index]) == name) return (ParseTreeNonTerminal)node[index];
            return null;
        }

        private static ParseTreeTerminal FirstToken(ParseTreeSymbol symbol)
        {
            var pending = new Stack<ParseTreeSymbol>();
            pending.Push(symbol);
            while (pending.Count != 0)
            {
                var node = pending.Pop();
                if (node is ParseTreeTerminal terminal) return terminal;
                if (node is ParseTreeNonTerminal nonterminal) PushChildren(pending, nonterminal);
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

        private static void Add(List<JavaScriptDiagnostic> diagnostics, string code, string message, ParseTreeSymbol node)
        {
            var tokens = Tokens(node);
            int start = tokens.Count == 0 ? 0 : tokens[0].Token.StartIndex;
            int end = tokens.Count == 0 ? start : tokens[tokens.Count - 1].Token.EndIndex + 1;
            diagnostics.Add(new JavaScriptDiagnostic(code, message, start, Math.Max(0, end - start)));
        }

        private sealed class Visit
        {
            internal Visit(ParseTreeSymbol symbol, bool awaitAllowed) { Symbol = symbol; AwaitAllowed = awaitAllowed; }
            internal ParseTreeSymbol Symbol { get; }
            internal bool AwaitAllowed { get; }
        }
    }
}
