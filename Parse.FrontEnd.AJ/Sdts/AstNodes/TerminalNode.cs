using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class TerminalNode : AJNode
    {
        public TokenData Token { get; private set; }

        public TerminalNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            // This has to be terminal type understandably
            Token = (Ast as AstTerminal).Token;

            return this;
        }
    }
}
