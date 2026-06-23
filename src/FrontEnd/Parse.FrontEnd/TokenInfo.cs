using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd
{
    public class TokenInfo
    {
        public string Name { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsNotUsed { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex => StartIndex + Name.Length - 1;


        public TokenInfo(TokenData token)
        {
            Name = token.Input;
            IsVirtual = token.IsVirtual;
            IsNotUsed = token.IsNotUsed;
            StartIndex = token.TokenCell.StartIndex;
        }
    }
}
