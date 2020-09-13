using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC
{
    public class TotalData
    {
        public TotalData(string originalData, string replacedData, IEnumerable<TokenCell> lexedData, ParsingResult parsedData)
        {
            OriginalData = originalData;
            ReplacedData = replacedData;
            LexedData = lexedData;
            ParsedData = parsedData;
        }

        public string OriginalData { get; }
        public string ReplacedData { get; }

        public IEnumerable<TokenCell> LexedData { get; }
        public ParsingResult ParsedData { get; }
        public MiniCNode RootNode { get; set; }
    }
}
