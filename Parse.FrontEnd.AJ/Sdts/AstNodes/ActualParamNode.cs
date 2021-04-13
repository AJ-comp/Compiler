using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ActualParamNode : AJNode
    {
        public IReadOnlyList<ExprNode> ParamNodeList => _paramNodeList;

        public ActualParamNode(AstSymbol node) : base(node)
        {
        }


        // logicalOrExp [LogicalOr]
        public override SdtsNode Build(SdtsParams param)
        {
            // if there is a prev information remove it.
            _paramNodeList.Clear();

            foreach (var item in Items)
                _paramNodeList.Add(item.Build(param) as ExprNode);

            return this;
        }


        private List<ExprNode> _paramNodeList = new List<ExprNode>();
    }
}
