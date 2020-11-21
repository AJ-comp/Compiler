using Parse.FrontEnd;
using System.Diagnostics;

namespace Parse.FrontEnd.Support.EventArgs
{
    public enum AlarmStatus { None, ParsingError, ParsingWarning }


    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class AlarmEventArgs : System.EventArgs
    {
        public AlarmStatus Status = AlarmStatus.None;

        public int TokenIndex { get; }
        public int Line { get; }
        public string ProjectName { get; }
        public string FileName { get; }
        public TokenData Token { get; }
        public ParsingErrorInfo AlarmInfo { get; }

        public AlarmEventArgs(string projectName, string fileName)
        {
            this.Status = AlarmStatus.None;
            this.ProjectName = projectName;
            this.FileName = fileName;
        }

        public AlarmEventArgs(string projectName, string fileName, int tokenIndex, int line, TokenData token, ParsingErrorInfo alarmInfo)
        {
            this.Status = AlarmStatus.ParsingError;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.TokenIndex = tokenIndex;
            this.Line = line;
            this.Token = token;
            this.AlarmInfo = alarmInfo;
        }

        private string DebuggerDisplay
            => string.Format("Status : {0}, TokenIndex : {1}, Token : {2}, Line : {3}, AlarmInfo : {4}", 
                                        Status, 
                                        TokenIndex, 
                                        Token, 
                                        Line, 
                                        AlarmInfo);
    }
}
