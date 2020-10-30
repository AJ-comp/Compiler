using Parse.Types.ConstantTypes;
using System.Diagnostics;

namespace Parse.Types.VarTypes
{
    [DebuggerDisplay("Value:{Value}, ValueState:{ValueState}")]
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
        public abstract bool CanAssign(IValue operand);
    }



    public abstract class ValueVariable : Variable
    {
        protected ValueVariable(IValue value) : base(value)
        {
        }
    }


    [DebuggerDisplay("Value:{Value}, ValueState:{ValueState}")]
    public class PointerVariable : Variable, ICanBePointerType
    {
        public PointerVariable(DType type, IValue value) : base(value)
        {
            TypeName = type;
        }

        public int PointerLevel { get; }
        public override DType TypeName { get; }
        uint ICanBePointerType.PointerLevel { get; }


        public override bool CanAssign(IValue operand)
        {
            throw new System.NotImplementedException();
        }

        public override IConstant Assign(IValue operand)
        {
            throw new System.NotImplementedException();
        }
    }
}
