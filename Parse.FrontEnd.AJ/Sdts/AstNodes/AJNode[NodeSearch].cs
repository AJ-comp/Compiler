using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode
    {
        public IEnumerable<ISymbolData> GetReferableSymbols()
        {
            var transNode = this;
            List<ISymbolData> result = new List<ISymbolData>();

            while (transNode != null)
            {
                if (transNode is IHasSymbol)
                    result.AddRangeExceptNull((transNode as IHasSymbol).SymbolList);

                transNode = transNode.Parent as AJNode;
            }

            return result;
        }


        public ISymbolData GetSymbol(TokenData toFindIdentToken)
        {
            if (toFindIdentToken == null) return null;

            ISymbolData result = null;
            AJNode curNode = this;

            while (true)
            {
                if (curNode == null) break;
                if (!(curNode is IHasSymbol))
                {
                    curNode = curNode.Parent as AJNode;
                    continue;
                }

                var symbolList = curNode as IHasSymbol;
                foreach (var symbol in symbolList.SymbolList)
                {
                    if (symbol.Name != toFindIdentToken.Input) continue;

                    result = symbol;
                    break;
                }

                if (result != null) break;  // found
                else curNode = curNode.Parent as AJNode;
            }

            return result;
        }


        public IEnumerable<ISymbolData> GetSymbols(TokenData toFindIdentToken)
        {
            var symbols = GetReferableSymbols();
            var result = new List<ISymbolData>();

            foreach (var symbol in symbols)
            {
                if (symbol.Name == toFindIdentToken.Input) result.Add(symbol);
            }

            return result;
        }
    }
}
