using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class VariableTypeNode : MiniCNode
    {
        public VariableTypeNode(AstSymbol node) : base(node)
        {
        }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType { get; private set; }
        public MiniCTypeInfo MiniCTypeInfo => new MiniCTypeInfo(ConstToken, DataTypeToken);

        public TokenData ConstToken { get; private set; }
        public TokenData DataTypeToken { get; private set; }

        public override string ToString() => string.Format("const : {0}, DataType : {1}", Const.ToString(), DataType.ToString());

        public override SdtsNode Build(SdtsParams param)
        {
            foreach (var item in Items)
            {
                var node = item.Build(param);

                if(node is ConstNode)
                {
                    ConstToken = (node as ConstNode).ConstToken;
                }
                else if (node is VoidNode)
                {
                    DataType = MiniCDataType.Void;
                    DataTypeToken = (node as VoidNode).DataTypeToken;
                }
                else if (node is IntNode)
                {
                    DataType = MiniCDataType.Int;
                    DataTypeToken = (node as IntNode).DataTypeToken;
                }
            }

            return this;
        }
    }
}
