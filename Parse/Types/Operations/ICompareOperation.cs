using Parse.Types.ConstantTypes;

namespace Parse.Types.Operations
{
    public interface ICompareOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this > operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant GreaterThan(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this < operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant LessThan(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this >= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant GreaterEqual(IConstant operand);


        /********************************************/
        /// <summary>
        /// This function means this <= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant LessEqual(IConstant operand);
    }


    public interface ICompareOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this > operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T GreaterThan(T operand);


        /********************************************/
        /// <summary>
        /// This function means this < operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T LessThan(T operand);


        /********************************************/
        /// <summary>
        /// This function means this >= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T GreaterEqual(T operand);


        /********************************************/
        /// <summary>
        /// This function means this <= operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T LessEqual(T operand);
    }
}
