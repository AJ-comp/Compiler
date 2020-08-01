using Parse.FrontEnd.Ast;

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

            if (Left is UseIdentNode) CheckLogic(Left as UseIdentNode);
            if (Right is UseIdentNode) CheckLogic(Right as UseIdentNode);

            return this;
        }

        private void CheckLogic(UseIdentNode varNode)
        {
            var varData = MiniCUtilities.GetVarDataFromReferableST(this, varNode.IdentToken);
            if (varData == null) return;

            if (!varData.IsVirtual)
            {
                // Check
                MiniCChecker.IsUsedNotInitValue(this, varData);
            }
        }
    }
}
