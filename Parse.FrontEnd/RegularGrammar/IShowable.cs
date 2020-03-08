using System;

namespace Parse.FrontEnd.RegularGrammar
{
    public interface IShowable
    {
        string ToGrammarString();
        string ToTreeString(UInt16 depth=1);
    }
}
