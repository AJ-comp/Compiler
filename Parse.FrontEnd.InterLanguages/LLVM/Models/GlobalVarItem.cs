using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class GlobalVarItem : TerminalItem, ISSVar
    {
        private IRVar _irVar;

        public string Name => "@" + _irVar.Name;

        public bool IsEqualWithIRVar(IRVar var) => _irVar.Equals(var);

        public GlobalVarItem(IRVar irvar) : base(irvar.Type)
        {
            _irVar = irvar;
        }

        public override bool Equals(object obj)
        {
            return obj is GlobalVarItem item &&
                   Type == item.Type &&
                   Name == item.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1979447941;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
