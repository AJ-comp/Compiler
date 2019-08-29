using Parse.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Ast
{
    public class AstNonTerminal : AstSymbol, IList<AstSymbol>
    {
        private List<AstSymbol> symbols = new List<AstSymbol>();

        public string Name => this.SignPost.MeaningUnit?.Name;

        public AstSymbol this[int index] { get => ((IList<AstSymbol>)symbols)[index]; set => ((IList<AstSymbol>)symbols)[index] = value; }
        public int Count => ((IList<AstSymbol>)symbols).Count;
        public bool IsReadOnly => ((IList<AstSymbol>)symbols).IsReadOnly;

        public AstNonTerminal(NonTerminalSingle singleNT)
        {
            this.SignPost = singleNT;
        }

        public void Add(AstSymbol item)
        {
            ((IList<AstSymbol>)symbols).Add(item);
        }

        public void Clear()
        {
            ((IList<AstSymbol>)symbols).Clear();
        }

        public bool Contains(AstSymbol item)
        {
            return ((IList<AstSymbol>)symbols).Contains(item);
        }

        public void CopyTo(AstSymbol[] array, int arrayIndex)
        {
            ((IList<AstSymbol>)symbols).CopyTo(array, arrayIndex);
        }

        public IEnumerator<AstSymbol> GetEnumerator()
        {
            return ((IList<AstSymbol>)symbols).GetEnumerator();
        }

        public int IndexOf(AstSymbol item)
        {
            return ((IList<AstSymbol>)symbols).IndexOf(item);
        }

        public void Insert(int index, AstSymbol item)
        {
            ((IList<AstSymbol>)symbols).Insert(index, item);
        }

        public bool Remove(AstSymbol item)
        {
            return ((IList<AstSymbol>)symbols).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<AstSymbol>)symbols).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<AstSymbol>)symbols).GetEnumerator();
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
                if (symbol is AstTerminal)
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
