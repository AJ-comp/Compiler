using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class AccesserNode : AJNode
    {
        public Access AccessState { get; private set; } = Access.Private;

        public AccesserNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            var terminalNode = Items[0].Compile(param) as TerminalNode;

            if (terminalNode.Token.Kind == AJGrammar.Private) AccessState = Access.Private;
            else if (terminalNode.Token.Kind == AJGrammar.Public) AccessState = Access.Public;

            return this;
        }
    }
}
