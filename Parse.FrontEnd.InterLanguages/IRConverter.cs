namespace Parse.MiddleEnd.IR
{
    public class IRConverter
    {
        public static int ToAlign(DataTypes type)
        {
            int result = 0;

            if (type == DataTypes.i8) result = 1;
            else if (type == DataTypes.i16) result = 2;
            else if (type == DataTypes.i32) result = 4;
            else if (type == DataTypes.Double) result = 8;

            return result;
        }
    }
}
