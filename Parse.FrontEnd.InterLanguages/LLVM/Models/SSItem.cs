using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class SSItem : IRData
    {
        public DataType Type { get; }
        public bool IsSigned => false;
        public bool IsNan => false;

        protected SSItem(DataType type)
        {
            Type = type;
        }
    }
}
