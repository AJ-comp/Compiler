using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class ParseTreeNonTerminal : ParseTreeSymbol, IList<ParseTreeSymbol>
    {
        private List<ParseTreeSymbol> _symbols = new List<ParseTreeSymbol>();

        public NonTerminalSingle SignPost { get; set; } = null;
        public IReadOnlyList<ParseTreeSymbol> Items => _symbols;
        public string Name => this.SignPost.MeaningUnit?.Name;
        public NonTerminal ToNonTerminal => this.SignPost.ToNonTerminal();

        public ParseTreeSymbol this[int index]
        {
            get => ((IList<ParseTreeSymbol>)_symbols)[index];
            set
            {
                value.Parent = this;
                if (_symbols[index] != null) _symbols[index].Parent = null;

                ((IList<ParseTreeSymbol>)_symbols)[index] = value;
            }
        }
        public int Count => ((IList<ParseTreeSymbol>)_symbols).Count;
        public bool IsReadOnly => ((IList<ParseTreeSymbol>)_symbols).IsReadOnly;

        public override bool HasVirtualChild
        {
            get
            {
                foreach (var item in _symbols)
                {
                    if (item.IsVirtual) return true;
                    if (item.HasVirtualChild) return true;
                }

                return false;
            }
        }
        public override bool IsVirtual
        {
            get
            {
                if (_symbols.Count == 0) return false;

                foreach(var item in _symbols)
                {
                    if (item.IsVirtual == false) return false;
                }

                return true;
            }
        }

        public string AllInputDatas
        {
            get
            {
                string result = string.Empty;

                foreach (var item in Items)
                {
                    if (item is ParseTreeTerminal) result += (item as ParseTreeTerminal).Token.Input + " ";
                    else if (item is ParseTreeNonTerminal) result += (item as ParseTreeNonTerminal).AllInputDatas;
                }

                return result;
            }
        }

        public override AstSymbol ToAst
        {
            get
            {
                // If curNode is Ast returns curNode else returns childAst.
                AstSymbol result = null;
                AstNonTerminal curNode = null;
                if (SignPost.MeaningUnit != null)
                {
                    curNode = new AstNonTerminal(SignPost);
                    curNode.ConnectedParseTree = this;
                    result = curNode;
                }

                foreach (var item in Items)
                {
                    var childAst = item.ToAst;
                    if(childAst != null)
                    {
                        if (curNode == null) result = childAst;
                        else curNode.Add(childAst);
                    }
                }

                return result;
            }
        }

        public ParseTreeNonTerminal(NonTerminalSingle singleNT)
        {
            this.SignPost = singleNT;
        }

        public void Add(ParseTreeSymbol item)
        {
            item.Parent = this;

            ((IList<ParseTreeSymbol>)_symbols).Add(item);
        }

        public void Clear()
        {
            foreach (var item in _symbols) item.Parent = null;

            ((IList<ParseTreeSymbol>)_symbols).Clear();
        }

        public bool Contains(ParseTreeSymbol item)
        {
            return ((IList<ParseTreeSymbol>)_symbols).Contains(item);
        }

        public void CopyTo(ParseTreeSymbol[] array, int arrayIndex)
        {
            ((IList<ParseTreeSymbol>)_symbols).CopyTo(array, arrayIndex);
        }

        public IEnumerator<ParseTreeSymbol> GetEnumerator()
        {
            return ((IList<ParseTreeSymbol>)_symbols).GetEnumerator();
        }

        public int IndexOf(ParseTreeSymbol item)
        {
            return ((IList<ParseTreeSymbol>)_symbols).IndexOf(item);
        }

        public void Insert(int index, ParseTreeSymbol item)
        {
            item.Parent = this;

            ((IList<ParseTreeSymbol>)_symbols).Insert(index, item);
        }

        public bool Remove(ParseTreeSymbol item)
        {
            item.Parent = null;

            return ((IList<ParseTreeSymbol>)_symbols).Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0) return;
            if (_symbols.Count <= index) return;
            _symbols[index].Parent = null;

            ((IList<ParseTreeSymbol>)_symbols).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<ParseTreeSymbol>)_symbols).GetEnumerator();
        }

        public override string ToGrammarString()
        {
            throw new NotImplementedException();
        }

        public override string ToTreeString(ushort depth = 1)
        {
            string result = string.Empty;

            for (int i = 1; i < depth; i++) result += "  ";

            result += "Nonterminal : " + this.Name + Environment.NewLine;

            foreach (var symbol in this._symbols)
            {
                if (symbol is ParseTreeTerminal)
                {
                    for (int i = 1; i < depth; i++) result += "  ";
                    result += "  " + "Terminal : " + symbol.ToString() + Environment.NewLine;
                }
                else result += symbol.ToTreeString((ushort)(depth + 1));
            }

            return result;
        }

        public override string ToString() => this.ToNonTerminal.ToString();  //this.Name?.ToString();
    }
}
