using System;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public interface IShowable
    {
        string ToGrammarString();
        string ToTreeString(UInt16 depth=1);
    }
}
