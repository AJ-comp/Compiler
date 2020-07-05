using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class GlobalVar : IRVar
    {
        protected GlobalVar(IRVar irvar)
        {
            _irVar = irvar;
        }

        public string Name => "@" + _irVar.Name;

        public bool IsEqualWithIRVar(IRVar var) => _irVar.Equals(var);

        public abstract DType TypeName { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }
        public abstract int Block { get; }
        public abstract int Offset { get; }
        public abstract int Length { get; }

        public override bool Equals(object obj)
        {
            return obj is GlobalVar var &&
                   Name == var.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1126147934;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        private IRVar _irVar;
    }

    public class GlobalVar<T> : GlobalVar where T : DataType
    {
        public GlobalVar(IRVar irvar) : base(irvar)
        {
        }

        public override DType TypeName => DataType.GetTypeName(typeof(T));
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }
        public override int Block { get; }
        public override int Offset { get; }
        public override int Length { get; }
    }
}
