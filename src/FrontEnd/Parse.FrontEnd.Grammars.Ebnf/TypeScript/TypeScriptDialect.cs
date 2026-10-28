using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    internal sealed class TypeScriptDialect : JavaScriptDialect
    {
        private readonly TypeScriptInputKind kind;
        private readonly ConditionalWeakTable<TokenData, SourceText> sources = new ConditionalWeakTable<TokenData, SourceText>();
        private readonly HashSet<LexicalGoal> transparent = new HashSet<LexicalGoal>();
        private TypeScriptLexicalPolicy lexicalPolicy;
        internal TypeScriptDialect(TypeScriptInputKind kind) { this.kind = kind; }
        internal override string EntryRule => kind == TypeScriptInputKind.Type ? "TSType" : null;
        internal override bool IsRuntimeTerminal(Terminal terminal)
            => terminal.Caption != "TSTypeIdentifierName" && terminal.Caption != "TSTypeOpen" &&
                terminal.Caption != "TSTypeClose" && terminal.Caption != "TSNonNullToken" &&
                terminal.Caption != "TSClassModifierToken" && terminal.Caption != "TSClassHeritageOpen";
        internal override bool IsTransparent(LexicalGoal goal) => transparent.Contains(goal);
        internal override void ObserveToken(TokenData token, string input)
        {
            sources.GetValue(token, _ => new SourceText(input));
            if (token.Kind.Caption == "TSTypeIdentifierName")
                JavaScriptTokenPatterns.DecodeIdentifier(token.Input, token.StartIndex);
        }

        internal override string PrepareGrammar(string text)
        {
            // TypeScript contextual words remain legal identifiers in JavaScript
            // expression positions. Literal priority must not reserve them globally.
            const string names = "'any' | 'unknown' | 'string' | 'number' | 'bigint' | 'symbol' | 'boolean' | 'undefined' | 'never' | 'object' | 'intrinsic' | 'keyof' | 'unique' | 'readonly' | 'infer' | 'abstract' | 'accessor' | 'as' | 'assert' | 'asserts' | 'constructor' | 'declare' | 'from' | 'get' | 'global' | 'is' | 'module' | 'namespace' | 'out' | 'override' | 'require' | 'satisfies' | 'set' | 'type' | 'using'";
            foreach (string rule in new[] { "IdentifierReference", "BindingIdentifier", "LabelIdentifier", "PropertyIdentifierName" })
                text = Edit(text, rule, body => body + " | " + names);
            text = TypeScriptExpressions.Prepare(text);
            text = TypeScriptNonNullExpressions.Prepare(text);
            if (kind != TypeScriptInputKind.Type)
            {
                text = AddRuntimeSyntax(text);
                text = TypeScriptClasses.Prepare(text);
            }
            return text + "\n" + TypeScriptTypes.Load() + (kind == TypeScriptInputKind.Type ? string.Empty : "\n" + TypeScriptDeclarations.Load());
        }

        private static string AddRuntimeSyntax(string text)
        {
            foreach (string rule in new[] { "UninitializedVariableDeclaration", "UninitializedLexicalBinding",
                "InitializedVariableDeclaration", "InitializedLexicalBinding", "ForVariableDeclaration", "ForLexicalBinding", "ForBinding", "CatchParameter" })
                text = Edit(text, rule, body => body.Replace("BindingIdentifier", "BindingIdentifier TSAnnotation?")
                    .Replace("BindingPattern", "BindingPattern TSAnnotation?"));
            text = Edit(text, "FormalParameter", _ => "BindingIdentifier TSOptional? TSAnnotation? Initializer? | BindingPattern TSOptional? TSAnnotation? Initializer? | TSThisParameter");
            text = Edit(text, "FunctionRestParameter", _ => "BindingRestElement TSAnnotation?");
            // Keep JavaScript's function and method node names. Its context expansion
            // and static checks can then be reused after dropping type-only children.
            foreach (string rule in new[] { "FunctionDeclaration", "FunctionExpression", "AsyncFunctionDeclaration", "AsyncFunctionExpression",
                "GeneratorDeclaration", "GeneratorExpression", "AsyncGeneratorDeclaration", "AsyncGeneratorExpression",
                "ClassMethod", "ClassAsyncMethod", "ClassGeneratorMethod", "ObjectMethod", "ObjectAsyncMethod", "ObjectGeneratorMethod" })
                text = Edit(text, rule, body => body.Replace("'(' FormalParameters ')' FunctionBody",
                    "TSOptionalTypeParameters '(' FormalParameters ')' TSOptionalReturnType FunctionBody"));
            // Both forms retain the same direct FunctionDeclaration header. The
            // JavaScript context expansion can therefore keep ordinary parameters
            // in ~Await before choosing a body or a TypeScript signature ending.
            text = Edit(text, "FunctionDeclaration", body => body + " | " + body.Replace("FunctionBody", "TSFunctionOverload"));
            text = Edit(text, "Declaration", body => body + " | TSDeclaration");
            text = Edit(text, "ModuleFrom", body => body.Replace("IdentifierName", "(IdentifierName | 'from')"));
            text = Edit(text, "ModuleAs", body => body.Replace("IdentifierName", "(IdentifierName | 'as')"));
            text = Edit(text, "ExportDeclaration", body => body + " | 'export' 'default' TSInterfaceDeclaration | TSNamespaceExportDeclaration");
            text = Edit(text, "ClassDeclaration", body => body.Replace("BindingIdentifier", "BindingIdentifier TSTypeParameters?"));
            text = Edit(text, "ClassExpression", body => body.Replace("BindingIdentifier", "BindingIdentifier TSTypeParameters?"));
            text = Edit(text, "ClassField", body => body.Replace("ClassFieldEnd", "TSAnnotation? ClassFieldEnd")
                .Replace(" Initializer", " TSAnnotation? Initializer"));
            return text + @"
TSAnnotation : ':' TSType ;
TSOptional : '?' ;
TSOptionalTypeParameters : TSTypeParameters? ;
TSOptionalReturnType : TSReturnTypeAnnotation? ;
TSThisParameter : 'this' TSAnnotation ;
TSDeclaration : {TSDeclarationStart()}? TSActualDeclaration ;
TSActualDeclaration : TSInterfaceDeclaration | TSTypeAliasDeclaration | TSEnumDeclaration | TSNamespaceDeclaration | TSAmbientDeclaration ;
# This marker distinguishes a declaration signature from an implementation after
# FunctionDeclaration has parsed their shared name, parameters and return type.
TSFunctionOverload : TSFunctionSignatureEnd ;
";
        }

        internal override bool AllowExpressionStatement(PredicateContext context) => !DeclarationStart(context);
        internal override bool AllowStatementListStatement(PredicateContext context) => !DeclarationStart(context);

        private bool DeclarationStart(PredicateContext context)
        {
            var tokens = Tokens(context);
            if (!DeclarationStart(tokens)) return false;
            if (tokens[0] == "enum" || tokens[0] == "const") return true;
            if (sources.TryGetValue(context.NextToken, out var source))
            {
                string after = source.Text.Substring(Math.Min(source.Text.Length, context.NextToken.EndIndex + 1));
                var trivia = Regex.Match(after, @"\A(?:\s|/\*.*?\*/|//[^\r\n\u2028\u2029]*)*", RegexOptions.Singleline).Value;
                if (trivia.IndexOfAny(new[] { '\r', '\n', '\u2028', '\u2029' }) >= 0) return false;
            }
            return true;
        }

        private static bool DeclarationStart(IReadOnlyList<string> tokens)
        {
            string first = TypeScriptLookahead.At(tokens, 0), next = TypeScriptLookahead.At(tokens, 1);
            if (first == "enum" || first == "interface") return true;
            if (first == "abstract" && next == "class") return true;
            if (first == "const" && next == "enum") return true;
            if (first == "type") return TypeScriptLookahead.IsName(next) &&
                (TypeScriptLookahead.At(tokens, 2) == "=" || TypeScriptLookahead.At(tokens, 2) == "<");
            if (first == "namespace" || first == "module") return TypeScriptLookahead.IsName(next) || next.StartsWith("\"", StringComparison.Ordinal) || next.StartsWith("'", StringComparison.Ordinal);
            if (first == "declare") return next == "function" || next == "class" || next == "abstract" || next == "var" || next == "let" || next == "const" || next == "namespace" || next == "module" || next == "global" || next == "enum" || next == "interface" || next == "type";
            return false;
        }

        internal override void ConfigurePredicates(EbnfReaderOptions options)
        {
            void Pair(string name, Func<IReadOnlyList<string>, bool> predicate)
            {
                options.RegisterPredicate("TS" + name, context => predicate(Tokens(context)));
                options.RegisterPredicate("TSNot" + name, context => !predicate(Tokens(context)));
            }
            Pair("FunctionTypeStart", TypeScriptLookahead.FunctionType);
            Pair("MappedTypeStart", TypeScriptLookahead.MappedType);
            Pair("NamedTupleStart", TypeScriptLookahead.NamedTuple);
            Pair("IndexSignatureStart", TypeScriptLookahead.IndexSignature);
            TypeScriptExpressions.ConfigurePredicates(options);
            TypeScriptNonNullExpressions.ConfigurePredicates(options);
            // Resolve the contextual 'asserts' keyword before reducing it as a normal
            // identifier. A later guard on the completed name cannot undo that Reduce.
            Func<PredicateContext, bool> startsAssertion = context =>
                context.NextToken.Input == "asserts" && sources.TryGetValue(context.NextToken, out var source) &&
                TypeScriptLookahead.AssertsTypePredicate(source.Text, context.NextToken.EndIndex + 1);
            options.RegisterPredicate("TSAssertsTypePredicateStart", startsAssertion);
            options.RegisterPredicate("TSNotAssertsTypePredicateStart", context => !startsAssertion(context));
            Func<PredicateContext, bool> startsAssertsNamePredicate = context =>
                context.NextToken.Input == "asserts" && sources.TryGetValue(context.NextToken, out var source) &&
                TypeScriptLookahead.AssertsNameTypePredicate(source.Text, context.NextToken.EndIndex + 1);
            options.RegisterPredicate("TSAssertsNameTypePredicateStart", startsAssertsNamePredicate);
            options.RegisterPredicate("TSNotAssertsNameTypePredicateStart", context => !startsAssertsNamePredicate(context));
            TypeScriptTypes.ConfigurePredicates(options);
            options.RegisterPredicate("TSTypeMemberEnd", context =>
                context.NextToken.Kind is EndMarker || context.NextToken.Input == "}" || context.NextToken.HasLineBreakBefore);
            if (kind != TypeScriptInputKind.Type)
            {
                TypeScriptClasses.ConfigurePredicates(options, Tokens, Source);
                options.RegisterPredicate("TSDeclarationStart", DeclarationStart);
                options.RegisterPredicate("TSFunctionSignatureEnd", context =>
                    context.NextToken.Input != "{" && context.NextToken.Input != ";" &&
                    (context.NextToken.Kind is EndMarker || context.NextToken.Input == "}" || context.NextToken.HasLineBreakBefore));
            }
        }

        private IReadOnlyList<string> Tokens(PredicateContext context)
        {
            if (sources.TryGetValue(context.NextToken, out var source))
                return TypeScriptLookahead.Scan(source.Text, context.NextToken.StartIndex);
            return new[] { context.NextToken.Input };
        }

        private string Source(PredicateContext context)
            => sources.TryGetValue(context.NextToken, out var source) ? source.Text : null;

        internal override LexicalPolicy CreatePolicy(EbnfGrammar grammar, IReadOnlyList<LexicalGoal> goals,
            Func<LexicalContext, LexicalGoal> select)
        {
            lexicalPolicy = new TypeScriptLexicalPolicy(grammar, kind, goals, select, transparent);
            return lexicalPolicy.Policy;
        }

        internal ITokenSource WrapSource(ITokenSource source, string input)
            => new TypeScriptTokenSource(source, input, lexicalPolicy);

        internal static string Edit(string text, string name, Func<string, string> edit)
        {
            var match = Regex.Match(text, @"(?m)^" + Regex.Escape(name) + @"\s*:\s*");
            if (!match.Success) throw new InvalidOperationException("Missing JavaScript production: " + name);
            int start = match.Index + match.Length;
            int end = JavaScriptFunctionGrammar.ProductionEnd(text, start);
            return text.Substring(0, start) + edit(text.Substring(start, end - start).Trim()) + text.Substring(end);
        }
        private sealed class SourceText { internal SourceText(string text) { Text = text; } internal string Text { get; } }
    }
}
