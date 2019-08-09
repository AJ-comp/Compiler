using Parse.Ast;
using Parse.Extensions;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using Parse.RegularGrammar;
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
        private Queue<TokenData> meaningTerminals = new Queue<TokenData>();

        public override TerminalSet PossibleTerminalSet => new TerminalSet();
        public override DataTable ParsingTable => this.parsingRule.ToDataTable();
        public override DataTable ParsingHistory
        {
            get
            {
                this.parsingRule.ActionCompleted = this.AddParsingHistory;
                this.Parse(this.recentCode);
                this.parsingRule.ActionCompleted = this.BuildAST;

                return this.parsingHistory;
            }
        }
        public override string AnalysisResult => this.C0.ToString();
        public override AstNonTerminal AstRoot { get; protected set; } = null;
        public override CanonicalTable C0 { get; } = new CanonicalTable();

        public SLRParser(Grammar grammar) : base(grammar)
        {
            this.CreateParsingHistoryTemplate();
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            this.C0.Calculate(virtualStartSymbol);
            this.followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            this.parsingRule.Calculate(this.C0, this.followAnalyzer.Datas);

            this.parsingRule.ActionCompleted = this.BuildAST;
            this.parsingRule.ActionFailed = this.AddFailedInfoToParsingHistory;
        }

        private void CreateParsingHistoryTemplate()
        {
            this.parsingHistory.AddColumn("prev stack");
            this.parsingHistory.AddColumn("input symbol");
            this.parsingHistory.AddColumn("action information");
            this.parsingHistory.AddColumn("current stack");
        }

        private void BuildAST(LRParsingEventArgs args)
        {
            if(args.ActionDir == ActionInfo.shift)
            {
                if (!args.InputValue.Kind.Meaning) return;

                this.meaningTerminals.Enqueue(args.InputValue);
            }
            else if(args.ActionDir == ActionInfo.reduce)
            {
                var item = args.ActionDest as NonTerminalSingle;
                if (item.MeaningUnit == Logic.MeaningUnit.Empty) return;

                AstNonTerminal nonTerminal = new AstNonTerminal(item.MeaningUnit);
                if (this.AstRoot != null) nonTerminal.Add(this.AstRoot);
                while (this.meaningTerminals.Count > 0)  nonTerminal.Add(new AstTerminal(this.meaningTerminals.Dequeue()));

                this.AstRoot = nonTerminal;
            }
        }

        /// <summary>
        /// This function writes parsing process information to the parsingHistory.
        /// </summary>
        /// <param name="args">parsing process information</param>
        private void AddParsingHistory(LRParsingEventArgs args)
        {
            this.BuildAST(args);

            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionDir.ToString() + " ";
            var param4 = Convert.ToString(args.CurrentStack.Reverse(), " ");

            if (args.ActionDir != ActionInfo.accept)
                param3 += (args.ActionDest is NonTerminalSingle) ? (args.ActionDest as NonTerminalSingle).ToGrammarString() : args.ActionDest.ToString();

            if (args.ActionDir == ActionInfo.reduce)
                this.parsingHistory.AddTreeInfo(args.ActionDest as NonTerminalSingle);

            this.parsingHistory.AddRow(param1, param2, param3, param4);
        }

        /// <summary>
        /// This function writes parsing error information to the parsingHistory.
        /// </summary>
        /// <param name="args">parsing process information when error generated</param>
        /// <param name="possibleSet">incorrect terminal set</param>
        private void AddFailedInfoToParsingHistory(LRParsingEventArgs args, TerminalSet possibleSet)
        {
            var param1 = Convert.ToString(args.PrevStack.Reverse(), " ");
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionDir.ToString() + " ";

            string message = Resource.CantShift + " " + possibleSet + " " + Resource.MustCome;
            this.parsingHistory.AddRow(param1, param2, message, string.Empty);
        }

        private void ParsingInit()
        {
            this.parsingRule.ParsingInit();
            this.parsingHistory.Clear();
            this.AstRoot = null;
            this.meaningTerminals.Clear();
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
                ActionInfo parsingResult = this.parsingRule.Parsing(token);
                if(parsingResult == ActionInfo.failed)
                {
                    result = false;
                    break;
                }
                else if(parsingResult == ActionInfo.accept)
                {
                    result = true;
                    break;
                }
                else if(parsingResult == ActionInfo.reduce || parsingResult == ActionInfo.moveto)
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
