using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class FuncHeadNode : MiniCNode
    {
        public string Name => NameToken?.Input;

        public VariableTypeNode ReturnType { get; private set; }
        public TokenData NameToken { get; private set; }
        public ParamListNode ParamVarList { get; private set; }

        public FuncHeadNode(AstSymbol node) : base(node)
        {
        }



        // [0] : VariableTypeNode [DclSpec]
        // [1] : TerminalNode [Name]
        // [2] : FormalPara (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            // build DclSpec node
            ReturnType = Items[0].Build(param) as VariableTypeNode;
            // set Name data
            NameToken = (Items[1].Build(param) as TerminalNode).Token;
            // build FormalPara node
            ParamVarList = Items[2].Build(param) as ParamListNode;

            return this;
        }
    }
}
