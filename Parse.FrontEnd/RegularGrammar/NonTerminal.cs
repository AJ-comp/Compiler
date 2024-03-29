﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.RegularGrammar
{
    public class NonTerminal : Symbol, IEnumerable<NonTerminalSingle>, ICloneable
    {
        private int position = -1;
        private NonTerminalAlter alters = new NonTerminalAlter();

        public bool IsStartSymbol { get; internal set; } = false;
        public bool AutoGenerated { get; } = false;
        public bool IsInduceEpsilon => alters.IsInduceEpsilon;
        public string Name { get; }
        public int Count => this.alters.Count;

        public NonTerminalSingle this[int index]
        {
            get
            {
                var concat = this.ElementAt(index);

                return new NonTerminalSingle(this, index, concat.Priority, concat.MeaningUnit);
            }
        }

        public NonTerminal(string name, bool bStartSymbol = false, bool autoGenerated = false)
        {
            this.IsStartSymbol = bStartSymbol;
            this.Name = name;
            this.AutoGenerated = autoGenerated;
        }

        public NonTerminal(NonTerminal target)
        {
            this.UniqueKey = target.UniqueKey;

            this.IsStartSymbol = target.IsStartSymbol;
            this.Name = target.Name;
            this.AutoGenerated = target.AutoGenerated;
        }

        public NonTerminal(NonTerminalSingle single)
        {
            this.UniqueKey = single.UniqueKey;

            this.IsStartSymbol = single.IsStartSymbol;
            this.Name = single.Name;
            this.AutoGenerated = single.AutoGenerated;
        }

        public bool IsSubSet(NonTerminalSingle singleNT) => this.UniqueKey == singleNT.UniqueKey;

        /// <summary>
        /// Explore to check whether exist the item in the tree
        /// </summary>
        /// <param name="item"></param>
        /// <param name="breakSet"></param>
        /// <returns></returns>
        public bool IsContain(Symbol item, HashSet<NonTerminal> breakSet = null)
        {
            if (breakSet == null) breakSet = new HashSet<NonTerminal>();
            breakSet.Add(this);

            bool result = false;

            foreach (var symbolList in this.alters)
            {
                foreach (var symbol in symbolList)
                {
                    if (symbol == item)
                    {
                        result = true;
                        break;
                    }

                    if (symbol is Terminal) continue;

                    NonTerminal ntSymbol = symbol as NonTerminal;

                    // skip if it was searched the item
                    if (breakSet.Contains(ntSymbol)) continue;
                    result = ntSymbol.IsContain(item, breakSet);
                    if (result) break;
                }
            }

            return result;
        }

        public bool IsExistRefContent(NonTerminal target)
        {
            foreach (var symbolList in this.alters)
            {
                foreach (var symbol in symbolList)
                {
                    if (symbol is Terminal) continue;

                    NonTerminal nonTerminal = symbol as NonTerminal;

                    if (nonTerminal == target) return true;
                    else if (nonTerminal.IsExistRefContent(target)) return true;
                }
            }

            return false;
        }

        public void Clear() => this.alters.Clear();

        public void Add(NonTerminalSingle singleNT) => this.alters.Add(singleNT.ToNonTerminalConcat());
        public void Add(NonTerminalConcat symbols) => this.alters.Add(symbols);
        public void Add(params Symbol[] symbols) => this.alters.Add(new NonTerminalConcat(symbols));
        public void AddAsConcat(params Symbol[] symbols) => this.alters.AddAsConcat(symbols);
        public void AddAsAlter(params Symbol[] symbols) => this.alters.AddAsAlter(symbols);
        public void UnionWith(IEnumerable<NonTerminalConcat> other) => this.alters.UnionWith(other);

        public NonTerminalConcat ElementAt(int index) => this.alters.ElementAt(index);

        /// <summary>
        /// It allocate the children value of Nonterminal
        /// </summary>
        /// <param name="treeBlockRoot"></param>
        /// <remarks>https://www.lucidchart.com/documents/edit/332a9afe-d053-4c13-ab2a-7110f25bff73/0</remarks>
        public void SetItem(NonTerminal treeBlockRoot, MeaningUnit meaningUnit = null)
        {
            this.alters.Clear();

            this.alters.Add(new NonTerminalConcat(treeBlockRoot));

            Optimizer.OptSingleChildNode(this);
            Optimizer.OptConcatNode(this);
            Optimizer.OptAltNode(this);
        }

        public void AddItem(NonTerminal treeBlockRoot, MeaningUnit meaningUnit = null)
        {
            this.AddItem(treeBlockRoot, 0, meaningUnit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeBlockRoot"></param>
        /// <param name="priority">A low value means higher priority</param>
        /// <param name="meaningUnit"></param>
        public void AddItem(NonTerminal treeBlockRoot, uint priority, MeaningUnit meaningUnit = null)
        {
            int prevCnt = this.Count;
            this.alters.Add(new NonTerminalConcat(treeBlockRoot));

            Optimizer.OptSingleChildNode(this);
            Optimizer.OptConcatNode(this);
            Optimizer.OptAltNode(this);

            // 최적화가 끝난 (필요없는 노드 삭제) 이후 우선순위와 의미단위를 넣는다
            for (int i = prevCnt; i < this.Count; i++)
            {
                this.ElementAt(i).Priority = priority;
                this.ElementAt(i).MeaningUnit = meaningUnit;
            }
        }

        public void AddItem(Terminal item, MeaningUnit meaningUnit = null)
        {
            int prevCnt = this.Count;
            this.alters.Add(new NonTerminalConcat(item));

            for (int i = prevCnt; i < this.Count; i++)
                this.ElementAt(i).MeaningUnit = meaningUnit;
        }

        public void SetChildrenOfItem(NonTerminal item)
        {
            this.alters.Clear();
            this.alters.UnionWith(item.alters);
        }

        public void SetChildren(Terminal item)
        {
            this.alters.Clear();
            this.alters.Add(new NonTerminalConcat(0, item));
        }

        public void Replace(NonTerminal from, NonTerminal to, SymbolSet breakSet = null)
        {
            if (breakSet == null) breakSet = new SymbolSet();
            breakSet.Add(this);

            NonTerminal temp = this.Clone() as NonTerminal;
            this.alters.Clear();

            foreach (var concat in temp.alters)
            {
                NonTerminalConcat addList = concat.Clone() as NonTerminalConcat;
                addList.Clear();

                foreach (var symbol in concat)
                {
                    if (symbol is Terminal) { addList.Add(symbol); continue; }

                    NonTerminal ntChild = symbol as NonTerminal;
                    if (ntChild == from) { addList.Add(to); continue; }
                    else addList.Add(ntChild);

                    if (ntChild == to) continue;
                    if (breakSet.Contains(ntChild)) continue;

                    ntChild.Replace(from, to, breakSet); // 아래 depth로 이동
                }

                this.alters.Add(addList);
            }
        }

        public HashSet<NonTerminal> ToNonTerminalSet() => this.alters.ToNonTerminalSet();


        public bool Equals(NonTerminal other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return this.UniqueKey == other.UniqueKey;
        }


        /// <summary>
        /// Get Ebnf string.
        /// </summary>
        /// <remarks>
        /// example :   <br/>
        /// usingDcl -> using Identifier       <br/>
        /// G1 -> usingDcl.ZeroOrMore()      <br/>
        /// 1. In usingDcl case
        /// if bContainLHS is true the result is "usingDcl ::= 'using' 'Identifier' ".  <br/>
        /// if bContainLHS is false the result is "usingDcl".   <br/>
        /// 2. In G1 case
        /// The EbnfString of G1 is set to usingDcl? by usingDcl.ZeroOrMore().
        /// if bContainLHS is false the result is "usingDcl?" because the EbnfString of G1 is set. Not "G1".   <br/>
        /// if bContainLHS is true the result is "G1 ::= "usingDcl?" because the EbnfString of G1 is set.   <br/>
        /// </remarks>
        /// <param name="bContainLHS"></param>
        /// <returns></returns>
        public override string ToEbnfString(bool bContainLHS = false)
        {
            if (bContainLHS)
            {
                string result = $"{Name} ::= ";

                if (AutoGenerated) result += EbnfString;
                else
                {
                    // items
                    for(int i=0; i<alters.Count; i++)
                    {
                        var sep = (i == alters.Count - 1) ? "" : "| ";
                        result += alters.ElementAt(i).ToEbnfString() + sep;
                    }
                }

                return result;
            }
            else
            {
                return (AutoGenerated) ? EbnfString : Name;
            }
        }


        public override string ToGrammarString()
        {
            string result = this.Name + " -> ";

            foreach (var symbolList in this.alters)
            {
                //                if (this.Children.Count > 1 && symbolList.Count > 1) result += "(";
                result += symbolList.ToString();
                //                if (this.Children.Count > 1 && symbolList.Count > 1) result += ")";

                result += Convert.ToBridgeSymbol(BridgeType.Alternation);
            }

            return result.Substring(0, result.Length - Convert.ToBridgeSymbol(BridgeType.Alternation).Length);
        }

        public override string ToTreeString(ushort depth = 1)
        {
            string result = string.Empty;

            for (int i = 1; i < depth; i++) result += "  ";

            result += "Nonterminal : " + this.Name + Environment.NewLine;

            foreach (var symbolList in this.alters)
            {
                foreach (var symbol in symbolList)
                {
                    for (int i = 1; i < depth; i++) result += "  ";
                    result += "  ";

                    if (symbol is Terminal) result += "Terminal : " + symbol.ToString();
                    else result += "NonTerminal : " + symbol.ToString();

                    result += Environment.NewLine;
                }
            }

            return result;
        }

        public override string ToString() => this.Name;

        public bool MoveNext()
        {
            if (this.position == this.alters.Count - 1)
            {
                this.Reset();
                return false;
            }
            return (++position < this.alters.Count);
        }

        public void Reset() => this.position = -1;

        IEnumerator<NonTerminalSingle> IEnumerable<NonTerminalSingle>.GetEnumerator()
        {
            for (int i = 0; i < this.alters.Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            NonTerminal result = new NonTerminal(this);

            result.alters.UnionWith(this.alters);

            return result;
        }
    }
}
