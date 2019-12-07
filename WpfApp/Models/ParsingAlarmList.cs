using Parse.WpfControls.SyntaxEditor.EventArgs;

namespace WpfApp.Models
{
    class ParsingAlarmData
    {
        public AlarmStatus Status { get; }

        public string Code { get; }
        public string Message { get; }
        public string ProjectName { get; }
        public string FileName { get; }
        public int TokenIndex { get; }
        public int Line { get; }

        public ParsingAlarmData(AlarmStatus status, string code, string message, string projectName, string fileName, int line)
        {
            this.Status = status;

            this.Code = code;
            this.Message = message;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.Line = line;
        }
    }
}
