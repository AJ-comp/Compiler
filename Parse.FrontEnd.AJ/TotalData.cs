using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;

namespace Parse.FrontEnd.AJ
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
        public AJNode RootNode { get; set; }
    }
}
