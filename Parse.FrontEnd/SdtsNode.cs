using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public abstract class SdtsNode
    {
        public bool IsNeedWhileIRGeneration { get; protected set; } = false;
        public AstSymbol Ast { get; protected set; }
        public SdtsNode Parent { get; set; }
        public List<SdtsNode> Items { get; } = new List<SdtsNode>();
        public MeaningErrInfoList ConnectedErrInfoList { get; } = new MeaningErrInfoList();
        public IReadOnlyList<TokenData> MeaningTokens => Ast.AllTokens;
        public IReadOnlyList<TokenData> AllTokens => Ast.ConnectedParseTree.AllTokens;

        public bool IsBuild { get; protected set; }

        public IReadOnlyList<SdtsNode> ErrNodes
        {
            get
            {
                List<SdtsNode> result = new List<SdtsNode>();

                foreach (var item in Items)
                {
                    if (item.ConnectedErrInfoList.Count > 0) result.Add(item);
                    result.AddRange(item.ErrNodes);
                }

                return result;
            }
        }

        public abstract SdtsNode Build(SdtsParams param);
    }
}
