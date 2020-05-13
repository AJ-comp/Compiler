using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Parse.FrontEnd.InterLanguages
{
    public enum VarPassWay { CallByValue, CallByAddress };

    public class UCode
    {
        public class Format
        {
            public enum DisplayCommentFormat { AssemblyFormat, CFormat };
            private string _comment = string.Empty;

            public string Label { get; }
            public string OpCode { get; }
            public StringCollection Operands { get; } = new StringCollection();
            public string Comment
            {
                get
                {
                    string result = string.Empty;
                    if(CommentFormat == DisplayCommentFormat.AssemblyFormat)
                        result = (_comment.Length > 0) ? "; " + _comment : string.Empty;
                    else
                        result = (_comment.Length > 0) ? "/* " + _comment + " */" : string.Empty;

                    return result;
                }
            }

            public DisplayCommentFormat CommentFormat { get; set; } = Format.DisplayCommentFormat.AssemblyFormat;

            public Format(string label, string opCode, string comment = "", params object[] operands)
            {
                Label = label;
                OpCode = opCode;
                _comment = comment;

                foreach (var operand in operands) Operands.Add(operand.ToString());
            }
        }

        public class Command
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

            private static Format UCodeFormat(string label, string opCode, string comment = "", params object[] operands)
            {
                return new Format(label, opCode, comment, operands);
            }

            public static Format DclVar(int bIndex, int oIndex, int length, string comment = "") => UCodeFormat(LabelSpace, "sym", comment, bIndex, oIndex, length);

            public static Format LoadVar(int bIndex, int oIndex, string comment = "") => UCodeFormat(LabelSpace, "lod", comment, bIndex, oIndex);

            public static Format DclValue(int value, string comment = "") => UCodeFormat(LabelSpace, "ldc", comment, value);

            public static Format ProcStart(string procName, int totalLength, int bIndex, string comment = "")
            {
                if (procName.Length > 10)
                    procName = procName.Substring(0, 10);

                for (int i = procName.Length; i < 11; i++) procName += " ";

                return UCodeFormat(procName, "proc", comment, totalLength, bIndex, 2);
            }

            public static Format ProcEnd(string comment = "") => UCodeFormat(LabelSpace, "end", comment);

            public static IReadOnlyList<Format> ProcCall(string procName, params ParamData[] param)
            {
                List<Format> result = new List<Format>
                {
                    UCodeFormat(LabelSpace, "ldp")
                };

                foreach (var item in param)
                {
                    if (item.PassWay == VarPassWay.CallByValue)
                        result.Add(UCodeFormat(LabelSpace, "lod", item.Comment, item.BIndex, item.OIndex));
                    else if (item.PassWay == VarPassWay.CallByAddress)
                        result.Add(UCodeFormat(LabelSpace, "lda", item.Comment, item.BIndex, item.OIndex));

                    result.Add(UCodeFormat(LabelSpace, "call", procName));
                }

                return result;
            }

            public static Format RetFromProc(string comment = "") => UCodeFormat(LabelSpace, "ret", comment);

            public static Format UnconditionalJump(string destLableName, string comment = "") => UCodeFormat(LabelSpace, "ujp", comment, destLableName);

            public static Format ConditionalJump(string destLabelName, string comment = "", bool bTrue = true)
            {
                string command = (bTrue) ? "tjp" : "fjp";

                return UCodeFormat(LabelSpace, command, comment, destLabelName);
            }

            public static Format Store(int bIndex, int oIndex, string comment = "") => UCodeFormat(LabelSpace, "str", comment, bIndex, oIndex);
            public static Format Add(string comment = "") => UCodeFormat(LabelSpace, "add", comment);
            public static Format Sub(string comment = "") => UCodeFormat(LabelSpace, "sub", comment);
            public static Format Multiple(string comment = "") => UCodeFormat(LabelSpace, "mult", comment);
            public static Format Div(string comment = "") => UCodeFormat(LabelSpace, "div", comment);
            public static Format Mod(string comment = "") => UCodeFormat(LabelSpace, "mod", comment);
            public static Format Swap(string comment = "") => UCodeFormat(LabelSpace, "swp", comment);
            public static Format And(string comment = "") => UCodeFormat(LabelSpace, "and", comment);
            public static Format Or(string comment = "") => UCodeFormat(LabelSpace, "or", comment);
            public static Format GreaterThan(string comment = "") => UCodeFormat(LabelSpace, "gt", comment);
            public static Format LessThan(string comment = "") => UCodeFormat(LabelSpace, "lt", comment);
            public static Format GreaterEqual(string comment = "") => UCodeFormat(LabelSpace, "ge", comment);
            public static Format LessEqual(string comment = "") => UCodeFormat(LabelSpace, "le", comment);
            public static Format Equal(string comment = "") => UCodeFormat(LabelSpace, "le", comment);
            public static Format NegativeEqual(string comment = "") => UCodeFormat(LabelSpace, "ne", comment);
            public static Format NotOperand(string comment = "") => UCodeFormat(LabelSpace, "notop", comment);
            public static Format Negative(string comment = "") => UCodeFormat(LabelSpace, "neg", comment);
            public static Format Increment(string comment = "") => UCodeFormat(LabelSpace, "inc", comment);
            public static Format Decrement(string comment = "") => UCodeFormat(LabelSpace, "dec", comment);
            public static Format Duplicate(string comment = "") => UCodeFormat(LabelSpace, "dup", comment);
        }
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
