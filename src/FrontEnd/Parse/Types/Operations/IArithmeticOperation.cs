using Parse.Types.ConstantTypes;

namespace Parse.Types.Operations
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
        IConstant Add(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this - operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Sub(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this * operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Mul(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this / operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Div(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this % operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Mod(IConstant operand);
    }


    public interface IArithmeticOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this + operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Add(T operand);


        /********************************************/
        /// <summary>
        /// This function means this - operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Sub(T operand);


        /********************************************/
        /// <summary>
        /// This function means this * operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Mul(T operand);


        /********************************************/
        /// <summary>
        /// This function means this / operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Div(T operand);


        /********************************************/
        /// <summary>
        /// This function means this % operand.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Mod(T operand);
    }
}
