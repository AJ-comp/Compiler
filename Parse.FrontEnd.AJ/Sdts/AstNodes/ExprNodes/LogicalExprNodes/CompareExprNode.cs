using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public abstract class CompareExprNode : BinaryExprNode, ICompareExpression
    {
        /* for interface ********************************************/
        public abstract IRCompareSymbol CompareOper { get; }
        public IExprExpression Left => LeftNode;
        public IExprExpression Right => RightNode;
        /******************************************************/


        protected CompareExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            try
            {
                if (CompareOper == IRCompareSymbol.EQ)
                {
                    Result = Constant.Equal(Left.Result, Right.Result);
                }
                else if (CompareOper == IRCompareSymbol.NE)
                {
                    Result = Constant.NotEqual(Left.Result, Right.Result);
                }
            }

            catch
            {
                AddMCL0023Error();
            }

            return this;
        }

        private void AddMCL0023Error()
        {
            ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(AllTokens,
                                                nameof(AlarmCodes.MCL0023),
                                                string.Format(AlarmCodes.MCL0023, 
                                                                     AJUtilities.ToSymbolString(CompareOper),
                                                                     Left.Result.TypeKind,
                                                                     Right.Result.TypeKind))
            );
        }
    }
}
