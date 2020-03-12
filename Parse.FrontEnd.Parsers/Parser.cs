using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public abstract class Parser
    {
        protected SymbolTableStack SymbolTableStack { get; } = new SymbolTableStack();

        /// <summary>
        /// This event handler is called when the input token is not in the expected terminal set.
        /// Terminal : Terminal that expected.
        /// TokenData : Input token
        /// </summary>
        public Action<Terminal, TokenData> MatchFailed { get; set; } = null;
        /// <summary> 
        /// This event handler is called when can not expand from the current state when the input token enter. 
        /// NonTerminal : Current state
        /// TokenData : Input token
        /// </summary>
        public Action<NonTerminal, TokenData> ExpandFailed { get; set; } = null;

        public Grammar Grammar { get; } = null;
        public string OptimizeList => Optimizer.ChangeableNodeData(this.Grammar.NonTerminalMultiples).ToString();

        public List<String> DelimiterList
        {
            get
            {
                List<String> result = new List<string>();

                foreach(var item in this.Grammar.DelimiterDic)  result.Add(item.Key);

                return result;
            }
        }

        /// <summary> Get the analysis result with string format. </summary>
        public abstract string AnalysisResult { get; }
        /// <summary> Get the parsing table with data table format. </summary>
        public abstract IParsingTable ParsingTable { get; }


        protected Parser(Grammar grammar)
        {
            this.Grammar = grammar;
        }

        /// <summary>
        /// This function returns a Terminal by converting a TokenCell after filtering.
        /// </summary>
        /// <param name="tokenCell">The tokenCell to convert</param>
        /// <param name="bEndIndex">This param means whether tokenCell is the last token.</param>
        /// <returns>Returns Terminal type converted if tokenCell has to be parsed, else Returns null.</returns>
        public static Terminal GetTargetTerminalFromTokenCell(TokenCell tokenCell, bool bEndIndex)
        {
            Terminal result = new Epsilon();
            if (tokenCell.Data == new EndMarker().Value && bEndIndex) result = new EndMarker();
            else
            {
                var typeData = tokenCell.PatternInfo.OptionData as Terminal;
                if (typeData == null) result = new NotDefined();
                else if (typeData.TokenType == TokenType.Delimiter || typeData.TokenType == TokenType.Comment) result = null;
                else result = typeData;
            }

            return result;
        }

        /// <summary>
        /// This function creates a new snippet for parsing.
        /// </summary>
        /// <returns>The parser snippet that created.</returns>
        public abstract ParserSnippet NewParserSnippet();
    }
}
