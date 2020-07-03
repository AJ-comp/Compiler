using Parse.FrontEnd.InterLanguages.Datas.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Datas
{
    public class IRFuncData : IRData
    {
        private List<IRVar> _arguments = new List<IRVar>();

        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IReadOnlyList<IRVar> Arguments => _arguments;

        public DataType Type => throw new System.NotImplementedException();
        public bool IsSigned => false;
        public bool IsNan => false;

        public IRFuncData(IReadOnlyList<IRVar> arguments, bool constReturn, ReturnType returnType, string name)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
        }
    }
}
