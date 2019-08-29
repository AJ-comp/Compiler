using Parse.Extensions;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.LR;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
//            Parser parser = new LLParser(new Ex8_10Grammar());
            Parser parser = new SLRParser(new MiniCGrammar());

            Console.WriteLine(parser.RegularGrammar);
            Console.WriteLine(parser.AnalysisResult);
            Console.WriteLine(parser.OptimizeList);
            parser.ParsingTable.Print();
            Console.WriteLine(parser.PossibleTerminalSet.ToString());

            //            parser.Parse("namespace std { public class name {} }");

            //            parser.Parse("a,a");
            parser.Parse("a*a+a");
            parser.ParsingHistory.Print();
            Console.WriteLine(parser.ToParsingTreeString());
        }
    }
}
