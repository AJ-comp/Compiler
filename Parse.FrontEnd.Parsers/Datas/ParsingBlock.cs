using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingBlock
    {
        internal List<ParsingUnit> units = new List<ParsingUnit>();
        internal List<ParsingErrorInfo> errorInfos = new List<ParsingErrorInfo>();

        public TokenData Token { get; } = null;
        public ParseTreeTerminal TokenTree { get; set; } = null;
        public IReadOnlyList<ParsingUnit> Units => this.units;
        public IReadOnlyList<ParsingErrorInfo> ErrorInfos => errorInfos;
        public IReadOnlyList<ParsingUnit> ErrorUnits
        {
            get
            {
                List<ParsingUnit> result = new List<ParsingUnit>();

                Parallel.ForEach(this.units, (unit) => 
                {
                    if (unit.IsError)
                    {
                        lock(result) result.Add(unit);
                    }
                });

                return result;
            }
        }
        public TerminalSet PossibleTerminalSet => (this.units.Count == 0) ? new TerminalSet() : this.units.Last().PossibleTerminalSet;

        public ParsingBlock(TokenData token)
        {
            this.Token = token;
        }

        /// <summary>
        /// This constructor creates instance has parsingUnit and token.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="token"></param>
        public ParsingBlock(ParsingUnit parsingUnit, TokenData token) : this(token)
        {
            this.units.Add(parsingUnit);
        }

        public ParsingBlock(IEnumerable<ParsingUnit> parsingUnits, TokenData token) : this(token)
        {
            this.units = new List<ParsingUnit>(parsingUnits);
        }

        /// <summary>
        /// This function returns the value after adding a new parsing item on the current parsing block.
        /// </summary>
        public ParsingUnit AddParsingItem(TokenData token = null)
        {
            if (this.units.Count == 0) this.units.Add(ParsingUnit.FirstParsingUnit);
            else this.units.Add(new ParsingUnit(this.Units.Last().AfterStack));
            this.units.Last().InputValue = token;

            return this.units.Last();
        }

        /// <summary>
        /// This function creates ParsingBlock that has ParsingUnit that next to prevParsingUnit.
        /// </summary>
        /// <param name="prevParsingUnit"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ParsingBlock CreateNextParsingBlock(ParsingUnit prevParsingUnit, TokenData token)
        {
            var result = new ParsingBlock(token);

            ParsingUnit unitToAdd = (prevParsingUnit == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevParsingUnit.AfterStack);
            result.units.Add(unitToAdd);

            return result;
        }

        public override string ToString() => string.Format("TokenCell : {0}, Unit count : {1}", this.Token, this.units.Count);
    }
}
