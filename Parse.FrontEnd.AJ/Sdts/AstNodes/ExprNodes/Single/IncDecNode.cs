using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
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

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            try
            {
                if (!(Items[0] is UseIdentNode))
                {
                    AddMCL0012Exception();
                    return this;
                }

                if (ProcessInfo == Info.PreInc) PreInc(ExprNode);
                else if (ProcessInfo == Info.PostDec) PostDec(ExprNode);
                else if (ProcessInfo == Info.PreDec) PreDec(ExprNode);
                else if (ProcessInfo == Info.PostInc) PostInc(ExprNode);
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


        public ExprNode PreInc(ExprNode source)
        {
            if (source.Type == null) return null;
            if (source.ValueState != State.Fixed) return this;

            if (Type.IsIntegerType())
            {
                Value = (int)source.Value + 1;
                ValueState = State.Fixed;
            }

            return this;
        }

        public ExprNode PostInc(ExprNode source)
        {
            if (source.Type == null) return null;
            if (source.ValueState != State.Fixed) return this;

            if (Type.IsIntegerType())
            {
                Value = (int)source.Value;
                ValueState = State.Fixed;
            }

            return this;
        }

        public ExprNode PreDec(ExprNode source)
        {
            if (source.Type == null) return null;
            if (source.ValueState != State.Fixed) return this;

            if (Type.IsIntegerType())
            {
                Value = (int)source.Value - 1;
                ValueState = State.Fixed;
            }

            return this;
        }

        public ExprNode PostDec(ExprNode source)
        {
            if (source.Type == null) return null;
            if (source.ValueState != State.Fixed) return this;

            if (Type.IsIntegerType())
            {
                Value = (int)source.Value;
                ValueState = State.Fixed;
            }

            return this;
        }
    }
}
