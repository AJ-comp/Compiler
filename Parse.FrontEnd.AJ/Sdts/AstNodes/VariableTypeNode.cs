using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class VariableTypeNode : AJNode
    {
        public VariableTypeNode(AstSymbol node) : base(node)
        {
        }

        public bool Const => ConstToken != null;
        public AJTypeInfo Type { get; private set; }

        public TokenData ConstToken { get; private set; }
        public AJDataType DataType => _dataTypeNode.Type;
        public TokenData DataTypeToken => _dataTypeNode.DataTypeToken;


        public override SdtsNode Compile(CompileParameter param)
        {
            foreach (var item in Items)
            {
                var node = item.Compile(param);

                if (node is ConstNode) ConstToken = (node as ConstNode).ConstToken;
                else if (node is TypeDeclareNode) _dataTypeNode = node as TypeDeclareNode;
                else if (node is ClassDefNode) _dataTypeNode = node as UserDefTypeNode;
            }

            Type = new AJTypeInfo(_dataTypeNode.Type);

            return this;
        }


        private DataTypeNode _dataTypeNode;

        private string GetDebuggerDisplay() => $"const : {Const}, DataType : {DataType}";
    }
}
