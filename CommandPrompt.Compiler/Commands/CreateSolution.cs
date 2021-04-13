using CommandPrompt.Compiler.Models;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandPrompt.Compiler.Commands
{
    public class CreateSolution : Command
    {
        public CreateSolution(string version, string name, string description = null) : base(name, description)
        {
            _version = version;
            Add(new Argument<string>("fullPath"));

            Handler = CommandHandler.Create<string>(Process);
        }


        private string _version;

        private int Process(string fullPath)
        {
            return this.Execute(() =>
            {
                if (Path.GetExtension(fullPath) != SolutionModel.Extension) return ExceptionCode.NotCorrectFileExtension;

                var path = Path.GetDirectoryName(fullPath);
                if (Directory.Exists(path)) return ExceptionCode.AlreadyExistDirectory;
                Directory.CreateDirectory(path);

                // create solution file (솔루션 파일을 생성합니다)
                SolutionModel solution = new SolutionModel();
                solution.Version = _version;
                solution.Write(fullPath);

                Console.WriteLine($"It was created the solution {fullPath}");

                return 0;
            });
        }
    }
}
