using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
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
