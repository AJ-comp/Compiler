using Janglim.FrontEnd.ParseTree;

namespace Janglim.FrontEnd
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
