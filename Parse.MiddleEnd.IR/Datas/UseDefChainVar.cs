using Parse.Types;
using System;

namespace Parse.MiddleEnd.IR.Datas
{
    public abstract class UseDefChainVar : IUseDefChainable, IRVar, ICanBePointerType
    {
        public string Name { get; protected set; }
        public abstract int Block { get; set; }
        public abstract int Offset { get; set; }
        public abstract int Length { get; }
        public abstract DType TypeName { get; }
        public uint PointerLevel { get; set; }

        // is not used only. for interface 
        public object Value => throw new NotImplementedException();
        public State ValueState => throw new NotImplementedException();

        protected UseDefChainVar(uint pointerLevel)
        {
            PointerLevel = pointerLevel;
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
