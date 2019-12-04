using Parse;
using System;

namespace Parse.FrontEnd.RegularGrammar
{
    public class Terminal : Symbol
    {
        private string caption = string.Empty;

        public TokenType TokenType { get; }
        public string Value { get; } = string.Empty;
        public bool Meaning { get; } = true;
        public bool CanDerived { get; } = false;

        public Terminal(TokenType type, string value, bool meaning = true, bool CanDerived = false) : this(type, value, value, meaning, CanDerived)
        {
        }
        public Terminal(TokenType type, string value, string caption, bool meaning = true, bool CanDerived = false)
        {
            this.TokenType = type;
            this.Value = value;
            this.caption = caption;
            this.Meaning = meaning;
            this.CanDerived = CanDerived;
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
            this.uniqueKey = KeyManager.NotDefinedKey;
        }
    }

    public class Epsilon : Terminal
    {
        public Epsilon() : base(TokenType.Epsilon, "ε", "epsilon")
        {
            this.uniqueKey = KeyManager.EpsilonKey;
        }
    }

    public class EndMarker : Terminal
    {
        static string EndMarkerChar = "щ";

        public EndMarker() : base(TokenType.Marker, EndMarkerChar)
        {
            this.uniqueKey = KeyManager.EndMarkerKey;
        }
    }
}
