using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single
{
    public sealed class IncDecNode : SingleExprNode, ICanbeStatement
    {
        public Info ProcessInfo { get; }


        public IncDecNode(AstSymbol node, string oper, Info processInfo) : base(node)
        {
            ProcessInfo = processInfo;
            IsNeedWhileIRGeneration = true;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                if (!(Items[0] is UseIdentNode))
                {
                    AddMCL0012Exception();
                    return this;
                }

                if (ProcessInfo == Info.PreInc) Result = ExprNode.Result.PreInc();
                else if (ProcessInfo == Info.PostDec) Result = ExprNode.Result.PostDec();
                else if (ProcessInfo == Info.PreDec) Result = ExprNode.Result.PreDec();
                else if (ProcessInfo == Info.PostInc) Result = ExprNode.Result.PostInc();
            }
            catch(Exception)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0012(this));
            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRSingleExpr();

            if (ProcessInfo == Info.PreInc) result.Operation = IRSingleOperation.PreInc;
            else if(ProcessInfo == Info.PostInc) result.Operation = IRSingleOperation.PostInc;
            else if (ProcessInfo == Info.PreDec) result.Operation = IRSingleOperation.PreDec;
            else if (ProcessInfo == Info.PostDec) result.Operation = IRSingleOperation.PostDec;

            result.Expression = ExprNode.To() as IRExpr;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
