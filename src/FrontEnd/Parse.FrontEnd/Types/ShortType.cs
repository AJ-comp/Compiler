using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public class ShortType : IntegerType
    {
        public override int Size => 16;
        public override StdType TypeKind => StdType.Short;
    }
}
