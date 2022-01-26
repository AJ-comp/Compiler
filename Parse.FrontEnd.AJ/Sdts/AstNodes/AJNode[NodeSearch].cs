using Parse.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract partial class AJNode
    {
        /// <summary>
        /// This function returns all referable symbols. <br/>
        /// 참조 가능한 모든 symbol들을 가져옵니다. <br/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ISymbolData> GetReferableSymbols(bool bIncludeVirtual = false)
        {
            var transNode = this;
            List<ISymbolData> result = new List<ISymbolData>();

            while (transNode != null)
            {
                if (transNode is ISymbolCenter)
                {
                    if(bIncludeVirtual) result.AddRangeExceptNull((transNode as ISymbolCenter).SymbolList);
                    else result.AddRangeExceptNull((transNode as ISymbolCenter).SymbolList.Where(s => s.NameToken.IsVirtual == false));
                }

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
                if (!(curNode is ISymbolCenter))
                {
                    curNode = curNode.Parent as AJNode;
                    continue;
                }

                var symbolCenter = curNode as ISymbolCenter;
                foreach (var symbol in symbolCenter.SymbolList)
                {
                    if (symbol.NameToken.Input != toFindIdentToken.Input) continue;

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
                if (symbol.NameToken.Input == toFindIdentToken.Input) result.Add(symbol);
            }

            return result;
        }
    }
}
