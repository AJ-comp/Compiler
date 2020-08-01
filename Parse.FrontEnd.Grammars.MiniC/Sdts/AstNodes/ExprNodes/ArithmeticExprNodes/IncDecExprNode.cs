using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class IncDecExprNode : SingleExprNode
    {
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

            var varNode = Items[0] as UseIdentNode;
            if (varNode.VarData is IString)
            {
                AddMCL0013Exception("--");
                return this;
            }

            return this;
        }
    }
}
