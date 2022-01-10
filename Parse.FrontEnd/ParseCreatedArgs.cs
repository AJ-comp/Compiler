using Parse.FrontEnd.ParseTree;

namespace Parse.FrontEnd
{
    public class ParseCreatedArgs
    {
        public ParseTreeSymbol ParseTreeNonT { get; }
        public CompileParameter SdtsParam { get; }

        public ParseCreatedArgs(ParseTreeSymbol parseTreeNonT, CompileParameter sdtsParam)
        {
            ParseTreeNonT = parseTreeNonT;
            SdtsParam = sdtsParam;
        }
    }
}
