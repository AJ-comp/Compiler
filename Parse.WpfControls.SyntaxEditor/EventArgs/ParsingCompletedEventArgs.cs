using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Datas;
using System;
using System.Collections.Generic;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
        public ParsingResult ParsingResult { get; }
        public SemanticAnalysisResult SemanticResult { get; }


        public ParsingCompletedEventArgs(ParsingResult parsingResult, SemanticAnalysisResult semanticResult)
        {
            ParsingResult = parsingResult;
            SemanticResult = semanticResult;
        }
    }
}
