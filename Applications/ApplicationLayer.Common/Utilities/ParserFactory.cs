using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Common.Utilities
{
    public class ParserFactory
    {
        private static ParserFactory instance = null;
        public static ParserFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new ParserFactory();

                return instance;
            }
        }

        public enum ParserKind { LL_Parser, SLR_Parser, LALR_Parser, GLR_Parser };

        private Dictionary<Tuple<ParserKind, Grammar>, Parser> parserCache = new Dictionary<Tuple<ParserKind, Grammar>, Parser>();

        /// <summary>
        /// This function returns a parser that parameters matched.
        /// </summary>
        /// <param name="parserKind">The parser kind to get</param>
        /// <param name="grammar">The grammar allocated in the parser</param>
        /// <returns>If it doesn't exist return null. If it exists return parser instance.</returns>
        public Parser GetParser(ParserKind parserKind, Grammar grammar)
        {
            var key = new Tuple<ParserKind, Grammar>(parserKind, grammar);
            return (this.parserCache.ContainsKey(key)) ? this.parserCache[key] : null;
        }

        /// <summary>
        /// This function registers a parser with parameters.
        /// </summary>
        /// <param name="parserKind">The parser kind to get</param>
        /// <param name="grammar">The grammar to allocate into the parser</param>
        public void RegisterParser(ParserKind parserKind, Grammar grammar)
        {
            var value = this.GetParser(parserKind, grammar);

            if (value == null)
            {
                var key = new Tuple<ParserKind, Grammar>(parserKind, grammar);

                if (parserKind == ParserKind.SLR_Parser) this.parserCache.Add(key, new SLRParser(grammar));
                else if (parserKind == ParserKind.LL_Parser) this.parserCache.Add(key, new LLParser(grammar));
            }
        }
    }
}
