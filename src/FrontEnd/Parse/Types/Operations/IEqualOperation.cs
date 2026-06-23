using Parse.Types.ConstantTypes;
using System;

namespace Parse.Types.Operations
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
        IConstant Equal(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this != operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant NotEqual(IConstant operand);
    }


    public interface IEqualOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this == operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Equal(T operand);


        /********************************************/
        /// <summary>
        /// This function means this != operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T NotEqual(T operand);
    }
}
