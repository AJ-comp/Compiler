using AJ.Common.Helpers;
using Parse.FrontEnd.Properties;
using Parse.FrontEnd.Types.Operations;
using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public class BitType : PLType, IBitwiseOperation
    {
        public override StdType TypeKind => StdType.Bit;
        public override int Size => 1;

        public (PLType, MeaningErrInfoList) And(PLType operand) => BitOperation(operand, "&&");
        public virtual (PLType, MeaningErrInfoList) BitAnd(PLType operand) => BitOperation(operand, "&");
        public virtual (PLType, MeaningErrInfoList) BitNot() => (new BitType(), new MeaningErrInfoList());
        public virtual (PLType, MeaningErrInfoList) BitOr(PLType operand) => BitOperation(operand, "|");
        public virtual (PLType, MeaningErrInfoList) BitXor(PLType operand) => BitOperation(operand, "^");
        public virtual (PLType, MeaningErrInfoList) Equal(PLType operand) => BitOperation(operand, "==");
        public virtual (PLType, MeaningErrInfoList) LeftShift(int count)
        {
            var errList = new MeaningErrInfoList();
            errList.Add(new MeaningErrInfo(nameof(AlarmCodes.TP0003), AlarmCodes.TP0003));

            return (null, errList);
        }
        public virtual (PLType, MeaningErrInfoList) Not() => (new BitType(), new MeaningErrInfoList());
        public virtual (PLType, MeaningErrInfoList) NotEqual(PLType operand) => BitOperation(operand, "!=");
        public virtual (PLType, MeaningErrInfoList) Or(PLType operand) => BitOperation(operand, "||");
        public virtual (PLType, MeaningErrInfoList) RightShift(int count)
        {
            var errList = new MeaningErrInfoList();
            errList.Add(new MeaningErrInfo(nameof(AlarmCodes.TP0003), AlarmCodes.TP0003));

            return (null, errList);
        }


        private (PLType, MeaningErrInfoList) BitOperation(PLType operand, string operation)
        {
            var errList = new MeaningErrInfoList();

            if (operand.TypeKind == StdType.Bit) return (new BitType(), errList);

            errList.Add(GetErrorWithTP0001(operand, operation));
            return (null, errList);
        }
    }
}
