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

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            Items[0].Compile(param);
            ConstToken = (Items[0] as TerminalNode).Token;

            return this;
        }
    }
}
