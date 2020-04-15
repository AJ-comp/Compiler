using System;

namespace Parse
{
    [Flags]
    public enum DrawOption { None = 0, Underline = 1, EndPointUnderline = 2, Selected = 4 }

    public enum TokenType { Skip, Delimiter, DefinedDataType, Operator, Identifier, Comment, NotDefined, Epsilon,
        NormalKeyword, RepeateKeyword, ControlKeyword, AccesserKeyword,
        Digit2, Digit10, Digit8, Digit16, 
        Marker };
}
