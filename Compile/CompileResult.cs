using Parse.FrontEnd;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Parsers.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Compile
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class CompileResult
    {
        public string FilePath { get; }
        public bool Result => Errors.Count() == 0;
        public ParsingResult ParsingResult { get; }
        public SdtsNode RootNode => ParsingResult.Success ? ParsingResult.AstRoot?.Sdts : null;
        public IEnumerable<AJTypeInfo> LinkErrorTypeList => _linkErrorTypeList;
        public IEnumerable<VariableAJ> LinkErrorVarList => _linkErrorVarList;

        public IEnumerable<ParsingErrorInfo> Errors
        {
            get
            {
                List<ParsingErrorInfo> result = new List<ParsingErrorInfo>();

                result.AddRange(ParsingResult.AllErrors);
                if (RootNode == null) return result;
                
                foreach (var alarmNode in RootNode.AllAlarmNodes)
                    result.AddRange(alarmNode.Alarms);

                return result;
            }
        }


        public CompileResult(string filePath, ParsingResult parsingResult)
        {
            FilePath = filePath;
            ParsingResult = parsingResult;
        }




        private List<AJTypeInfo> _linkErrorTypeList = new List<AJTypeInfo>();
        private List<VariableAJ> _linkErrorVarList = new List<VariableAJ>();

        private string GetDebuggerDisplay() => $"{FilePath} {Result}";
    }
}
