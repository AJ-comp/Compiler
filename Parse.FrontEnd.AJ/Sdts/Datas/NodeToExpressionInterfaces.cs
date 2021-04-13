using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IExpression
    {
    }

    public interface IUsingExpression : IExpression, IHasName
    {
    }


    public interface INamespaceExpression : IExpression, IHasName
    {
        IEnumerable<IClassExpression> ClassDatas { get; }
    }


    public interface IClassExpression : IExpression, IHasName
    {
        IEnumerable<IDeclareVarExpression> VarList { get; }
        IEnumerable<IFunctionExpression> FuncList { get; }
    }


    public interface IFunctionExpression : IExpression, IHasName
    {
        IClassExpression PartyInfo { get; }
        bool IsStatic { get; }
        bool IsConstReturn { get; }
        MiniCDataType ReturnType { get; }
        IEnumerable<IDeclareVarExpression> ParamVars { get; }
        IStmtExpression Statement { get; }
    }


    public interface IDeclareVarExpression : IExpression, IHasDclVarProperties, IHasName
    {
        bool IsStatic { get; }
        bool IsConst { get; }
        string TypeName { get; }

        IExprExpression InitialExpr { get; }


        // 먼 훗날 가상코드 생성 기능을 위해 레퍼런스 리스트가 필요함
    }

    #region 식에 대한 인터페이스
    /// *****************************/
    /// <summary>
    /// 식 기능을 위한 인터페이스 입니다.
    /// </summary>
    /// *****************************/
    public interface IExprExpression : IExpression
    {
        IConstant Result { get; }
    }


    /// *****************************/
    /// <summary>
    /// 산술식을 위한 인터페이스입니다.
    /// </summary>
    /// *****************************/
    public interface IArithmeticExpression : IExprExpression
    {
        IROperation Operation { get; }
        IExprExpression Left { get; }
        IExprExpression Right { get; }
    }


    /// *****************************/
    /// <summary>
    /// 비교식을 위한 인터페이스입니다.
    /// </summary>
    /// *****************************/
    public interface ICompareExpression : IExprExpression
    {
        IRCompareSymbol CompareOper { get; }
        IExprExpression Left { get; }
        IExprExpression Right { get; }
    }


    /// *****************************/
    /// <summary>
    /// 할당식을 위한 인터페이스입니다.
    /// </summary>
    /// *****************************/
    public interface IAssignExpression : IExprExpression
    {
        IDeclareVarExpression Left { get; }
        IExprExpression Right { get; }
    }


    /// *****************************/
    /// <summary>
    /// 전위 후위 식을 위한 인터페이스입니다.
    /// </summary>
    /// *****************************/
    public interface IIncDecExpression : IExprExpression
    {
        Info ProcessInfo { get; }
        IDeclareVarExpression Var { get; }
    }


    /// **************************************/
    /// <summary>
    /// 산술 연산 후 할당식을 위한 인터페이스입니다.
    /// </summary>
    /// **************************************/
    public interface IArithmeticAssignExpression : IAssignExpression
    {
        IROperation Operation { get; }
    }


    /// ***************************/
    /// <summary>
    /// 호출식을 위한 인터페이스 입니다.
    /// </summary>
    /// ***************************/
    public interface ICallExpression : IExpression
    {
        IFunctionExpression CallFuncDef { get; }
    }

    public interface IUseIdentExpression : IExprExpression
    {
        IDeclareVarExpression Var { get; }
        IFunctionExpression Func { get; }
    }


    #endregion

    #region 구문에 대한 인터페이스
    public interface IStmtExpression : IExpression
    {
    }

    /// ************************************/
    /// <summary>
    /// 복합구문을 위한 인터페이스입니다.
    /// </summary>
    /// ************************************/
    public interface ICompoundStmtExpression : IStmtExpression
    {
        IEnumerable<IDeclareVarExpression> LocalVars { get; }
        IEnumerable<IStmtExpression> Statements { get; }
    }

    // related to statement
    public interface IConditionStatement : IStmtExpression
    {
        ICompareExpression ConditionExpression { get; }
        StatementNode TrueStatement { get; }
        StatementNode FalseStatement { get; }
    }

    public interface IExprTypeStatement : IStmtExpression
    {
        public ExprNode Expr { get; }
    }

    #endregion
}
