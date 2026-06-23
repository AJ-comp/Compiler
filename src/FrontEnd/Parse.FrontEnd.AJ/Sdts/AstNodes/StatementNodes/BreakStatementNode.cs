using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class BreakStatementNode : StatementNode
    {
        public BreakStatementNode(AstSymbol node) : base(node)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            var repeatableStatement = GetParentAs(typeof(RoopStatementNode)) as RoopStatementNode;

            // The break keyword can be used only if in roop statement.
            if (repeatableStatement == null)
            {
                Alarms.Add(AJAlarmFactory.CreateAJ0049(this));
            }
            else
            {
                repeatableStatement.IncludeBreak = true;
            }

            return this;
        }

        public override IRExpression To() => new IRControlStatement(IRControlType.Break);

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
