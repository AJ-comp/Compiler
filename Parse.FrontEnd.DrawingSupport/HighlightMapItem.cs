using Parse.FrontEnd.DrawingSupport.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.DrawingSupport
{
    public class HighlightMapItem
    {
        public TokenType TokenType { get; }

        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }

        public HashSet<TokenType> IgnoreTokenList { get; } = new HashSet<TokenType>();

        public string TokenTypeString
        {
            get
            {
                if (TokenType == null) return string.Empty;
                if (IgnoreTokenList.Contains(TokenType)) return string.Empty;

                #region Keyword
                if (TokenType is Keyword) return Resources.Keyword;
                else if (TokenType is NormalKeyword) return Resources.EtcKeyword;
                else if (TokenType is Repeateword) return Resources.RepeatStatement;
                else if (TokenType is Controlword) return Resources.ControlStatement;
                else if (TokenType is Accessword) return Resources.Accesser;
                else if (TokenType is DefinedDataType) return Resources.DefinedDataType;
                #endregion

                #region Digit
                else if (TokenType is Digit) return Resources.Digit;
                else if (TokenType is Digit2) return Resources.Digit2;
                else if (TokenType is Digit8) return Resources.Digit8;
                else if (TokenType is Digit10) return Resources.Digit10;
                else if (TokenType is Digit16) return Resources.Digit16;
                #endregion

                #region Operator
                else if (TokenType is Operator) return Resources.Operator;
                else if (TokenType is NormalOperator) return Resources.EtcOperator;
                else if (TokenType is Comma) return Resources.Comma;
                else if (TokenType is Square) return Resources.Squre;
                else if (TokenType is Parenthesis) return Resources.Parenthesis;
                else if (TokenType is CurlyBrace) return Resources.CurlyBrace;
                #endregion

                #region Special Token
                else if (TokenType is SpecialToken) return Resources.SpecialWord;
                else if (TokenType is Comment) return Resources.Comment;
                else if (TokenType is LineComment) return Resources.LineComment;
                else if (TokenType is ScopeComment) return Resources.ScopeComment;
                #endregion

                #region Identifier
                else if (TokenType is Identifier) return Resources.Identifier;
                #endregion

                return TokenType.ToString();
            }
        }


        public HighlightMapItem(TokenType tokenType, Brush foregroundBrush, Brush backgroundBrush)
        {
            TokenType = tokenType;
            ForegroundBrush = foregroundBrush;
            BackgroundBrush = backgroundBrush;

            IgnoreTokenList.Add(TokenType.SpecialToken.Delimiter);
            IgnoreTokenList.Add(TokenType.SpecialToken.NotDefined);
            IgnoreTokenList.Add(TokenType.SpecialToken.Epsilon);
            IgnoreTokenList.Add(TokenType.SpecialToken.Marker);
        }
    }
}
