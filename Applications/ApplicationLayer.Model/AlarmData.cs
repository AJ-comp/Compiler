using Parse.FrontEnd;
using System.Collections.Generic;

namespace ApplicationLayer.Models
{
    public class AlarmData
    {
        public int Status { get; }

        public object FromControl { get; }
        public string Code { get; }
        public string Message { get; }
        public string FullPath { get; }
        public string ProjectName { get; }
        public string FileName { get; }
        public int TokenIndex { get; }
        public TokenData TokenData { get; }
        public int Line { get; }

        public AlarmData(object fromControl, int status, string code, string message, string fullPath, string projectName, string fileName, int tokenIndex, TokenData tokenData, int line)
        {
            this.Status = status;

            this.FromControl = fromControl;
            this.Code = code;
            this.Message = message;
            this.FullPath = fullPath;
            this.ProjectName = projectName;
            this.FileName = fileName;
            this.TokenIndex = tokenIndex;
            this.TokenData = tokenData;
            this.Line = line;
        }

        public override bool Equals(object obj)
        {
            return obj is AlarmData data &&
                   Code == data.Code &&
                   FullPath == data.FullPath &&
                   TokenIndex == data.TokenIndex;
        }

        public override int GetHashCode()
        {
            var hashCode = -1608452051;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
            hashCode = hashCode * -1521134295 + TokenIndex.GetHashCode();
            return hashCode;
        }
    }
}
