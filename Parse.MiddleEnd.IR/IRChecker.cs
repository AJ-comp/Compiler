using Parse.Types;

namespace Parse.MiddleEnd.IR
{
    public class IRChecker
    {
        public static IDataTypeSpec GetGreaterType(IDataTypeSpec p1, IDataTypeSpec p2)
        {
            var p1Size = p1.Size;
            var p2Size = p2.Size;

            return (p1Size >= p2Size) ? p1 : p2;
        }
    }
}
