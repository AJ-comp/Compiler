using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.Types;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single
{
    public abstract class SingleExprNode : ExprNode
    {
        public TokenData Token { get; private set; }
        public ExprNode ExprNode { get; private set; }

        public SingleExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            ExprNode = Items[0].Compile(param) as ExprNode;

            return this;
        }

        protected void AddMCL0012Exception()
        {
            Alarms.Add(new MeaningErrInfo(Items[0].MeaningTokens,
                                                            nameof(AlarmCodes.MCL0012),
                                                            string.Format(AlarmCodes.MCL0012)));
        }
    }
}
