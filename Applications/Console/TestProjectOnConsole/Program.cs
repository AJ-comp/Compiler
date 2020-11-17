using Parse.FrontEnd.MiniC;
using System;
using System.Diagnostics;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MiniCCompiler parser = new MiniCCompiler();

            var parsingResult = parser.Operate("test.mc", "void main(){}\r\n");
            bool result = parsingResult.Success;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            parsingResult = parser.Operate("test.mc", 10, "int a");
            result = parsingResult.Success;

            stopWatch.Stop();
            Console.WriteLine("Cost Time : {0} msec", stopWatch.ElapsedMilliseconds);

            parsingResult = parser.Operate("test.mc", 10, 5);
            result = parsingResult.Success;
        }
    }
}
