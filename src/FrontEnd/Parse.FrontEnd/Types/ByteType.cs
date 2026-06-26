using Janglim.Types;

namespace Janglim.FrontEnd.Types
{
    public class ByteType : IntegerType
    {
        public override int Size => 8;
        public override StdType TypeKind => StdType.Char;
    }
}
