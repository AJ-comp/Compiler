using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.IO;

namespace ApplicationLayer.Common
{
    public class LanguageExtensions
    {
        public static string MiniCSource { get; } = "mc";
        public static string AJSource { get; } = "aj";
        public static string Header { get; } = "h";


        public static Grammar GetGrammarByExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            Grammar result = null;
            if (extension == MiniCSource) result = new MiniCGrammar();
            else if (extension == Header) result = new MiniCGrammar();


            return result;
        }
    }
}
