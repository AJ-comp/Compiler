using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LRParserSnippet : ParserSnippet
    {
        private int curTokenIndex = 0;
        private TokenData prevToken = null;

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public event EventHandler<ParsingFailResult> ActionFailed;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<LRParsingEventArgs> ActionSuccessed;

        protected override void CreateParsingHistoryTemplate()
        {
            this.ParsingHistory.AddColumn("prev stack");
            this.ParsingHistory.AddColumn("input symbol");
            this.ParsingHistory.AddColumn("action information");
            this.ParsingHistory.AddColumn("current stack");
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            foreach (var item in this.ParsingHistory.TreeInfo.ToReverseList())
                result += item.ToTreeString();

            return result;
        }

        public LRParserSnippet(Parser parser) : base(parser)
        {
        }

        private void BuildParseTree(LRParsingSuccessResult datas)
        {
            if (datas.ActionData.ActionDirection == ActionDir.shift)
            {
                //                if (!args.InputValue.Kind.Meaning) return;

                this.AllStack.Push(new TreeTerminal(datas.InputValue));
            }
            else if (datas.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                for (int i = 0; i < item.Count; i++) nonTerminal.Insert(0, this.AllStack.Pop());

                this.AllStack.Push(nonTerminal);
            }
            else if (datas.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;
                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);

                this.AllStack.Push(nonTerminal);
            }
        }

        private void BuildAST(LRParsingSuccessResult datas)
        {
            if (datas.ActionData.ActionDirection == ActionDir.shift)
            {
                if (!datas.InputValue.Kind.Meaning) return;

                this.MeaningStack.Push(new TreeTerminal(datas.InputValue));
            }
            else if (datas.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

                if (item.MeaningUnit != null)
                {
                    TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                    for (int i = 0; i < item.Count; i++)
                    {
                        var symbol = this.MeaningStack.Pop();
                        if(symbol is TreeNonTerminal)
                        {
                            var nonTerminalSymbol = symbol as TreeNonTerminal;
                            if (nonTerminalSymbol.Items.Count == 0)  // epsilon
                                continue;
                        }
                        nonTerminal.Insert(0, symbol);
                    }
                    this.MeaningStack.Push(nonTerminal);
                }
            }
            else if (datas.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

//                if (item.MeaningUnit != null)
//                {
                    TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                    this.MeaningStack.Push(nonTerminal);
//                }
            }
        }


        /// <summary>
        /// This function is a gateway to enter to the ParsingHistory.
        /// </summary>
        /// <param name="data">parsing process information</param>
        private void AddParsingHistory(ParsingProcessResult data)
        {
            if (data is LRParsingSuccessResult)
                this.AddSuccessInfoToParsingHistory(data as LRParsingSuccessResult);
            else
                this.AddFailedInfoToParsingHistory(data as ParsingFailResult);
        }

        /// <summary>
        /// This function writes parsing process information to the parsingHistory.
        /// </summary>
        /// <param name="data">parsing process information</param>
        private void AddSuccessInfoToParsingHistory(LRParsingSuccessResult data)
        {
            var param1 = Convert.ToString(data.PrevStack.Reverse(), " ");
            var param2 = data.InputValue.ToString();
            var param3 = data.ActionData.ActionDirection.ToString() + " ";
            var param4 = Convert.ToString(data.CurrentStack.Reverse(), " ");

            if (data.ActionData.ActionDirection != ActionDir.accept)
                param3 += (data.ActionData.ActionDest is NonTerminalSingle) ? (data.ActionData.ActionDest as NonTerminalSingle).ToGrammarString() : data.ActionData.ActionDest.ToString();

            if (data.ActionData.ActionDirection == ActionDir.reduce)
                this.ParsingHistory.AddTreeInfo(data.ActionData.ActionDest as NonTerminalSingle);

            this.ParsingHistory.AddRow(param1, param2, param3, param4);
        }

        /// <summary>
        /// This function writes parsing error information to the parsingHistory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data">parsing process information when error generated</param>
        private void AddFailedInfoToParsingHistory(ParsingFailResult data)
        {
            var param1 = Convert.ToString(data.PrevStack.Reverse(), " ");
            var param2 = data.InputValue.ToString();
            //            var param3 = args.ActionData.ActionDirection.ToString() + " ";

            this.ParsingHistory.AddRow(param1, param2, data.ErrorMessage, string.Empty);
        }





        private ActionData Process(Tuple<ActionDir, object> matchedValue, TokenData inputValue, Stack<object> stack)
        {
            ActionData result = new ActionData
            {
                ActionDest = matchedValue.Item2
            };

            if (matchedValue.Item1 == ActionDir.shift)
            {
                stack.Push(inputValue);
                stack.Push(matchedValue.Item2);

                result.ActionDirection = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for (int i = 0; i < reduceDest.Count * 2; i++) stack.Pop();
                stack.Push(reduceDest);
                result.ActionDirection = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                stack.Push(matchedValue.Item2 as NonTerminalSingle);

                result.ActionDirection = ActionDir.epsilon_reduce;
            }
            else if (matchedValue.Item1 == ActionDir.accept) result.ActionDirection = ActionDir.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ParsingProcessResult ShiftAndReduce(TokenData inputValue, Stack<object> prevStack, Stack<object> stack)
        {
            var topData = stack.Peek();

            if (topData is NonTerminalSingle) return new ParsingFailResult(prevStack, stack, inputValue, null, this.curTokenIndex);

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                return new ParsingFailResult(prevStack, stack, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex);
            }
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2.GetType() == typeof(ErrorHandler))
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                return new ParsingFailResult(prevStack, stack, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex, value.Item2 as ErrorHandler);
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            var result = this.Process(matchedValue, inputValue, stack);

            return new LRParsingSuccessResult(result, prevStack, stack, inputValue, IxMetrix.PossibleTerminalSet);
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="prevStack"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private ParsingProcessResult GoTo(TokenData inputValue, Stack<object> prevStack, Stack<object> stack)
        {
            var topData = stack.Peek();

            if (!(topData is NonTerminalSingle)) return new ParsingFailResult(prevStack, stack, inputValue, null, this.curTokenIndex);

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = stack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                return new ParsingFailResult(prevStack, stack, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex);
            }
            else
            {
                var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

                stack.Push((int)matchedValue.Item2);
                var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
                return new LRParsingSuccessResult(actionData, prevStack, stack, inputValue, IxMetrix.PossibleTerminalSet);
            }
        }

        /// <summary>
        /// This function returns a calculation result (it includes data about the next stack and how action did) with the current stack and input terminal.
        /// </summary>
        /// <param name="stack">current stack</param>
        /// <param name="inputValue">input terminal</param>
        /// <returns></returns>
        public ParsingProcessResult Parsing(Stack<object> stack, TokenData inputValue)
        {
            Stack<object> prevStack = stack.Clone();

            ParsingProcessResult result = this.GoTo(inputValue, prevStack, stack);
            if (result is ParsingFailResult) result = this.ShiftAndReduce(inputValue, prevStack, stack);

            return result;
        }

        public override ParsingResult Parsing(TokenCell[] tokenCells)
        {
            LRParsingResult result = new LRParsingResult();

            if (tokenCells.Length <= 0) return result;

            this.ParsingHistory.Clear();
            this.AllStack.Clear();
            this.MeaningQueue.Clear();
            Stack<object> stack = new Stack<object>();
            stack.Push(0);

            var tokens = tokenCells.ToList();
            tokens.Add(new TokenCell(-1, new EndMarker().Value, null));

            for (int i = 0; i < tokens.Count; i++)
            {
                this.curTokenIndex = i;
                var item = tokens[i];

                Terminal type = new Epsilon();
                if (item.Data == new EndMarker().Value && i == tokens.Count - 1) type = new EndMarker();
                else
                {
                    var typeData = item.PatternInfo.OptionData as Terminal;
                    if (typeData == null) type = new NotDefined();
                    else if (typeData.TokenType == TokenType.Delimiter) { prevToken = new TokenData(item.Data, type, item); continue; }
                    else if (typeData.TokenType == TokenType.Comment) { prevToken = new TokenData(item.Data, type, item); continue; }
                    else type = typeData;
                }

                var token = new TokenData(item.Data, type, item);

                if (token.Kind == new NotDefined())
                {
                }

                var parsingResult = this.Parsing(stack, token);
                this.AddParsingHistory(parsingResult);

                if(parsingResult is LRParsingSuccessResult)
                {
                    var successResult = parsingResult as LRParsingSuccessResult;
                    this.BuildParseTree(successResult);
                    this.BuildAST(successResult);

                    // syntax analysis complete
                    if (successResult.ActionData.ActionDirection == ActionDir.accept)
                    {
                        result.SetData(this.ParsingHistory, this.AllStack);
                        result.SetSuccess();
                        this.Parser.Grammar.SDTS.Process(this.MeaningStack.Pop() as TreeSymbol);
                        break;
                    }
                    else if (successResult.ActionData.ActionDirection == ActionDir.reduce ||
                                successResult.ActionData.ActionDirection == ActionDir.epsilon_reduce ||
                                successResult.ActionData.ActionDirection == ActionDir.moveto)
                    {
                        this.ActionSuccessed?.Invoke(this, new LRParsingEventArgs(parsingResult.PrevStack, parsingResult.CurrentStack, parsingResult.InputValue, successResult.ActionData));
                        i--;
                    }
                }
                else if(parsingResult is ParsingFailResult)
                {
                    var failedResult = parsingResult as ParsingFailResult;
                    result.AddFailedList(failedResult);
                    this.ActionFailed?.Invoke(this, failedResult);

                    if (failedResult.ErrorHandler == null) break;
                    else
                    {
                        var handleResult = failedResult.ErrorHandler.Call(failedResult.CurrentStack, tokens.ToArray(), curTokenIndex);
                        stack = handleResult.Stack;
                        curTokenIndex = handleResult.SeeingTokenIndex;
                    }
                }

                prevToken = token;
            }

            return result;
        }

        /*
        public ParsingResult Parsing(TokenCell[] tokenCells, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            this.AllStack.Reverse
            prevParsingInfo.
        }
        */
    }
}
