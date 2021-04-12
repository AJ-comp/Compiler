using Parse.Extensions;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public sealed class IRFuncDefInfo
    {
        public int Index { get; } = 0;
        public StdType ReturnType { get; }
        public string Name { get; }
        public IEnumerable<IRDeclareVar> Arguments => _arguments;
        public bool IsSigned => false;
        public bool IsNan => false;
        public string ArgumentsString => Arguments.ItemsString(PrintType.Property, "TypeName");

        public IRStatement Statement { get; }

        public IRFuncDefInfo(IEnumerable<IRDeclareVar> arguments, 
                                        StdType returnType, 
                                        string name, 
                                        IRStatement statement)
        {
            _arguments.AddRange(arguments);
            ReturnType = returnType;
            Name = name;
            Statement = statement;
        }

        public IRFuncDefInfo(int index, 
                                        IEnumerable<IRDeclareVar> arguments, 
                                        StdType returnType, 
                                        string name, 
                                        IRStatement statement)
            : this(arguments, returnType, name, statement)
        {
            Index = index;
        }

        private List<IRDeclareVar> _arguments = new List<IRDeclareVar>();

        private string DebuggerDisplay
        {
            get
            {
                string result = string.Empty;

                result += string.Format("{0} {1}", Helper.GetEnumDescription(ReturnType), Name);

                result += "(";
                Arguments.ItemsString(PrintType.Property, "TypeName");
                result += ")";

                return result;
            }
        }
    }
}
