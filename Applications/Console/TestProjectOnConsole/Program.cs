using Compile.AJ;
using System;
using System.Diagnostics;
using System.IO;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            AJCompiler compiler = new AJCompiler();

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
                Console.WriteLine($"Cost Time : {stopWatch.ElapsedMilliseconds} msec");

                var analysisResult = compiler.StartSemanticAnalysis(fileFullPath);
                var finalExpression = compiler.CreateFinalExpression(analysisResult.SdtsRoot);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
