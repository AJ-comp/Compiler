using Parse.Types.ConstantTypes;
using System;

namespace Parse.Types.Operations
{
    public interface ICompareOperation : IOperation
    {
        ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This function means this == operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        ///////////////////////////////////////////////////////////////////////////////////////
        IConstant Equal(IValue operand);

        ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// This function means this != operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        ///////////////////////////////////////////////////////////////////////////////////////
        IConstant NotEqual(IValue operand);
    }
}
