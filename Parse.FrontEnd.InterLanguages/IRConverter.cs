using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR
{
    public class IRConverter
    {
        public static int ToAlign(DType type)
        {
            int result = 0;

            if (type == DType.Bit) result = 1;
            else if (type == DType.Byte) result = 8;
            else if (type == DType.Short) result = 16;
            else if (type == DType.Int) result = 32;
            else if (type == DType.Double) result = 64;

            return result;
        }
    }
}
