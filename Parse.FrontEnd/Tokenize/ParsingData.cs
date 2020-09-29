using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Tokenize
{
    public class ParsingData
    {
        public IReadOnlyList<TokenData> Tokens => _tokens;
        public TokenizeImpactRanges Ranges { get; }


        public ParsingData(TokenizeImpactRanges ranges, IEnumerable<TokenData> tokens)
        {
            Ranges = ranges;
            _tokens.AddRange(tokens);
        }


        private List<TokenData> _tokens = new List<TokenData>();
    }
}
