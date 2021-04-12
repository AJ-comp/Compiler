using Parse.Extensions;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public abstract class SdtsNode
    {
        public string NodeName => GetType().Name;
        public bool IsNeedWhileIRGeneration { get; protected set; } = false;
        public AstSymbol Ast { get; protected set; }
        public SdtsNode Parent { get; set; }
        public List<SdtsNode> Items { get; } = new List<SdtsNode>();
        public MeaningErrInfoList ConnectedErrInfoList { get; } = new MeaningErrInfoList();
        public IReadOnlyList<TokenData> MeaningTokens => Ast.AllTokens;
        public IReadOnlyList<TokenData> AllTokens => Ast.ConnectedParseTree.AllTokens;

        public bool IsBuild { get; protected set; }


        public SdtsNode GetParent(Type toFindParent)
        {
            var travNode = this;

            while (travNode != null)
            {
                if (travNode.GetType() == toFindParent) break;

                travNode = travNode.Parent;
            }

            return travNode;
        }


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

        public IEnumerable<SdtsNode> GetMatchedTypeNodes(Type type)
        {
            List<SdtsNode> result = new List<SdtsNode>();

            foreach (var child in Items)
            {
                if (child.GetType() == type) result.Add(child);

                result.AddRange(child.GetMatchedTypeNodes(type));
            }

            return result;
        }

        public abstract SdtsNode Build(SdtsParams param);

        public override string ToString()
        {
            string result = string.Format("Ast: {0}, Error node count: {1}", Ast, ErrNodes.Count);

            if (Items.Count > 0)
            {
                result += string.Format(", Expression: {0} -> ", NodeName);
                result += Items.ItemsString(PrintType.Type);
            }

            return result;
        }
    }
}
