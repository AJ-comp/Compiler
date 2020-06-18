using System.Collections.Specialized;

namespace Parse.FrontEnd.InterLanguages.UCode
{

    public class Instruction : IRUnit
    {
        private string _comment;

        public string OpCode => OpCodeType.ToString();
        public OpCodeKind OpCodeType { get; }
        public StringCollection Operands { get; } = new StringCollection();

        public string Comment => "; " + _comment;

        public Instruction(OpCodeKind opCode, string comment = "", params object[] operands)
        {
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
