namespace Parse.FrontEnd.InterLanguages
{
    public class IRConverter
    {
        public static int ToAlign(DataType type)
        {
            int result = 0;

            if (type == DataType.i8) result = 1;
            else if (type == DataType.i16) result = 2;
            else if (type == DataType.i32) result = 4;
            else if (type == DataType.Double) result = 8;

            return result;
        }
    }
}
