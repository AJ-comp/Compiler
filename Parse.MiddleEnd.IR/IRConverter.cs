using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR
{
    public class IRConverter
    {
        public static int ToAlign(DType type) => GetType(type).Size;

        public static DataType GetType(DType type)
        {
            DataType result = new UnKnown();

            if (type == DType.Bit) result = new Bit();
            else if (type == DType.Byte) result = new Byte();
            else if (type == DType.Short) result = new Short();
            else if (type == DType.Int) result = new Int();
            else if (type == DType.Double) result = new DoubleType();

            return result;
        }
    }
}
