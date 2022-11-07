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
        public ParsingResult ParsingResult { get; }
        public SdtsNode RootNode => ParsingResult.Success ? ParsingResult.AstRoot?.Sdts : null;
        public IEnumerable<AJType> LinkErrorTypeList => _linkErrorTypeList;
        public IEnumerable<VariableAJ> LinkErrorVarList => _linkErrorVarList;
        public bool Result
        {
            get
            {
                foreach (var alarm in Errors)
                {
                    if (alarm.ErrType == ErrorType.Error) return false;
                }

                return true;
            }
        }

        public IEnumerable<ParsingErrorInfo> Errors
        {
            get
            {
                List<ParsingErrorInfo> result = new List<ParsingErrorInfo>();

                result.AddRange(ParsingResult.AllErrors);
                if (RootNode == null) return result;

                foreach (var alarmNode in RootNode.AllAlarmNodes)
                {
                    result.AddRange(alarmNode.Alarms);
                }

                return result;
            }
        }


        public CompileResult(string filePath, ParsingResult parsingResult)
        {
            FilePath = filePath;
            ParsingResult = parsingResult;
        }




        private List<AJType> _linkErrorTypeList = new List<AJType>();
        private List<VariableAJ> _linkErrorVarList = new List<VariableAJ>();

        private string GetDebuggerDisplay() => $"{FilePath} {Result}";
    }
}
