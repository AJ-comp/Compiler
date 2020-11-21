using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class UsingStNode : StatementNode
    {
        public UsingStNode(AstSymbol node) : base(node)
        {
        }

        // [0] = Ident [TerminalNode]
        public override SdtsNode Build(SdtsParams param)
        {
            Items[0].Build(param);

            return this;
        }
    }
}
