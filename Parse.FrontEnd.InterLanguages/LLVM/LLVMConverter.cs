namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class LLVMConverter
    {
        public static string ToString(DataType type)
        {
            string result = string.Empty;

            if (type == DataType.i8) result = "i8";
            else if (type == DataType.i16) result = "i16";
            else if (type == DataType.i32) result = "i32";
            else if (type == DataType.Double) result = "double";

            return result;
        }

        public static int ToAlign(DataType type)
        {
            int result = 0;

            if (type == DataType.i8) result = 1;
            else if (type == DataType.i16) result = 2;
            else if (type == DataType.i32) result = 4;
            else if (type == DataType.Double) result = 8;

            return result;
        }

        public static string ToString(ReturnType type)
        {
            string result = string.Empty;

            if (type == ReturnType.Void) result = "void";
            else if (type == ReturnType.i8) result = "i8";
            else if (type == ReturnType.i16) result = "i16";
            else if (type == ReturnType.i32) result = "i32";
            else if (type == ReturnType.Double) result = "double";

            return result;
        }

        public static string ToString(IROperation operation)
        {
            string result = string.Empty;

            if (operation == IROperation.Add) result = "add";
            else if (operation == IROperation.Sub) result = "sub";
            else if (operation == IROperation.Mul) result = "mul";
            else if (operation == IROperation.Div) result = "div";
            else if (operation == IROperation.Mod) result = "mod";

            return result;
        }

        public static string LocalOffsetName(int offset) => "%" + offset;
    }
}
