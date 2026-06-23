using ParsingLibrary.Datas;
using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Grammars;
using ParsingLibrary.Parsers.Collections;
using ParsingLibrary.Parsers.RelationAnalyzers;
using ParsingLibrary.Utilities;
using ParsingLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data;

namespace ParsingLibrary.Parsers
{
    public class LLParser : Parser
    {
        private RelationAnalyzer relationAnalyzer = new RelationAnalyzer();
        private ParsingHistory parsingHistory = new ParsingHistory();
        private Stack<Symbol> stack = new Stack<Symbol>();

        public string ShowStack => string.Join("", this.stack);
        public override DataTable ParsingHistory => this.parsingHistory;
        public override DataTable ParsingTable => this.relationAnalyzer.ParsingDic.ToDataTable(this.Grammar.NonTerminalMultiples);

        public override TerminalSet PossibleTerminalSet
        {
            get
            {
                TerminalSet result = new TerminalSet();

                Symbol curStatus = this.stack.Peek();
                if (curStatus is Terminal) result = new TerminalSet(curStatus as Terminal);
                else result = this.relationAnalyzer.ParsingDic.PossibleTerminalSet(curStatus as NonTerminal);

                return result;
            }
        }

        public override string AnalysisResult
        {
            get
            {
                string result = string.Empty;

                foreach (var symbol in this.Grammar.NonTerminalMultiples)
                    result += string.Format("First({0}) = {1}", symbol.Name, Analyzer.FirstTerminalSet(symbol).ToString()) + Environment.NewLine;

                result += Environment.NewLine;

                foreach (var symbol in this.Grammar.NonTerminalMultiples)
                    result += string.Format("Follow({0}) = {1}", symbol.Name, relationAnalyzer.FollowAnalyzer.Datas[symbol]) + Environment.NewLine;

                return result;
            }
        }

        public LLParser(Grammar grammar) : base(grammar)
        {
            DataTableExtensionMethods.ASCIIBorder();
            this.CreateParsingTableTemplate();
            this.SetGrammar(grammar);
        }

        private void CreateParsingTableTemplate()
        {
            this.parsingHistory.AddColumn("stack");
            this.parsingHistory.AddColumn("input");
            this.parsingHistory.AddColumn("action");
            this.parsingHistory.AddColumn("target", typeof(NonTerminalSingle));
        }

        /// <summary>
        /// It is a gateway that calls the correct inner method according to curStatus type.
        /// </summary>
        /// <param name="curStatus"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool Expand(Symbol curStatus, TokenData input)
        {
            bool result = false;

            if (curStatus is Terminal)
            {
                result = this.EraseWhenMatched(curStatus as Terminal, input);
                if (!result) this.MatchFailed?.Invoke(curStatus as Terminal, input);
            }
            else
            {
                result = this.Expand(curStatus as NonTerminal, input);
                if (!result) this.ExpandFailed?.Invoke(curStatus as NonTerminal, input);
            }

            return result;
        }

        /// <summary>
        /// It deletes the symbol from the stack when equals input value (Terminal) with current status (Terminal).
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool EraseWhenMatched(Terminal curStatus, TokenData token)
        {
            if (curStatus != token.Kind) return false;

            this.parsingHistory.AddRow(this.stack.ToElementString(), token.ToString(), "pop");
            this.stack.Pop();

            return true;
        }

        /// <summary>
        /// It expands the symbol when receives input value (Terminal) at current status (NonTerminal).
        /// </summary>
        /// <param name="curStatus"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool Expand(NonTerminal curStatus, TokenData input)
        {
            if (!this.relationAnalyzer.ParsingDic.ContainsKey(input.Kind, curStatus)) return false;

            this.parsingHistory.AddRow(this.stack.ToElementString(), input.ToString(), "expand", this.relationAnalyzer.ParsingDic[input.Kind, curStatus]);
            this.stack.Pop();
            foreach (var symbol in this.relationAnalyzer.ParsingDic[input.Kind, curStatus].ToReverseList())
            {
                if (symbol != new Epsilon()) this.stack.Push(symbol);
            }

            this.Lexer.RollBackTokenReadIndex();

            return true;
        }

        public void SetGrammar(Grammar grammar)
        {
            this.relationAnalyzer.Analysis(this.Grammar.NonTerminalMultiples);

            this.stack.Clear();
            this.stack.Push(new EndMarker());
            this.stack.Push(this.Grammar.StartSymbol);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Parse(string data)
        {
            bool result = false;
            this.Lexer.SetCode(data);

            while (true)
            {
                var token = this.Lexer.NextToken;
                if (token.Kind == new NotDefined())
                {
                    this.parsingHistory.AddRow(this.stack.ToElementString(), token.ToString(), "error");
                    break;
                }

                Symbol topSymbol = this.stack.Peek();
                if (topSymbol == new EndMarker() && token.Kind == new EndMarker())
                {
                    this.parsingHistory.AddRow(this.stack.ToElementString(), token.ToString(), "accept");
                    result = true;
                    break;
                }

                if (!this.Expand(topSymbol, token))
                {
                    this.parsingHistory.AddRow(this.stack.ToElementString(), token.ToString(), "error");
                    break;
                }
            }

            return result;
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            ushort depth = 1;
            foreach (DataRow item in this.parsingHistory.Rows)
            {
                if (item[2].ToString() != "expand") continue;

                result += (item[3] as NonTerminalSingle).ToTreeString(depth++);
            }

            return result;
        }
    }
}
