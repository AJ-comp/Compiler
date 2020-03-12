using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
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

        private static int AdjustToken(ParsingResult parsingResult, TokenCell[] tokens, int seeingTokenIndex, HashSet<Terminal> synchronizeTokens)
        {
            while (seeingTokenIndex < tokens.Length - 1)
            {
                //                var ixMetrix = parsingTable[curStatus];
                TokenCell curToken = tokens[++seeingTokenIndex];

                // create a new block
                ParsingUnit lastParsingUnit = parsingResult.Last().Units.Last();
                parsingResult.Add(new ParsingBlock(new ParsingUnit(lastParsingUnit.AfterStack, lastParsingUnit.AfterStack), curToken));
                lastParsingUnit = parsingResult.Last().Units.Last();
                lastParsingUnit.ChangeToFailedState(Resource.RecoverWithPanicMode);

                // convert to terminal from tokencell
                Terminal targetTerminal = Parser.GetTargetTerminalFromTokenCell(curToken, (seeingTokenIndex >= tokens.Length - 1));
                lastParsingUnit.InputValue = new TokenData(curToken.Data, targetTerminal, curToken);

                if (targetTerminal != null && synchronizeTokens.Contains(targetTerminal)) break;

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
        public static ErrorHandlingResult LRProcess(LRParsingTable parsingTable, ParsingResult parsingResult, TokenCell[] tokens, int seeingTokenIndex, HashSet<Terminal> synchronizeTokens)
        {
            ErrorHandlingResult result = new ErrorHandlingResult(parsingResult, seeingTokenIndex);

            if (synchronizeTokens.Count == 0) return result;
            TokenCell curToken = tokens[seeingTokenIndex];
            Terminal targetTerminal = Parser.GetTargetTerminalFromTokenCell(curToken, (seeingTokenIndex >= tokens.Length - 1));
            TokenData tokenData = new TokenData(curToken.Data, targetTerminal, curToken);

            // if the terminal of the seeing token is not a synchronizeTokens kind then skip token until to see a synchronizeTokens kind.
            if (synchronizeTokens.Contains(targetTerminal) == false)
            {
                seeingTokenIndex = PanicMode.AdjustToken(parsingResult, tokens, seeingTokenIndex, synchronizeTokens);

                curToken = tokens[seeingTokenIndex];
                targetTerminal = Parser.GetTargetTerminalFromTokenCell(curToken, (seeingTokenIndex >= tokens.Length - 1));
                tokenData = new TokenData(curToken.Data, targetTerminal, curToken);
            }

            // add a new parsing unit and initialize
            parsingResult.Last().AddParsingItem();
            ParsingUnit lastParsingUnit = parsingResult.Last().Units.Last();
            lastParsingUnit.InputValue = tokenData;
            lastParsingUnit.CopyBeforeStackToAfterStack();
            lastParsingUnit.ChangeToFailedState(Resource.RecoverWithPanicMode);

            // find a matrix index that can process a modified token.
            var processStack = new Stack<object>(parsingResult.Last().Units.Last().AfterStack.Reverse());
            if (AdjustStack(parsingTable, processStack, targetTerminal)) parsingResult.Last().Units.Last().AfterStack = processStack;
            else return result;

            return new ErrorHandlingResult(parsingResult, seeingTokenIndex);
        }
    }
}
