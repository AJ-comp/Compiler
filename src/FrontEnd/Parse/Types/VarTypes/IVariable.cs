using Janglim.Types.ConstantTypes;
using Janglim.Types.Operations;

namespace Janglim.Types.VarTypes
{
    public interface IVariable : IValue, IAssignOperation
    {
        public int Address { get; set; }
        public IConstant ValueConstant { get; }
    }
}
