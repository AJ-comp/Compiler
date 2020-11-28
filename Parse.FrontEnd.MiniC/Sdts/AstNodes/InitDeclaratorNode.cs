using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class InitDeclaratorNode : MiniCNode
    {
        public InitDeclaratorNode(AstSymbol node) : base(node)
        {
        }

        public int Level => System.Convert.ToInt32(LevelToken?.Input);
        public string VarName => NameToken?.Input;
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);

        public TokenData LevelToken { get; private set; }
        public TokenData NameToken => LeftVar.NameToken;
        public TokenData DimensionToken => LeftVar.DimensionToken;

        public DeclareVarNode LeftVar { get; private set; }
        public ExprNode Right { get; private set; }


        // format summary
        // [0] : VariableNode   [Variable]
        // [1] : LiteralNode?    [LiteralNode]
        //      | UseVarNode?   [UseVar]
        //      | ExprNode?       
        public override SdtsNode Build(SdtsParams param)
        {
            // build Simple or ArrayVar
            LeftVar = Items[0].Build(param) as DeclareVarNode;

            if (Items.Count > 1)
                Right = Items[1].Build(param) as ExprNode;

            return this;
        }
    }
}
