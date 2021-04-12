using Parse.Types.ConstantTypes;
using System.Diagnostics;

namespace Parse.Types.VarTypes
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public abstract class Variable : IVariable
    {
        public int Address { get; set; }
        public object Value => ValueConstant.Value;
        public State ValueState => ValueConstant.ValueState;
        public abstract StdType TypeKind { get; }

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

        public abstract IConstant Assign(IConstant operand);
        public abstract bool CanAssign(IConstant operand);


        private string DebuggerDisplay
            => string.Format("{0} Value:{1}, ValueState:{2}",
                                        Helper.GetEnumDescription(TypeKind),
                                        Value, 
                                        ValueState);
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
        public PointerVariable(StdType type, IValue value) : base(value)
        {
            TypeKind = type;
        }

        public int PointerLevel { get; }
        public override StdType TypeKind { get; }
        uint ICanBePointerType.PointerLevel { get; }


        public override bool CanAssign(IConstant operand)
        {
            throw new System.NotImplementedException();
        }

        public override IConstant Assign(IConstant operand)
        {
            throw new System.NotImplementedException();
        }
    }
}
