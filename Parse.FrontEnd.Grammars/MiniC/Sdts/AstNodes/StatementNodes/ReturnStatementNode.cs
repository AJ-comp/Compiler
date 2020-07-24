using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class ReturnStatementNode : StatementNode
    {
        public ReturnStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            Items[1].Build(param);

            return this;
        }
    }
}
