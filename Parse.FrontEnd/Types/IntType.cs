using AJ.Common.Helpers;
using Parse.FrontEnd.Properties;
using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public class IntType : IntegerType
    {
        public override int Size => 32;
        public override StdType TypeKind => StdType.Int;
    }
}
