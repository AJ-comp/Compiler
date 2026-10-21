using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// ECMAScript 2026 strict Script grammar with ordinary/async/generator functions, arrows, classes,
    /// statements and declaration/parameter binding patterns. Parse the source with LALRParser, then
    /// call Validate on the successful tree to check static early errors.
    /// <para><see href="../../../../docs/ko/javascript-statements.html">JavaScript 함수·문장·선언 문법</see></para>
    /// <para><see href="../../../../docs/ko/javascript-classes.html">JavaScript 클래스와 멤버 문맥</see></para>
    /// <para><see href="../../../../docs/ko/javascript-modules.html#api">Script와 Module의 입력 문맥 선택</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html">함수 종류와 객체 메서드</see></para>
    /// </summary>
    public sealed class JavaScriptScriptGrammar
    {
        public const int Edition = JavaScriptExpressionGrammar.Edition;
        private readonly JavaScriptExpressionGrammar language = new JavaScriptExpressionGrammar(true);
        public EbnfGrammar Grammar => language.Grammar;
        public LexicalPolicy Policy => language.Policy;
        public string GrammarText => language.GrammarText;
        public ITokenSource CreateTokenSource(string input) => language.CreateTokenSource(input);
        public IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree) => language.Validate(tree);
    }
}
