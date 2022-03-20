using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using Parse.Utilities;
using System;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.AJ.ErrorHandler
{
    public class AJ_LRErrorHandlerFactory : Singleton<AJ_LRErrorHandlerFactory>
    {
        private AJGrammar grammar = new AJGrammar();
        private LRParsingTable parsingTable;
        private DefaultErrorHandler defaultHandler;

        private AJ_LRErrorHandlerFactory()
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

                    var actionList = new ActionDataList();
                    if (terminal == grammar.If)
                        actionList.Add(new ActionData(ActionDir.Failed, new If_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.Else)
                        actionList.Add(new ActionData(ActionDir.Failed, new Else_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.While)
                        actionList.Add(new ActionData(ActionDir.Failed, new While_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.Return)
                        actionList.Add(new ActionData(ActionDir.Failed, new Return_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == AJGrammar.Const)
                        actionList.Add(new ActionData(ActionDir.Failed, new Const_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == AJGrammar.Void)
                        actionList.Add(new ActionData(ActionDir.Failed, new Void_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == AJGrammar.Int)
                        actionList.Add(new ActionData(ActionDir.Failed, new Int_ErrorHandler(grammar, ixIndex)));
                    else if(terminal == AJGrammar.Ident)
                        actionList.Add(new ActionData(ActionDir.Failed, new Ident_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.OpenParenthesis)
                        actionList.Add(new ActionData(ActionDir.Failed, new OpenParenthesis_ErrorHandler(grammar, ixIndex)));
                    else if(terminal == grammar.CloseParenthesis)
                        actionList.Add(new ActionData(ActionDir.Failed, new CloseParenthesis_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.OpenCurlyBrace)
                        actionList.Add(new ActionData(ActionDir.Failed, new OpenCurlyBrace_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == grammar.CloseCurlyBrace)
                        actionList.Add(new ActionData(ActionDir.Failed, new CloseCurlyBrace_ErrorHandler(grammar, ixIndex)));
                    else if (terminal == new EndMarker())
                        actionList.Add(new ActionData(ActionDir.Failed, new EndMarker_ErrorHandler(grammar, ixIndex)));
                    else
                        actionList.Add(new ActionData(ActionDir.Failed, this.defaultHandler));

                    if(actionList.Count > 0) rowData.MatchedValueSet.Add(terminal, actionList);
                }
            }
        }
    }
}
