using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.RegularGrammar
{
    public class NonTerminalConcat : IList<Symbol>, ICloneable<NonTerminalConcat>, ITemplateCreatable<NonTerminalConcat>
    {
        protected List<Symbol> symbols = new List<Symbol>();

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
        public bool IsEpsilon
        {
            get
            {
                if (this.Count == 1)
                {
                    if (this[0] == new Epsilon()) return true;
                }

                return false;
            }
        }
        public uint Priority { get; internal set; } = 0;
        public MeaningUnit MeaningUnit { get; internal set; } = null;

        public int Count => ((IList<Symbol>)symbols).Count;
        public bool IsReadOnly => ((IList<Symbol>)symbols).IsReadOnly;
        public Symbol this[int index] { get => ((IList<Symbol>)symbols)[index]; set => ((IList<Symbol>)symbols)[index] = value; }

        public NonTerminalConcat(params Symbol[] symbols)
        {
            this.AddRange(symbols);
        }

        public NonTerminalConcat(uint priority, params Symbol[] symbols)
        {
            this.AddRange(symbols);
            this.Priority = priority;
        }

        public NonTerminalConcat(uint priority, MeaningUnit meaningUnit)
        {
            this.Priority = priority;
            this.MeaningUnit = meaningUnit;
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

        public void Replace(int index, NonTerminalConcat symbolList)
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
                    result += (symbol as Terminal).ToString() + Convert.ToBridgeSymbol(BridgeType.Concatenation);
                else
                    result += (symbol as NonTerminal).Name + Convert.ToBridgeSymbol(BridgeType.Concatenation);
            }

            return result.Substring(0, result.Length - Convert.ToBridgeSymbol(BridgeType.Concatenation).Length);
        }

        public int IndexOf(Symbol item) => ((IList<Symbol>)symbols).IndexOf(item);

        public void Insert(int index, Symbol item) => ((IList<Symbol>)symbols).Insert(index, item);

        public void RemoveAt(int index) => ((IList<Symbol>)symbols).RemoveAt(index);

        public void Add(Symbol item) => ((IList<Symbol>)symbols).Add(item);

        public void AddRange(params Symbol[] symbols)
        {
            foreach (var symbol in symbols) this.Add(symbol);
        }

        public void AddRange(NonTerminalConcat symbols)
        {
            foreach (var symbol in symbols) this.Add(symbol);
        }

        public void Clear() => ((IList<Symbol>)symbols).Clear();

        public bool Contains(Symbol item) => ((IList<Symbol>)symbols).Contains(item);

        public void CopyTo(Symbol[] array, int arrayIndex) => ((IList<Symbol>)symbols).CopyTo(array, arrayIndex);

        public bool Remove(Symbol item) => ((IList<Symbol>)symbols).Remove(item);

        public IEnumerator<Symbol> GetEnumerator() => ((IList<Symbol>)symbols).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList<Symbol>)symbols).GetEnumerator();

        public NonTerminalConcat ToReverse()
        {
            var result = this.Clone() as NonTerminalConcat;
            result.symbols.Reverse();

            return result;
        }

        public NonTerminalConcat Clone()
        {
            NonTerminalConcat result = new NonTerminalConcat(this.Priority, this.symbols.ToArray())
            {
                MeaningUnit = this.MeaningUnit
            };

            return result;
        }

        public NonTerminalConcat Template()
        {
            NonTerminalConcat result = this.Clone() as NonTerminalConcat;
            result.Clear();

            return result;
        }
    }
}
