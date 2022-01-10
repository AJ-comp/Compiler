using Parse.Types.ConstantTypes;

namespace Parse.Types.Operations
{
    public interface IAssignOperation : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this = operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        IConstant Assign(IConstant operand);
    }


    public interface IAssignOperation<T> : IOperation
    {
        /********************************************/
        /// <summary>
        /// This function means this = operand2.
        /// </summary>
        /// <param name="operand"></param>
        /// <returns></returns>
        /********************************************/
        T Assign(T operand);
    }
}
