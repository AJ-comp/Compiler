using System;
using System.Collections.Generic;

namespace Parse.WpfControls.Common
{
    public class TokenizeTextBox : ExtensionTextBox
    {
        public TokenData TokenData { get; }

        public TokenizeTextBox()
        {
            this.Loaded += (s, e) =>
            {

            };

//            this.TextChanged
        }
    }

    public class TokenWithCaretInfo
    {
        public enum PositionFromCaret { Included, FrontOfCaret, BackOfCaret }

        public int TokenIndex { get; }
        public TokenData TokenData { get; }

        public PositionFromCaret Type { get; }

        public TokenWithCaretInfo(int tokenIndex, TokenData tokenData, PositionFromCaret type)
        {
            this.TokenIndex = tokenIndex;
            this.TokenData = TokenData;
            this.Type = type;
        }
    }

}
