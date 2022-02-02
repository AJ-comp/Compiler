using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Expressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class UsingStNode : StatementNode
    {
        public string Name { get; private set; }

        public UsingStNode(AstSymbol node) : base(node)
        {
        }

        // [0] = Ident [TerminalNode]
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            var node = Items[0].Compile(param) as TerminalNode;
            Name = node.Token.Input;

            return this;
        }

        public override IRExpression To()
        {
            throw new System.NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
