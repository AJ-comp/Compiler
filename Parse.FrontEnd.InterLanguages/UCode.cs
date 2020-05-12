using System;

namespace Parse.FrontEnd.InterLanguages
{
    public enum VarPassWay { CallByValue, CallByAddress };

    public class UCode
    {
        private static string LabelSpace
        {
            get
            {
                string space = string.Empty;
                for (int i = 0; i < 11; i++) space += " ";

                return space;
            }
        }

        private static string UCodeFormat(string label, string opCode, string comment = "", params object[] operands)
        {
            string result = string.Format("{0} {1}", label, opCode);

            foreach (var item in operands) result += " " + item;
            if (comment.Length > 0)
                result += string.Format("\t/*{0}*/", comment);

            return result + Environment.NewLine;
        }

        public static string DclVar(int bIndex, int oIndex, int length, string comment = "") => UCodeFormat(LabelSpace, "sym", comment, bIndex, oIndex, length);

        public static string LoadVar(int bIndex, int oIndex, string comment = "") => UCodeFormat(LabelSpace, "lod", comment, bIndex, oIndex);

        public static string DclValue(int value, string comment = "") => UCodeFormat(LabelSpace, "ldc", comment, value);

        public static string ProcStart(string procName, int totalLength, int bIndex, string comment = "")
        {
            if (procName.Length > 10)
                procName = procName.Substring(0, 10);

            for (int i = procName.Length; i < 11; i++) procName += " ";

            return UCodeFormat(procName, "proc", comment, totalLength, bIndex, 2);
        }

        public static string ProcEnd(string comment = "") => UCodeFormat(LabelSpace, "end", comment);

        public static string ProcCall(string procName, params ParamData[] param)
        {
            string result = UCodeFormat(LabelSpace, "ldp");

            foreach(var item in param)
            {
                if (item.PassWay == VarPassWay.CallByValue)
                    result += UCodeFormat(LabelSpace, "lod", item.Comment, item.BIndex, item.OIndex);
                else if(item.PassWay == VarPassWay.CallByAddress)
                    result += UCodeFormat(LabelSpace, "lda", item.Comment, item.BIndex, item.OIndex);

                result += UCodeFormat(LabelSpace, "call", procName);
            }

            return result;
        }

        public static string RetFromProc(string comment = "")
        {
            return UCodeFormat(LabelSpace, "ret", comment);
        }

        public static string UnconditionalJump(string destLableName, string comment = "")
        {
            return UCodeFormat(LabelSpace, "ujp", comment, destLableName);
        }

        public static string ConditionalJump(string destLabelName, string comment = "", bool bTrue = true)
        {
            string command = (bTrue) ? "tjp" : "fjp";

            return UCodeFormat(LabelSpace, command, comment, destLabelName);
        }

        public static string Store(int bIndex, int oIndex, string comment = "") => UCodeFormat(LabelSpace, "str", comment, bIndex, oIndex);
        public static string Add(string comment = "") => UCodeFormat(LabelSpace, "add", comment);
        public static string Sub(string comment = "") => UCodeFormat(LabelSpace, "sub", comment);
        public static string Multiple(string comment = "") => UCodeFormat(LabelSpace, "mult", comment);
        public static string Div(string comment = "") => UCodeFormat(LabelSpace, "div", comment);
        public static string Mod(string comment = "") => UCodeFormat(LabelSpace, "mod", comment);
        public static string Swap(string comment = "") => UCodeFormat(LabelSpace, "swp", comment);
        public static string And(string comment = "") => UCodeFormat(LabelSpace, "and", comment);
        public static string Or(string comment = "") => UCodeFormat(LabelSpace, "or", comment);
        public static string GreaterThan(string comment = "") => UCodeFormat(LabelSpace, "gt", comment);
        public static string LessThan(string comment = "") => UCodeFormat(LabelSpace, "lt", comment);
        public static string GreaterEqual(string comment = "") => UCodeFormat(LabelSpace, "ge", comment);
        public static string LessEqual(string comment = "") => UCodeFormat(LabelSpace, "le", comment);
        public static string Equal(string comment = "") => UCodeFormat(LabelSpace, "le", comment);
        public static string NegativeEqual(string comment = "") => UCodeFormat(LabelSpace, "ne", comment);
        public static string NotOperand(string comment = "") => UCodeFormat(LabelSpace, "notop", comment);
        public static string Negative(string comment = "") => UCodeFormat(LabelSpace, "neg", comment);
        public static string Increment(string comment = "") => UCodeFormat(LabelSpace, "inc", comment);
        public static string Decrement(string comment = "") => UCodeFormat(LabelSpace, "dec", comment);
        public static string Duplicate(string comment = "") => UCodeFormat(LabelSpace, "dup", comment);
    }


    public class ParamData
    {
        public int BIndex { get; }
        public int OIndex { get; }

        public VarPassWay PassWay { get; }
        public string Comment { get; }

        public ParamData(int bIndex, int oIndex, VarPassWay passWay, string comment)
        {
            BIndex = bIndex;
            OIndex = oIndex;
            PassWay = passWay;
            Comment = comment;
        }

    }
}
