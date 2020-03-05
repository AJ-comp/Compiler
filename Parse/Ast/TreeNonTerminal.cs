using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class TreeNonTerminal : TreeSymbol, IList<TreeSymbol>
    {
        public NonTerminalSingle signPost = null;

        private List<TreeSymbol> symbols = new List<TreeSymbol>();
        public IReadOnlyList<TreeSymbol> Items => symbols;

        public string Name => this.signPost.MeaningUnit?.Name;

        public TreeSymbol this[int index] { get => ((IList<TreeSymbol>)symbols)[index]; set => ((IList<TreeSymbol>)symbols)[index] = value; }
        public int Count => ((IList<TreeSymbol>)symbols).Count;
        public bool IsReadOnly => ((IList<TreeSymbol>)symbols).IsReadOnly;

        public TreeNonTerminal(NonTerminalSingle singleNT)
        {
            this.signPost = singleNT;
        }

        public object ActionLogic(SymbolTable symbolTable, MeaningParsingInfo parsingInfo, MeaningErrInfoList errList) 
            => this.signPost?.MeaningUnit?.ActionLogic(this, symbolTable, parsingInfo, errList);

        public void Add(TreeSymbol item)
        {
            ((IList<TreeSymbol>)symbols).Add(item);
        }

        public void Clear()
        {
            ((IList<TreeSymbol>)symbols).Clear();
        }

        public bool Contains(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)symbols).Contains(item);
        }

        public void CopyTo(TreeSymbol[] array, int arrayIndex)
        {
            ((IList<TreeSymbol>)symbols).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TreeSymbol> GetEnumerator()
        {
            return ((IList<TreeSymbol>)symbols).GetEnumerator();
        }

        public int IndexOf(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)symbols).IndexOf(item);
        }

        public void Insert(int index, TreeSymbol item)
        {
            ((IList<TreeSymbol>)symbols).Insert(index, item);
        }

        public bool Remove(TreeSymbol item)
        {
            return ((IList<TreeSymbol>)symbols).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<TreeSymbol>)symbols).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<TreeSymbol>)symbols).GetEnumerator();
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

            foreach (var symbol in this.symbols)
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

        public override string ToString() => this.Name.ToString();
    }
}
