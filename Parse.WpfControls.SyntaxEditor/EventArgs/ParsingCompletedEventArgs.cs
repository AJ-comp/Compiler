using Parse.FrontEnd;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
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
    }
}
