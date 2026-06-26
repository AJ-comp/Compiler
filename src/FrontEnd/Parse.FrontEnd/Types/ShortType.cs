using Janglim.Types;

namespace Janglim.FrontEnd.Types
{
    public class ShortType : IntegerType
    {
        public override int Size => 16;
        public override StdType TypeKind => StdType.Short;
    }
}
