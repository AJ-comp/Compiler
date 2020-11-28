using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class DeclareVarNode : MiniCNode
    {
        public string VarName => NameToken?.Input;
        public int Dimension => (DimensionToken != null) ? System.Convert.ToInt32(DimensionToken?.Input) : 0;

        public TokenData NameToken { get; private set; }
        public TokenData DimensionToken { get; private set; }

        public DeclareVarNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // [0] : ident (AstTerminal)
        // [1] : number? (AstTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            var identNode = Items[0].Build(param) as TerminalNode;
            NameToken = identNode.Token;

            if(Items.Count > 1)
            {
                var literalNode = Items[1].Build(param) as TerminalNode;
                DimensionToken = literalNode.Token;
            }

            return this;
        }
    }
}
