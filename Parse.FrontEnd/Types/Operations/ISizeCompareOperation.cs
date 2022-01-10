using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Types.Operations
{
    public interface ISizeCompareOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this > operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) GreaterThan(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this < operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) LessThan(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this >= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) GreaterEqual(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this <= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) LessEqual(PLType operand);
    }
}
