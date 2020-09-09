using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class DefinePrepNode : MiniCNode
    {
        public DefinePrepNode(AstSymbol node) : base(node)
        {
        }

        // #define [0]
        // Ident [1]
        // ExprNode [2]
        public override SdtsNode Build(SdtsParams param)
        {
            Items[1].Build(param);
            Items[2].Build(param);

            return this;
        }
    }
}
