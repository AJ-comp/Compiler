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

            public override string ToString()
            {
                string result = string.Format("{0} {1}", Label, OpCode);

                foreach (var item in Operands) result += " " + item;

                return result + Comment;
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

            private static Format UCodeFormat(string labelName, string opCode, string comment = "", params object[] operands)
                => new Format(labelName, opCode, comment, operands);

            public static Format DclVar(string labelName, int bIndex, int oIndex, int length, string comment = "")
                => UCodeFormat(labelName, "sym", comment, bIndex, oIndex, length);

            public static Format LoadVar(string labelName, int bIndex, int oIndex, string comment = "")
                => UCodeFormat(labelName, "lod", comment, bIndex, oIndex);

            public static Format DclValue(string labelName, int value, string comment = "")
                => UCodeFormat(labelName, "ldc", comment, value);

            public static Format ProcStart(string procName, int totalLength, int bIndex, string comment = "")
                => UCodeFormat(procName, "proc", comment, totalLength, bIndex, 2);

            public static Format ProcEnd(string labelName, string comment = "")
                => UCodeFormat(labelName, "end", comment);

            public static IReadOnlyList<Format> ProcCall(string labelName, string procName, params ParamData[] param)
            {
                List<Format> result = new List<Format>
                {
                    UCodeFormat(labelName, "ldp")
                };

                foreach (var item in param)
                {
                    if (item.PassWay == VarPassWay.CallByValue)
                        result.Add(UCodeFormat("", "lod", item.Comment, item.BIndex, item.OIndex));
                    else if (item.PassWay == VarPassWay.CallByAddress)
                        result.Add(UCodeFormat("", "lda", item.Comment, item.BIndex, item.OIndex));

                    result.Add(UCodeFormat("", "call", procName));
                }

                return result;
            }

            public static Format RetFromProc(string labelName, string comment = "")
                => UCodeFormat(labelName, "ret", comment);

            public static Format UnconditionalJump(string labelName, string destLableName, string comment = "")
                => UCodeFormat(labelName, "ujp", comment, destLableName);

            public static Format ConditionalJump(string labelName, string destLabelName, bool bTrue = true, string comment = "")
            {
                string command = (bTrue) ? "tjp" : "fjp";

                return UCodeFormat(labelName, command, comment, destLabelName);
            }

            public static Format Store(string labelName, int bIndex, int oIndex, string comment = "") => UCodeFormat(labelName, "str", comment, bIndex, oIndex);
            public static Format Add(string labelName, string comment = "") => UCodeFormat(labelName, "add", comment);
            public static Format Sub(string labelName, string comment = "") => UCodeFormat(labelName, "sub", comment);
            public static Format Multiple(string labelName, string comment = "") => UCodeFormat(labelName, "mult", comment);
            public static Format Div(string labelName, string comment = "") => UCodeFormat(labelName, "div", comment);
            public static Format Mod(string labelName, string comment = "") => UCodeFormat(labelName, "mod", comment);
            public static Format Swap(string labelName, string comment = "") => UCodeFormat(labelName, "swp", comment);
            public static Format And(string labelName, string comment = "") => UCodeFormat(labelName, "and", comment);
            public static Format Or(string labelName, string comment = "") => UCodeFormat(labelName, "or", comment);
            public static Format GreaterThan(string labelName, string comment = "") => UCodeFormat(labelName, "gt", comment);
            public static Format LessThan(string labelName, string comment = "") => UCodeFormat(labelName, "lt", comment);
            public static Format GreaterEqual(string labelName, string comment = "") => UCodeFormat(labelName, "ge", comment);
            public static Format LessEqual(string labelName, string comment = "") => UCodeFormat(labelName, "le", comment);
            public static Format Equal(string labelName, string comment = "") => UCodeFormat(labelName, "eq", comment);
            public static Format NegativeEqual(string labelName, string comment = "") => UCodeFormat(labelName, "ne", comment);
            public static Format Not(string labelName, string comment = "") => UCodeFormat(labelName, "not", comment);
            public static Format Negative(string labelName, string comment = "") => UCodeFormat(labelName, "neg", comment);
            public static Format Increment(string labelName, string comment = "") => UCodeFormat(labelName, "inc", comment);
            public static Format Decrement(string labelName, string comment = "") => UCodeFormat(labelName, "dec", comment);
            public static Format Duplicate(string labelName, string comment = "") => UCodeFormat(labelName, "dup", comment);
            public static Format NoOperate(string labelName, string comment = "") => UCodeFormat(labelName, "nop", comment);
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
