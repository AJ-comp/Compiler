using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class LLVMConverter
    {
        public static string ToInstructionName(DataType type)
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

        public static string ToInstructionName(ReturnType type)
        {
            string result = string.Empty;

            if (type == ReturnType.Void) result = "void";
            else if (type == ReturnType.i8) result = "i8";
            else if (type == ReturnType.i16) result = "i16";
            else if (type == ReturnType.i32) result = "i32";
            else if (type == ReturnType.Double) result = "double";

            return result;
        }

        public static string ToInstructionName(IROperation operation)
        {
            string result = string.Empty;

            if (operation == IROperation.Add) result = "add";
            else if (operation == IROperation.Sub) result = "sub";
            else if (operation == IROperation.Mul) result = "mul";
            else if (operation == IROperation.Div) result = "sdiv";
            else if (operation == IROperation.Mod) result = "srem";

            return result;
        }

        public static string ToInstructionNameForInteger(IRCondition condition, bool bSigned)
        {
            string result = string.Empty;

            if (condition == IRCondition.EQ) result = "eq";
            else if (condition == IRCondition.NE) result = "ne";

            else if (bSigned)
            {
                if (condition == IRCondition.GT) result = "sgt";
                else if (condition == IRCondition.GE) result = "sge";
                else if (condition == IRCondition.LT) result = "slt";
                else if (condition == IRCondition.LE) result = "sle";
            }
            else
            {
                if (condition == IRCondition.GT) result = "ugt";
                else if (condition == IRCondition.GE) result = "uge";
                else if (condition == IRCondition.LT) result = "ult";
                else if (condition == IRCondition.LE) result = "ule";
            }

            return result;
        }

        public static string ToInstructionNameForDouble(IRCondition condition, bool bNans = false)
        {
            string result = string.Empty;

                if (condition == IRCondition.EQ) result = "ueq";
                else if (condition == IRCondition.NE) result = "une";

            if (bNans)
            {
                if (condition == IRCondition.GT) result = "ogt";
                else if (condition == IRCondition.GE) result = "oge";
                else if (condition == IRCondition.LT) result = "olt";
                else if (condition == IRCondition.LE) result = "ole";
            }
            else
            {
                if (condition == IRCondition.GT) result = "ugt";
                else if (condition == IRCondition.GE) result = "uge";
                else if (condition == IRCondition.LT) result = "ult";
                else if (condition == IRCondition.LE) result = "ule";
            }

            return result;
        }

        public static IRDoubleLiteral ToDoubleLiteral(IRLiteral literal) => new IRDoubleLiteral((double)literal.Value);
    }
}
