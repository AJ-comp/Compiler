using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class AccesserNode : MiniCNode
    {
        public Access AccessState { get; private set; } = Access.Private;

        public AccesserNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var terminalNode = Items[0].Build(param) as TerminalNode;

            if (terminalNode.Token.Kind == MiniCGrammar.Private) AccessState = Access.Private;
            else if (terminalNode.Token.Kind == MiniCGrammar.Public) AccessState = Access.Public;

            return this;
        }
    }
}
