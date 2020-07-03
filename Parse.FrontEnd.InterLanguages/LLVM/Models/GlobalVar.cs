using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class GlobalVar : ISSVar
    {
        protected GlobalVar(IRVar irvar)
        {
            _irVar = irvar;
        }

        public string Name => "@" + _irVar.Name;

        public bool IsEqualWithIRVar(IRVar var) => _irVar.Equals(var);

        public abstract DataType Type { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }

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

        public override DataType Type => _type;
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

        private T _type;
    }
}
