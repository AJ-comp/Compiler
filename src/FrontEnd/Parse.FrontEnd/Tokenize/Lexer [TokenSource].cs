using System.Linq;

namespace Janglim.FrontEnd.Tokenize
{
    public partial class Lexer
    {
        /// <summary>
        /// Creates an independent request-driven source using the current token rules and their priority order.
        /// The source retains the supplied policy, including its registered lexical goals and selection logic.
        /// Goals in the policy must use the original registered Terminal instances. The parser
        /// obtains the policy from this source; callers do not register the goals again.
        /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#exchange">Parser-driven lexing: token exchange</see></para>
        /// </summary>
        public ITokenSource CreateTokenSource(string source, LexicalPolicy policy)
            => new ContextualTokenSource(source, _tokenPatternList.ToArray(), policy);
    }
}
