using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public abstract class LogicalExprNode : BinaryExprNode
    {

        protected LogicalExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            CheckLogic(LeftToken);
            CheckLogic(RightToken);

            return this;
        }

        private void CheckLogic(TokenData tokenToCheck)
        {
            var varData = MiniCUtilities.GetVarDataFromReferableST(this, tokenToCheck);
            if (!varData.IsVirtual)
            {
                // Check
                MiniCChecker.IsUsedNotInitValue(this, varData);
            }
        }
    }
}
