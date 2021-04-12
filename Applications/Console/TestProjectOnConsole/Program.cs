using Parse.FrontEnd.MiniC;
using System;
using System.Diagnostics;
using System.IO;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MiniCCompiler compiler = new MiniCCompiler();

            try
            {
                var fileFullPath = Path.Combine(Environment.CurrentDirectory, "test.mc");

                compiler.CreateAssembly("TestAssembly");
                compiler.AddFileToAssembly("TestAssembly", fileFullPath);

                var parsingResult = compiler.NewParsing(fileFullPath);
                bool result = parsingResult.Success;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

//                parsingResult = compiler.Parsing(fileFullPath, 2, 10, "int a");
//                result = parsingResult.Success;

                stopWatch.Stop();
                Console.WriteLine("Cost Time : {0} msec", stopWatch.ElapsedMilliseconds);

                compiler.StartSemanticAnalysis(fileFullPath);
                result = compiler.AllBuild();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
