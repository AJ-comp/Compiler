using System;

namespace Janglim.FrontEnd.RegularGrammar
{
    public class Terminal : Symbol
    {
        public TokenType TokenType { get; }
        public string Value { get; } = string.Empty;
        public string Caption { get; } = string.Empty;
        public bool Meaning { get; } = true;
        public bool IsWordPattern { get; } = false;

        /// <summary>
        /// If this argument is true regex expression is displayed like as (ex : ++ -> \+\+)
        /// </summary>
        public bool IsOper => (TokenType is ScopeComment || 
                                         TokenType is Operator || 
                                         TokenType is Delimiter);

        public bool IsNumber => TokenType is Digit10;

        public bool IsTextLiteral => (TokenType is StringLiteral || TokenType is CharLiteral);

        public string RegexExpression
        {
            get
            {
                if (IsOper) return RegexGenerator.GetOperatorRegex(Value);
                // Regex token rules are used exactly as authored. In particular, Identifier
                // and Keyword do not acquire .NET word boundaries: a language may allow '$'
                // or other characters outside .NET's \w. The lexer compares actual match
                // lengths and uses rule priority only to break ties. Explicit \b stays intact.
                return Value;
            }
        }


        /***************************************************************************************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="meaning"></param>
        /// <param name="bWord">Whether this is a regex rule rather than a literal, for display and tie priority.</param>
        /***************************************************************************************/
        public Terminal(TokenType type, string value, bool meaning = true, bool bWord = false) : this(type, value, value, meaning, bWord)
        {
        }
        public Terminal(TokenType type, string value, string caption, bool meaning = true, bool bWordPattern = false)
        {
            this.TokenType = type;
            this.Value = value;
            // Caption is the terminal's display label (ToString, table headers, FIRST/FOLLOW,
            // diagnostics); a null there crashes the text renderers, so it must never be null. When
            // no caption is supplied — or it resolves to null, e.g. from a missing resource string —
            // fall back to the value. (Value itself is left exactly as given: it is the lexer's
            // matching value/pattern, NOT a display string, so it must not be altered.)
            this.Caption = caption ?? value ?? string.Empty;
            this.Meaning = meaning;
            this.IsWordPattern = bWordPattern;

            EbnfString = value;
        }

        public override string ToString() => Caption;


        public override string ToEbnfString(bool bContainLHS = false)
        {
            return (IsWordPattern) ? $"{EbnfString}" : $"'{EbnfString}'";
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

        public override string ToString() => $"This is EndMarker! char : {EndMarkerChar}";
    }
}
