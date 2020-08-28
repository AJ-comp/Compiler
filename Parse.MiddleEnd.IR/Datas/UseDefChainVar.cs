using Parse.Types;
using Parse.Types.VarTypes;
using System;

namespace Parse.MiddleEnd.IR.Datas
{
    public abstract class UseDefChainVar : Variable, IUseDefChainable, IRVar
    {
        public abstract string Name { get; }
        public abstract int Block { get; }
        public abstract int Offset { get; protected set; }
        public abstract int Length { get; }

        protected UseDefChainVar(IValue value) : base(value)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is UseDefChainVar var &&
                   TypeName == var.TypeName &&
                   Name == var.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeName, Name);
        }

        public abstract void Link(DependencyChainVar toLinkObject);
    }
}
