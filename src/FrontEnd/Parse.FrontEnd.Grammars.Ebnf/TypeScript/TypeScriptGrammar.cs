using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Composes TypeScript type syntax with the existing JavaScript grammar, lexical
    /// source and early-error checks. The original concrete tree retains type nodes.
    /// Parse <see cref="CreateTokenSource"/> with LALRParser, then call <see cref="Validate"/>.
    /// Type-name resolution and type assignability belong to a separate type checker.
    /// <para><see href="../../../../docs/ko/typescript.html">TypeScript 문법의 구성과 사용 순서</see></para>
    /// </summary>
    public sealed class TypeScriptGrammar
    {
        public const string LanguageVersion = "7.0.2";
        private readonly JavaScriptGrammar language;
        private readonly TypeScriptDialect dialect;
        public TypeScriptInputKind InputKind { get; }
        public JavaScriptMode Mode => language.Mode;
        public EbnfGrammar Grammar => language.Grammar;
        public LexicalPolicy Policy => language.Policy;
        public string GrammarText => language.GrammarText;

        public TypeScriptGrammar(TypeScriptInputKind inputKind = TypeScriptInputKind.Script,
            JavaScriptMode mode = JavaScriptMode.NonStrict)
        {
            if (!Enum.IsDefined(typeof(TypeScriptInputKind), inputKind))
                throw new ArgumentOutOfRangeException(nameof(inputKind));
            InputKind = inputKind;
            var kind = inputKind == TypeScriptInputKind.Module ? JavaScriptInputKind.Module :
                inputKind == TypeScriptInputKind.Script ? JavaScriptInputKind.Script : JavaScriptInputKind.Expression;
            dialect = new TypeScriptDialect(inputKind);
            language = new JavaScriptGrammar(mode, false, kind, dialect);
        }

        public ITokenSource CreateTokenSource(string input)
            => dialect.WrapSource(language.CreateTokenSource(input), input);
        public IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree)
            => TypeScriptValidation.Validate(tree, Mode);
    }
}
