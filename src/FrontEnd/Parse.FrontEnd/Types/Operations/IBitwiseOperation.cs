using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Types.Operations
{
    public interface IBitwiseOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this & operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) BitAnd(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this | operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) BitOr(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means ~this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) BitNot();


        /********************************************/
        /// <summary>
        /// This function means this ^ operand
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) BitXor(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this << count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) LeftShift(int count);


        /********************************************/
        /// <summary>
        /// This function means this >> count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) RightShift(int count);
    }
}
