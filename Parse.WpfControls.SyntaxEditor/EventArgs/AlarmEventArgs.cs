using Parse.FrontEnd.Parsers.EventArgs;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public enum AlarmStatus { None, ParsingError }

    public class AlarmEventArgs : System.EventArgs
    {
        public AlarmStatus Status = AlarmStatus.None;

        public string ProjectName { get; }
        public string FileName { get; }

        public ParsingFailedEventArgs ParsingFailedArgs { get; } = null;

        public AlarmEventArgs(string projectName, string fileName)
        {
            this.Status = AlarmStatus.None;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.ParsingFailedArgs = null;
        }

        public AlarmEventArgs(string projectName, string fileName, ParsingFailedEventArgs e)
        {
            this.Status = AlarmStatus.ParsingError;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.ParsingFailedArgs = e;
        }
    }
}
