using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Datas
{
    public class IRFuncData
    {
        private List<IRVar> _arguments = new List<IRVar>();

        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IEnumerable<IRVar> Arguments => _arguments;
        public bool IsSigned => false;
        public bool IsNan => false;

        public IRFuncData(IEnumerable<IRVar> arguments, bool constReturn, ReturnType returnType, string name)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
        }
    }
}
