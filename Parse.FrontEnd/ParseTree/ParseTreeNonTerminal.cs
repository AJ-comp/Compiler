using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

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
                if (SignPost.IsInduceEpsilon) return false;
                if (_symbols.Count == 0) return false;

                foreach(var item in _symbols)
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

                    else if(item is ParseTreeNonTerminal)
                    {
                        if ((item as ParseTreeNonTerminal).SignPost.MeaningUnit != null)
                            return true;
                    }
                }

                return false;
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
                bool bTracking = true;
                return CreateAst(null, null, this, ref bTracking);
            }
        }

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
                result = new AstNonTerminal(convertedParentTree.SignPost);
                result.ConnectedParseTree = curTree as ParseTreeNonTerminal;

                if (result.SignPost.IsInduceEpsilon) epsilonableTree = result;
                // if 'curTree' is Ast and Ast doesn't exist of the children tree then it(curTree) can same that induce epsilon in the Ast rule.
                else if (convertedParentTree.HasMeaningChildren == false) epsilonableTree = result;

                if (newParentTree == null) newParentTree = result;
                else if (newParentTree != result)
                {
                    (newParentTree as AstNonTerminal).Add(result);
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
