using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

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
        public List<ParsingErrorInfo> _errorInfos { get; } = new List<ParsingErrorInfo>();
        #endregion

        public IEnumerable<ParsingUnitHistory> History => _history;

        public bool IsAllUnitConnected => FirstDisconnectedIndex < 0;

        public int FirstDisconnectedIndex
        {
            get
            {
                int result = -1;

                for (int i = 0; i < _units.Count; i++)
                {
                    if (!IsUnitConnected(i))
                    {
                        result = i;
                        break;
                    }
                }

                return result;
            }
        }

        public IReadOnlyList<TokenData> UnitTokenList
        {
            get
            {
                List<TokenData> result = new List<TokenData>();
                List<TokenCell> compareList = new List<TokenCell>();

                foreach (var unit in _units)
                {
                    if (compareList.Contains(unit.InputValue.TokenCell)) continue;

                    result.Add(unit.InputValue);
                    compareList.Add(unit.InputValue.TokenCell);
                }

                return result;
            }
        }

        public TokenData LastUnitToken => (_units.Count > 0) ? _units.Last().InputValue : null;

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
                        lock (result) result.Add(unit);
                    }
                });

                return result;
            }
        }
        public TerminalSet PossibleTerminalSet
            => (this._units.Count == 0) ? new TerminalSet() : this._units.First().PossibleTerminalSet;

        public bool IsIgnore { get; set; } = false;

        public ParsingBlock(TokenData token)
        {
            this.Token = token;
        }


        /***************************************************/
        /// <summary>
        /// This constructor creates instance has parsingUnit and token.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="token"></param>
        /***************************************************/
        public ParsingBlock(ParsingUnit parsingUnit, TokenData token) : this(token)
        {
            _units.Add(parsingUnit);
            _history.Add(new ParsingUnitHistory(parsingUnit));
        }

        public ParsingBlock(IEnumerable<ParsingUnit> parsingUnits, TokenData token) : this(token)
        {
            _units = new List<ParsingUnit>(parsingUnits);

            foreach (var unit in _units)
                _history.Add(new ParsingUnitHistory(unit));
        }

        public void AddItem(TokenData tokenData)
        {
            var lastUnit = Units.Last();
            var newUnit = (lastUnit == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(lastUnit.AfterStack);
            newUnit.InputValue = tokenData;

            _units.Add(newUnit);
            _history.Add(new ParsingUnitHistory(newUnit));
        }

        public void AddItem(ParsingStackUnit initStack)
        {
            var newUnit = (initStack == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(initStack);

            _units.Add(newUnit);
            _history.Add(new ParsingUnitHistory(newUnit));
        }

        public void AddItem(ParsingUnit parsingUnit)
        {
            _units.Add(parsingUnit);
            _history.Add(new ParsingUnitHistory(parsingUnit));
        }

        /// <summary>
        /// This function checks if unit[unitIndex] is connectd with unit[unitIndex+1]. <br/>
        /// if unitIndex is last index return always true.
        /// </summary>
        /// <param name="unitIndex"></param>
        /// <returns></returns>
        public bool IsUnitConnected(int unitIndex)
        {
            if (unitIndex < 0) return false;
            if (unitIndex >= _units.Count) return false;
            if (unitIndex == _units.Count - 1) return true;

            return _units[unitIndex].AfterStack == _units[unitIndex + 1].BeforeStack;
        }

        public void AddRecoveryMessageToLastHistory(string recoveryMessage)
        {
            _history.Last().Unit.IsError = true;
            _history.Last().RecoveryMessage = recoveryMessage;
        }


        /// <summary>
        /// This function starts transaction.
        /// </summary>
        public void Try() => _unitMark = _units.Count - 1;

        public void RollBack()
        {
            try
            {
                if (_unitMark < 0) return;

                int startIndex = _unitMark + 1;
                int count = _units.Count - startIndex;

                if (count == 0) return;

                _units.RemoveRange(startIndex, count);
            }
            finally
            {
                _unitMark = -1;
            }
        }

        public void Commit() => _unitMark = -1;


        /// <summary>
        /// This function removes all unit on in last token.
        /// </summary>
        /// <param name="bRemoveIfFailed">If this param is true removes only if failed unit is included in last token.</param>
        public void RemoveLastToken(bool bRemoveIfFailed = false)
        {
            if (_units.Count == 0) return;

            int endIndex = _units.Count - 1;
            int count = CollectAllUnitOfLastToken();

            int startIndex = endIndex - count + 1;

            // condition
            if (bRemoveIfFailed)
            {
                if (!IsIncludedFailedUnit(startIndex, count)) return;
            }

            RemoveRange(startIndex, count);
        }

        public void RemoveAllToken() => RemoveRange(0, _units.Count());

        public void Clear()
        {
            _units.Clear();
            _history.Clear();
            _errorInfos.Clear();
        }


        /// <summary>
        /// This function checks if there is failed unit in the range.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool IsIncludedFailedUnit(int startIndex, int count)
        {
            bool result = false;
            int end = startIndex + count;

            for (int i = startIndex; i < end; i++)
            {
                if (_units[i].Action.Direction == ActionDir.Failed)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This function collects all units of last token
        /// </summary>
        /// <returns></returns>
        private int CollectAllUnitOfLastToken()
        {
            int count = 0;
            var lastToken = _units.Last().InputValue;

            for (int i = _units.Count - 1; i >= 0; i--)
            {
                if (_units[i].InputValue.TokenCell != lastToken.TokenCell) break;

                count++;
            }

            return count;
        }


        private void RemoveRange(int startIndex, int count)
        {
            // first unit of block don't remove
            if (startIndex == 0)
            {
                _units[0].CopyBeforeStackToAfterStack();
                startIndex = 1;

                if (startIndex >= _units.Count) return;
            }

            var parsingUnit = new ParsingUnit(_units[startIndex + count - 1].AfterStack, _units[startIndex].BeforeStack);
            _history.Add(new ParsingUnitHistory(parsingUnit, string.Format(Resource.RemoveParsingUnits, count)));
            _units.RemoveRange(startIndex, count);
        }


        private string DebuggerDisplay
        {
            get
            {
                var result = string.Format("TokenCell : {0}, Unit count : {1}", 
                                                        Token, 
                                                        _units.Count);

                if (ErrorInfos.Count() > 0) result += " Fired Error";
                if (IsIgnore) result += "This token is ignored";

                return result;
            }
        }


        private List<ParsingUnitHistory> _history = new List<ParsingUnitHistory>();
        private List<ParsingUnit> _units = new List<ParsingUnit>();
        private int _unitMark = -1;
    }

    public class UnitTokenRange : Range
    {
        public UnitTokenRange(int startIndex, int count) : base(startIndex, count)
        {
        }
    }

    public class ParsingUnitHistory
    {
        public ParsingUnit Unit { get; }
        public string RecoveryMessage { get; set; }

        public ParsingUnitHistory(ParsingUnit unit) : this(unit, string.Empty)
        {
        }

        public ParsingUnitHistory(ParsingUnit unit, string recoveryMessage)
        {
            Unit = unit;
            RecoveryMessage = recoveryMessage;
        }
    }
}
