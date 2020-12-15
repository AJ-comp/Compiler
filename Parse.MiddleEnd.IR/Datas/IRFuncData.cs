using Parse.Extensions;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public sealed class IRFuncData : IUseDefChainable
    {
        public bool ConstReturn { get; }
        public ReturnType ReturnType { get; }
        public string Name { get; }
        public IEnumerable<IRVar> Arguments => _arguments;
        public bool IsSigned => false;
        public bool IsNan => false;
        public uint PointerLevel { get; set; }
        public string ArgumentsString => Arguments.ItemsString(PrintType.Property, "TypeName");

        public IRFuncData(IEnumerable<IRVar> arguments, bool constReturn, ReturnType returnType, string name, uint pointerLevel)
        {
            _arguments.AddRange(arguments);
            ConstReturn = constReturn;
            ReturnType = returnType;
            Name = name;
            PointerLevel = pointerLevel;
        }

        private List<IRVar> _arguments = new List<IRVar>();

        private string DebuggerDisplay
        {
            get
            {
                string result = string.Empty;

                if (ConstReturn) result += "const ";
                result += string.Format("{0} {1}", ReturnType, Name);

                result += "(";
                Arguments.ItemsString(PrintType.Property, "TypeName");
                result += ")";

                return result;
            }
        }
    }
}
