using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.MiniC.Properties;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class SingleExprNode : ExprNode
    {
        public TokenData Token { get; private set; }

        public SingleExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            Items[0].Build(param);

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
