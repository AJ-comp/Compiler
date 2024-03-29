﻿using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using ParsingLibrary.Parsers.RelationAnalyzers;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers
{
    public class LLParser : Parser
    {
        private RelationAnalyzer relationAnalyzer = new RelationAnalyzer();
        private ParsingHistory parsingHistory = new ParsingHistory();
        private Stack<Symbol> stack = new Stack<Symbol>();

        public override event EventHandler<ParseCreatedArgs> ParseTreeCreated;
        public override event EventHandler<AstSymbol> ASTCreated;

        public override FirstAndFollowCollection GetFirstAndFollow()
        {
            throw new Exception();
        }

        public string ShowStack => string.Join("", this.stack);
        public override IParsingTable ParsingTable => null;

        public TerminalSet PossibleTerminalSet
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
                    result += string.Format("First({0}) = {1}", symbol.Name, relationAnalyzer.FFAnalyzer.First(symbol).ToString()) + Environment.NewLine;

                result += Environment.NewLine;

                foreach (var symbol in this.Grammar.NonTerminalMultiples)
                    result += string.Format("Follow({0}) = {1}", symbol.Name, relationAnalyzer.FFAnalyzer.Datas[symbol]) + Environment.NewLine;

                return result;
            }
        }

        public LLParser(Grammar grammar) : base(grammar)
        {
            DataTableExtensionMethods.ASCIIBorder();
            this.SetGrammar(grammar);
        }

        /// <summary>
        /// It is a gateway that calls the correct inner method according to curStatus type.
        /// </summary>
        /// <param name="curStatus"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool Expand(Symbol curStatus, TokenData input)
        {
            bool result;

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
            foreach (var symbol in this.relationAnalyzer.ParsingDic[input.Kind, curStatus].ToReverse())
            {
                if (symbol != new Epsilon()) this.stack.Push(symbol);
            }

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
/*
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

                if (this.Expand(topSymbol, token)) this.Lexer.RollBackTokenReadIndex();
                else
                {
                    this.parsingHistory.AddRow(this.stack.ToElementString(), token.ToString(), "error");
                    break;
                }
            }

            return result;
        }
        */

        public override ParsingResult Parsing(IReadOnlyList<TokenCell> tokens)
        {
            throw new NotImplementedException();
        }

        public override ParsingResult Parsing(LexingData lexingData, ParsingResult prevParsingInfo)
        {
            throw new NotImplementedException();
        }
    }
}
