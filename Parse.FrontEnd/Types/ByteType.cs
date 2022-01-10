using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public class ByteType : IntegerType
    {
        public override int Size => 8;
        public override StdType TypeKind => StdType.Char;
    }
}
