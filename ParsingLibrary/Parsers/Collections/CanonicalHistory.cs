﻿using ParsingLibrary.Datas;
using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Parsers.Datas;
using ParsingLibrary.Utilities;
using System;
using System.Collections.Generic;
using static ParsingLibrary.Parsers.Datas.LRParsingData;

namespace ParsingLibrary.Parsers.Collections
{
    /// <summary>
    /// First : prev status index
    /// Second : see MarkSymbol
    /// Third : current status index
    /// Fourth : current canonical
    /// </summary>
    public class CanonicalHistory : Dictionary<Tuple<int, Symbol>, Tuple<int, Canonical>>
    {
        private NonTerminal virtualStartSymbol = null;

        public int MaxIxIndex
        {
            get
            {
                int maxIndex = 0;

                foreach (var item in this.Values)
                {
                    if (item.Item1 > maxIndex) maxIndex = item.Item1;
                }

                return maxIndex;
            }
        }


        public void Calculate(NonTerminal virtualStartSymbol)
        {
            this.virtualStartSymbol = virtualStartSymbol;
            Queue<Canonical> tempC0 = new Queue<Canonical>();
            //I0
            foreach (var singleNT in virtualStartSymbol)
            {
                Canonical param = new Canonical();
                param.Add(new CanonicalItem(singleNT));

                var curStatus = Analyzer.Closure(param);
                tempC0.Enqueue(curStatus);
                this.AddFirstCanonical(curStatus);
            }

            while (tempC0.Count > 0)
            {
                var prevIx = tempC0.Dequeue();

                foreach (var markSymbol in prevIx.MarkSymbolSet)
                {
                    var curIx = Analyzer.Goto(prevIx, markSymbol);
                    if (this.Add(prevIx, markSymbol, curIx)) tempC0.Enqueue(curIx);
                }
            }
        }


        /// <summary>
        /// Add first canonical.
        /// </summary>
        /// <param name="curStatus"></param>
        public void AddFirstCanonical(Canonical curStatus)
        {
            this.Clear();

            var key = new Tuple<int, Symbol>(-1, null);
            var value = new Tuple<int, Canonical>(0, curStatus);

            this.Add(key, value);
        }

        /// <summary>
        /// Goto(prevStatus, markSymbol) : Iy {curStatus}
        /// </summary>
        /// <param name="prevStatus">prev status</param>
        /// <param name="markSymbol">mark symbol</param>
        /// <param name="curStatus">current status</param>
        /// <returns>Return true if added new current status, return false if already exist the current status on the history.  </returns>
        public bool Add(Canonical prevStatus, Symbol markSymbol, Canonical curStatus)
        {
            int prevIndex = 0;
            int curIndex = this.MaxIxIndex + 1;

            foreach(var item in this.Values)
            {
                if (item.Item2.SetEquals(prevStatus))
                {
                    prevIndex = item.Item1;
                    break;
                }
            }

            foreach(var item in this.Values)
            {
                if(item.Item2.SetEquals(curStatus))
                {
                    curIndex = item.Item1;
                    break;
                }
            }

            var key = new Tuple<int, Symbol>(prevIndex, markSymbol);
            var value = new Tuple<int, Canonical>(curIndex, curStatus);

            bool result = !this.ContainsKey(key);
            if(result)  this.Add(key, value);

            return result;
        }

        /// <summary>
        /// Get the current status from the Ix index
        /// </summary>
        /// <param name="IxIndex">Ix index</param>
        /// <returns></returns>
        public Canonical GetCurStatusFromIxIndex(int IxIndex)
        {
            Canonical result = null;

            foreach(var item in this)
            {
                if(item.Value.Item1 == IxIndex)   result = item.Value.Item2;
            }

            return result;
        }

        /// <summary>
        /// Get all symbol and the match value that can see from the current status.
        /// </summary>
        /// <param name="IxIndex"></param>
        /// <param name="relationData">follow information</param>
        /// <returns></returns>
        public Dictionary<Symbol, Tuple<ActionInfo, object>> GetCanSeeMatchValue(int IxIndex, RelationData relationData)
        {
            var result = new Dictionary<Symbol, Tuple<ActionInfo, object>>();
            Canonical curStatus = this.GetCurStatusFromIxIndex(IxIndex);

            // add shift action
            foreach (var item in this)
            {
                if (item.Key.Item1 == IxIndex)
                {
                    ActionInfo actionInfo = (item.Key.Item2 is Terminal) ? ActionInfo.shift : ActionInfo.moveto;

                    result.Add(item.Key.Item2, new Tuple<ActionInfo, object>(actionInfo, item.Value.Item1));
                }
            }

            // add reduce action
            foreach (var singleNT in curStatus.EndMarkSymbolSet)
            {
                foreach (var followItem in relationData[singleNT.ToNonTerminal()])
                {
                    ActionInfo actionInfo = (this.virtualStartSymbol.IsSubSet(singleNT)) ? ActionInfo.accept : ActionInfo.reduce;
                    result.Add(followItem, new Tuple<ActionInfo, object>(actionInfo, singleNT));
                }
            }

            return result;
        }


        public override string ToString()
        {
            string result = string.Empty;

            foreach(var data in this)
            {
                if (data.Key.Item1 == -1) result += string.Format("I{0} : ", data.Value.Item1);
                else result += string.Format("Goto(I{0},{1}) = I{2} : ", data.Key.Item1, data.Key.Item2, data.Value.Item1);

                result += data.Value.Item2.ToString() + Environment.NewLine;
            }

            return result;
        }
    }
}
