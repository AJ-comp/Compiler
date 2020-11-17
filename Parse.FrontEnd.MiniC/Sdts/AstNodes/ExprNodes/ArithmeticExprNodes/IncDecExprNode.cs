using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class IncDecExprNode : SingleExprNode
    {
        public UseIdentNode UseIdentNode { get; private set; }

        protected IncDecExprNode(AstSymbol node, string oper) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if (!(Items[0] is UseIdentNode))
            {
                AddMCL0012Exception();
                return this;
            }

            UseIdentNode = Items[0] as UseIdentNode;
            if (UseIdentNode.VarData is IString)
            {
                AddMCL0013Exception("--");
                return this;
            }

            return this;
        }
    }
}
