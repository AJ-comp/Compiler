﻿using Parse.Types.ConstantTypes;

namespace Parse.Types.Operations
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
        IConstant BitAnd(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this | operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant BitOr(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means ~this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        IConstant BitNot();


        /********************************************/
        /// <summary>
        /// This function means this ^ operand
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant BitXor(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this << count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        IConstant LeftShift(int count);


        /********************************************/
        /// <summary>
        /// This function means this >> count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        IConstant RightShift(int count);
    }


    public interface IBitwiseOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this & operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T BitAnd(T operand);


        /********************************************/
        /// <summary>
        /// This function means this | operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T BitOr(T operand);


        /********************************************/
        /// <summary>
        /// This function means ~this
        /// </summary>
        /// <returns></returns>
        /********************************************/
        T BitNot();


        /********************************************/
        /// <summary>
        /// This function means this ^ operand
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T BitXor(T operand);


        /********************************************/
        /// <summary>
        /// This function means this << count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        T LeftShift(int count);


        /********************************************/
        /// <summary>
        /// This function means this >> count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /********************************************/
        T RightShift(int count);
    }
}
