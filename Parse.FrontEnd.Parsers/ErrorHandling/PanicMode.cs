using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public class PanicMode
    {
        /// <summary>
        /// This function adjusts a stack to process with targetTerminal.
        /// </summary>
        /// <param name="parsingTable">The parsing table to refer</param>
        /// <param name="processStack">The stack to adjust</param>
        /// <param name="targetTerminal">The token to process</param>
        /// <returns>Returns index if successed. Returns -1 if failed.</returns>
        private static bool AdjustStack(LRParsingTable parsingTable, Stack<object> processStack, Terminal targetTerminal)
        {
            bool result = false;

            /*
            while (true)
            {
                if (processStack.Count == 0) break;
                else
                {
                    result = true;
                    processStack.Pop();
                    if(processStack.Peek() is TokenData) processStack.Pop();
                    break;
                }
            }
            */

            while (true)
            {
                var topData = processStack.Peek();
                if (topData is NonTerminalSingle)
                {
                    processStack.Pop();
                    continue;
                }

                var metrixIndex = (int)topData;
                var ixMetrix = parsingTable[metrixIndex];

                if (ixMetrix.PossibleTerminalSet.Contains(targetTerminal)) { result = true; break; }
                else
                {
                    processStack.Pop();
                    if (processStack.Peek() is TokenData) processStack.Pop();
                }
            }

            return result;
        }

        /// <summary>
        /// This function adjusts tokens as doing skip token until seeing one of the synchronizeTokens.
        /// </summary>
        /// <param name="parsingResult"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="synchronizeTokens"></param>
        /// <returns></returns>
        private static int AdjustToken(ParsingResult parsingResult, int seeingTokenIndex, HashSet<Terminal> synchronizeTokens)
        {
            while (seeingTokenIndex < parsingResult.Count - 1)
            {
                //                var ixMetrix = parsingTable[curStatus];

                ParsingBlock prevBlock = (seeingTokenIndex == 0) ? null : parsingResult[seeingTokenIndex - 1];
                ParsingBlock blockToSkip = parsingResult[seeingTokenIndex];
                TokenData curToken = blockToSkip.Token;
                blockToSkip.units.Clear(); // it has to delete an all unit because a block to skip.

                // check
                var targetTerminal = curToken.Kind;
                if (targetTerminal != null && synchronizeTokens.Contains(targetTerminal)) break;

                seeingTokenIndex++;

                // create a new unit
                ParsingUnit newParsingUnit = (prevBlock == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevBlock.Units.Last().AfterStack);
                newParsingUnit.InputValue = curToken;
                newParsingUnit.CopyBeforeStackToAfterStack();   // stack sync because to throw away token.
                newParsingUnit.SetRecoveryMessage(string.Format("({0}, {1})", Resource.RecoverWithPanicMode, Resource.SkipToken));

                blockToSkip.units.Add(newParsingUnit);
                blockToSkip.ErrorInfo = new ParsingErrorInfo(ParsingErrorInfo.ErrorType.Error, nameof(AlarmCodes.CE0001), string.Format(AlarmCodes.CE0001, blockToSkip.Token.Input));

                //                if (ixMetrix.PossibleTerminalSet.Contains(curToken.Data)) break;
                //                else lastParsingUnit.ChangeToFailedState(Resource.RecoverWithPanicMode);
            }

            return seeingTokenIndex;
        }

        /// <summary>
        /// This function is an error handler for LR parsing.
        /// </summary>
        /// <param name="parsingTable"></param>
        /// <param name="parsingResult"></param>
        /// <param name="tokens"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="synchronizeTokens">The token set to process</param>
        /// <returns></returns>
        public static ErrorHandlingResult LRProcess(LRParserSnippet snippet, LRParsingTable parsingTable, ParsingResult parsingResult, int seeingTokenIndex, HashSet<Terminal> synchronizeTokens)
        {
            ErrorHandlingResult result = new ErrorHandlingResult(parsingResult, seeingTokenIndex, false);
            ParsingUnit seeingParsingUnit = null;
            ParsingBlock curBlock = null;

            try
            {
                if (synchronizeTokens.Count == 0) return result;
                var tokenData = parsingResult[seeingTokenIndex].Token;
                var targetTerminal = tokenData.Kind;

                // if the terminal of the seeing token is not a synchronizeTokens kind then skip token until to see a synchronizeTokens kind.
                if (synchronizeTokens.Contains(targetTerminal) == false)
                {
                    seeingTokenIndex = PanicMode.AdjustToken(parsingResult, seeingTokenIndex, synchronizeTokens);
                    tokenData = parsingResult[seeingTokenIndex].Token;
                    targetTerminal = tokenData.Kind;
                }

                // add a new parsing unit and initialize
                ParsingBlock prevBlock = (seeingTokenIndex == 0) ? null : parsingResult[seeingTokenIndex - 1];
                curBlock = parsingResult[seeingTokenIndex];
                seeingParsingUnit = (prevBlock == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevBlock.Units.Last().AfterStack);
                seeingParsingUnit.InputValue = tokenData;
                seeingParsingUnit.SetRecoveryMessage(string.Format("({0}, {1})", Resource.RecoverWithPanicMode, Resource.TryAdjustStackWithThisToken));
                curBlock.units.Add(seeingParsingUnit);

                // find a matrix index that can process a modified token.
                var stackToProcess = new Stack<object>(seeingParsingUnit.BeforeStack.Reverse());
                if (AdjustStack(parsingTable, stackToProcess, targetTerminal))
                {
                    seeingParsingUnit.AfterStack = stackToProcess;
                    snippet.BlockParsing(parsingResult[seeingTokenIndex], true);
                    parsingResult[seeingTokenIndex].Units.Last().SetRecoveryMessage(string.Format("({0})", Resource.RecoverySuccessed));
                }
                else return result;

                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            catch(Exception ex)
            {
                if(curBlock != null)
                {
                    var newUnit = new ParsingUnit(seeingParsingUnit.AfterStack);
                    newUnit.InputValue = seeingParsingUnit.InputValue;
                    newUnit.SetRecoveryMessage(string.Format("({0})", Resource.RecoveryFailed));
                    curBlock.units.Add(newUnit);
                }

                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, false);
            }
        }
    }
}
