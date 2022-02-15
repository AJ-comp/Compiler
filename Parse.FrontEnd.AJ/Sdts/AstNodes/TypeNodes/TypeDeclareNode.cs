using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public enum TypeKind
    {
        PreDef,
        UserDef
    }

    public class TypeDeclareNode : DataTypeNode
    {
        public bool Signed { get; } = false;
        public TypeKind TypeKind { get; }

        // predef type
        public TypeDeclareNode(AstSymbol node, AJDataType stdType, bool signed, string name) : base(node)
        {
            Type = stdType;
            Signed = signed;
            Name = name;

            TypeKind = TypeKind.PreDef;
        }

        // userdef type
        public TypeDeclareNode(AstSymbol node) : base(node)
        {
            Type = AJDataType.Unknown;

            TypeKind = TypeKind.UserDef;
        }


        public AJTypeInfo ToAJTypeInfo(bool bConst)
        {
            AJTypeInfo result = new AJTypeInfo(Type, DataTypeToken);
            result.Const = bConst;
            result.Signed = Signed;

            return result;
        }


        public override AJDataType Type { get; }
        public override uint Size => AJUtilities.SizeOf(this);
        public override string Name { get; }

    }
}
