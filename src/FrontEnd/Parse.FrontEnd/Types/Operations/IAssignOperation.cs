using Janglim.Types.ConstantTypes;
using Janglim.Types.VarTypes;

namespace Janglim.FrontEnd.Types.Operations
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
