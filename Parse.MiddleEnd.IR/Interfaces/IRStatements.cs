using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Interfaces
{
    public interface IRStatement
    {
    }

    public interface IRCompoundStatement : IRStatement
    {
        IEnumerable<IRDeclareVar> LocalVars { get; }
        IEnumerable<IRStatement> Statements { get; }
    }

    public interface IRExprStatement : IRStatement
    {
        IRExpr Expression { get; }
    }

    public interface IRIFStatement : IRStatement
    {
        IRCompareOpExpr CondExpr { get; }
        IRStatement TrueStatement { get; }
    }

    public interface IRIFElseStatement : IRIFStatement
    {
        IRStatement FalseStatement { get; }
    }

    public interface IRWhileStatement : IRStatement
    {
        IRCompareOpExpr CondExpr { get; }
        IRStatement TrueStatement { get; }
    }
}
