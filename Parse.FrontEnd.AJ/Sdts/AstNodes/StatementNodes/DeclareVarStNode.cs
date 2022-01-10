using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class DeclareVarStNode : StatementNode
    {
        public DeclareVarStNode(AstSymbol node) : base(node)
        {
        }

        /// <summary>
        /// [0] : DeclareVar
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override SdtsNode Compile(CompileParameter param)
        {
            var dclVar = Items[0].Compile(param) as DeclareVarNode;

            _varList.Add(dclVar.Variable);

            return this;
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
