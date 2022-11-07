using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class DeclareVarStNode : StatementNode, IDeclareable
    {
        public DeclareVarStNode(AstSymbol node) : base(node)
        {
        }

        /// <summary>
        /// [0] : DeclareVar
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            var dclVar = Items[0].Compile(param) as DeclareVarNode;

            _varList.Add(dclVar.Variable);

            return this;
        }

        public override IRExpression To()
        {
            List<IRVariable> vars = new List<IRVariable>();
            foreach (var varItem in VarList) vars.Add(varItem.ToIR());

            return new IRDclVarStatement(vars);
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
