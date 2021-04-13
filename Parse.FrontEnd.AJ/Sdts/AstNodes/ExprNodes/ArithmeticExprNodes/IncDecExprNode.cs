using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class IncDecExprNode : SingleExprNode, IIncDecExpression
    {
        public UseIdentNode UseIdentNode { get; private set; }


        // for interface **********************************/
        public IDeclareVarExpression Var => UseIdentNode.Var;
        public abstract Info ProcessInfo { get; }
        /********************************************/


        protected IncDecExprNode(AstSymbol node, string oper) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if (!(Items[0] is UseIdentNode))
            {
                AddMCL0012Exception();
                return this;
            }

            UseIdentNode = Items[0] as UseIdentNode;
            if (UseIdentNode.VarData.DataType == MiniCDataType.String)
            {
                AddMCL0013Exception("--");
                return this;
            }

            return this;
        }
    }
}
