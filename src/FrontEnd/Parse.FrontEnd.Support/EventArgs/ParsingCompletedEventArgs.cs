using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using System.Diagnostics;

namespace Parse.FrontEnd.Support.EventArgs
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ParsingCompletedEventArgs
    {
        public LexingData LexingData { get; }
        public ParsingResult ParsingResult { get; }
        public SemanticAnalysisResult SemanticResult { get; }


        public ParsingCompletedEventArgs(LexingData lexingData, 
                                                            ParsingResult parsingResult, 
                                                            SemanticAnalysisResult semanticResult)
        {
            LexingData = lexingData;
            ParsingResult = parsingResult;
            SemanticResult = semanticResult;
        }

        private string DebuggerDisplay
            => string.Format("Parsing Block count: {0}, Semantic result: {1}",
                                        ParsingResult.Count,
                                        SemanticResult.AllNodes.Count);
    }
}
