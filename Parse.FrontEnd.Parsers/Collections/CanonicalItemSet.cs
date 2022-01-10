using Parse.FrontEnd.Parsers.Datas.LR;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Collections
{
    public class CanonicalItemSet : HashSet<CanonicalItem>
    {
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
        public bool IsEndMarkSymbol
        {
            get
            {
                foreach (var data in this)
                {
                    if (data.IsEnd) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// This property gets the NonTerminalSingle Set that mark index reached to the end.
        /// This means reduce items.
        /// </summary>
        public HashSet<NonTerminalSingle> EndMarkSymbolSet
        {
            get
            {
                HashSet<NonTerminalSingle> result = new HashSet<NonTerminalSingle>();

                foreach (var data in this)
                {
                    if (data.IsEnd) result.Add(data.SingleNT);
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
                    if (!data.IsEnd) result.Add(data.SingleNT);
                }

                return result;
            }
        }

        /// <summary>
        /// This function gets NonTerminalSingle Set that has markSymbol matched with markSymbol of the parameter.
        /// </summary>
        /// <param name="markSymbol"></param>
        /// <returns></returns>
        public HashSet<NonTerminalSingle> GetNTSingleMatchedMarkSymbol(Symbol markSymbol)
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
        public CanonicalItem GetMatchedItem(SymbolSet seenSymbols, Terminal markSymbol)
        {
            CanonicalItem result = null;

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
    }
}
