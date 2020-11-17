using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class TerminalNode : MiniCNode
    {
        public TokenData Token { get; private set; }

        public TerminalNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            // This has to be terminal type understandably
            Token = (Ast as AstTerminal).Token;

            return this;
        }
    }
}
