using Parse.FrontEnd.MiniCParser;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MiniCCompiler parser = new MiniCCompiler();
            var parsingResult = parser.Operate("test.mc", "void main(){}\r\n");
            bool result = parsingResult.Success;

            parsingResult = parser.Operate("test.mc", 10, "int a");
            result = parsingResult.Success;

            parsingResult = parser.Operate("test.mc", 10, 5);
            result = parsingResult.Success;
        }
    }
}
