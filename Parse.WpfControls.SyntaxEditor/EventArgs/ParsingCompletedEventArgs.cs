using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
//        private List<AlarmEventArgs>

        public ParsingResult ParsingResult { get; }
        public TreeSymbol Ast { get; }

        public ParsingCompletedEventArgs(ParsingResult parsingResult, TreeSymbol ast)
        {
            ParsingResult = parsingResult;
            Ast = ast;
        }
    }
}
