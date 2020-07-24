using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
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
                if(LeftData.Value.IsZero || RightData.Value.IsZero)
                    ;
            }

            return this;
        }
    }
}
