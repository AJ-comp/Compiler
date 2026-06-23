using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;

namespace Parse.FrontEnd.Types.Operations
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
        (bool, MeaningErrInfoList) Assign(PLType operand);
    }
}
