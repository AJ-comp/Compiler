using System;

namespace Parse.FrontEnd.RegularGrammar
{
    public class Terminal : Symbol
    {
        private string caption = string.Empty;

        public TokenType TokenType { get; }
        public string Value { get; } = string.Empty;
        public bool Meaning { get; } = true;
        public bool bWord { get; } = false;

        /// <summary>
        /// If this argument is true regex expression is displayed like as (ex : ++ -> \+\+)
        /// </summary>
        public bool bOper => (TokenType is ScopeComment || 
                                         TokenType is Operator || 
                                         TokenType is Delimiter);
        public string RegexExpression
        {
            get
            {
                return (bOper) ? RegexGenerator.GetOperatorRegex(Value)
                                      : RegexGenerator.GetWordRegex(Value);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="meaning"></param>
        /// <param name="bWord">If this argument is false regex expression is displayed like as (ex : ++ -> \b++\b)</param>
        public Terminal(TokenType type, string value, bool meaning = true, bool bWord = false) : this(type, value, value, meaning, bWord)
        {
        }
        public Terminal(TokenType type, string value, string caption, bool meaning = true, bool bWord = false)
        {
            this.TokenType = type;
            this.Value = value;
            this.caption = caption;
            this.Meaning = meaning;
            this.bWord = bWord;
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
            this.UniqueKey = terminal.UniqueKey;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }

    public class NotDefined : Terminal
    {
        public NotDefined(string value = "") : base(TokenType.SpecialToken.NotDefined, value)
        {
            this.UniqueKey = KeyManager.NotDefinedKey;
        }
    }

    public class CustomTerminal : Terminal
    {
        public CustomTerminal(TokenType type, string value = "") : base(type, value)
        {
            this.UniqueKey = KeyManager.CustomTerminalKey;
        }
    }

    public class Epsilon : Terminal
    {
        public Epsilon() : base(TokenType.SpecialToken.Epsilon, "ε", "epsilon")
        {
            this.UniqueKey = KeyManager.EpsilonKey;
        }
    }

    public class EndMarker : Terminal
    {
        static string EndMarkerChar = "щ";

        public EndMarker() : base(TokenType.SpecialToken.Marker, EndMarkerChar)
        {
            this.UniqueKey = KeyManager.EndMarkerKey;
        }

        public override string ToString() => string.Format("This is EndMarker! char : {0}", EndMarkerChar);
    }
}
