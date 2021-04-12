using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public abstract class AssignExprNode : BinaryExprNode
    {
        public VariableMiniC LeftVar { get; private set; }

        public AssignExprNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public abstract void LeftIsIdentProcess();

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if (LeftNode is UseIdentNode)
            {
                LeftVar = (LeftNode as UseIdentNode).VarData;

                // case for not declared.
                if (LeftVar is null)
                {
                    AddException(nameof(AlarmCodes.MCL0001),
                                        string.Format(AlarmCodes.MCL0001, (LeftNode as UseIdentNode).IdentToken));

                    return this;
                }

                LeftIsIdentProcess();
            }
            else if(LeftNode is DeRefExprNode)
            {
            }
            else
            {
                AddException(nameof(AlarmCodes.MCL0004), AlarmCodes.MCL0004);
            }

            return this;
        }

        protected void AddException(string exceptionName, string exceptionMessage)
        {
            LeftNode.ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(LeftNode.MeaningTokens, exceptionName, exceptionMessage)
            );
        }
    }
}
