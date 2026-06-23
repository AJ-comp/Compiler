using ParsingLibrary.Utilities;
using System;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public enum TokenType { Keyword, Operator, Identifier, NotDefined, Epsilon, Digit2, Digit10, Digit8, Digit16, Marker };

    public class Terminal : Symbol
    {
        private string caption = string.Empty;

        public TokenType TokenType { get; }
        public string Value { get; } = string.Empty;
        public bool Pattern { get; } = false;

        public Terminal(TokenType type, string value, bool pattern = false)
        {
            this.TokenType = type;
            this.Value = value;
            this.caption = value;
            this.Pattern = pattern;
        }

        public Terminal(TokenType type, string value, string caption, bool pattern = false)
        {
            this.TokenType = type;
            this.Value = value;
            this.caption = caption;
            this.Pattern = pattern;
        }

        public override string ToString()
        {
            return this.caption;
        }

        public override string ToGrammarString()
        {
            return this.ToString();
        }

        public override string ToTreeString(ushort depth = 1)
        {
            string result = string.Empty;
            for (int i = 1; i < depth; i++) result += "  ";

            result += "Terminal : " + this.ToString() + Environment.NewLine;

            return result;
        }
    }

    public class InputTerminal : Terminal
    {
        public InputTerminal(Terminal terminal) : base(terminal.TokenType, terminal.Value)
        {
            this.uniqueKey = terminal.uniqueKey;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }

    public class NotDefined : Terminal
    {
        public NotDefined(string value = "") : base(TokenType.NotDefined, value)
        {
            this.uniqueKey = AutoGenerator.NotDefinedKey;
        }
    }

    public class Epsilon : Terminal
    {
        public Epsilon() : base(TokenType.Epsilon, "ε", "epsilon")
        {
            this.uniqueKey = AutoGenerator.EpsilonKey;
        }
    }

    public class EndMarker : Terminal
    {
        public EndMarker() : base(TokenType.Marker, "$")
        {
            this.uniqueKey = AutoGenerator.EndMarkerKey;
        }
    }
}
