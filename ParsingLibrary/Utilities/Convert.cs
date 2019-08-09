using ParsingLibrary.Datas.RegularGrammar;

namespace ParsingLibrary.Utilities
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
    }
}
