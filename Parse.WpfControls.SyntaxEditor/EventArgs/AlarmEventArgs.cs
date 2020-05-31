using Parse.FrontEnd;
using Parse.FrontEnd.Parsers.Datas;
using System.Collections.Generic;

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

        public override string ToString() => string.Format("Status : {0}, TokenIndex : {1}, Token : {2}, Line : {3}, AlarmInfo : {4}", 
                                                                                    this.Status, this.TokenIndex, this.Token, this.Line, this.AlarmInfo);
    }
}
