using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    public abstract class ExprExpression : AJExpression, IRExpr
    {
        public IConstant Result { get; protected set; }

        protected ExprExpression()
        {
        }

        protected ExprExpression(IRExpr expr)
        {
            Result = expr.Result;
        }

        protected ExprExpression(IExprExpression expr)
        {
            Result = expr.Result;
        }

        public static ExprExpression Create(IRExpr node)
        {
            // use literal
            if (node is IRBitLiteralExpr)
                return new LiteralBoolExpression(node as IRBitLiteralExpr);
            if (node is IRCharLiteralExpr)
                return new LiteralCharExpression(node as IRCharLiteralExpr);
            if (node is IRInt32LiteralExpr)
                return new LiteralIntegerExpression(node as IRInt32LiteralExpr);
            if (node is IRDoubleLiteralExpr)
                return new LiteralDoubleExpression(node as IRDoubleLiteralExpr);
            if (node is IRStringLiteralExpr)
                return new LiteralStringExpression(node as IRStringLiteralExpr);

            throw new System.Exception();
        }


        public static ExprExpression Create(IExprExpression node)
        {
            // use var
            if (node is IUseIdentExpression)
                return new UseIdentExpression(node as IUseIdentExpression);

            // binary operation
            if (node is IArithmeticExpression)
                return new ArithmeticExpression(node as IArithmeticExpression);
            if (node is IAssignExpression)
                return new AssignExpression(node as IAssignExpression);
            if (node is IArithmeticAssignExpression)
                return new ArithmeticAssignExpression(node as IArithmeticAssignExpression);
            if (node is ICompareExpression)
                return new ConditionExpression(node as ICompareExpression);

            // single operation
            if (node is IIncDecExpression)
                return new IncDecExpression(node as IIncDecExpression);




            throw new System.Exception();
        }

        public static ExprExpression Create(IArithmeticExpression expression) => new ArithmeticExpression(expression);
        public static ExprExpression Create(ICallExpression callExpression) => new CallExpression(callExpression);
    }
}
