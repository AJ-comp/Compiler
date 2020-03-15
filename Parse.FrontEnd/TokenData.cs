﻿using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;

namespace Parse.FrontEnd
{
    public class TokenData
    {
        public string Input => TokenCell?.Data;
        public Terminal Kind { get; } = new NotDefined();
        public TokenCell TokenCell { get; }

        public TokenData(Terminal kindTerminal, TokenCell tokenCell)
        {
            this.Kind = kindTerminal;
            this.TokenCell = tokenCell;
        }

        public static TokenData CreateFromTokenCell(TokenCell tokenCell, bool bEndIndex)
        {
            Terminal terminal;

            if (tokenCell.Data == new EndMarker().Value && bEndIndex) terminal = new EndMarker();
            else
            {
                var typeData = tokenCell.PatternInfo.OptionData as Terminal;
                if (typeData == null) terminal = new NotDefined();
                else if (typeData.TokenType == TokenType.Delimiter || typeData.TokenType == TokenType.Comment) terminal = null;
                else terminal = typeData;
            }

            return new TokenData(terminal, tokenCell);
        }

        public override string ToString() => this.Input;
    }
}
