using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Grammars;
using ParsingLibrary.Parsers.Collections;
using ParsingLibrary.Parsers.EventArgs;
using ParsingLibrary.Parsers.RelationAnalyzers;
using ParsingLibrary.Utilities.Extensions;
using System.Data;
using System.Linq;
using static ParsingLibrary.Parsers.Datas.LRParsingData;

namespace ParsingLibrary.Parsers
{
    public class SLRParser : Parser
    {
        private CanonicalHistory C0 = new CanonicalHistory();
        private ParsingRule parsingRule = new ParsingRule();
        private ParsingHistory parsingHistory = new ParsingHistory();
        private FollowAnalyzer followAnalyzer = new FollowAnalyzer();

        public override TerminalSet PossibleTerminalSet => new TerminalSet();
        public override DataTable ParsingTable => this.parsingRule.ToDataTable();
        public override DataTable ParsingHistory => this.parsingHistory;
        public override string AnalysisResult => this.C0.ToString();

        public SLRParser(Grammar grammar) : base(grammar)
        {
            this.CreateParsingHistoryTemplate();
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            this.C0.Calculate(virtualStartSymbol);
            this.followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            this.parsingRule.Calculate(this.C0, this.followAnalyzer.Datas);
        }

        private void CreateParsingHistoryTemplate()
        {
            this.parsingHistory.AddColumn("prev stack");
            this.parsingHistory.AddColumn("input symbol");
            this.parsingHistory.AddColumn("action information");
            this.parsingHistory.AddColumn("current stack");
        }

        private void AddParsingHistory(LRParsingEventArgs args)
        {
            var param1 = args.PrevStack.Reverse().ToElementString();
            var param2 = args.InputValue.ToString();
            var param3 = args.ActionDir.ToString() + " ";
            var param4 = args.CurrentStack.Reverse().ToElementString();

            if (args.ActionDir != ActionInfo.accept)
                param3 += (args.ActionDest is NonTerminalSingle) ? (args.ActionDest as NonTerminalSingle).ToGrammarString() : args.ActionDest.ToString();

            this.parsingHistory.AddRow(param1, param2, param3, param4);
        }

        public override bool Parse(string data)
        {
            bool result = false;
            this.Lexer.SetCode(data);
            this.parsingRule.ParsingInit();
            this.parsingRule.ActionCompleted = this.AddParsingHistory;

            while (true)
            {
                var token = this.Lexer.NextToken;
                if (token.Kind == new NotDefined())
                {
                }
                ActionInfo pasingResult = this.parsingRule.Parsing(token);
                if(pasingResult == ActionInfo.failed)
                {
                    result = false;
                    break;
                }
                else if(pasingResult == ActionInfo.accept)
                {
                    result = true;
                    break;
                }
                else if(pasingResult == ActionInfo.reduce || pasingResult == ActionInfo.moveto)
                {
                    this.Lexer.RollBackTokenReadIndex();
                }
            }

            return result;
        }

        public override string ToParsingTreeString()
        {
            return string.Empty;
        }
    }
}
