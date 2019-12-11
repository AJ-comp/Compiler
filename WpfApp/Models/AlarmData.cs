using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;

namespace WpfApp.Models
{
    public class AlarmData
    {
        public AlarmStatus Status { get; }

        public object FromControl { get; }
        public string Code { get; }
        public string Message { get; }
        public string ProjectName { get; }
        public string FileName { get; }
        public int TokenIndex { get; }
        public int Line { get; }

        public Action<int> IndicateLogic { get; set; } = null;

        public AlarmData(object fromControl, AlarmStatus status, string code, string message, string projectName, string fileName, int tokenIndex, int line)
        {
            this.Status = status;

            this.FromControl = fromControl;
            this.Code = code;
            this.Message = message;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.TokenIndex = tokenIndex;
            this.Line = line;
        }
    }
}
