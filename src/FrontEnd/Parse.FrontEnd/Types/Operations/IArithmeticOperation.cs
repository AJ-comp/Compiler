using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Types.Operations
{
    public interface IArithmeticOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this + operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Add(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this - operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Sub(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this * operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Mul(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this / operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Div(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this % operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Mod(PLType operand);
    }
}
