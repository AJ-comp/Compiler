using AJ.Common.Helpers;
using Parse.FrontEnd.Properties;
using Parse.FrontEnd.Types.Operations;
using Parse.Types;

namespace Parse.FrontEnd.Types
{
    public abstract class IntegerType : PLType, IArithmeticOperation, IBitwiseOperation, IEqualOperation
    {
        public bool Signed { get; protected set; }


        public virtual (PLType, MeaningErrInfoList) Equal(PLType operand) => ArithmeticCompare(operand, "==");
        public virtual (PLType, MeaningErrInfoList) NotEqual(PLType operand) => ArithmeticCompare(operand, "!=");
        public virtual (PLType, MeaningErrInfoList) Add(PLType operand)
        {
            /*
            var errList = new MeaningErrInfoList();
            if (operand.TypeKind == StdType.String) return (new StringType(), errList);
            */

            return ArithmeticOperation(operand, "+");
        }
        public virtual (PLType, MeaningErrInfoList) Sub(PLType operand) => ArithmeticOperation(operand, "-");
        public virtual (PLType, MeaningErrInfoList) Mul(PLType operand) => ArithmeticOperation(operand, "*");
        public virtual (PLType, MeaningErrInfoList) Div(PLType operand) => ArithmeticOperation(operand, "/");
        public virtual (PLType, MeaningErrInfoList) Mod(PLType operand) => ArithmeticOperation(operand, "%");
        public virtual (PLType, MeaningErrInfoList) BitAnd(PLType operand) => BitwiseOperation(operand, "&");
        public virtual (PLType, MeaningErrInfoList) BitOr(PLType operand) => BitwiseOperation(operand, "|");
        public virtual (PLType, MeaningErrInfoList) BitNot() => (new IntType(), new MeaningErrInfoList());
        public virtual (PLType, MeaningErrInfoList) BitXor(PLType operand) => BitwiseOperation(operand, "^");
        public virtual (PLType, MeaningErrInfoList) LeftShift(int count) => (new IntType(), new MeaningErrInfoList());
        public virtual (PLType, MeaningErrInfoList) RightShift(int count) => (new IntType(), new MeaningErrInfoList());



        private (PLType, MeaningErrInfoList) BitwiseOperation(PLType operand, string operatorSymbol)
        {
            var errList = new MeaningErrInfoList();

            if(operand.TypeKind == StdType.Bit)
            {
                errList.Add(GetErrorWithTP0001(operand, operatorSymbol));
                return (null, errList);
            }

            if (!(operand is IBitwiseOperation))
            {
                errList.Add(GetErrorWithTP0001(operand, operatorSymbol));
                return (null, errList);
            }

            return (new BitType(), errList);
        }
    }
}
