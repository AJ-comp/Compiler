using Parse.FrontEnd.AJ;
using Parse.FrontEnd.Grammars;
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
            if (extension == MiniCSource) result = new AJGrammar();
            else if (extension == Header) result = new AJGrammar();


            return result;
        }
    }
}
