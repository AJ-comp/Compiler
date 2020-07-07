using Parse.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Parse.FrontEnd.Support.Drawing
{
    public class HighlightMapHelper : Singleton<HighlightMapHelper>
    {
        private Dictionary<Type, TokenResourceInfo> _dic = new Dictionary<Type, TokenResourceInfo>();

        public string TokenTypeString(Type type)
        {
            if (type == null) return string.Empty;
            if (_dic.ContainsKey(type) == false) return string.Empty;

            return _dic[type].DisplayName;
        }

        public Color DefaultForegroundColor(Type type)
        {
            if (type == null) return Color.Transparent;
            if (_dic.ContainsKey(type) == false) return Color.Transparent;

            return _dic[type].DefaultForegroundColor;
        }

        public Color DefaultBackgroundColor(Type type)
        {
            if (type == null) return Color.Transparent;
            if (_dic.ContainsKey(type) == false) return Color.Transparent;

            return _dic[type].DefaultBackgroundColor;
        }

        private HighlightMapHelper()
        {
            #region Keyword
            _dic.Add(typeof(Keyword), new TokenResourceInfo(Resources.Keyword, Color.LightCyan, Color.Transparent));
            _dic.Add(typeof(NormalKeyword), new TokenResourceInfo(Resources.EtcKeyword, Color.LightCyan, Color.Transparent));
            _dic.Add(typeof(Repeateword), new TokenResourceInfo(Resources.RepeatStatement, Color.LightCyan, Color.Transparent));
            _dic.Add(typeof(Controlword), new TokenResourceInfo(Resources.ControlStatement, Color.LightCyan, Color.Transparent));
            _dic.Add(typeof(Accessword), new TokenResourceInfo(Resources.Accesser, Color.LightCyan, Color.Transparent));
            _dic.Add(typeof(DefinedDataType), new TokenResourceInfo(Resources.DefinedDataType, Color.LightCyan, Color.Transparent));
            #endregion

            #region Digit
            _dic.Add(typeof(Digit), new TokenResourceInfo(Resources.Digit, Color.Purple, Color.Transparent));
            _dic.Add(typeof(Digit2), new TokenResourceInfo(Resources.Digit2, Color.Purple, Color.Transparent));
            _dic.Add(typeof(Digit8), new TokenResourceInfo(Resources.Digit8, Color.Purple, Color.Transparent));
            _dic.Add(typeof(Digit10), new TokenResourceInfo(Resources.Digit10, Color.Purple, Color.Transparent));
            _dic.Add(typeof(Digit16), new TokenResourceInfo(Resources.Digit16, Color.Purple, Color.Transparent));
            #endregion

            #region Operator
            _dic.Add(typeof(Operator), new TokenResourceInfo(Resources.Operator, Color.White, Color.Transparent));
            _dic.Add(typeof(NormalOperator), new TokenResourceInfo(Resources.EtcOperator, Color.White, Color.Transparent));
            _dic.Add(typeof(Comma), new TokenResourceInfo(Resources.Comma, Color.White, Color.Transparent));
            _dic.Add(typeof(Square), new TokenResourceInfo(Resources.Squre, Color.White, Color.Transparent));
            _dic.Add(typeof(Parenthesis), new TokenResourceInfo(Resources.Parenthesis, Color.White, Color.Transparent));
            _dic.Add(typeof(CurlyBrace), new TokenResourceInfo(Resources.CurlyBrace, Color.White, Color.Transparent));
            #endregion

            #region Special Token
            _dic.Add(typeof(SpecialToken), new TokenResourceInfo(Resources.SpecialWord, Color.Green, Color.Transparent));
            _dic.Add(typeof(Comment), new TokenResourceInfo(Resources.Comment, Color.Green, Color.Transparent));
            _dic.Add(typeof(LineComment), new TokenResourceInfo(Resources.LineComment, Color.Green, Color.Transparent));
            _dic.Add(typeof(ScopeComment), new TokenResourceInfo(Resources.ScopeComment, Color.Green, Color.Transparent));
            #endregion

            #region Identifier
            _dic.Add(typeof(Identifier), new TokenResourceInfo(Resources.Identifier, Color.White, Color.Transparent));
            #endregion
        }
    }

    public class TokenResourceInfo
    {
        public string DisplayName { get; }
        public Color DefaultForegroundColor { get; }
        public Color DefaultBackgroundColor { get; }

        public TokenResourceInfo(string displayName, Color defaultForegroundColor, Color defaultBackgroundColor)
        {
            DisplayName = displayName;
            DefaultForegroundColor = defaultForegroundColor;
            DefaultBackgroundColor = defaultBackgroundColor;
        }
    }
}
