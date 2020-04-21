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
        public IReadOnlyList<ParsingErrorInfo> AlarmInfos { get; }

        public AlarmEventArgs(string projectName, string fileName)
        {
            this.Status = AlarmStatus.None;
            this.ProjectName = projectName;
            this.FileName = fileName;
        }

        public AlarmEventArgs(string projectName, string fileName, int tokenIndex, int line, TokenData token, IReadOnlyList<ParsingErrorInfo> alarmInfos)
        {
            this.Status = AlarmStatus.ParsingError;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.TokenIndex = tokenIndex;
            this.Line = line;
            this.Token = token;
            this.AlarmInfos = alarmInfos;
        }

        public override string ToString() => string.Format("Status : {0}, TokenIndex : {1}, Token : {2}, Line : {3}, AlarmCount : {4}", 
                                                                                    this.Status, this.TokenIndex, this.Token, this.Line, this.AlarmInfos.Count);
    }
}
