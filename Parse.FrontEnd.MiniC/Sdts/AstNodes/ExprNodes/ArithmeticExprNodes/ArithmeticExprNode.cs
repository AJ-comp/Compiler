using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class ArithmeticExprNode : BinaryExprNode, IArithmeticExpression
    {
        protected ArithmeticExprNode(AstSymbol node) : base(node)
        {
        }


        // for interface ************************************/
        public abstract IROperation Operation { get; }
        public IExprExpression Left { get; private set; }
        public IExprExpression Right { get; private set; }
        /**********************************************/

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            Result = ArithimeticOperation(Operation);
            Right = RightNode;

            return this;
        }
    }
}
