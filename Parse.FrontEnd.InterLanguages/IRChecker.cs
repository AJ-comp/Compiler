using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR
{
    public class IRChecker
    {
        public static DType GetGreaterType(DType p1, DType p2)
        {
            var p1Size = IRConverter.ToAlign(p1);
            var p2Size = IRConverter.ToAlign(p2);

            return (p1Size >= p2Size) ? p1 : p2;
        }
    }
}
