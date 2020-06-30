using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class Condition : NamelessItem
    {
        public bool ValueRealType => (bool)Value;
        public IRCond ToIRCondVar => new IRCond(ValueRealType);

        public Condition(bool value) : base(DataType.i1, value)
        {
        }
    }
}
