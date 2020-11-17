using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class AndExprNode : LogicalExprNode
    {
        public AndExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if (IsCalculateable)
            {
//                if(LeftData.Value.IsZero || RightData.Value.IsZero)
//                    ;
            }

            return this;
        }
    }
}
