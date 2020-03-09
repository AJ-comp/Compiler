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

        protected ParserSnippet(Parser parser)
        {
            this.Parser = parser;
        }

        /// <summary>
        /// Start parsing with tokenCells.
        /// </summary>
        /// <param name="tokenCells"></param>
        /// <returns>Returns true if successed.</returns>
        public abstract ParsingResult Parsing(TokenCell[] tokenCells);
    }
}
