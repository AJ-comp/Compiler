using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class DataTypeNode : AJNode
    {
        public TokenDataList FullDataTypeToken { get; } = new TokenDataList();

        // abstract
        public abstract uint Size { get; }
        public abstract AJDataType Type { get; }

        // readonly
        public string Name => DataTypeToken.Input;
        public string FullName => FullDataTypeToken.ToListString();
        public TokenData DataTypeToken => FullDataTypeToken.Last();

        protected DataTypeNode(AstSymbol node) : base(node)
        {
        }


        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            FullDataTypeToken.Clear();
            return base.CompileLogic(param);
        }


        public override string ToString()
        {
            return $"<{Type}> {FullName} [{GetType().Name}]";
        }
    }
}
