using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class DataTypeNode : AJNode
    {
        public abstract AJDataType Type { get; }
        public TokenDataList FullDataTypeToken { get; } = new TokenDataList();
        public TokenData DataTypeToken => FullDataTypeToken.Last();
        public abstract uint Size { get; }
        public string Name => DataTypeToken.Input;
        public string FullName => FullDataTypeToken.ToListString();

        protected DataTypeNode(AstSymbol node) : base(node)
        {
        }
    }
}
