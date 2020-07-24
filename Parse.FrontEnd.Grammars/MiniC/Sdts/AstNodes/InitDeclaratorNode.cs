using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.LiteralNodes;
using Parse.MiddleEnd.IR.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class InitDeclaratorNode : MiniCNode
    {
        public InitDeclaratorNode(AstSymbol node) : base(node)
        {
        }

        public int Level => System.Convert.ToInt32(LevelToken?.Input);
        public string Name => NameToken?.Input;
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);

        public TokenData LevelToken { get; private set; }
        public TokenData NameToken { get; private set; }
        public TokenData DimensionToken { get; private set; }
        public ValueData Value { get; private set; }



        // format summary
        // [0] : VariableNode [Variable]
        // [1] : LiteralNode? [LiteralNode]
        public override SdtsNode Build(SdtsParams param)
        {
            // build Simple or ArrayVar
            var node0 = Items[0].Build(param);

            NameToken = (node0 as DeclareVarNode).NameToken;
            DimensionToken = (node0 as DeclareVarNode).DimensionToken;

            if (Items.Count > 1)
            {
                // build LiteralNode
                var node1 = Items[1].Build(param) as LiteralNode;
            }

            // check duplication
            MiniCChecker.IsDuplicated(this, param, NameToken);

            return this;
        }
    }
}
