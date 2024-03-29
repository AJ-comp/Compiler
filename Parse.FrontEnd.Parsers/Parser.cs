﻿using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public abstract class Parser
    {
        protected SymbolTableStack SymbolTableStack { get; } = new SymbolTableStack();

        /// <summary>
        /// This event handler is called when the input token is not in the expected terminal set. <br/>
        /// Terminal : Terminal that expected. <br/>
        /// TokenData : Input token <br/>
        /// </summary>
        public Action<Terminal, TokenData> MatchFailed { get; set; } = null;
        /// <summary> 
        /// This event handler is called when can not expand from the current state when the input token enter. <br/>
        /// NonTerminal : Current state <br/>
        /// TokenData : Input token <br/>
        /// </summary>
        public Action<NonTerminal, TokenData> ExpandFailed { get; set; } = null;

        public Grammar Grammar { get; } = null;
        public string OptimizeList => Optimizer.ChangeableNodeData(this.Grammar.NonTerminalMultiples).ToString();

        public List<string> DelimiterList
        {
            get
            {
                List<string> result = new List<string>();

                foreach(var item in this.Grammar.DelimiterDic)  result.Add(item.Key);

                return result;
            }
        }

        /// <summary> Get the analysis result with string format. </summary>
        public abstract string AnalysisResult { get; }
        /// <summary> Get the parsing table with data table format. </summary>
        public abstract IParsingTable ParsingTable { get; }
        public abstract event EventHandler<ParseCreatedArgs> ParseTreeCreated;
        public abstract event EventHandler<AstSymbol> ASTCreated;

        public abstract FirstAndFollowCollection GetFirstAndFollow();

        /// <summary>
        /// This function performs whole parsing for tokenCells
        /// </summary>
        /// <param name="tokens">The tokenCells to parsing</param>
        /// <returns>The parsing result</returns>
        public abstract ParsingResult Parsing(IReadOnlyList<TokenCell> tokens);
        public abstract ParsingResult Parsing(LexingData lexingData, ParsingResult prevParsingInfo);


        protected Parser(Grammar grammar, bool bLogging = false)
        {
            this.Grammar = grammar;
            _logging = bLogging;
        }

        protected bool _logging = false;
    }
}
