using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class UsingStNode : StatementNode, IUsingExpression
    {
        public string Name { get; private set; }

        public UsingStNode(AstSymbol node) : base(node)
        {
        }

        // [0] = Ident [TerminalNode]
        public override SdtsNode Build(SdtsParams param)
        {
            var node = (Items[0].Build(param) as TerminalNode);
            Name = node.Token.Input;

            return this;
        }
    }
}
