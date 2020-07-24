using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class CallNode : MiniCNode
    {
        public CallNode(AstSymbol node) : base(node)
        {
        }



        // [0] : Ident (AstTerminal)
        // [1] : ActualParam (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            // build ActualParam node
            var result = Items[0].Build(param);

            return this;
        }
    }
}
