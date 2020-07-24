using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class BinaryExprNode : ExprNode
    {
        public TokenData LeftToken { get; protected set; }
        public TokenData RightToken { get; protected set; }
        public ValueData Result { get; protected set; }

        public MiniCVarData LeftData => MiniCUtilities.GetVarDataFromReferableST(this, LeftToken);
        public MiniCVarData RightData => MiniCUtilities.GetVarDataFromReferableST(this, RightToken);
        public bool IsCalculateable => (!LeftData.IsVirtual && !RightData.IsVirtual);


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            Items[0].Build(param);
            Items[1].Build(param);

            return this;
        }
    }
}
