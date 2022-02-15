using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using Parse.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd
{
    public class TokenData
    {
        public string Input => TokenCell?.Data;
        public int StartIndex => (TokenCell != null) ? TokenCell.StartIndex : -1;
        public int EndIndex => (TokenCell != null) ? TokenCell.EndIndex : -1;
        public Terminal Kind { get; } = new NotDefined();
        public TokenCell TokenCell { get; }
        public bool IsVirtual { get; private set; }
        public bool IsStubCode { get; private set; }
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


        /***************************************************/
        /// <summary>
        /// This function creates the virtual token.        <br/>
        /// 가상 토큰을 생성합니다.                           <br/>
        /// </summary>
        /// <param name="virtualT"></param>
        /// <param name="valueWhenIdent">
        /// This param means when virtualT type is ident
        /// </param>
        /// <returns></returns>
        /***************************************************/
        public static TokenData CreateVirtualToken(Terminal virtualT, string valueWhenIdent = "ident")
        {
            Random random = new Random();
            var asId = random.Next(-99999999, -1);
            string value = virtualT.Value;

            if (virtualT.TokenType == TokenType.Identifier) value = valueWhenIdent;

            return new TokenData(virtualT, new TokenCell(asId, value, null), true);
        }


        public static TokenData CreateStubToken(Terminal virtualT, string valueWhenIdent = "ident")
        {
            var result = CreateVirtualToken(virtualT, valueWhenIdent);

            result.IsVirtual = false;
            result.IsStubCode = true;

            return result;
        }

        public override string ToString() => Input;

        public override bool Equals(object obj)
        {
            return obj is TokenData data &&
                   Input == data.Input &&
                   EqualityComparer<Terminal>.Default.Equals(Kind, data.Kind) &&
                   IsVirtual == data.IsVirtual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Input, Kind, IsVirtual);
        }

        public static bool operator ==(TokenData left, TokenData right)
        {
            return EqualityComparer<TokenData>.Default.Equals(left, right);
        }

        public static bool operator !=(TokenData left, TokenData right)
        {
            return !(left == right);
        }
    }


    public class TokenDataList : List<TokenData>
    {
        public string ToListString(string separator = ".") => this.ItemsString(PrintType.Property, "Input", separator);
    }
}
