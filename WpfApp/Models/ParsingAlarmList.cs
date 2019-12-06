namespace WpfApp.Models
{
    class ParsingAlarmData
    {
        public enum Kind { Warning, Error }
        public string Code { get; }
        public string Message { get; }
        public string ProjectName { get; }
        public string FileName { get; }
        public string Line { get; }

        public ParsingAlarmData(string code, string message, string projectName, string fileName, string line)
        {
            this.Code = code;
            this.Message = message;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.Line = line;
        }
    }
}
