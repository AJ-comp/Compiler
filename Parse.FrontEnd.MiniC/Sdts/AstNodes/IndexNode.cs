using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class IndexNode : MiniCNode
    {
        public IndexNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // [0] : VarNode (AstNonTerminal)
        // [1] : Exp (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            throw new NotImplementedException();
        }
    }
}
