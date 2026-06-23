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
        public string FullPath => _alarmFileInfo?.FullPath;
        public string ProjectName => _alarmFileInfo?.ProjectName;
        public string FileName => _alarmFileInfo?.FileName;
        public int TokenIndex => (_alarmTokenInfo != null) ? _alarmTokenInfo.TokenIndex : 0;
        public TokenData TokenData => _alarmTokenInfo?.TokenData;
        public int Line => (_alarmTokenInfo != null) ? _alarmTokenInfo.Line : 0;

        public AlarmData(object fromControl, int status, string code, string message, AlarmFileInfo alarmFileInfo, AlarmTokenInfo alarmTokenInfo)
        {
            this.Status = status;

            this.FromControl = fromControl;
            this.Code = code;
            this.Message = message;

            _alarmFileInfo = alarmFileInfo;
            _alarmTokenInfo = alarmTokenInfo;
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
            return System.HashCode.Combine(Code, FullPath, TokenIndex);
        }

        private AlarmFileInfo _alarmFileInfo;
        private AlarmTokenInfo _alarmTokenInfo;
    }


    public class AlarmFileInfo
    {
        public string FullPath { get; }
        public string ProjectName { get; }
        public string FileName { get; }


        public AlarmFileInfo(string fullPath, string projectName, string fileName)
        {
            FullPath = fullPath;
            ProjectName = projectName;
            FileName = fileName;
        }
    }

    public class AlarmTokenInfo
    {
        public int TokenIndex;
        public TokenData TokenData;
        public int Line;

        public AlarmTokenInfo(int tokenIndex, TokenData tokenData, int line)
        {
            this.TokenIndex = tokenIndex;
            this.TokenData = tokenData;
            this.Line = line;
        }
    }
}
