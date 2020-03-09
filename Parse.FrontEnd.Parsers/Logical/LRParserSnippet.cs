using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

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

        public LRParserSnippet(Parser parser) : base(parser)
        {
        }

        private void BuildParseTree(LRParsingSuccessResult datas, ParsingResult parsingResult)
        {
            if (datas.ActionData.ActionDirection == ActionDir.shift)
            {
                //                if (!args.InputValue.Kind.Meaning) return;

                parsingResult.MeaningStack.Push(new TreeTerminal(datas.InputValue));
            }
            else if (datas.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                for (int i = 0; i < item.Count; i++) nonTerminal.Insert(0, parsingResult.MeaningStack.Pop());

                parsingResult.MeaningStack.Push(nonTerminal);
            }
            else if (datas.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;
                TreeNonTerminal nonTerminal = new TreeNonTerminal(item);

                parsingResult.MeaningStack.Push(nonTerminal);
            }
        }

        private void BuildAST(LRParsingSuccessResult datas, ParsingResult parsingResult)
        {
            if (datas.ActionData.ActionDirection == ActionDir.shift)
            {
                if (!datas.InputValue.Kind.Meaning) return;

                parsingResult.MeaningStack.Push(new TreeTerminal(datas.InputValue));
            }
            else if (datas.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

                if (item.MeaningUnit != null)
                {
                    TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                    for (int i = 0; i < item.Count; i++)
                    {
                        var symbol = parsingResult.MeaningStack.Pop();
                        if(symbol is TreeNonTerminal)
                        {
                            var nonTerminalSymbol = symbol as TreeNonTerminal;
                            if (nonTerminalSymbol.Items.Count == 0)  // epsilon
                                continue;
                        }
                        nonTerminal.Insert(0, symbol);
                    }
                    parsingResult.MeaningStack.Push(nonTerminal);
                }
            }
            else if (datas.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = datas.ActionData.ActionDest as NonTerminalSingle;

//                if (item.MeaningUnit != null)
//                {
                    TreeNonTerminal nonTerminal = new TreeNonTerminal(item);
                    parsingResult.MeaningStack.Push(nonTerminal);
//                }
            }
        }


        /// <summary>
        /// This function is a gateway to enter to the ParsingHistory.
        /// </summary>
        /// <param name="data">parsing process information</param>
        private void AddParsingHistory(ParsingProcessResult data, ParsingResult parsingResult)
        {
            if (data is LRParsingSuccessResult)
                this.AddSuccessInfoToParsingHistory(data as LRParsingSuccessResult, parsingResult);
            else
                this.AddFailedInfoToParsingHistory(data as ParsingFailResult, parsingResult);
        }

        /// <summary>
        /// This function writes parsing process information to the parsingHistory.
        /// </summary>
        /// <param name="data">parsing process information</param>
        private void AddSuccessInfoToParsingHistory(LRParsingSuccessResult data, ParsingResult parsingResult)
        {
            var param1 = Convert.ToString(data.BlockItem.BeforeStack.Reverse(), " ");
            var param2 = data.InputValue.ToString();
            var param3 = data.ActionData.ActionDirection.ToString() + " ";
            var param4 = Convert.ToString(data.BlockItem.AfterStack.Reverse(), " ");

            if (data.ActionData.ActionDirection != ActionDir.accept)
                param3 += (data.ActionData.ActionDest is NonTerminalSingle) ? (data.ActionData.ActionDest as NonTerminalSingle).ToGrammarString() : data.ActionData.ActionDest.ToString();

            if (data.ActionData.ActionDirection == ActionDir.reduce)
                parsingResult.ParsingHistory.AddTreeInfo(data.ActionData.ActionDest as NonTerminalSingle);

            parsingResult.ParsingHistory.AddRow(param1, param2, param3, param4);
        }

        /// <summary>
        /// This function writes parsing error information to the parsingHistory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data">parsing process information when error generated</param>
        private void AddFailedInfoToParsingHistory(ParsingFailResult data, ParsingResult parsingResult)
        {
            var param1 = Convert.ToString(data.BlockItem.BeforeStack.Reverse(), " ");
            var param2 = data.InputValue.ToString();
            //            var param3 = args.ActionData.ActionDirection.ToString() + " ";

            parsingResult.ParsingHistory.AddRow(param1, param2, data.ErrorMessage, string.Empty);
        }




        /// <summary>
        /// This function writes a value into stack following to action value (matchedValue).
        /// </summary>
        /// <param name="matchedValue"></param>
        /// <param name="inputValue"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
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
        private ParsingProcessResult ShiftAndReduce(TokenData inputValue, BlockStackItem blockItem)
        {
            var topData = blockItem.BeforeStack.Peek();

            if (topData is NonTerminalSingle) return new ParsingFailResult(blockItem, inputValue, null, this.curTokenIndex);

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                return new ParsingFailResult(blockItem, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex);
            }
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2.GetType() == typeof(ErrorHandler))
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                return new ParsingFailResult(blockItem, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex, value.Item2 as ErrorHandler);
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            var result = this.Process(matchedValue, inputValue, blockItem.AfterStack);

            return new LRParsingSuccessResult(result, blockItem, inputValue, IxMetrix.PossibleTerminalSet);
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="blockItem"></param>
        /// <returns></returns>
        private ParsingProcessResult GoTo(TokenData inputValue, BlockStackItem blockItem)
        {
            var topData = blockItem.BeforeStack.Peek();

            if (!(topData is NonTerminalSingle)) return new ParsingFailResult(blockItem, inputValue, null, this.curTokenIndex);

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = blockItem.BeforeStack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                return new ParsingFailResult(blockItem, inputValue, IxMetrix.PossibleTerminalSet, this.curTokenIndex);
            }
            else
            {
                var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

                blockItem.AfterStack.Push((int)matchedValue.Item2);
                var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
                return new LRParsingSuccessResult(actionData, blockItem, inputValue, IxMetrix.PossibleTerminalSet);
            }
        }

        /// <summary>
        /// This function returns a calculation result (it includes data about the next stack and how action did) with the current stack and input terminal.
        /// </summary>
        /// <param name="stack">current stack</param>
        /// <param name="inputValue">input terminal</param>
        /// <returns></returns>
        private ParsingProcessResult Parsing(BlockParsingStack blockStack, TokenData inputValue)
        {
            blockStack.Stacks.Last().CopyBeforeStackToAfterStack();

            ParsingProcessResult result = this.GoTo(inputValue, blockStack.Stacks.Last());
            if (result is ParsingFailResult) result = this.ShiftAndReduce(inputValue, blockStack.Stacks.Last());

            return result;
        }

        public override ParsingResult Parsing(TokenCell[] tokenCells)
        {
            LRParsingResult result = new LRParsingResult();

            if (tokenCells.Length <= 0) return result;

            var tokens = tokenCells.ToList();
            tokens.Add(new TokenCell(-1, new EndMarker().Value, null));

            bool bAdd = true;
            for (int i = 0; i < tokens.Count; i++)
            {
                this.curTokenIndex = i;
                var item = tokens[i];
                if (bAdd)
                {
                    var stack = (result.ParsingStack.Count == 0) ? new BlockStackItem() : new BlockStackItem(result.ParsingStack.Last().Stacks.Last().AfterStack);
                    result.ParsingStack.Add(new BlockParsingStack(stack, item));    // add new parsing block.
                }
                else
                {
                    result.ParsingStack.Last().AddParsingItem(); // add new parsing item on current parsing block.
                }
                bAdd = true;

                Terminal type = new Epsilon();
                if (item.Data == new EndMarker().Value && i == tokens.Count - 1) type = new EndMarker();
                else
                {
                    var typeData = item.PatternInfo.OptionData as Terminal;
                    if (typeData == null) type = new NotDefined();
                    else if (typeData.TokenType == TokenType.Delimiter || typeData.TokenType == TokenType.Comment) 
                    { 
                        prevToken = new TokenData(item.Data, type, item);
                        result.ParsingStack.Last().Stacks.Last().CopyBeforeStackToAfterStack();
                        continue; 
                    }
                    else type = typeData;
                }

                var token = new TokenData(item.Data, type, item);

                if (token.Kind == new NotDefined())
                {
                }

                var processResult = this.Parsing(result.ParsingStack.Last(), token);
                this.AddParsingHistory(processResult, result);

                if(processResult is LRParsingSuccessResult)
                {
                    var successResult = processResult as LRParsingSuccessResult;
                    this.BuildParseTree(successResult, result);
                    this.BuildAST(successResult, result);

                    // syntax analysis complete
                    if (successResult.ActionData.ActionDirection == ActionDir.accept)
                    {
                        result.SetSuccess();
                        this.Parser.Grammar.SDTS.Process(result.MeaningStack.Pop() as TreeSymbol);
                        break;
                    }
                    else if (successResult.ActionData.ActionDirection == ActionDir.reduce ||
                                successResult.ActionData.ActionDirection == ActionDir.epsilon_reduce ||
                                successResult.ActionData.ActionDirection == ActionDir.moveto)
                    {
                        this.ActionSuccessed?.Invoke(this, new LRParsingEventArgs(result.ParsingStack.Last().Stacks.Last(), processResult.InputValue, successResult.ActionData));
                        bAdd = false; i--;
                    }
                }
                else if(processResult is ParsingFailResult)
                {
                    var failedResult = processResult as ParsingFailResult;
                    result.AddFailedList(failedResult);
                    this.ActionFailed?.Invoke(this, failedResult);

                    if (failedResult.ErrorHandler == null) break;
                    else
                    {
                        var handleResult = failedResult.ErrorHandler.Call(failedResult.BlockItem.AfterStack, tokens.ToArray(), curTokenIndex);
                        failedResult.BlockItem.AfterStack = handleResult.Stack;
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
            var totalList = this.AllStack.ToList()
            prevParsingInfo.
        }
        */
    }
}
