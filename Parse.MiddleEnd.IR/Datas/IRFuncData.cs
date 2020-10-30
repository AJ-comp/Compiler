using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Datas
{
    public class IRFuncData : IUseDefChainable
    {
        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IEnumerable<IRVar> Arguments => _arguments;
        public bool IsSigned => false;
        public bool IsNan => false;
        public uint PointerLevel { get; set; }

        public IRFuncData(IEnumerable<IRVar> arguments, bool constReturn, ReturnType returnType, string name, uint pointerLevel)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
            PointerLevel = pointerLevel;
        }

        private List<IRVar> _arguments = new List<IRVar>();
    }
}
