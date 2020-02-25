using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.EventArgs;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public enum AlarmStatus { None, ParsingError, ParsingWarning }

    public class AlarmEventArgs : System.EventArgs
    {
        public AlarmStatus Status = AlarmStatus.None;

        public int TokenIndex { get; }
        public int Line { get; }
        public string ProjectName { get; }
        public string FileName { get; }

        public ParsingFailResult ParsingFailedArgs { get; } = null;

        public AlarmEventArgs(string projectName, string fileName)
        {
            this.Status = AlarmStatus.None;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.ParsingFailedArgs = null;
        }

        public AlarmEventArgs(string projectName, string fileName, int tokenIndex, int line, ParsingFailResult e)
        {
            this.Status = AlarmStatus.ParsingError;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.TokenIndex = tokenIndex;
            this.Line = line;
            this.ParsingFailedArgs = e;
        }
    }
}
