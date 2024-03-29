﻿using Parse.Types.ConstantTypes;

namespace Parse.Types.Operations
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
        IConstant And(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this || operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Or(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means !this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        IConstant Not();
    }


    public interface ILogicalOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this && operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T And(T operand);


        /********************************************/
        /// <summary>
        /// This function means this || operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Or(T operand);


        /********************************************/
        /// <summary>
        /// This function means !this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        T Not();
    }
}
