using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System.Data;

namespace Parse.FrontEnd.Parsers.Logical
{
    public class LLParserSnippet : ParserSnippet
    {

        protected override void CreateParsingHistoryTemplate()
        {
            this.ParsingHistory.AddColumn("stack");
            this.ParsingHistory.AddColumn("input");
            this.ParsingHistory.AddColumn("action");
            this.ParsingHistory.AddColumn("target", typeof(NonTerminalSingle));
        }

        public override string ToParsingTreeString()
        {
            string result = string.Empty;

            ushort depth = 1;
            foreach (DataRow item in this.ParsingHistory.Rows)
            {
                if (item[2].ToString() != "expand") continue;

                result += (item[3] as NonTerminalSingle).ToTreeString(depth++);
            }

            return result;
        }

        public override bool Parsing(TokenCell[] tokenCells)
        {
            throw new System.NotImplementedException();
        }

        public LLParserSnippet(Parser parser) : base(parser)
        {
        }
    }
}
