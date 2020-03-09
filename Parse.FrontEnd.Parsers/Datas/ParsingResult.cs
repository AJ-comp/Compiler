using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Datas
{
    public abstract class ParsingResult
    {
        private List<ParsingFailResult> failedList = new List<ParsingFailResult>();

        public bool Success { get; private set; }
        public ParsingHistory ParsingHistory { get; private set; }
        public Stack<TreeSymbol> MeaningStack { get; internal set; } = new Stack<TreeSymbol>();

        public IReadOnlyList<TreeSymbol> ParseTree => this.MeaningStack.Reverse().ToList();
        public IReadOnlyList<ParsingFailResult> FailedList => failedList;
        public ParsingStack ParsingStack { get; } = new ParsingStack();


        protected ParsingResult(bool success)
        {
            Success = success;
        }

        internal void AddFailedList(ParsingFailResult data)
        {
            this.Success = false;

            this.failedList.Add(data);
        }

        public void SetSuccess()
        {
            this.Success = true;
            this.failedList.Clear();
        }

        /// <summary> Get the parsing tree with string format. </summary>
        /// <returns>tree string</returns>
        public abstract string ToParsingTreeString();
    }
}
