using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.Types.Operations
{
    public interface ILogicalOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this && operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) And(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this || operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Or(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means !this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Not();
    }
}
