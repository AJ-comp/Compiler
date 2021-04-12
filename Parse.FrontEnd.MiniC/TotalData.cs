using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Expressions;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC
{
    public class TotalData
    {
        public TotalData(string originalData, string replacedData, LexingData lexedData, ParsingResult parsedData)
        {
            OriginalData = originalData;
            ReplacedData = replacedData;
            LexedData = lexedData;
            ParsedData = parsedData;
        }

        public string OriginalData { get; }
        public string ReplacedData { get; }

        public LexingData LexedData { get; }
        public ParsingResult ParsedData { get; }
        public MiniCNode RootNode { get; set; }
        public AJExpression FinalExpression { get; set; }
    }
}
