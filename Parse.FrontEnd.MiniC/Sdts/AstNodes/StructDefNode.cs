﻿using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class StructDefNode : MiniCNode
    {
        public StructDefNode(AstSymbol node) : base(node)
        {
        }

        // [0] : Ident [TerminalNode]
        // [1] : declaration
        public override SdtsNode Build(SdtsParams param)
        {
            Items[0].Build(param);
            Items[1].Build(param);

            return this;
        }
    }
}
