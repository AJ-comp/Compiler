using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using Parse.Utilities;
using System;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class MiniC_LRErrorHandlerFactory : Singleton<MiniC_LRErrorHandlerFactory>
    {
        private AJGrammar grammar = new AJGrammar();
        private LRParsingTable parsingTable;
        private DefaultErrorHandler defaultHandler;

        private MiniC_LRErrorHandlerFactory()
        { }

        public void AddErrorHandler(LRParser parser)
        {
            if (parser.Grammar.ToString() != grammar.ToString()) return;

            int ixIndex = -1;
            this.parsingTable = parser.ParsingTable as LRParsingTable;
            this.defaultHandler = new DefaultErrorHandler(this.grammar, this.parsingTable);

            foreach (var rowData in parsingTable)
            {
                ixIndex++;

                foreach (var terminal in parsingTable.RefTerminalSet)
                {
                    if (rowData.MatchedValueSet.ContainsKey(terminal)) continue;

                    if (terminal == grammar.If)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new If_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.Else)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Else_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.While)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new While_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.Return)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Return_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.Const)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Const_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == AJGrammar.Void)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Void_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == AJGrammar.Int)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Int_ErrorHandler(grammar, ixIndex)));
                    else if(terminal == AJGrammar.Ident)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new Ident_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.OpenParenthesis)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new OpenParenthesis_ErrorHandler(grammar, ixIndex)));
                    else if(terminal == grammar.CloseParenthesis)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new CloseParenthesis_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.OpenCurlyBrace)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new OpenCurlyBrace_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.CloseCurlyBrace)
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new CloseCurlyBrace_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == new EndMarker())
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, new EndMarker_ErrorHandler(grammar, ixIndex)));
                    else
                        rowData.MatchedValueSet.Add(terminal, new Tuple<ActionDir, object>(ActionDir.failed, this.defaultHandler));
                }
            }
        }
    }
}
