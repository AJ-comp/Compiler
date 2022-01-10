using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class LambdaStNode : StatementNode, IRootable
    {
        public bool IsRoot => !(Parent is StatementNode);

        public LambdaStNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            throw new NotImplementedException();
        }

        public override IRExpression To()
        {
            throw new NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
