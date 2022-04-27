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
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            // if there is a prev information remove it.
            base.CompileLogic(param);
            _paramNodeList.Clear();

            foreach (var item in Items)
                _paramNodeList.Add(item.Compile(param) as ExprNode);

            return this;
        }


        private List<ExprNode> _paramNodeList = new List<ExprNode>();
    }
}
