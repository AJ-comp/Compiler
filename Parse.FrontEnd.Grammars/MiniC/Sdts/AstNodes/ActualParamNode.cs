using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class ActualParamNode : MiniCNode
    {
        public ActualParamNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            foreach (var item in Items) item.Build(param);

            return this;
        }
    }
}
