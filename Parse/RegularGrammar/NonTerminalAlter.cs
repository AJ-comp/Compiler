using System.Collections;
using System.Collections.Generic;

namespace Parse.RegularGrammar
{
    public class NonTerminalAlter : ISet<NonTerminalConcat>
    {
        private HashSet<NonTerminalConcat> concatSymbols = new HashSet<NonTerminalConcat>();

        public int Count => ((ISet<NonTerminalConcat>)concatSymbols).Count;

        public bool IsReadOnly => ((ISet<NonTerminalConcat>)concatSymbols).IsReadOnly;

        public void AddAsConcat(params Symbol[] symbols)
        {
            this.Add(new NonTerminalConcat(symbols));
        }

        public void AddAsAlter(params Symbol[] symbols)
        {
            foreach (var symbol in symbols) this.Add(new NonTerminalConcat(symbol));
        }

        public void AddSymbols(params Symbol[] symbols)
        {
            // 모든 노드(List)에 대해 추가한다
            foreach (var symbolList in this)
            {
                foreach(var symbol in symbols)  symbolList.Add(symbol);
            }
        }

        public void InsertSymbol(int index, params Symbol[] symbols)
        {
            // 모든 노드(List)에 대해 추가한다
            foreach (var symbolList in this)
            {
                foreach(var symbol in symbols)  symbolList.Insert(index, symbol);
            }
        }

        /// <summary>
        /// SymbolSet에 존재하는 NonTerminal 집합을 리턴한다 (1depth)
        /// </summary>
        /// <returns></returns>
        public HashSet<NonTerminal> ToNonTerminalSet()
        {
            HashSet<NonTerminal> result = new HashSet<NonTerminal>();

            foreach(var symbolList in this)
            {
                foreach(var symbol in symbolList)
                {
                    if (symbol is Terminal) continue;

                    result.Add((symbol as NonTerminal));
                }
            }

            return result;
        }

        public bool Add(NonTerminalConcat item)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).Add(item);
        }

        public void UnionWith(IEnumerable<NonTerminalConcat> other)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).UnionWith(other);
        }

        public void IntersectWith(IEnumerable<NonTerminalConcat> other)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<NonTerminalConcat> other)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<NonTerminalConcat> other)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).Overlaps(other);
        }

        public bool SetEquals(IEnumerable<NonTerminalConcat> other)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).SetEquals(other);
        }

        void ICollection<NonTerminalConcat>.Add(NonTerminalConcat item)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).Add(item);
        }

        public void Clear()
        {
            ((ISet<NonTerminalConcat>)concatSymbols).Clear();
        }

        public bool Contains(NonTerminalConcat item)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).Contains(item);
        }

        public void CopyTo(NonTerminalConcat[] array, int arrayIndex)
        {
            ((ISet<NonTerminalConcat>)concatSymbols).CopyTo(array, arrayIndex);
        }

        public bool Remove(NonTerminalConcat item)
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).Remove(item);
        }

        public IEnumerator<NonTerminalConcat> GetEnumerator()
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ISet<NonTerminalConcat>)concatSymbols).GetEnumerator();
        }
    }
}
