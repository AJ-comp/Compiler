using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ConstNode : AJNode
    {
        public bool IsConst => (ConstToken != null);
        public TokenData ConstToken { get; private set; }

        public ConstNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            Items[0].Build(param);
            ConstToken = (Items[0] as TerminalNode).Token;

            return this;
        }
    }
}
