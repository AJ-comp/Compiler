using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public class TokenData
    {
        public string Input => TokenCell?.Data;
        public Terminal Kind { get; } = new NotDefined();
        public TokenCell TokenCell { get; }
        public bool IsVirtual { get; private set; }
        public bool IsNotUsed { get; set; }

        public TokenData(Terminal kindTerminal, TokenCell tokenCell, bool isVirtual = false)
        {
            Kind = kindTerminal;
            TokenCell = tokenCell;
            IsVirtual = isVirtual;
            IsNotUsed = false;
        }

        public static TokenData CreateFromTokenCell(TokenCell tokenCell, bool bEndIndex)
        {
            Terminal terminal;

            if (tokenCell.Data == new EndMarker().Value && bEndIndex) terminal = new EndMarker();
            else
            {
                var typeData = tokenCell.PatternInfo.Terminal;
                if (typeData == null) terminal = new NotDefined();
                else if (typeData.TokenType == TokenType.SpecialToken.Delimiter || 
                            typeData.TokenType == TokenType.SpecialToken.Comment) terminal = null;
                else terminal = typeData;
            }

            //            return (terminal != null) ? new TokenData(terminal, tokenCell) : null;
            return new TokenData(terminal, tokenCell);
        }

        public override string ToString() => this.Input;

        public override bool Equals(object obj)
        {
            return obj is TokenData data &&
                   EqualityComparer<Terminal>.Default.Equals(Kind, data.Kind);
        }

        public override int GetHashCode()
        {
            return -2026186021 + EqualityComparer<Terminal>.Default.GetHashCode(Kind);
        }
    }
}
