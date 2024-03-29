﻿using Parse.FrontEnd.Ast;
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
        public IReadOnlyList<ParseTreeSymbol> MeaningList
        {
            get
            {
                List<ParseTreeSymbol> result = new List<ParseTreeSymbol>();

                foreach (var item in _symbols)
                {
                    if (item.IsMeaning) result.Add(item);
                }

                return result;
            }
        }

        public override bool IsTerminal => false;

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
                if (SignPost.IsInduceEpsilon) return false;
                if (_symbols.Count == 0) return false;

                foreach (var item in _symbols)
                {
                    if (item.IsVirtual == false) return false;
                }

                return true;
            }
        }

        public bool HasMeaningChildren
        {
            get
            {
                foreach (var item in _symbols)
                {
                    if (item is ParseTreeTerminal)
                    {
                        if ((item as ParseTreeTerminal).Token.Kind.Meaning)
                            return true;
                    }

                    else if (item is ParseTreeNonTerminal)
                    {
                        if ((item as ParseTreeNonTerminal).SignPost.MeaningUnit != null)
                            return true;
                    }
                }

                return false;
            }
        }

        public bool HasMeaningToken
        {
            get
            {
                foreach (var item in _symbols)
                {
                    if (item is ParseTreeTerminal)
                    {
                        if ((item as ParseTreeTerminal).Token.Kind.Meaning)
                            return true;
                    }
                }

                return false;
            }
        }

        public IReadOnlyList<ParseTreeTerminal> AllTreeTerminal
        {
            get
            {
                List<ParseTreeTerminal> result = new List<ParseTreeTerminal>();

                foreach (var item in Items)
                {
                    if (item is ParseTreeTerminal) result.Add((item as ParseTreeTerminal));
                    else if (item is ParseTreeNonTerminal) result.AddRange((item as ParseTreeNonTerminal).AllTreeTerminal);
                }

                return result;
            }
        }

        public override IReadOnlyList<TokenData> AllTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                foreach (var item in Items) result.AddRange(item.AllTokens);

                return result;
            }
        }

        public override string AllInputDatas
        {
            get
            {
                string result = string.Empty;

                bool bSpaceReady = false;
                foreach (var token in AllTokens)
                {
                    string space = string.Empty;

                    if (token.Kind.TokenType is Operator) bSpaceReady = false;
                    else if (token.Kind.TokenType is Delimiter) bSpaceReady = false;
                    else if (bSpaceReady == false) bSpaceReady = true;
                    else space = " ";

                    result += space + token.Input;
                }

                return result;
            }
        }

        public override AstSymbol ToAst
        {
            get
            {
                bool bTracking = true;
                return CreateAst(null, null, this, ref bTracking);
            }
        }

        public override bool IsMeaning => (SignPost.MeaningUnit != null);

        public ParseTreeNonTerminal(NonTerminalSingle singleNT)
        {
            this.SignPost = singleNT;
        }

        /// <summary>
        /// This function make an AST.
        /// </summary>
        /// <param name="newParentTree"></param>
        /// <param name="epsilonableTree"></param>
        /// <param name="curTree"></param>
        /// <param name="bTracking"></param>
        /// <see cref="https://app.lucidchart.com/documents/edit/589fb725-e547-4eb0-bd15-3408f0692062/0_0"/>
        /// <returns></returns>
        private static AstNonTerminal CreateAst(AstNonTerminal newParentTree, AstNonTerminal epsilonableTree, ParseTreeSymbol curTree, ref bool bTracking)
        {
            AstNonTerminal result = null;

            // Ast must not have a virtual node.
            if (curTree.IsVirtual)
            {
                // if epsilonableTree is null can't build Ast.
                if (epsilonableTree == null) throw new Exception();

                bTracking = false;
                epsilonableTree.Clear();

                return result;
            }

            if (curTree is ParseTreeTerminal)
            {
                var ast = curTree.ToAst;
                if (ast != null) newParentTree.Add(ast);

                return result;
            }

            // if curTree is ParseTreeNonTerminal
            var convertedParentTree = curTree as ParseTreeNonTerminal;

            if (convertedParentTree.SignPost.MeaningUnit != null)
            {
                result = new AstNonTerminal(curTree as ParseTreeNonTerminal);

                if (result.SignPost.IsInduceEpsilon) epsilonableTree = result;
                // if 'curTree' is Ast and Ast doesn't exist of the children tree then it(curTree) can same that induce epsilon in the Ast rule.
                else if (convertedParentTree.HasMeaningChildren == false) epsilonableTree = result;

                if (newParentTree == null) newParentTree = result;
                else if (newParentTree != result)
                {
                    newParentTree.Add(result);
                    newParentTree = result;
                }
            }

            // it can't use Parallel because order.
            foreach (var node in convertedParentTree)
            {
                if (bTracking) CreateAst(newParentTree, epsilonableTree, node, ref bTracking);

                // if a bTracking value was changed in the CreateAst function.
                if (bTracking == false)
                {
                    // cancel tracking until curTree is a parent of the epsilonableTree.
                    if (convertedParentTree.SignPost == epsilonableTree.SignPost) bTracking = true;
                    break;
                }
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
