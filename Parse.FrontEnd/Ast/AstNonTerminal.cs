﻿using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class AstNonTerminal : AstSymbol, IList<AstSymbol>
    {
        public NonTerminalSingle SignPost { get; set; } = null;
        public IReadOnlyList<AstSymbol> Items => _symbols;
        public string Name => this.SignPost.MeaningUnit?.Name;
        public NonTerminal ToNonTerminal => this.SignPost.ToNonTerminal();

        public int Count => ((IList<AstSymbol>)_symbols).Count;
        public bool IsReadOnly => ((IList<AstSymbol>)_symbols).IsReadOnly;

        public AstSymbol this[int index]
        {
            get => ((IList<AstSymbol>)_symbols)[index];
            set
            {
                value.Parent = this;
                if (_symbols[index] != null) _symbols[index].Parent = null;

                ((IList<AstSymbol>)_symbols)[index] = value;
            }
        }

        public override IReadOnlyList<TokenData> AllTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                foreach (var item in Items)
                    result.AddRange(item.AllTokens);

                return result;
            }
        }



        public AstNonTerminal(ParseTreeNonTerminal connectedParseTree)
        {
            this.SignPost = connectedParseTree.SignPost;
            ConnectedParseTree = connectedParseTree;
        }

        public void Add(AstSymbol item)
        {
            item.Parent = this;

            ((IList<AstSymbol>)_symbols).Add(item);
        }

        public void AddRange(IEnumerable<AstSymbol> items)
        {
            foreach (var item in items)
            {
                if (item != null) Add(item);
            }
        }

        public void Clear()
        {
            foreach (var item in _symbols) item.Parent = null;

            ((IList<AstSymbol>)_symbols).Clear();
            ClearConnectedInfo();
        }

        public bool Contains(AstSymbol item)
        {
            return ((IList<AstSymbol>)_symbols).Contains(item);
        }

        public void CopyTo(AstSymbol[] array, int arrayIndex)
        {
            ((IList<AstSymbol>)_symbols).CopyTo(array, arrayIndex);
        }

        public IEnumerator<AstSymbol> GetEnumerator()
        {
            return ((IList<AstSymbol>)_symbols).GetEnumerator();
        }

        public int IndexOf(AstSymbol item)
        {
            return ((IList<AstSymbol>)_symbols).IndexOf(item);
        }

        public void Insert(int index, AstSymbol item)
        {
            item.Parent = this;

            ((IList<AstSymbol>)_symbols).Insert(index, item);
        }

        public bool Remove(AstSymbol item)
        {
            item.Parent = null;

            return ((IList<AstSymbol>)_symbols).Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0) return;
            if (_symbols.Count <= index) return;
            _symbols[index].Parent = null;

            ((IList<AstSymbol>)_symbols).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<AstSymbol>)_symbols).GetEnumerator();
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
                if (symbol is AstTerminal)
                {
                    for (int i = 1; i < depth; i++) result += "  ";
                    result += "  " + "Terminal : " + symbol.ToString() + Environment.NewLine;
                }
                else result += symbol.ToTreeString((ushort)(depth + 1));
            }

            return result;
        }

        public override string ToString() => this.ToNonTerminal.ToString();  //this.Name?.ToString();


        private List<AstSymbol> _symbols = new List<AstSymbol>();
    }
}
