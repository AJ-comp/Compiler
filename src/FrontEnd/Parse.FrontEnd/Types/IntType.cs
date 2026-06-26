using AJ.Common.Helpers;
using Janglim.FrontEnd.Properties;
using Janglim.Types;

namespace Janglim.FrontEnd.Types
{
    public class IntType : IntegerType
    {
        public override int Size => 32;
        public override StdType TypeKind => StdType.Int;
    }
}
