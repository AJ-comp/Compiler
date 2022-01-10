using Parse.FrontEnd.Types;
using Parse.FrontEnd.Types.Operations;
using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public class DoubleType : PLType, IArithmeticOperation, IEqualOperation
    {
        public override int Size => 64;
        public bool Nan { get; }

        public override StdType TypeKind => StdType.Double;

        public virtual (PLType, MeaningErrInfoList) Add(PLType operand)
        {
            var errList = new MeaningErrInfoList();
//            if (operand.TypeKind == StdType.String) return (new StringType(), errList);

            return ArithmeticOperation(operand, "+");
        }


        public virtual (PLType, MeaningErrInfoList) Div(PLType operand) => ArithmeticOperation(operand, "/");
        public virtual (PLType, MeaningErrInfoList) Equal(PLType operand) => ArithmeticCompare(operand, "==");
        public virtual (PLType, MeaningErrInfoList) Mod(PLType operand) => ArithmeticOperation(operand, "%");
        public virtual (PLType, MeaningErrInfoList) Mul(PLType operand) => ArithmeticOperation(operand, "*");
        public virtual (PLType, MeaningErrInfoList) NotEqual(PLType operand) => ArithmeticCompare(operand, "!=");
        public virtual (PLType, MeaningErrInfoList) Sub(PLType operand) => ArithmeticOperation(operand, "-");
    }
}
