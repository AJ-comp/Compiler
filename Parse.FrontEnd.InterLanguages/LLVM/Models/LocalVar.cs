using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class LocalVar : IRVar
    {
        public LocalVar(int offset)
        {
            Offset = offset;
        }

        public string Name => "%" + Offset;
        public int Offset { get; }

        public abstract DType TypeName { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }
        public abstract int Block { get; }
        public abstract int Length { get; }

        public override bool Equals(object obj)
        {
            return obj is LocalVar var &&
                   Offset == var.Offset;
        }

        public override int GetHashCode()
        {
            return -149965190 + Offset.GetHashCode();
        }
    }

    public class LocalVar<T> : LocalVar, IRVar<T> where T : DataType
    {
        public LocalVar(int offset) : base(offset)
        {
        }

        public override DType TypeName => DataType.GetTypeName(typeof(T));
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }
        public override int Block { get; }
        public override int Length { get; }
    }
}
