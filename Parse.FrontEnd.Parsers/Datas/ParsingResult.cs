using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    public abstract class ParsingResult
    {
        public bool Success { get; }
        public ParsingHistory ParsingHistory { get; }
        public Stack<AstSymbol> meaningStack { get; } = new Stack<AstSymbol>();
        public List<AstSymbol> ParseTree => this.meaningStack.Reverse().ToList();

        protected ParsingResult(bool success, ParsingHistory parsingHistory, Stack<AstSymbol> meaningStack)
        {
            Success = success;
            ParsingHistory = parsingHistory;
            this.meaningStack = meaningStack;
        }
    }
}
