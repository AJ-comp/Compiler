using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class ConstNode : MiniCNode
    {
        public bool IsConst => (ConstToken != null);
        public TokenData ConstToken { get; private set; }

        public ConstNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            ConstToken = (Items[0] as TerminalNode).Token;

            return this;
        }
    }
}
