using CommandPrompt.Compiler.Commands;
using CommandPrompt.Compiler.Properties;
using Compile.AJ;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Threading.Tasks;

namespace CommandPrompt.Compiler
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand();

            cmd.Name = "ajc";
            cmd.Description = Resource.Ajc;

            cmd.Add(new Option<string>("/t:bin", Resource.ToBinary));
//            cmd.Add(new Option<string>("/t:lib", Resource.ToLibrary));


            return await cmd.InvokeAsync(args);

            /*
            var cmd = new RootCommand
            {
                new Command("-v", Resource.Version).WithHandler(nameof(HandleVersion)),
                new Command("-c", Resource.Compile)
                {
                    new Option("-g"),
                    new Option("-m32"),
                    new Argument<string>("inputFileName", Resource.InputFile),
                    new Argument<string>("outputFileName", Resource.InputFile),
                }.WithHandler(nameof(HandleCompile)),
            };

            return await cmd.InvokeAsync(args);
            */
        }


        private static int ExecHandle(Func<int> func)
        {
            int result = 0;

            try
            {
                result = func.Invoke();
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                result = ex.HResult;
            }

            return result;
        }

        private static int HandleVersion(IConsole console)
        {
            console.Out.WriteLine($"Compiler Version : {_compiler.Version}");
            //            console.Out.WriteLine($"Linker Version : {}");

            return 0;
        }


        /**********************************************/
        /// <summary>
        /// 컴파일 명령을 수행합니다.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="m32"></param>
        /// <param name="inputFileName"></param>
        /// <param name="outputFileName"></param>
        /// <param name="console"></param>
        /// <returns></returns>
        /**********************************************/
        private static int HandleCompile(bool g, bool m32, string inputFileName, string outputFileName, IConsole console)
        {
            return ExecHandle(() =>
            {
                _compiler.CreateProject("TestAssembly");

                string receiveCommand = string.Empty;
                if (g) receiveCommand = "-g ";
                if (m32) receiveCommand += "-m32 ";
                receiveCommand += inputFileName;
                receiveCommand += outputFileName;

                console.Out.WriteLine($"{receiveCommand}");

                return 0;
            });
        }


        static AJCompiler _compiler = new AJCompiler();
    }
}
