using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public class ErrorHandlingResult
    {
        public Stack<object> Stack { get; }
        public int SeeingTokenIndex { get; }

        public ErrorHandlingResult(Stack<object> stack, int seeingTokenIndex)
        {
            Stack = stack;
            SeeingTokenIndex = seeingTokenIndex;
        }
    }
}
