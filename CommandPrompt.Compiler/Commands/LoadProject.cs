using CommandPrompt.Compiler.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace CommandPrompt.Compiler.Commands
{
    public class LoadProject : Command
    {
        public LoadProject(string name, string description = null) : base(name, description)
        {
            Add(new Argument<string>("projPathWithFile"));
            Add(new Argument<string>("solutionFullPath"));

            Handler = CommandHandler.Create<string, string>(Process);
        }



        private int Process(string projPathWithFile, string solutionFullPath)
        {
            return this.Execute(() =>
            {
                if (Path.GetExtension(projPathWithFile) != ProjectModel.Extension) return ExceptionCode.NotCorrectFileExtension;
                var pathInfo = new PathInfo(projPathWithFile, Path.GetDirectoryName(solutionFullPath));

                File.Open(pathInfo.TargetAbsoluteFullPath, FileMode.Open);

                // insert to solution file the modified content (솔루션 파일에 변경된 내용을 기재합니다)
                var solution = SolutionModel.Read(solutionFullPath);
                solution.ProjectPaths.Add(new PathModel(pathInfo.GetPath()));
                solution.Write(solutionFullPath);

                return 0;
            });
        }
    }
}
