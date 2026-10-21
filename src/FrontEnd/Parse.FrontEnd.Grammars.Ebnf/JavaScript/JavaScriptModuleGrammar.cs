using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// ECMAScript 2026 Module syntax with static imports/exports, import attributes,
    /// import expressions and top-level await. Parsing and Validate check syntax and
    /// early errors without loading, linking or evaluating any JavaScript module.
    /// <para><see href="../../../../docs/ko/javascript-modules.html">JavaScript 모듈과 import·export 문법</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html#context">모듈과 async 함수의 문맥</see></para>
    /// </summary>
    public sealed class JavaScriptModuleGrammar
    {
        public const int Edition = JavaScriptExpressionGrammar.Edition;
        private readonly JavaScriptExpressionGrammar language = new JavaScriptExpressionGrammar(JavaScriptSourceGoal.Module);
        public EbnfGrammar Grammar => language.Grammar;
        public LexicalPolicy Policy => language.Policy;
        public string GrammarText => language.GrammarText;
        public ITokenSource CreateTokenSource(string input) => language.CreateTokenSource(input);
        public IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree) => language.Validate(tree);
    }
}
