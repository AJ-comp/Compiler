using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Logical
{
    public abstract class ParserSnippet
    {
        /// <summary>
        /// Critical Section.
        /// </summary>
        public Parser Parser { get; }

        /// <summary> Get the parsing history with data table format.</summary>
        public ParsingHistory ParsingHistory { get; } = new ParsingHistory();

        /// <summary> Get the parse tree </summary>
        public IReadOnlyList<TreeSymbol> ParseTree => this.AllStack.Reverse().ToList();
        public IReadOnlyList<TreeSymbol> Ast => this.MeaningStack.Reverse().ToList();

        public Stack<TreeSymbol> AllStack { get; } = new Stack<TreeSymbol>();
        public Stack<TreeSymbol> MeaningStack { get; } = new Stack<TreeSymbol>();
        public Queue<TreeSymbol> MeaningQueue { get; } = new Queue<TreeSymbol>();

        protected ParserSnippet(Parser parser)
        {
            this.Parser = parser;

            this.CreateParsingHistoryTemplate();
        }

        protected abstract void CreateParsingHistoryTemplate();

        /// <summary> Get the parsing tree with string format. </summary>
        /// <returns>tree string</returns>
        public abstract string ToParsingTreeString();

        /// <summary>
        /// Start parsing with tokenCells.
        /// </summary>
        /// <param name="tokenCells"></param>
        /// <returns>Returns true if successed.</returns>
        public abstract ParsingResult Parsing(TokenCell[] tokenCells);
    }
}
