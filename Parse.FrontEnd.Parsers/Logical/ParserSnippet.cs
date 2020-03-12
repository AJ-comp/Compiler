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
        /// This function performs whole parsing for tokenCells
        /// </summary>
        /// <param name="tokenCells">The tokenCells to parsing</param>
        /// <param name="bFullSource">If this param is true checks the end marker.</param>
        /// <returns>The parsing result</returns>
        public abstract ParsingResult Parsing(TokenCell[] tokenCells);
    }
}
