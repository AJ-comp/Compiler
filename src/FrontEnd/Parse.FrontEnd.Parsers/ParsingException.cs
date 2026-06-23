using System;

namespace Parse.FrontEnd.Parsers
{
    public class ParsingException : Exception
    {
        public int SeeingIndex { get; }

        public ParsingException(int seeingIndex)
        {
            SeeingIndex = seeingIndex;
        }
    }
}
