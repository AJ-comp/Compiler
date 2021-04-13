using System.IO;

namespace CommandPrompt.Compiler
{
    public class PathInfo
    {
        public string ParentPath { get; }
        public string TargetFileName { get; }
        public string TargetAbsoluteFullPath => Path.Combine(_targetPath, TargetFileName);
        public bool Relative => _targetPath.Contains(ParentPath);
        public string TargetRelativeFullPath
        {
            get
            {
                if (!Relative) return string.Empty;

                var targetPath = _targetPath.Substring(_targetPath.IndexOf(ParentPath));
                return Path.Combine(targetPath, TargetFileName);
            }
        }


        public PathInfo(string targetPathWithFile, string parentPath)
        {
            ParentPath = parentPath;
            var targetPath = Path.GetDirectoryName(targetPathWithFile);
            TargetFileName = Path.GetFileName(targetPathWithFile);

            _targetPath = (Path.IsPathRooted(targetPath)) ? targetPath : Path.Combine(parentPath, targetPath);
        }


        /**********************************************/
        /// <summary>
        /// 상대경로라면 상대경로를 반환하고 
        /// 상대경로가 아니라면 절대경로를 반환합니다.
        /// </summary>
        /// <returns></returns>
        /**********************************************/
        public string GetPath() => (Relative) ? TargetRelativeFullPath : TargetAbsoluteFullPath;


        private string _targetPath;
    }
}
