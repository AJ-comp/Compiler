using Janglim.FrontEnd.Tokenize;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    // Retain the domain used for each actual read, rather than guessing it later
    // from a spelling such as '<'. Prediction forks have distinct token objects.
    internal sealed class TypeScriptTokenSource : ITokenSource
    {
        private readonly ITokenSource source;
        private readonly TypeScriptLexicalPolicy policy;

        internal TypeScriptTokenSource(ITokenSource source, string input, TypeScriptLexicalPolicy policy)
        {
            this.source = source;
            this.policy = policy;
            policy.Register(source.Start.SourceIdentity, input);
        }

        public TokenCursor Start => source.Start;
        public int Length => source.Length;
        public LexicalPolicy Policy => source.Policy;

        public TokenRead Peek(TokenCursor start, LexicalGoal goal)
        {
            var read = source.Peek(start, goal);
            policy.Record(start, goal, read);
            return read;
        }
    }
}
