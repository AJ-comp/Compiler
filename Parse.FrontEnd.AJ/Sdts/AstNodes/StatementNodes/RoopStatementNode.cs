using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public abstract class RoopStatementNode : CondStatementNode
    {
        public bool IncludeBreak { get; internal set; } = false;

        protected RoopStatementNode(AstSymbol node) : base(node)
        {
        }
    }
}
