using Parse.FrontEnd.MiniCParser;

namespace TestProjectOnConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MiniCParser parser = new MiniCParser();
            var parsingResult = parser.Operate("test.mc", "#define A 10\r\n void main()\r\n");

            bool result = parsingResult.Success;
        }
    }
}
