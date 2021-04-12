using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class SingleExprNode : ExprNode
    {
        public TokenData Token { get; private set; }
        public ExprNode ExprNode { get; private set; }

        public SingleExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            ExprNode = (Items[0].Build(param) as ExprNode);

            Result = ExprNode.Result;

            return this;
        }

        protected void AddMCL0012Exception()
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(Items[0].MeaningTokens,
                                                    nameof(AlarmCodes.MCL0012),
                                                    string.Format(AlarmCodes.MCL0012))
                );
        }

        protected void AddMCL0013Exception(string oper)
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MeaningTokens,
                                                    nameof(AlarmCodes.MCL0013),
                                                    string.Format(AlarmCodes.MCL0013, oper))
                );
        }
    }
}
