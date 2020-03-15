using Parse.Extensions;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.ErrorHandling;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
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
        public enum SuccessedKind { Completed, ReduceOrGoto, Shift, NotApplicable };

        /// <summary>
        /// The Error Handler that if the action failed.
        /// ParsingFailResult : The state information when error generated
        /// </summary>
        public event EventHandler<ParsingUnit> ActionFailed;
        /// <summary>
        /// The Error Handler that if the goto failed.
        /// TokenData : input data
        /// NonTerminalSet : possible nonterminal set
        /// </summary>
        public Action<TokenData, HashSet<NonTerminal>> GotoFailed { get; set; } = null;
        /// <summary>
        /// The Error Handler that if the action completed.
        /// </summary>
        public event EventHandler<ParsingUnit> ActionSuccessed;

        public LRParserSnippet(Parser parser) : base(parser)
        {
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
                Dest = matchedValue.Item2
            };

            if (matchedValue.Item1 == ActionDir.shift)
            {
                stack.Push(inputValue);
                stack.Push(matchedValue.Item2);

                result.Direction = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;

                for (int i = 0; i < reduceDest.Count * 2; i++) stack.Pop();
                stack.Push(reduceDest);
                result.Direction = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                stack.Push(matchedValue.Item2 as NonTerminalSingle);

                result.Direction = ActionDir.epsilon_reduce;
            }
            else if (matchedValue.Item1 == ActionDir.accept) result.Direction = ActionDir.accept;

            return result;
        }

        /// <summary>
        /// This function processes shift and reduce process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="parsingUnit"></param>
        /// <returns></returns>
        private bool ShiftAndReduce(TokenData inputValue, ParsingUnit parsingUnit)
        {
            parsingUnit.ChangeToNormalState();
            var topData = parsingUnit.BeforeStack.Peek();

            if (topData is NonTerminalSingle)
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (!IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind))
            {
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState();
                return false;
            }
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2.GetType() == typeof(ErrorHandler))
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.Item2 as ErrorHandler);
                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[inputValue.Kind];
            parsingUnit.Action = this.Process(matchedValue, inputValue, parsingUnit.AfterStack);

            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
            return true;
        }

        /// <summary>
        /// This function processes goto process.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="parsingUnit"></param>
        /// <returns></returns>
        private bool GoTo(TokenData inputValue, ParsingUnit parsingUnit)
        {
            parsingUnit.ChangeToNormalState();
            var topData = parsingUnit.BeforeStack.Peek();

            if (!(topData is NonTerminalSingle))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var seenSingleNT = topData as NonTerminalSingle;
            var secondData = parsingUnit.BeforeStack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal()))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal()];

            var actionData = new ActionData(matchedValue.Item1, matchedValue.Item2);
            parsingUnit.AfterStack.Push((int)matchedValue.Item2);
            parsingUnit.Action = actionData;
            parsingUnit.InputValue = inputValue;
            parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;

            return true;
        }

        /// <summary>
        /// This function is performed if a parsing process result is a success.
        /// </summary>
        /// <param name="successResult">The result of the 1 level parsing</param>
        /// <returns></returns>
        private SuccessedKind ParsingSuccessedProcess(ParsingUnit successResult)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;

            // syntax analysis complete
            if (successResult.Action.Direction == ActionDir.accept)
            {
                result = SuccessedKind.Completed;
                //                this.Parser.Grammar.SDTS.Process(parsingResult.MeaningStack.Pop() as TreeSymbol);
            }
            else if (successResult.Action.Direction == ActionDir.reduce ||
                        successResult.Action.Direction == ActionDir.epsilon_reduce ||
                        successResult.Action.Direction == ActionDir.moveto)
            {
                result = SuccessedKind.ReduceOrGoto;
                this.ActionSuccessed?.Invoke(this, successResult);
            }
            else if(successResult.Action.Direction == ActionDir.shift)  result = SuccessedKind.Shift;

            return result;
        }

        /// <summary>
        /// This function is performed if parsing process result is a fail.
        /// </summary>
        /// <param name="parsingResult">The result of the whole parsing</param>
        /// <param name="tokens">The whole tokens, this needs to recover an error.</param>
        /// <param name="curTokenIndex">The token index seeing current</param>
        /// <returns>Returns ErrorHandlingResult if an error was recovered (this means that it can parse continue) else returns null.</returns>
        private ErrorHandlingResult ParsingFailedProcess(ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, int curTokenIndex)
        {
            ErrorHandlingResult result = null;
            ParsingUnit lastParsingUnit = parsingResult.Last().Units.Last();

            this.ActionFailed?.Invoke(this, lastParsingUnit);

            if (lastParsingUnit.ErrorHandler != null)
                result = lastParsingUnit.ErrorHandler.Call(this, parsingResult, tokens.ToArray(), curTokenIndex);

            return result;
        }

        /// <summary>
        /// This function processes the result that be created after parsing.
        /// </summary>
        /// <param name="successedType">The result kind of the parsing</param>
        /// <param name="parsingResult">The whole result of the parsing until the current</param>
        /// <param name="tokens"></param>
        /// <param name="bErrorRecover"></param>
        /// <param name="index"></param>
        /// <returns>Return true if a token that must to parse does not exist (Completed, Failed (Error is not Handling)), else Return false.</returns>
        private bool PostProcessing(SuccessedKind successedType, ParsingResult parsingResult, IReadOnlyList<TokenData> tokens, bool bErrorRecover, ref int index)
        {
            bool result = false;

            if (successedType == SuccessedKind.Completed) result = true;
            else if (successedType == SuccessedKind.ReduceOrGoto) index--;
            else if (successedType == SuccessedKind.Shift) { }
            else if (bErrorRecover)
            {
                var errorRecoverInfo = this.ParsingFailedProcess(parsingResult, tokens, index);
                if (errorRecoverInfo == null) result = true;
                else if (errorRecoverInfo.SuccessRecover == false) result = true;
                else
                {
                    index = errorRecoverInfo.TokenIndexToSee;
                    if (index > 0) index--;
                }
            }

            return result;
        }

        /// <summary>
        /// This function is parsing after creates a new block on the basis of the prev parsing unit information.
        /// </summary>
        /// <param name="tokenDatas">The tokens to parse</param>
        /// <param name="blockToken">The block token</param>
        /// <param name="prevAfterStack">The prev parsing unit information</param>
        /// <returns></returns>
        public Tuple<SuccessedKind, ParsingBlock> BlockParsing(IReadOnlyList<TokenData> tokenDatas, TokenData blockToken, ParsingUnit prevParsingUnit = null)
        {
            ParsingBlock newParsingBlock = ParsingBlock.CreateNextParsingBlock(prevParsingUnit, blockToken);

            var resultItem1 = this.BlockParsing(tokenDatas, newParsingBlock, false);
            return new Tuple<SuccessedKind, ParsingBlock>(resultItem1, newParsingBlock);
        }

        /// <summary>
        /// This function is parsing continue from the current parsing block.
        /// </summary>
        /// <param name="tokenDatas">The tokens to parse</param>
        /// <param name="parsingBlock">The current parsing block</param>
        /// <param name="bFromLastNext"></param>
        /// <returns></returns>
        public SuccessedKind BlockParsing(IReadOnlyList<TokenData> tokenDatas, ParsingBlock parsingBlock, bool bFromLastNext = true)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;
            if (parsingBlock == null) return result;

            for (int i = 0; i < tokenDatas.Count; i++)
            {
                var token = tokenDatas[i];
                ParsingUnit newParsingUnit = (bFromLastNext) ? parsingBlock.AddParsingItem() : parsingBlock.Units.Last();
                bFromLastNext = true;
                newParsingUnit.InputValue = token;

                if (this.Parsing(newParsingUnit, token))
                {
                    result = this.ParsingSuccessedProcess(newParsingUnit);
                    if (token.Kind == null) result = SuccessedKind.Shift;
                    if (result == SuccessedKind.ReduceOrGoto) i--;
                }
                else break;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenDatas"></param>
        /// <param name="parsingResult"></param>
        /// <param name="bErrorRecover"></param>
        /// <returns></returns>
        public ParsingResult ParsingCore(IReadOnlyList<TokenData> tokenDatas, ParsingResult parsingResult, bool bErrorRecover = true)
        {
            ParsingResult result = (parsingResult == null) ? new ParsingResult() : parsingResult;
            if (tokenDatas.Count <= 0) return result;

            for (int i = 0; i < tokenDatas.Count; i++)
            {
                var token = tokenDatas[i];
                var prevStack = (result.Count == 0) ? null : result.Last().Units.Last();

                var tokens = new List<TokenData>();
                tokens.Add(token);
                var blockParsingResult = this.BlockParsing(tokens, token, prevStack);
                parsingResult.Add(blockParsingResult.Item2);

                if (this.PostProcessing(blockParsingResult.Item1, parsingResult, tokenDatas, bErrorRecover, ref i)) break;
            }

            return result;
        }

        /// <summary>
        /// This function returns a parsing result (after goto or shift or reduce process)
        /// This function also changes parsingUnit value.
        /// </summary>
        /// <param name="parsingBlock"></param>
        /// <param name="token">input terminal</param>
        /// <returns></returns>
        public bool Parsing(ParsingUnit parsingUnit, TokenData token)
        {
            parsingUnit.InputValue = token;
            parsingUnit.CopyBeforeStackToAfterStack();

            // filtering
            if (token.Kind == null) return true;
            if (token.Kind == new NotDefined()) { }

            bool result = this.GoTo(token, parsingUnit);
            if (result == false) result = this.ShiftAndReduce(token, parsingUnit);

            return result;
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells)
        {
            if (tokenCells.Count <= 0) return new ParsingResult();

            var tokens = tokenCells.ToList();
            tokens.Add(new TokenCell(-1, new EndMarker().Value, null));

            var result = new ParsingResult();
            var tokenDatas = this.ToTokenDataList(tokens);
            return this.ParsingCore(tokenDatas, result);
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokenCells, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            if (prevParsingInfo == null) return this.Parsing(tokenCells);
            if (rangeToParse == null) return this.Parsing(tokenCells);

            ParsingResult result = new ParsingResult();
//            var totalList = prevParsingInfo.ParsingStack.Count;

            foreach (var range in rangeToParse.CurRanges)
            {
                var tokensToParse = tokenCells.Skip(range.StartIndex).Take(range.Count);
                var prevParsingBlock = prevParsingInfo.Take(range.EndIndex);

                if (tokensToParse.Count() == 0) return this.Parsing(tokenCells);
                if (prevParsingBlock.Count() == 0) return this.Parsing(tokenCells);

                if (range.StartIndex == 0)
                {
                    if (tokenCells.Count() == range.Count) return this.Parsing(tokenCells);
                    else
                    {
                        var postParsingResult = prevParsingInfo.Take(range.EndIndex + 1);
                        var tokenDatas = this.ToTokenDataList(tokenCells);
                        result = this.ParsingCore(tokenDatas, new ParsingResult());
                        result.AddRange(postParsingResult); // merge ParsingResult with ParsingResult.
                    }
                }
                else
                {

                }


                //                result = this.ParsingCore(tokensToParse.ToArray(), ) as ParsingResult;
            }

            return result;
        }
    }
}
