using System.Collections.Generic;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public class SymbolList : List<Symbol>
    {
        public bool IsAllTerminal
        {
            get
            {
                foreach(var symbol in this)
                {
                    if (symbol is NonTerminal) return false;
                }

                return true;
            }
        }

        public bool IsNull => this.Count == 0;

        public SymbolList(params Symbol[] symbols)
        {
            this.AddRange(symbols);
        }

        public void Replace(int index, Symbol item)
        {
            this.RemoveAt(index);
            this.Insert(index, item);
        }

        public void Replace(Symbol from, Symbol to)
        {
            for(int i=0; i<this.Count; i++)
            {
                if(this[i] == from) this.Replace(i, to);
            }
        }

        public void Replace(int index, SymbolList symbolList)
        {
            this.RemoveAt(index);

            foreach(var symbol in symbolList)   this.Insert(index++, symbol);
        }

        public HashSet<NonTerminal> ToNonTerminalSet()
        {
            HashSet<NonTerminal> result = new HashSet<NonTerminal>();

            foreach(var item in this)
            {
                if (item is NonTerminal) result.Add(item as NonTerminal);
            }

            return result;
        }

        public TerminalSet ToTerminalSet()
        {
            TerminalSet result = new TerminalSet();

            foreach (var item in this)
            {
                if (item is Terminal) result.Add(item as Terminal);
            }

            return result;
        }

        public override string ToString()
        {
            string result = string.Empty;
            if (this.Count == 0) return result;

            foreach (var symbol in this)
            {
                if (symbol is Terminal)
                    result += (symbol as Terminal).ToString() + Utilities.Convert.ToBridgeSymbol(BridgeType.Concatenation);
                else
                    result += (symbol as NonTerminal).Name + Utilities.Convert.ToBridgeSymbol(BridgeType.Concatenation);
            }

            return result.Substring(0, result.Length - Utilities.Convert.ToBridgeSymbol(BridgeType.Concatenation).Length);
        }
    }
}
