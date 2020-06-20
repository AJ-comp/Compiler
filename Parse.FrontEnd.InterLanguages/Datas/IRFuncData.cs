using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRFuncData : IRData
    {
        private List<IRVarData> _arguments = new List<IRVarData>();

        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IReadOnlyList<IRVarData> Arguments => _arguments;

        public IRFuncData(IReadOnlyList<IRVarData> arguments, bool constReturn, ReturnType returnType, string name)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
        }
    }
}
