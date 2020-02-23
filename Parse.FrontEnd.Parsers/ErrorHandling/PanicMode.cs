using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public class PanicMode
    {
        /// <summary>
        /// This function adjusts a stack to process the syncronizeTokens.
        /// </summary>
        /// <param name="parsingTable">The parsing table to refer</param>
        /// <param name="processStack">The stack to adjust</param>
        /// <param name="syncronizeTokens">The token set to process</param>
        /// <returns>Returns index if successed. Returns -1 if failed.</returns>
        private static int AdjustStack(LRParsingTable parsingTable, Stack<object> processStack, HashSet<Terminal> syncronizeTokens)
        {
            int result = -1;
            // adjust stack
            while (true)
            {
                var topData = processStack.Peek();
                if (topData is NonTerminalSingle)
                {
                    processStack.Pop();
                    continue;
                }

                result = (int)topData;
                var ixMetrix = parsingTable[result];

                var count = ixMetrix.PossibleTerminalSet.Count;
                var exceptCount = ixMetrix.PossibleTerminalSet.Except(syncronizeTokens).Count();

                // if any item of the syncronizeTokens exists in the PossibleTerminalSet then break; (completed stack adjust for syncronizeTokens)
                if (exceptCount != count) break;
            }

            return result;
        }

        /// <summary>
        /// This function is an error handler for LR parsing.
        /// </summary>
        /// <param name="parsingTable"></param>
        /// <param name="stack"></param>
        /// <param name="tokens"></param>
        /// <param name="seeingTokenIndex"></param>
        /// <param name="syncronizeTokens">The token set to process</param>
        /// <returns></returns>
        public static ErrorHandlingResult LRProcess(LRParsingTable parsingTable, Stack<object> stack, TokenCell[] tokens, int seeingTokenIndex, HashSet<Terminal> syncronizeTokens)
        {
            ErrorHandlingResult result = new ErrorHandlingResult(stack, seeingTokenIndex);

            if (syncronizeTokens.Count == 0) return result;

            var processStack = new Stack<object>(stack);
            var curStatus = AdjustStack(parsingTable, processStack, syncronizeTokens);
            if (curStatus < 0) return result;

            while(seeingTokenIndex < tokens.Length)
            {
                var ixMetrix = parsingTable[curStatus];
                TokenCell curToken = tokens[seeingTokenIndex++];

                if (ixMetrix.PossibleTerminalSet.Contains(curToken.Data)) break;
            }

            return new ErrorHandlingResult(processStack, seeingTokenIndex);
        }
    }
}
