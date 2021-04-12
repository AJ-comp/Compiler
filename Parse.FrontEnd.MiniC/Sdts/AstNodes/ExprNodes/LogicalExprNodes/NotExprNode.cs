using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class NotExprNode : CompareExprNode, ICompareExpression
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.EQ;
        

        public new IRExpr Right => new IntConstant(0) as IRLiteralExpr;


        public NotExprNode(AstSymbol node) : base(node)
        {

        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if(!(Result is BitConstant))
            {
                AddMCL0022Error();
            }

            return this;
        }


        private void AddMCL0022Error()
        {
            ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(LeftNode.AllTokens,
                                                nameof(AlarmCodes.MCL0022),
                                                string.Format(AlarmCodes.MCL0022))
            );
        }
    }
}
