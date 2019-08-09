using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Parsers.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingLibrary.Parsers.Collections
{
    public class Canonical : HashSet<CanonicalItem>
    {
        public SymbolSet MarkSymbolSet
        {
            get
            {
                SymbolSet result = new SymbolSet();
                foreach (var data in this)
                {
                    if(data.MarkSymbol != null) result.Add(data.MarkSymbol);
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
                foreach(var data in this)
                {
                    if (data.IsEnd) return true;
                }

                return false;
            }
        }

        /// <summary>
        /// The NonTerminalSingle set that mark index reached the end.
        /// </summary>
        public HashSet<NonTerminalSingle> EndMarkSymbolSet
        {
            get
            {
                HashSet<NonTerminalSingle> result = new HashSet<NonTerminalSingle>();

                foreach(var data in this)
                {
                    if (data.IsEnd) result.Add(data.singleNT);
                }

                return result;
            }
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
    }
}
