using Parse.Types.ConstantTypes;
using System.Diagnostics;

namespace Parse.Types.VarTypes
{
    [DebuggerDisplay("Value:{Value}, ValueState:{ValueState}, PointerLevel:{PointerLevel}")]
    public abstract class Variable : IVariable
    {
        public int Address { get; set; }
        public object Value => ValueConstant.Value;
        public State ValueState => ValueConstant.ValueState;
        public abstract DType TypeName { get; }

        public IConstant ValueConstant { get; protected set; }


        protected Variable(IValue value)
        {
            if(value is IVariable)
            {
                var cVar = value as IVariable;

                ValueConstant = cVar.ValueConstant;
                Address = cVar.Address;
            }
            else if(value is IConstant)
            {
                ValueConstant = value as IConstant;
            }
        }

        public abstract IConstant Assign(IValue operand);
    }
}
