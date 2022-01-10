using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace CommandPrompt.Compiler.Commands
{
    public class LoadSource : Command
    {
        public LoadSource(string name, string description = null) : base(name, description)
        {
            Add(new Argument<string>("srcPathWithFile"));
            Add(new Argument<string>("projFullPath"));

            Handler = CommandHandler.Create<string, string>(Process);
        }


        private int Process(string srcPathWithFile, string projFullPath)
        {
            return this.Execute(() =>
            {
                if (Path.GetExtension(srcPathWithFile) != SourceModel.Extension) return ExceptionCode.NotCorrectFileExtension;
                var pathInfo = new PathInfo(srcPathWithFile, Path.GetDirectoryName(projFullPath));

                File.Open(pathInfo.TargetAbsoluteFullPath, FileMode.Open);

                // insert to project file the modified content (프로젝트 파일에 변경된 내용을 기재합니다)
                var project = ProjectModel.Read(projFullPath);
                if (pathInfo.Relative) project.RemoveFiles.Remove(new PathModel(pathInfo.TargetRelativeFullPath));
                else project.ExternalFiles.Add(new PathModel(pathInfo.TargetAbsoluteFullPath));
                project.Write(projFullPath);

                return 0;
            });
        }
    }
}
