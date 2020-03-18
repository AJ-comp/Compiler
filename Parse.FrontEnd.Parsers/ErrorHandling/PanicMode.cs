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

        private static int AdjustToken(ParsingResult parsingResult, int seeingTokenIndex, HashSet<Terminal> synchronizeTokens)
        {
            while (seeingTokenIndex < parsingResult.Count - 1)
            {
                //                var ixMetrix = parsingTable[curStatus];

                ParsingBlock curBlock = parsingResult[seeingTokenIndex];
                ParsingUnit curBlockLastUnit = curBlock.Units.Last();
                TokenData curToken = curBlock.Token;

                // create a new unit
                ParsingUnit newParsingUnit = new ParsingUnit(curBlockLastUnit.AfterStack);
                newParsingUnit.InputValue = curToken;
                newParsingUnit.CopyBeforeStackToAfterStack();   // stack sync because to throw away token.

                curBlockLastUnit = newParsingUnit;
                curBlockLastUnit.ChangeToFailedState(Resource.RecoverWithPanicMode);

                // check
                var targetTerminal = curToken.Kind;
                if (targetTerminal != null && synchronizeTokens.Contains(targetTerminal)) break;

                seeingTokenIndex++;

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
                ParsingUnit seeingParsingUnit = parsingResult[seeingTokenIndex].AddParsingItem();
                seeingParsingUnit.InputValue = tokenData;
                seeingParsingUnit.CopyBeforeStackToAfterStack();
                seeingParsingUnit.ChangeToFailedState(Resource.RecoverWithPanicMode);

                // find a matrix index that can process a modified token.
                var processStack = new Stack<object>(seeingParsingUnit.AfterStack.Reverse());
                if (AdjustStack(parsingTable, processStack, targetTerminal))
                {
                    seeingParsingUnit.AfterStack = processStack;
                    snippet.BlockParsing(parsingResult[seeingTokenIndex]);
                }
                else return result;

                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, true);
            }
            catch(Exception ex)
            {
                return new ErrorHandlingResult(parsingResult, seeingTokenIndex, false);
            }
        }
    }
}
