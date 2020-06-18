using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class FuncData
    {
        private List<VarData> _arguments = new List<VarData>();

        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IReadOnlyList<VarData> Arguments => _arguments;

        public FuncData(IReadOnlyList<VarData> arguments, bool constReturn, ReturnType returnType, string name)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
        }
    }
}
