using System.Collections.Specialized;

namespace Parse.MiddleEnd.IR.UCode
{

    public class Instruction : IRUnit
    {
        public string OpCode => OpCodeType.ToString();
        public OpCodeKind OpCodeType { get; }
        public StringCollection Operands { get; } = new StringCollection();

        public string Comment => "; " + _comment;

        private string _comment;
        private string _labelName;

        public Instruction(string labelName, OpCodeKind opCode, string comment = "", params object[] operands)
        {
            _labelName = labelName;
            OpCodeType = opCode;
            _comment = comment;

            foreach (var operand in operands) Operands.Add(operand.ToString());
        }

        public string ToFormatString()
        {
            string result = string.Format("{0}", OpCode);

            foreach (var item in Operands) result += " " + item;

            return result + Comment;
        }

        public override string ToString() => ToFormatString();
    }
}
