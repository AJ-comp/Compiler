using CommandPrompt.Compiler.Models;
using Parse.BackEnd.Target;
using Parse.BackEnd.Target.ARMv7.MSeries.CortexM3Models;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace CommandPrompt.Compiler.Commands
{
    public enum TargetType
    {
        stm32,
        avr,
    }

    public class CreateProject : Command
    {
        public CreateProject(string name, string description = null) : base(name, description)
        {
            var option = new Option("-target") { Argument = new Argument<TargetType>("target") };
            Add(option);
            Add(new Argument<string>("projPathWithFile"));
            Add(new Argument<string>("solutionFullPath"));

            Handler = CommandHandler.Create<TargetType, string, string>(Process);
        }


        private ProjectModel _projectModel = new ProjectModel();

        private int Process(TargetType target, string projPathWithFile, string solutionFullPath)
        {
            return this.Execute(() =>
            {
                if (target == TargetType.stm32) return HandleCreateProject(new CortexM3(), projPathWithFile, solutionFullPath);
                else if (target == TargetType.avr)
                {

                }

                return 1;
            });
        }


        /**********************************************/
        /// <summary>
        /// 프로젝트 생성 명령을 수행합니다.
        /// </summary>
        /// <param name="projPathWithFile"></param>
        /// <param name="solutionFullPath"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        /**********************************************/
        private int HandleCreateProject(Target target, string projPathWithFile, string solutionFullPath)
        {
            if (Path.GetExtension(projPathWithFile) != ProjectModel.Extension) return ExceptionCode.NotCorrectFileExtension;
            var pathInfo = new PathInfo(projPathWithFile, Path.GetDirectoryName(solutionFullPath));

            var projPath = Path.GetDirectoryName(pathInfo.TargetAbsoluteFullPath);
            if (Directory.Exists(projPath)) return ExceptionCode.AlreadyExistDirectory;
            Directory.CreateDirectory(projPath);

            // insert to solution file the modified content (솔루션 파일에 변경된 내용을 기재합니다)
            var solution = SolutionModel.Read(solutionFullPath);
            solution.ProjectPaths.Add(new PathModel(pathInfo.GetPath()));
            solution.Write(solutionFullPath);

            // 프로젝트 파일을 생성합니다
            ProjectModel project = new ProjectModel();
            //                project.Target = AssemblyManager.CreateInstanceFromClassName(ProjectData.Target) as Target;
            project.Target = target.Name;
            project.Write(projPathWithFile);

            //                _compiler.CreateAssembly(projFullPath);
            Console.WriteLine($"It was created the project {projPathWithFile}");

            return 0;
        }
    }
}
