using Parse.FrontEnd.DrawingSupport.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Parse.FrontEnd.DrawingSupport
{
    public class HighlightMapItem
    {
        public Type Type { get; }

        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }

        public static string TokenTypeString(Type type)
        {
            if (type == null) return string.Empty;

            #region Keyword
            if (type == typeof(Keyword)) return Resources.Keyword;
            else if (type == typeof(NormalKeyword)) return Resources.EtcKeyword;
            else if (type == typeof(Repeateword)) return Resources.RepeatStatement;
            else if (type == typeof(Controlword)) return Resources.ControlStatement;
            else if (type == typeof(Accessword)) return Resources.Accesser;
            else if (type == typeof(DefinedDataType)) return Resources.DefinedDataType;
            #endregion

            #region Digit
            else if (type == typeof(Digit)) return Resources.Digit;
            else if (type == typeof(Digit2)) return Resources.Digit2;
            else if (type == typeof(Digit8)) return Resources.Digit8;
            else if (type == typeof(Digit10)) return Resources.Digit10;
            else if (type == typeof(Digit16)) return Resources.Digit16;
            #endregion

            #region Operator
            else if (type == typeof(Operator)) return Resources.Operator;
            else if (type == typeof(NormalOperator)) return Resources.EtcOperator;
            else if (type == typeof(Comma)) return Resources.Comma;
            else if (type == typeof(Square)) return Resources.Squre;
            else if (type == typeof(Parenthesis)) return Resources.Parenthesis;
            else if (type == typeof(CurlyBrace)) return Resources.CurlyBrace;
            #endregion

            #region Special Token
            else if (type == typeof(SpecialToken)) return Resources.SpecialWord;
            else if (type == typeof(Comment)) return Resources.Comment;
            else if (type == typeof(LineComment)) return Resources.LineComment;
            else if (type == typeof(ScopeComment)) return Resources.ScopeComment;
            #endregion

            #region Identifier
            else if (type == typeof(Identifier)) return Resources.Identifier;
            #endregion

            return type.ToString();
        }


        public HighlightMapItem(Type type, Brush foregroundBrush, Brush backgroundBrush)
        {
            Type = type;
            ForegroundBrush = foregroundBrush;
            BackgroundBrush = backgroundBrush;
        }
    }
}
