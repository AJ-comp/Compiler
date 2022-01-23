using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class CanonicalState : HashSet<LRItem>
    {
        public int StateNumber { get; internal set; } = -1;

        public SymbolSet MarkSymbolSet
        {
            get
            {
                SymbolSet result = new SymbolSet();
                foreach (var data in this)
                {
                    if (data.MarkSymbol != null) result.Add(data.MarkSymbol);
                }

                return result;
            }
        }

        /// <summary>
        /// example A-> ab. Does it exist that mark index reached the end?
        /// </summary>
        public bool IsReachedHandle
        {
            get
            {
                foreach (var data in this)
                {
                    if (data.IsReachedHandle) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// This property gets the NonTerminalSingle Set that mark index reached to the end.
        /// This means reduce items.
        /// </summary>
        public HashSet<NonTerminalSingle> ReachedHandleSet
        {
            get
            {
                HashSet<NonTerminalSingle> result = new HashSet<NonTerminalSingle>();

                foreach (var data in this)
                {
                    if (data.IsReachedHandle) result.Add(data.SingleNT);
                }

                return result;
            }
        }

        /// <summary>
        /// Create a new CanonicalState that consists of the LRItems that reached the handle and return it.
        /// </summary>
        public CanonicalState ReachedHandleItem
        {
            get
            {
                CanonicalState result = new CanonicalState(StateNumber);

                foreach (var data in this)
                {
                    if (data.IsReachedHandle) result.Add(data);
                }

                return result;
            }
        }

        public HashSet<NonTerminalSingle> ShiftItemList
        {
            get
            {
                HashSet<NonTerminalSingle> result = new HashSet<NonTerminalSingle>();

                foreach (var data in this)
                {
                    if (!data.IsReachedHandle) result.Add(data.SingleNT);
                }

                return result;
            }
        }


        public CanonicalState() { }

        public CanonicalState(int stateNumber)
        {
            StateNumber = stateNumber;
        }

        public CanonicalState(int stateNumber, LRItem item) : this(stateNumber)
        {
            Add(item);
        }

        /// <summary>
        /// Check if has LR item.
        /// </summary>
        /// <param name="toFindItem"></param>
        /// <returns></returns>
        public bool HasItem(LRItem toFindItem) => GetItem(toFindItem) != null;


        public LRItem GetItem(LRItem toFindItem)
        {
            LRItem result = null;

            foreach (var item in this)
            {
                if (item == toFindItem)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// Calculate Follow for all LRItem in the state.
        /// </summary>
        /// <param name="relationData"></param>
        internal void CalculateFollow(RelationData relationData)
        {
            foreach (var item in this)
                item.Follow = relationData[item.SingleNT.ToNonTerminal()];
        }


        /// <summary>
        /// <para>This function gets NonTerminalSingle Set that has markSymbol matched with markSymbol of the parameter.</para>
        /// <para>파라메터로 넘어온 마크심벌을 가지는 비단말단일식을 가져옵니다.</para>
        /// </summary>
        /// <param name="markSymbol"></param>
        /// <returns></returns>
        public HashSet<NonTerminalSingle> GetNTSingleHasMarkSymbol(Symbol markSymbol)
        {
            HashSet<NonTerminalSingle> result = new HashSet<NonTerminalSingle>();

            foreach (var data in this)
            {
                if (data.MarkSymbol == markSymbol) result.Add(data.SingleNT);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seenSymbols">already seen symbols</param>
        /// <param name="markSymbol">symbol to see</param>
        /// <returns></returns>
        public LRItem GetMatchedItem(SymbolSet seenSymbols, Terminal markSymbol)
        {
            LRItem result = null;

            foreach (var item in this)
            {
                if (item.MarkSymbol != markSymbol) continue;

                SymbolSet symbolSetAwayMarkSymbol = new SymbolSet();
                for (int i = 0; i < seenSymbols.Count; i++)
                {
                    symbolSetAwayMarkSymbol.Add(item.GetSymbolFarMarkIndex(i));
                }

                if (symbolSetAwayMarkSymbol == seenSymbols)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// <para>Construct the new CanonicalState that consists of LRItems has the same markSymbol and return it.</para>
        /// <para>동일한 markSymbol만 있는 새로운 LRItem 집합 CanonicalState를 구성하고 그것을 반환합니다.</para>
        /// <example>
        /// If this is consists of LRItems as below <br/>
        /// A -> .Bca <br/>
        /// A -> .pa  <br/>
        /// A -> adq.B <br/> <br/>
        /// and the markSymbol is B then return the new CanonicalState consists of LRItems as below<br/>
        /// A -> .B  <br/>
        /// A -> adq.B <br/>
        /// </example>
        /// </summary>
        /// <param name="markSymbol"></param>
        /// <returns></returns>
        public CanonicalState GetNewStateHasMarkSymbol(Symbol markSymbol)
        {
            var result = new CanonicalState(StateNumber);

            foreach (var item in this)
            {
                if (item.MarkSymbol == markSymbol) result.Add(item);
            }

            return result;
        }


        /// <summary>
        /// Get all lookahead set in state.
        /// </summary>
        /// <returns></returns>
        public TerminalSet GetAllLookAheadSet()
        {
            var result = new TerminalSet();

            foreach (var item in this) result.UnionWith(item.LookAhead);

            return result;
        }


        /// <summary>
        /// Get all follow set in state.
        /// </summary>
        /// <returns></returns>
        public TerminalSet GetAllFollowSet()
        {
            var result = new TerminalSet();

            foreach (var item in this) result.UnionWith(item.Follow);

            return result;
        }


        public HashSet<NonTerminal> GetAllNonTerminalSet()
        {
            HashSet<NonTerminal> result = new HashSet<NonTerminal>();

            foreach (var item in this)
                result.Add(item.SingleNT.ToNonTerminal());

            return result;
        }


        /// <summary>
        /// Check if there is a reduce - reduce conflict.
        /// </summary>
        /// <param name="reduceParameter"></param>
        /// <returns></returns>
        public bool IsReduceReduceConflict(ReduceParameter reduceParameter)
        {
            bool result = false;

            TerminalSet allLookAhead = new TerminalSet();
            foreach (var reduceItem in ReachedHandleItem)
            {
                if (allLookAhead.Intersect(reduceItem.LookAhead).Count() > 0)
                {
                    result = true;
                    break;
                }

                allLookAhead.UnionWith(reduceItem.LookAhead);
            }

            return result;
        }


        /// <summary>
        /// Check if there is a shift - reduce conflict.
        /// </summary>
        /// <param name="reduceParameter"></param>
        /// <returns></returns>
        public bool IsShiftReduceConflict(ReduceParameter reduceParameter)
        {
            var reduceLookAheadList = GetTerminalSetForReduceable(reduceParameter);

            return reduceLookAheadList.Intersect(MarkSymbolSet).Count() > 0;
        }


        /// <summary>
        /// Get the all terminal set that can reduce. <br/>
        /// LR(0) case is follow set <br/>
        /// LALR(1) case is lookahead set <br/>
        /// </summary>
        /// <param name="reduceParameter"></param>
        /// <returns></returns>
        private TerminalSet GetTerminalSetForReduceable(ReduceParameter reduceParameter)
        {
            var reduceItems = ReachedHandleItem;

            return (reduceParameter == ReduceParameter.Follow) ? reduceItems.GetAllFollowSet()
                                                                                            : (reduceParameter == ReduceParameter.LalrLookAhead)
                                                                                            ? reduceItems.GetAllLookAheadSet()
                                                                                            : reduceItems.GetAllLookAheadSet();
        }


        public override string ToString()
        {
            string result = " {";
            foreach (var item in this)
            {
                result += "[";
                result += item.ToString();
                result += "]";
            }
            result += "}";

            return result;
        }

        public string ToLineString()
        {
            string result = " {";
            result += $"{StateNumber}, ";
            foreach (var item in this)
            {
                result += "[";
                result += item.ToString();
                result += "]";
                result += Environment.NewLine;
            }
            result = result.Substring(0, result.Length - Environment.NewLine.Length);
            result += "}";

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is CanonicalState state &&
                   StateNumber == state.StateNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StateNumber);
        }

        public static bool operator ==(CanonicalState left, CanonicalState right)
        {
            return EqualityComparer<CanonicalState>.Default.Equals(left, right);
        }

        public static bool operator !=(CanonicalState left, CanonicalState right)
        {
            return !(left == right);
        }
    }
}
