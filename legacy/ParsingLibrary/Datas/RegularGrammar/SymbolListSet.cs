using System.Collections.Generic;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public class SymbolListSet : HashSet<SymbolList>
    {
        public void AddAsConcat(params Symbol[] symbols)
        {
            this.Add(new SymbolList(symbols));
        }

        public void AddAsAlter(params Symbol[] symbols)
        {
            foreach (var symbol in symbols) this.Add(new SymbolList(symbol));
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
    }
}
