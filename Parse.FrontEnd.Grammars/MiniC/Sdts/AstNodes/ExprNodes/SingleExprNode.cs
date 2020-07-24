using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class SingleExprNode : ExprNode
    {
        public TokenData Token { get; private set; }
        public ValueData Value { get; private set; }

        public SingleExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            throw new NotImplementedException();
        }
    }
}
