using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    public abstract class ParsingResult
    {
        public bool Success { get; private set; }
        public ParsingHistory ParsingHistory { get; private set; }
        public Stack<AstSymbol> MeaningStack { get; private set; } = new Stack<AstSymbol>();
        public IReadOnlyList<AstSymbol> ParseTree => this.MeaningStack.Reverse().ToList();

        private List<ParsingFailResult> failedList = new List<ParsingFailResult>();
        public IReadOnlyList<ParsingFailResult> FailedList => failedList;

        protected ParsingResult(bool success)
        {
            Success = success;
        }

        internal void AddFailedList(ParsingFailResult data)
        {
            this.Success = false;

            this.failedList.Add(data);
        }

        public void SetData(ParsingHistory parsingHistory, Stack<AstSymbol> meaningStack)
        {
            this.ParsingHistory = parsingHistory;
            this.MeaningStack = meaningStack;
        }

        public void SetSuccess()
        {
            this.Success = true;
            this.failedList.Clear();
        }
    }
}
