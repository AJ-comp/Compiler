﻿using Parse.FrontEnd.Ast;
using Parse.Extensions;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Parse.FrontEnd.Parsers.Datas.LRParsingData;

namespace Parse.FrontEnd.Parsers.LR
{
    public class SLRParser : LRParser
    {
        private string recentCode = string.Empty;
        private ParsingRule parsingRule = new ParsingRule();
        private ParsingHistory parsingHistory = new ParsingHistory();
        private FollowAnalyzer followAnalyzer = new FollowAnalyzer();
        private Stack<AstSymbol> meaningStack = new Stack<AstSymbol>();
        private TerminalSet recentPossibleSet = new TerminalSet();

        public override TerminalSet PossibleTerminalSet => this.recentPossibleSet;
        public override DataTable ParsingTable => this.parsingRule.ToDataTable();
        public override DataTable ParsingHistory
        {
            get
            {
                /*
                this.parsingRule.ActionCompleted -= this.AddParsingHistory;
                this.Parse(this.recentCode);
                this.parsingRule.ActionCompleted += this.BuildParseTree;
                */

                return this.parsingHistory;
            }
        }
        public override string AnalysisResult => this.C0.ToString();
        public override List<AstSymbol> ParseTree => this.meaningStack.Reverse().ToList();
        public override CanonicalTable C0 { get; } = new CanonicalTable();

        public SLRParser(Grammar grammar) : base(grammar)
        {
            this.CreateParsingHistoryTemplate();
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            this.C0.Calculate(virtualStartSymbol);
            this.followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            this.parsingRule.Calculate(this.C0, this.followAnalyzer.Datas);

            //            this.parsingRule.ActionCompleted += this.BuildParseTree;
            this.parsingRule.ActionCompleted += AddParsingHistory;
            this.parsingRule.ActionFailed += this.AddFailedInfoToParsingHistory;
        }

        private void CreateParsingHistoryTemplate()
        {
            this.parsingHistory.AddColumn("prev stack");
            this.parsingHistory.AddColumn("input symbol");
            this.parsingHistory.AddColumn("action information");
            this.parsingHistory.AddColumn("current stack");
        }

        private void BuildParseTree(object sender, LRParsingEventArgs args)
        {
            if(args.ActionData.ActionDirection == ActionDir.shift)
            {
//                if (!args.InputValue.Kind.Meaning) return;

                this.meaningStack.Push(new AstTerminal(args.InputValue));
            }
            else if(args.ActionData.ActionDirection == ActionDir.reduce)
            {
                var item = args.ActionData.ActionDest as NonTerminalSingle;

                AstNonTerminal nonTerminal = new AstNonTerminal(item);
                for(int i=0; i<item.Count; i++) nonTerminal.Insert(0, this.meaningStack.Pop());
                this.meaningStack.Push(nonTerminal);

                item.MeaningUnit?.ActionLogic(nonTerminal);
            }
            else if(args.ActionData.ActionDirection == ActionDir.epsilon_reduce)
            {
                var item = args.ActionData.ActionDest as NonTerminalSingle;
                AstNonTerminal nonTerminal = new AstNonTerminal(item);

                this.meaningStack.Push(nonTerminal);

                item.MeaningUnit?.ActionLogic(nonTerminal);
            }
        }

        /// <summary>
        /// This function writes parsing process information to the parsingHistory.
        /// </summary>
        /// <param name="args">parsing process information</param>
        private void AddParsingHistory(object sender, LRParsingEventArgs args)
        {
            this.BuildParseTree(sender, args);

            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionData.ActionDirection.ToString() + " ";
            var param4 = Convert.ToString(args.CurrentStack.Reverse(), " ");

            if (args.ActionData.ActionDirection != ActionDir.accept)
                param3 += (args.ActionData.ActionDest is NonTerminalSingle) ? (args.ActionData.ActionDest as NonTerminalSingle).ToGrammarString() : args.ActionData.ActionDest.ToString();

            if (args.ActionData.ActionDirection == ActionDir.reduce)
                this.parsingHistory.AddTreeInfo(args.ActionData.ActionDest as NonTerminalSingle);

            this.parsingHistory.AddRow(param1, param2, param3, param4);
        }

        /// <summary>
        /// This function writes parsing error information to the parsingHistory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">parsing process information when error generated</param>
        private void AddFailedInfoToParsingHistory(object sender, ParsingFailedEventArgs args)
        {
            this.recentPossibleSet = args.PossibleSet;

            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionData.ActionDirection.ToString() + " ";

            string message = Resource.CantShift + " " + args.PossibleSet + " " + Resource.MustCome;
            this.parsingHistory.AddRow(param1, param2, message, string.Empty);

            this.OnParsingFailed(args);
        }

        private void ParsingInit()
        {
            this.parsingRule.ParsingInit();
            this.parsingHistory.Clear();
            this.meaningStack.Clear();
        }

        public override bool Parse(string data)
        {
            bool result = false;
            this.recentCode = data;
            this.Lexer.SetCode(data);
            this.ParsingInit();

            while (true)
            {
                var token = this.Lexer.NextToken;
                if (token.Kind == new NotDefined())
                {
                }
                ActionDir parsingResult = this.parsingRule.Parsing(token);
                if(parsingResult == ActionDir.failed)
                {
                    result = false;
                    break;
                }
                else if(parsingResult == ActionDir.accept)
                {
                    result = true;
                    break;
                }
                else if(parsingResult == ActionDir.reduce || parsingResult == ActionDir.epsilon_reduce || parsingResult == ActionDir.moveto)
                {
                    this.Lexer.RollBackTokenReadIndex();
                }
            }

            return result;
        }

        public override bool Parse(string[] tokens)
        {
            bool result = false;

            foreach(var t in tokens)
            {
                var token = this.Lexer.GetTokenInfo(t);
                if (token.Kind == new NotDefined())
                {
                }
                ActionDir parsingResult = this.parsingRule.Parsing(token);
                if (parsingResult == ActionDir.failed)
                {
                    result = false;
                    break;
                }
                else if (parsingResult == ActionDir.accept)
                {
                    result = true;
                    break;
                }
                else if (parsingResult == ActionDir.reduce || parsingResult == ActionDir.epsilon_reduce || parsingResult == ActionDir.moveto)
                {
                    this.Lexer.RollBackTokenReadIndex();
                }
            }

            return result;
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            foreach(var item in this.parsingHistory.TreeInfo.ToReverseList())
                result += item.ToTreeString();

            return result;
        }
    }
}
