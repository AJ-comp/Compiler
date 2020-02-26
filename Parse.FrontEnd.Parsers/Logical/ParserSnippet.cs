﻿using Parse.FrontEnd.Ast;
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
        public IReadOnlyList<AstSymbol> ParseTree => this.MeaningStack.Reverse().ToList();
        public Stack<AstSymbol> MeaningStack { get; } = new Stack<AstSymbol>();

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
