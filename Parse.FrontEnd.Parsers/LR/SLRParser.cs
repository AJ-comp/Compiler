using Parse.Extensions;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.LR
{
    public class SLRParser : LRParser
    {
        private FollowAnalyzer followAnalyzer = new FollowAnalyzer();

        public override IParsingTable ParsingTable { get; } = new LRParsingTable();
        public override string AnalysisResult => this.C0.ToString();
        public override CanonicalTable C0 { get; } = new CanonicalTable();

        public SLRParser(Grammar grammar) : base(grammar)
        {
            DataTableExtensionMethods.ASCIIBorder();

            var virtualStartSymbol = this.Grammar.CreateVirtualSymbolForLRParsing("Accept");
            this.C0.Calculate(virtualStartSymbol);
            this.followAnalyzer.CalculateAllFollow(this.Grammar.NonTerminalMultiples);
            this.ParsingTable.CreateParsingTable(this.C0, this.followAnalyzer.Datas);
        }

        public override ParserSnippet NewParserSnippet()
        {
            ParserSnippet result = new LRParserSnippet(this);

            return result;
        }
    }
}
