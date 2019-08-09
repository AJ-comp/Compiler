using Parse.RegularGrammar;
using System.Collections.Generic;

namespace Parse
{
    public static class Convert
    {
        public static string ToBridgeSymbol(BridgeType bridgeType)
        {
            //"·"
            return (bridgeType == BridgeType.Concatenation) ? " " : " | ";
        }

        public static string ToBridgeWord(BridgeType bridgeType)
        {
            return (bridgeType == BridgeType.Concatenation) ? "C" : "A";
        }

        public static string ToString(Stack<object> stack, string separator = "")
        {
            object[] copy = new object[stack.Count];
            string result = string.Empty;
            stack.CopyTo(copy, 0);

            foreach(var data in copy)
            {
                result += separator;
                if (data is NonTerminalSingle) result += (data as NonTerminalSingle).Name;
                else result += data.ToString();
            }

            return result;
        }
    }
}
