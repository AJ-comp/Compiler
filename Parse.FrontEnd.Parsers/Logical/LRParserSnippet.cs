using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.ErrorHandling;
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
                stack.Push(new TreeTerminal(inputValue));
                stack.Push(matchedValue.Item2);

                result.Direction = ActionDir.shift;
            }
            else if (matchedValue.Item1 == ActionDir.reduce)
            {
                var reduceDest = matchedValue.Item2 as NonTerminalSingle;
                var dataToInsert = new TreeNonTerminal(reduceDest);

                for (int i = 0; i < reduceDest.Count * 2; i++)
                {
                    var data = stack.Pop();
                    if (i % 2 > 0)
                        dataToInsert.Add(data as TreeSymbol);
                }
                stack.Push(dataToInsert);
                result.Direction = ActionDir.reduce;
            }
            else if (matchedValue.Item1 == ActionDir.epsilon_reduce)
            {
                stack.Push(new TreeNonTerminal(matchedValue.Item2 as NonTerminalSingle));

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

            if (topData is TreeNonTerminal)
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)topData];

            // invalid input symbol, can't shift (error handler also not exist)
            if (IxMetrix.MatchedValueSet.ContainsKey(inputValue.Kind) == false)
            {
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState();
                return false;
            }
            // invalid input symbol, can't shift (error handler exists)
            else if (IxMetrix.MatchedValueSet[inputValue.Kind].Item2 is IErrorHandlable)
            {
                var value = IxMetrix.MatchedValueSet[inputValue.Kind];
                parsingUnit.PossibleTerminalSet = IxMetrix.PossibleTerminalSet;
                parsingUnit.ChangeToFailedState(value.Item2 as IErrorHandlable);
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

            if (!(topData is TreeNonTerminal))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var seenSingleNT = topData as TreeNonTerminal;
            var secondData = parsingUnit.BeforeStack.SecondItemPeek();
            LRParsingTable parsingTable = this.Parser.ParsingTable as LRParsingTable;
            var IxMetrix = parsingTable[(int)secondData];

            // can't goto action
            if (!IxMetrix.MatchedValueSet.ContainsKey(seenSingleNT.ToNonTerminal))
            {
                parsingUnit.ChangeToFailedState();
                return false;
            }

            var matchedValue = IxMetrix.MatchedValueSet[seenSingleNT.ToNonTerminal];

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
        private ErrorHandlingResult ParsingFailedProcess(ParsingResult parsingResult, int curTokenIndex)
        {
            ErrorHandlingResult result = null;
            ParsingUnit lastParsingUnit = parsingResult[curTokenIndex].Units.Last();

            this.ActionFailed?.Invoke(this, lastParsingUnit);

            if (lastParsingUnit.ErrorHandler != null)
                result = lastParsingUnit.ErrorHandler.Call(this, parsingResult, curTokenIndex);

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
        /// <returns>Return true if a token that must to parse does not exist (Completed), else Return false.</returns>
        /// <exception cref="">If error handling failed then throw exception.</exception>
        private bool PostProcessing(SuccessedKind successedType, ParsingResult parsingResult, bool bErrorRecover, ref int index)
        {
            bool result = false;

            // ReduceOrGoto state can't arrive in this function because it is processed in 'BlockParsing' function.
            // Therefore this function doesn't process ReduceOrGoto state.
            if (successedType == SuccessedKind.Completed) result = true;
            else if (successedType == SuccessedKind.Shift) { }
            else if (bErrorRecover)
            {
                var errorRecoverInfo = this.ParsingFailedProcess(parsingResult, index);
                if(errorRecoverInfo == null) throw new ParsingException(index);
                else if (errorRecoverInfo.SuccessRecover == false) throw new ParsingException(errorRecoverInfo.TokenIndexToSee);
                else
                {
                    index = errorRecoverInfo.TokenIndexToSee;
//                    if (index > 0) index--;
                }
            }

            return result;
        }

        private ParsingResult ReplaceRange(ParsingResult srcResult, Range range, IReadOnlyList<TokenData> newTokens, Range newRange)
        {
            var prevParsingResult = new ParsingResult(srcResult.Take(range.StartIndex));
            var postParsingResult = new ParsingResult(srcResult.Skip(range.EndIndex + 1));

            for (int i = newRange.StartIndex; i <= newRange.EndIndex; i++)
            {
                prevParsingResult.Add(new ParsingBlock(newTokens[i]));
            }

            prevParsingResult.AddRange(postParsingResult);

            return prevParsingResult;
        }

        private ParsingResult ReplaceRanges(ParsingResult srcResult, IReadOnlyList<TokenData> newTokens, TokenizeImpactRanges ranges)
        {
            int addVal = 0;

            foreach(var range in ranges)
            {
                var adjustRange = new Range(range.Item1.StartIndex + addVal, range.Item1.Count);

                srcResult = this.ReplaceRange(srcResult, adjustRange, newTokens, range.Item2);
                addVal += range.Item2.Count - range.Item1.Count;
            }

            return srcResult;
        }

        /// <summary>
        /// This function is parsing for block that an error fired.
        /// </summary>
        /// <param name="parsingBlock">The prev parsing unit information</param>
        /// <param name="recoveryTokenInfos">The param is used when the units of the block must have multiple tokens</param>
        /// <returns></returns>
        public SuccessedKind RecoveryBlockParsing(ParsingBlock parsingBlock, IReadOnlyList<ParsingRecoveryData> recoveryTokenInfos)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;
            foreach(var recoveryInfo in recoveryTokenInfos)
            {
                var lastUnit = parsingBlock.Units.Last();
                var newUnit = new ParsingUnit(lastUnit.AfterStack) { InputValue = recoveryInfo.RecoveryToken };
                parsingBlock.units.Add(newUnit);
                lastUnit = parsingBlock.Units.Last();

                while (this.UnitParsing(lastUnit, recoveryInfo.RecoveryToken))
                {
                    result = this.ParsingSuccessedProcess(lastUnit);
                    newUnit.SetRecoveryMessage(recoveryInfo.RecoveryMessage); // Replace the message that may exist to the recovery message.

                    if (recoveryInfo.RecoveryToken.Kind == null) result = SuccessedKind.Shift;
                    if (result != SuccessedKind.ReduceOrGoto) break;

                    // ready for next parsing.
                    newUnit = new ParsingUnit(lastUnit.AfterStack) { InputValue = recoveryInfo.RecoveryToken };
                    parsingBlock.units.Add(newUnit);
                    lastUnit = parsingBlock.Units.Last();
                }

                if (result == LRParserSnippet.SuccessedKind.NotApplicable) break;
            }

            return result;
        }

        public SuccessedKind BlockParsing(ParsingBlock parsingBlock, bool bFromLastNext = true)
        {
            SuccessedKind result = SuccessedKind.NotApplicable;
            if (parsingBlock == null) return result;

            // remove prev information before block parsing.
            parsingBlock.errorInfos.Clear();

            var token = parsingBlock.Token;
            ParsingUnit newParsingUnit = (bFromLastNext) ? parsingBlock.AddParsingItem() : parsingBlock.Units.Last();
            newParsingUnit.InputValue = token;

            while (this.UnitParsing(newParsingUnit, token))
            {
                result = this.ParsingSuccessedProcess(newParsingUnit);

                if (token.Kind == null) result = SuccessedKind.Shift;
                if (result != SuccessedKind.ReduceOrGoto) break;

                // ready for next parsing.
                newParsingUnit = parsingBlock.AddParsingItem();
            }

            return result;
        }

        public void PartialParsing(ParsingResult target, TokenizeImpactRanges rangesToParse)
        {
            int indexToSee = 0;

            foreach(var range in rangesToParse)
            {
                if (range.Item2.StartIndex < indexToSee && indexToSee < range.Item2.EndIndex)
                {
                }
                else if (indexToSee > range.Item2.EndIndex) continue;

                // parsing update range.
                for (int i = range.Item2.StartIndex; i <= range.Item2.EndIndex; i++)
                {
                    if (i == 0) target[i] = new ParsingBlock(ParsingUnit.FirstParsingUnit, target[i].Token);
                    else target[i] = new ParsingBlock(new ParsingUnit(target[i - 1].Units.Last().AfterStack), target[i].Token);

                    var successKind = this.BlockParsing(target[i], false);
                    this.PostProcessing(successKind, target, true, ref i);

                    indexToSee = i + 1;
                }

                // parsing for post range until sync.
                for (int i = indexToSee; i < target.Count; i++)
                {
                    var targetToCompare = target[i];
                    target[i] = new ParsingBlock(new ParsingUnit(target[i - 1].Units.Last().AfterStack), target[i].Token);
                    var successKind = this.BlockParsing(target[i], false);
                    if (targetToCompare.Units.First().AfterStack.SequenceEqual(target[i].Units.Last().AfterStack))
//                        if (ElementsEquals(targetToCompare.Units.Last().AfterStack.ToList(), target[i].Units.Last().AfterStack.ToList()))
                    {
                        indexToSee = i + 1;
                        break;
                    }

                    this.PostProcessing(successKind, target, true, ref i);
                    indexToSee = i + 1;
                }
            }
        }

        /// <summary>
        /// This function returns a parsing result (after goto or shift or reduce process)
        /// This function also changes parsingUnit value.
        /// </summary>
        /// <param name="parsingBlock"></param>
        /// <param name="token">input terminal</param>
        /// <returns>It returns true if successed else returns false</returns>
        public bool UnitParsing(ParsingUnit parsingUnit, TokenData token)
        {
            parsingUnit.InputValue = token;
            parsingUnit.CopyBeforeStackToAfterStack();

            // filtering
            if (token.Kind == null) return true;
            if (token.Kind == new NotDefined()) { }

            bool result = this.GoTo(token, parsingUnit);
            if (result == false) result = this.ShiftAndReduce(token, parsingUnit);

            if (result) ParseTreeBuilder.BuildTree(parsingUnit);

            return result;
        }

        /// <summary>
        /// This function performs the following process
        /// It parsing next to 'basis' from 'seeingIndex' of 'wholeTokens' as much as 'rangeToParse'.
        /// </summary>
        /// <param name="basis"></param>
        /// <param name="wholeTokens"></param>
        /// <param name="rangeToParse"></param>
        /// <param name="seeingIndex"></param>
        /// <returns></returns>
        private void AllParsing(ParsingResult target)
        {
            for (int i = 0; i < target.Count; i++)
            {
                var token = target[i].Token;
                var unit = (i == 0) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(target[i - 1].Units.Last().AfterStack);
                unit.InputValue = token;
                target[i].units.Add(unit);

                var blockParsingResult = this.BlockParsing(target[i], false);

                if (this.PostProcessing(blockParsingResult, target, true, ref i)) break;
            }
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenData> tokens)
        {
            ParsingResult result = new ParsingResult();
            try
            {
                if (tokens == null) return result;
                if (tokens.Count <= 0) return result;

                result = new ParsingResult();
                foreach (var item in tokens) result.Add(new ParsingBlock(item));

                this.AllParsing(result);
                result.Success = true;
            }
            catch(ParsingException ex)
            {
                result = new ParsingResult(result.Take(ex.SeeingIndex + 1))
                {
                    Success = false
                };
            }
            catch (Exception)
            {
                result.Success = false;
            }

            return result;
        }

        public override ParsingResult Parsing(IReadOnlyList<TokenData> tokens, ParsingResult prevParsingInfo, TokenizeImpactRanges rangeToParse)
        {
            if (prevParsingInfo == null) return this.Parsing(tokens);
            if (prevParsingInfo.Success == false) return this.Parsing(tokens);
            if (rangeToParse == null) return this.Parsing(tokens);

            ParsingResult result = null;
            try
            {
                result = this.ReplaceRanges(prevParsingInfo, tokens, rangeToParse);
                this.PartialParsing(result, rangeToParse);
                result.Success = true;
            }
            catch(ParsingException ex)
            {
                result = new ParsingResult(result.Take(ex.SeeingIndex + 1))
                {
                    Success = false
                };
            }
            catch (Exception)
            {
                result.Success = false;
            }

            return result;
        }
    }
}
