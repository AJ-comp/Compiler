using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class TreeNonTerminal : TreeSymbol, IList<TreeSymbol>
    {
        public NonTerminalSingle _signPost = null;
        private List<TreeSymbol> _symbols = new List<TreeSymbol>();

        public IReadOnlyList<TreeSymbol> Items => _symbols;
        public string Name => this._signPost.MeaningUnit?.Name;
        public NonTerminal ToNonTerminal => this._signPost.ToNonTerminal();

        public TreeSymbol this[int index] { get => ((IList<TreeSymbol>)_symbols)[index]; set => ((IList<TreeSymbol>)_symbols)[index] = value; }
        public int Count => ((IList<TreeSymbol>)_symbols).Count;
        public bool IsReadOnly => ((IList<TreeSymbol>)_symbols).IsReadOnly;
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

        public TreeNonTerminal(NonTerminalSingle singleNT)
        {
            this._signPost = singleNT;
        }

        public object ActionLogic(SymbolTable symbolTable, int blockLevel, MeaningErrInfoList errList) 
            => this._signPost?.MeaningUnit?.ActionLogic(this, symbolTable, blockLevel, errList);

        public void Add(TreeSymbol item)
        {
            ((IList<TreeSymbol>)_symbols).Add(item);
        }

        public void Clear()
        {
            ((IList<TreeSymbol>)_symbols).Clear();
        }

        public bool Contains(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)_symbols).Contains(item);
        }

        public void CopyTo(TreeSymbol[] array, int arrayIndex)
        {
            ((IList<TreeSymbol>)_symbols).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TreeSymbol> GetEnumerator()
        {
            return ((IList<TreeSymbol>)_symbols).GetEnumerator();
        }

        public int IndexOf(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)_symbols).IndexOf(item);
        }

        public void Insert(int index, TreeSymbol item)
        {
            ((IList<TreeSymbol>)_symbols).Insert(index, item);
        }

        public bool Remove(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)_symbols).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<TreeSymbol>)_symbols).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<TreeSymbol>)_symbols).GetEnumerator();
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
                if (symbol is TreeTerminal)
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
