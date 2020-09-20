using Parse.FrontEnd.ParseTree;

namespace Parse.FrontEnd
{
    public class ParseCreatedArgs
    {
        public ParseTreeSymbol ParseTreeNonT { get; }
        public SdtsParams SdtsParam { get; }

        public ParseCreatedArgs(ParseTreeSymbol parseTreeNonT, SdtsParams sdtsParam)
        {
            ParseTreeNonT = parseTreeNonT;
            SdtsParam = sdtsParam;
        }
    }
}
