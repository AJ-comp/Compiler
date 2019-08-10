using Parse.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Data;

namespace Parse.FrontEnd.Parsers
{
    public abstract class Parser
    {
        protected Lexer Lexer { get; } = null;
        protected Grammar Grammar { get; } = null;
        protected SymbolTableStack SymbolTableStack { get; } = new SymbolTableStack();

        /// <summary> this event handler called when the input terminal does not exists in the expected terminal set. </summary>
        public Action<Terminal, TokenData> MatchFailed { get; set; } = null;
        /// <summary> this event handler called when can not expand from the current status when the input terminal enter. </summary>
        public Action<NonTerminal, TokenData> ExpandFailed { get; set; } = null;

        public string OptimizeList => Optimizer.ChangeableNodeData(this.Grammar.NonTerminalMultiples).ToString();

        public string RegularGrammar
        {
            get
            {
                string result = string.Empty;

                foreach (var symbol in this.Grammar.NonTerminalMultiples) result += symbol.ToGrammarString() + Environment.NewLine;

                return result;
            }
        }

        /// <summary> Get the permissible terminal set from current status </summary>
        public abstract TerminalSet PossibleTerminalSet { get; }
        /// <summary> Get the analysis result with string format. </summary>
        public abstract string AnalysisResult { get; }
        /// <summary> Get the parsing table with data table format. </summary>
        public abstract DataTable ParsingTable { get; }
        /// <summary> Get the parsing history with data table format.</summary>
        public abstract DataTable ParsingHistory { get; }
        public abstract AstNonTerminal AstRoot { get; protected set; }


        public Parser(Grammar grammar)
        {
            this.Grammar = grammar;
            this.Lexer = new Lexer(this.Grammar);
        }

        /// <summary>
        /// Start parsing about parameter.
        /// </summary>
        /// <param name="data"></param>
        /// <returns> Return true if succeed. </returns>
        public abstract bool Parse(string data);

        /// <summary> Get the parsing tree with string format. </summary>
        /// <returns>tree string</returns>
        public abstract string ToParsingTreeString();
    }
}
