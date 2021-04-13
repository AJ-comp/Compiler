using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class AccesserNode : AJNode
    {
        public Access AccessState { get; private set; } = Access.Private;

        public AccesserNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var terminalNode = Items[0].Build(param) as TerminalNode;

            if (terminalNode.Token.Kind == AJGrammar.Private) AccessState = Access.Private;
            else if (terminalNode.Token.Kind == AJGrammar.Public) AccessState = Access.Public;

            return this;
        }
    }
}
