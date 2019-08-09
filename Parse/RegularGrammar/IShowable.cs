using System;

namespace Parse.RegularGrammar
{
    public interface IShowable
    {
        string ToGrammarString();
        string ToTreeString(UInt16 depth=1);
    }
}
