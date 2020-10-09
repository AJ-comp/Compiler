using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Tokenize
{
    public class ParsingData
    {
        public IReadOnlyList<TokenData> Tokens => _tokens;
        public RangeList RangesToParse { get; }


        public ParsingData(RangeList ranges, IEnumerable<TokenData> tokens)
        {
            RangesToParse = ranges;
            _tokens.AddRange(tokens);
        }


        private List<TokenData> _tokens = new List<TokenData>();
    }
}
