using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class WhileStatementNode : StatementNode
    {
        public WhileStatementNode(AstSymbol node) : base(node)
        {
        }



        // [0] : while (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            Items[1].Build(param);
            Items[2].Build(param.CloneForNewBlock());

            return this;
        }
    }
}
