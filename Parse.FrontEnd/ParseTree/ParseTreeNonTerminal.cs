using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.ParseTree
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

        public override AstSymbol ToAst => CreateAst(null, this);

        public ParseTreeNonTerminal(NonTerminalSingle singleNT)
        {
            this.SignPost = singleNT;
        }

        private static AstNonTerminal CreateAst(AstNonTerminal newParentTree, ParseTreeSymbol curTree)
        {
            AstNonTerminal result = null;

            if (curTree is ParseTreeTerminal)
            {
                var ast = curTree.ToAst;
                if (ast != null) newParentTree.Add(ast);
            }
            else
            {
                var convertedParentTree = curTree as ParseTreeNonTerminal;
                if (convertedParentTree.SignPost.MeaningUnit != null)
                {
                    result = new AstNonTerminal(convertedParentTree.SignPost);
                    result.ConnectedParseTree = curTree as ParseTreeNonTerminal;

                    if (newParentTree == null) newParentTree = result;
                    else if (newParentTree != result)
                    {
                        (newParentTree as AstNonTerminal).Add(result);
                        newParentTree = result;
                    }
                }

                // it can't use Parallel because order.
                foreach (var node in convertedParentTree) CreateAst(newParentTree, node);
            }

            return result;
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
