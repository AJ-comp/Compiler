using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ParsingBlock
    {
        #region This has to be capsule later
        public List<ParsingUnit> _units = new List<ParsingUnit>();
        public List<ParsingErrorInfo> _errorInfos = new List<ParsingErrorInfo>();
        #endregion

        public TokenData Token { get; } = null;
        public IReadOnlyList<ParsingUnit> Units => this._units;
        public IReadOnlyList<ParsingErrorInfo> ErrorInfos => _errorInfos;
        public IEnumerable<ParsingUnit> ErrorUnits
        {
            get
            {
                List<ParsingUnit> result = new List<ParsingUnit>();

                Parallel.ForEach(this._units, (unit) => 
                {
                    if (unit.IsError)
                    {
                        lock(result) result.Add(unit);
                    }
                });

                return result;
            }
        }
        public TerminalSet PossibleTerminalSet => (this._units.Count == 0) ? new TerminalSet() : this._units.Last().PossibleTerminalSet;

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
            this._units.Add(parsingUnit);
        }

        public ParsingBlock(IEnumerable<ParsingUnit> parsingUnits, TokenData token) : this(token)
        {
            this._units = new List<ParsingUnit>(parsingUnits);
        }

        public void AddItem()
        {
            var lastUnit = Units.Last();
            var newUnit = (lastUnit == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(lastUnit.AfterStack);

            _units.Add(newUnit);
        }

        public void AddItem(ParsingStackUnit initStack)
        {
            var newUnit = (initStack == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(initStack);

            _units.Add(newUnit);
        }


        public void Clear()
        {
            _units.Clear();
            _errorInfos.Clear();
        }


        private string DebuggerDisplay
        {
            get
            {
                var result = string.Format("TokenCell : {0}, Unit count : {1}", this.Token, this._units.Count);
                if (ErrorInfos.Count() > 0) result += string.Format(" Fired Error");

                return result;
            }
        }
    }
}
