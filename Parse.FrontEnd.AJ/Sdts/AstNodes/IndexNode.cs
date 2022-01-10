﻿using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class IndexNode : AJNode
    {
        public IndexNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // [0] : VarNode (AstNonTerminal)
        // [1] : Exp (AstNonTerminal)
        public override SdtsNode Compile(CompileParameter param)
        {
            throw new NotImplementedException();
        }
    }
}
