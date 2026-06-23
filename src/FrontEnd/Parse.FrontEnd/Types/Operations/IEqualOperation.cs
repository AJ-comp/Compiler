using Parse.Types.ConstantTypes;
using System;

namespace Parse.FrontEnd.Types.Operations
{
    public interface IEqualOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this == operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) Equal(PLType operand);


        /********************************************/
        /// <summary>
        /// This function means this != operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        (PLType, MeaningErrInfoList) NotEqual(PLType operand);
    }
}
