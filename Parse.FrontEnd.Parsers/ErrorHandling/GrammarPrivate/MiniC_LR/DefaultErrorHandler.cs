﻿using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate.MiniC_LR
{
    public class DefaultErrorHandler : GrammarPrivateErrorHandler
    {
        public static LRParsingTable ParsingTable { get; private set; }

        public DefaultErrorHandler(Grammar grammar, LRParsingTable parsingTable) : base(grammar)
        {
            ParsingTable = parsingTable;
        }

        public override ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            return DefaultErrorHandler.Process(this.grammar as MiniCGrammar, snippet, parsingResult, seeingTokenIndex);
        }

        public static ErrorHandlingResult Process(MiniCGrammar grammar, ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex)
        {
            var synchronizeTokens = new HashSet<Terminal>
            {
                grammar.SemiColon,
                grammar.CloseCurlyBrace,
                new EndMarker()
            };

            return PanicMode.LRProcess(snippet as LRParserSnippet, ParsingTable, parsingResult, seeingTokenIndex, synchronizeTokens);
        }
    }
}