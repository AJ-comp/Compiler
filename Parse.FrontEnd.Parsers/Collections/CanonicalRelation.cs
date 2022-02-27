using AJ.Common.Helpers;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Collections
{
    public enum ReduceParameter
    {
        Follow,
        LalrLookAhead,
        LookAhead1,
        LookAhead2
    }

    /// <summary>
    /// First : prev status index   <br/>
    /// Second : seeing MarkSymbol  <br/>
    /// Third : current status  <br/>
    /// </summary>
    public class CanonicalRelation : Dictionary<int, HashSet<(Symbol, CanonicalState)>>
    {
        private NonTerminal virtualStartSymbol = null;

        private Dictionary<int, IEnumerable<(NonTerminalConcat, CanonicalState)>> _allPossibleShiftPaths
            = new Dictionary<int, IEnumerable<(NonTerminalConcat, CanonicalState)>>();

        public int NextIxIndex { get; private set; } = 0;
        public Dictionary<int, CanonicalState> IndexStateDic { get; } = new Dictionary<int, CanonicalState>();
        public Dictionary<(int, LRItem), TerminalSet> LookAheadTable = new Dictionary<(int, LRItem), TerminalSet>();

        public IEnumerable<string> ConflictLogs => _conflictLogs;
        public ReduceParameter ReduceParameter { get; set; } = ReduceParameter.LalrLookAhead;

        public CanonicalStateSet AllCanonicalStates
        {
            get
            {
                CanonicalStateSet result = new CanonicalStateSet();

                foreach (var item in this)
                {
                    foreach (var value in item.Value) result.Add(value.Item2);
                }

                return result;
            }
        }


        public void Calculate(NonTerminal virtualStartSymbol, RelationData relationData)
        {
            Clear();

            //I0 (virtualStartSymbol is always single not alter)
            this.virtualStartSymbol = virtualStartSymbol;
            var i0 = AddFirstState(virtualStartSymbol);
            if (i0.Count == 0) return;

            AddGoToState(i0);
            SetCache();

            //            CalculatePossibleShiftPath();
            CalculateLookAhead();

            // calculate follow for each CanonicalState.
            foreach (var item in IndexStateDic) item.Value.CalculateFollow(relationData);

            // calculate lookahead for each CanonicalState.
            foreach (var item in LookAheadTable)
            {
                var state = IndexStateDic[item.Key.Item1];
                state.GetItem(item.Key.Item2).LookAhead = item.Value;
            }
        }


        private CanonicalState AddFirstState(NonTerminal virtualStartSymbol)
        {
            var singleNT = virtualStartSymbol[0];

            CanonicalState param = new CanonicalState(-1, new LRItem(singleNT));
            var result = Analyzer.Closure(param);
            var value = new HashSet<(Symbol, CanonicalState)>();
            value.Add((null, result));

            Add(-1, value);
            result.StateNumber = NextIxIndex++;

            return result;
        }


        private void AddGoToState(CanonicalState i0)
        {
            if (i0 == null) return;

            Queue<CanonicalState> tempC0 = new Queue<CanonicalState>();
            tempC0.Enqueue(i0);

            while (tempC0.Count > 0)
            {
                var prevIx = tempC0.Dequeue();

                foreach (var markSymbol in prevIx.MarkSymbolSet)
                {
                    var curIx = Analyzer.Goto(prevIx, markSymbol);
                    if (!ContainsKey(prevIx.StateNumber)) Add(prevIx.StateNumber, new HashSet<(Symbol, CanonicalState)>());
                    if (this.Add(prevIx, markSymbol, curIx)) tempC0.Enqueue(curIx);
                }
            }
        }


        private void SetCache()
        {
            foreach (var state in AllCanonicalStates)
            {
                if (state.StateNumber == -1) continue;
                IndexStateDic.Add(state.StateNumber, GetStatusFromIxIndex(state.StateNumber));
            }
        }


        /**********************************************************************/
        /// <summary>
        /// Calculate all possible shift path lists for the specified state.
        /// </summary>
        /// <param name="sawSymbolList"></param>
        /// <param name="stateIndex"></param>
        /// <see cref="https://lucid.app/lucidchart/665cbfea-b0a6-4d65-8756-1b1ef85a267c/edit?invitationId=inv_0a3c86aa-072b-45d0-b2b2-708cd8193a37&page=0_0#"/>
        /// <returns></returns>
        /**********************************************************************/
        private IEnumerable<(NonTerminalConcat, CanonicalState)> GetAllPossibleShiftPathFrom(NonTerminalConcat sawSymbolList, int stateIndex)
        {
            var result = new List<(NonTerminalConcat, CanonicalState)>();
            if (!this.ContainsKey(stateIndex)) return result;

            foreach (var value in this[stateIndex])
            {
                NonTerminalConcat sawAllSymbolList = new NonTerminalConcat();
                sawAllSymbolList.AddRange(sawSymbolList);
                sawAllSymbolList.Add(value.Item1);

                result.Add((sawAllSymbolList, value.Item2));

                if (value.Item2.StateNumber == stateIndex) continue;

                result.AddRange(GetAllPossibleShiftPathFrom(sawAllSymbolList, value.Item2.StateNumber));
            }

            return result;
        }


        private void CalculatePossibleShiftPath()
        {
            foreach (var item in this)
            {
                if (item.Key == -1) continue;
                var result = GetAllPossibleShiftPathFrom(new NonTerminalConcat(), item.Key);

                _allPossibleShiftPaths.Add(item.Key, result);
            }
        }


        private void CalculateLookAhead()
        {
            foreach (var item in GetStateSetHasReachedHandle())
            {
                foreach (var reduceItem in item.ReachedHandleItem) LookAhead(item, reduceItem);
            }
        }


        private TerminalSet LookAhead(CanonicalState state, LRItem item)
        {
            if (LookAheadTable.ContainsKey((state.StateNumber, item))) return LookAheadTable[(state.StateNumber, item)];
            LookAheadTable.Add((state.StateNumber, item), new TerminalSet());

            var result = new TerminalSet();
            var newMarkSymbol = item.SingleNT.ToNonTerminal();

            if (newMarkSymbol == virtualStartSymbol)
            {
                result.Add(new EndMarker());
                LookAheadTable[(state.StateNumber, item)] = result;
                return result;
            }

            // ex :
            // item : R -> aL.
            // Pred func inner : R -> .aL
            // newMarkSymbol : R
            // newState: X -> .R, Y -> ac.R

            var predStates = PredEx(state, item);

            foreach (var predState in predStates)
            {
                foreach (var newLRItem in predState)
                {
                    if (newLRItem.MarkSymbol != newMarkSymbol) continue;

                    var alpha2 = Analyzer.FirstTerminalSet(newLRItem.SymbolListAfterMarkSymbol);
                    if (alpha2.Contains(new Epsilon()) || alpha2.Count == 0)
                    {
                        var laTerminalSet = LookAheadTable.ContainsKey((predState.StateNumber, newLRItem))
                                                   ? LookAheadTable[(predState.StateNumber, newLRItem)]
                                                   : LookAhead(predState, newLRItem);

                        result.UnionWith(alpha2.RingSum(laTerminalSet));
                    }
                    else result.UnionWith(alpha2);
                }
            }

            LookAheadTable[(state.StateNumber, item)] = result;

            return result;
        }




        /**********************************************************************/
        /// <summary>
        /// Get the state set reachable to targetState through shift.
        /// </summary>
        /// <param name="targetState"></param>
        /// <returns></returns>
        /**********************************************************************/
        private HashSet<int> GetAllStartStateShiftableTo(CanonicalState targetState)
        {
            HashSet<int> result = new HashSet<int>();

            foreach (var item in _allPossibleShiftPaths)
            {
                foreach (var value in item.Value)
                {
                    if (value.Item2 != targetState) continue;

                    if (!result.Contains(item.Key)) result.Add(item.Key);
                    break;
                }
            }

            return result;
        }


        /**********************************************************************/
        /// <summary>
        /// Check if there is connection from sIndex to eIndex in C0.
        /// </summary>
        /// <param name="sIndex"></param>
        /// <param name="eIndex"></param>
        /// <returns></returns>
        /**********************************************************************/
        private bool IsConnection(int sIndex, int eIndex)
        {
            bool result = false;
            if (!_allPossibleShiftPaths.ContainsKey(sIndex)) return result;

            foreach (var value in _allPossibleShiftPaths[sIndex])
            {
                if (value.Item2.StateNumber == eIndex)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        /**********************************************************************/
        /// <summary>
        /// Get the state set that has state that reached to the handle.
        /// </summary>
        /// <returns></returns>
        /**********************************************************************/
        private IEnumerable<CanonicalState> GetStateSetHasReachedHandle()
        {
            ConcurrentBag<CanonicalState> result = new ConcurrentBag<CanonicalState>();

            /* comment because difficult to debugging (use on release mode)
            Parallel.ForEach(IndexStateDic, (item) =>
            {
                if (item.Value.IsReachedHandle) result.Add(item.Value);
            });
            */

            foreach (var item in IndexStateDic)
            {
                if (item.Value.IsReachedHandle) result.Add(item.Value);
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lrItem"></param>
        /// <returns></returns>
        private CanonicalStateSet Pred(CanonicalState currentState, LRItem lrItem)
        {
            var firstLRItem = lrItem.FirstLRItem();
            CanonicalStateSet result = new CanonicalStateSet();

            foreach (var prevStateIdx in GetAllStartStateShiftableTo(currentState))
            {
                var prevState = IndexStateDic[prevStateIdx];
                if (!prevState.HasItem(firstLRItem)) continue;

                // if has item
                result.Add(prevState);
            }

            return result;
        }


        private CanonicalStateSet PredEx(CanonicalState currentState, LRItem lrItem)
        {
            CanonicalStateSet result = new CanonicalStateSet();
            if (lrItem.IsFirst)
            {
                if (currentState.HasItem(lrItem)) result.Add(currentState);
                return result;
            }

            // get prev states of currentState
            var prevStates = GetPrevStates(currentState);

            foreach (var item in prevStates) result.UnionWith(PredEx(item, lrItem.PrevLRItem()));

            return result;
        }


        private CanonicalStateSet GetPrevStates(CanonicalState currentState)
        {
            CanonicalStateSet result = new CanonicalStateSet();

            foreach (var item in this)
            {
                foreach (var value in item.Value)
                {
                    if (value.Item2 == currentState) result.Add(IndexStateDic[item.Key]);
                }
            }

            return result;
        }


        /**********************************************************************/
        /// <summary>
        /// Goto(prevStatus, markSymbol) : Iy {curStatus}
        /// </summary>
        /// <param name="prevStatus">prev status</param>
        /// <param name="markSymbol">mark symbol</param>
        /// <param name="curStatus">current status</param>
        /// <returns>Return true if added new current status, return false if already exist the current status on the history.  </returns>
        /**********************************************************************/
        public bool Add(CanonicalState prevStatus, Symbol markSymbol, CanonicalState curStatus)
        {
            foreach (var item in this)
            {
                foreach (var value in item.Value)
                {
                    if (value.Item2.SetEquals(curStatus))
                    {
                        this[prevStatus.StateNumber].Add((markSymbol, value.Item2));
                        return false;
                    }
                }
            }

            //            if (this[prevStatus.StateNumber].Contains((markSymbol, curStatus))) return false;

            curStatus.StateNumber = NextIxIndex++;
            this[prevStatus.StateNumber].Add((markSymbol, curStatus));
            return true;
        }


        /**********************************************************************/
        /// <summary>
        /// Get the current status from the Ix index
        /// </summary>
        /// <param name="IxIndex">Ix index</param>
        /// <returns></returns>
        /**********************************************************************/
        public CanonicalState GetStatusFromIxIndex(int IxIndex)
        {
            CanonicalState result = null;

            foreach (var item in this)
            {
                foreach (var value in item.Value)
                {
                    if (value.Item2.StateNumber == IxIndex)
                    {
                        result = value.Item2;
                        break;
                    }
                }
            }

            return result;
        }


        /**********************************************************************/
        /// <summary>
        /// Get all symbol and the match value that can see from the current status.
        /// </summary>
        /// <param name="ixIndex"></param>
        /// <returns></returns>
        /**********************************************************************/
        public ActionDicSymbolMatched GetCanSeeMatchValue(int ixIndex)
        {
            var result = new ActionDicSymbolMatched();
            var tempStorage = new Dictionary<Symbol, uint>();
            CanonicalState curStatus = IndexStateDic[ixIndex];

            if(curStatus.IsShiftReduceConflict(ReduceParameter))
                _conflictLogs.Add($"Fired shift - reduce conflict at I{curStatus.StateNumber}");
            if (curStatus.IsReduceReduceConflict(ReduceParameter))
                _conflictLogs.Add($"Fired reduce - reduce conflict at I{curStatus.StateNumber}");

            CollectShiftOrGotoAction(ixIndex, result);
            CollectReduceOrAcceptAction(ixIndex, result);

            return result;
        }


        public IEnumerable<CanonicalLine> ToCanonicalLineList()
        {
            List<CanonicalLine> result = new List<CanonicalLine>();

            foreach (var item in this)
            {
                foreach (var value in item.Value)
                {
                    CanonicalLine line = new CanonicalLine(item.Key, value.Item1, value.Item2);
                    result.Add(line);
                }
            }

            return result;
        }


        public CanonicalCollection ToCanonicalCollection()
        {
            var result = new CanonicalCollection();

            foreach (var item in this)
            {
                foreach (var value in item.Value) result.Add(value.Item2);
            }

            return result;
        }


        public DataTable ToDataTable()
        {
            DataTable result = new DataTable();

            result.CreateColumns(_stateNumber, _stateMember, _markSymbol, _followSet, _lookAheadSet, _srConflict, _rrConflict);
            this.CreateRows(result);

            return result;
        }

        private void CreateRows(DataTable dataTable)
        {
            foreach (var item in IndexStateDic)
            {
                DataRow row = dataTable.NewRow();
                row[_stateNumber] = item.Key;
                row[_srConflict] = item.Value.IsShiftReduceConflict(ReduceParameter);
                row[_rrConflict] = item.Value.IsReduceReduceConflict(ReduceParameter);

                foreach (var lrItem in item.Value)
                {
                    row[_stateMember] = lrItem.ToString();
                    row[_markSymbol] = lrItem.MarkSymbol;
                    row[_followSet] = lrItem.Follow;
                    row[_lookAheadSet] = lrItem.LookAhead;

                    dataTable.Rows.Add(row);
                    row = dataTable.NewRow();
                }

                dataTable.Rows.Add(row);
            }
        }


        private string _stateNumber = "state number";
        private string _stateMember = "LR items";
        private string _markSymbol = "mark symbol";
        private string _followSet = "follow set";
        private string _lookAheadSet = "lookahead set";
        private string _srConflict = "shift-reduce conflict";
        private string _rrConflict = "reduce-reduce conflict";

        public override string ToString()
        {
            string result = string.Empty;

            foreach (var data in this)
            {
                foreach (var value in data.Value)
                {
                    if (data.Key == 0) result += string.Format($"I0 : CLOSURE({virtualStartSymbol}) = [{value.Item2}]");
                    else result += string.Format($"Goto(I{data.Key},{value.Item1}) = I{value.Item2} : ");

                    result += Environment.NewLine;
                }
            }

            return result;
        }


        private void CollectShiftOrGotoAction(int ixIndex, ActionDicSymbolMatched result)
        {
            var tempStorage = new Dictionary<Symbol, uint>();
            CanonicalState curStatus = IndexStateDic[ixIndex];

            // add shift or goto action
            if (!ContainsKey(ixIndex)) return;
            var movedValues = this[ixIndex];
            uint highestPriority = int.MaxValue;

            foreach (var value in movedValues)
            {
                foreach (var singleNT in curStatus.GetNTSingleHasMarkSymbol(value.Item1))
                {
                    if (singleNT.Priority < highestPriority) highestPriority = singleNT.Priority;
                }

                ActionDir actionInfo = (value.Item1 is Terminal) ? ActionDir.Shift : ActionDir.Goto;

                tempStorage.Add(value.Item1, highestPriority);
                result.Add(value.Item1, new ActionData(actionInfo, value.Item2.StateNumber));
            }
        }


        private void CollectReduceOrAcceptAction(int ixIndex, ActionDicSymbolMatched result)
        {
            CanonicalState curStatus = IndexStateDic[ixIndex];

            // add reduce or accept action
            foreach (var handleItem in curStatus.ReachedHandleItem)
            {
                var singleNT = handleItem.SingleNT;
                var reduceItems = (ReduceParameter == ReduceParameter.Follow)
                                        ? handleItem.Follow
                                        : (ReduceParameter == ReduceParameter.LalrLookAhead)
                                        ? handleItem.LookAhead
                                        : handleItem.LookAhead;

                foreach (var item in reduceItems)
                {
                    ActionDir actionInfo = ActionDir.Reduce;
                    if (this.virtualStartSymbol.IsSubSet(singleNT)) actionInfo = ActionDir.Accept;
                    else if (singleNT.IsEpsilon) actionInfo = ActionDir.EpsilonReduce;
                    //                    ActionInfo actionInfo = (this.virtualStartSymbol.IsSubSet(singleNT)) ? ActionInfo.accept : ActionInfo.reduce;

                    //                        tempStorage.Add(followItem, singleNT.Priority);
                    // shift first, if contain key then this item is already registered at shift so don't regist.   // shift-reduce or reduce-reduce conflict
                    if (result.ContainsKey(item)) continue;
                    result.Add(item, new ActionData(actionInfo, singleNT));
                }
            }
        }


        private void ConflictProcess(CanonicalState curStatus, Terminal seeingToken, ActionDicSymbolMatched result)
        {
            // shift - reduce conflict
            if (result[seeingToken].Direction == ActionDir.Shift)
            {
                _conflictLogs.Add($"Fired shift - reduce conflict [I{curStatus.StateNumber} - {seeingToken}]");
            }
            // reduce - reduce conflict
            else if (result[seeingToken].Direction == ActionDir.Reduce)
                _conflictLogs.Add($"Fired reduce - reduce conflict [I{curStatus.StateNumber} - {seeingToken}]");

            /*
            // prevent shift - reduce or reduce - reduce conflict using priority.
            if (tempStorage[followItem] > singleNT.Priority)
            {
                tempStorage.Remove(followItem);
                tempStorage.Add(followItem, singleNT.Priority);

                result.Remove(followItem);
                result.Add(followItem, new Tuple<ActionDir, object>(actionInfo, singleNT));
            }
            */
        }

        private List<string> _conflictLogs = new List<string>();
    }
}
