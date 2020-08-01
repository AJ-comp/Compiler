using Parse.Types.ConstantTypes;
using Parse.Types.Operations;

namespace Parse.Types.VarTypes
{
    public interface IVariable : IValue, IAssignOperation
    {
        public int Address { get; set; }
        public IConstant ValueConstant { get; }
    }
}
