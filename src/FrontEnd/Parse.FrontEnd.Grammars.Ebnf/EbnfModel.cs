using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.Ebnf
{
    /// <summary>
    /// A parsed EBNF grammar, ready to be turned into a <see cref="EbnfGrammar"/>. Produced by the
    /// reader from grammar text; can also be built by hand.
    /// </summary>
    public sealed class EbnfModel
    {
        /// <summary>Token (terminal) rules: <c>Name := "regex"</c>.</summary>
        public List<EbnfTokenDef> Tokens { get; } = new List<EbnfTokenDef>();

        /// <summary>Non-terminal rules: <c>Name : alt | alt ;</c>. The first rule's head is the start symbol.</summary>
        public List<EbnfRuleDef> Rules { get; } = new List<EbnfRuleDef>();
    }

    /// <summary>A token rule: a named terminal defined by a regular-expression pattern.</summary>
    public sealed class EbnfTokenDef
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
    }

    /// <summary>A non-terminal rule: a head name and one or more alternatives (each a sequence of symbols).</summary>
    public sealed class EbnfRuleDef
    {
        public string Name { get; set; }
        public List<List<EbnfSym>> Alternatives { get; } = new List<List<EbnfSym>>();
    }

    /// <summary>A repetition marker on a symbol: none, <c>*</c> (zero-or-more), <c>+</c> (one-or-more), or <c>?</c> (optional).</summary>
    public enum EbnfQuantifier { None, ZeroOrMore, OneOrMore, Optional }

    /// <summary>
    /// One symbol inside a production body. It is a literal (<c>'x'</c>), a reference to a rule/token by
    /// name, or a group (<c>( … | … )</c>) — and it may carry a quantifier (<c>* + ?</c>). Groups and
    /// quantifiers are lowered to auto-generated productions when the grammar is built (see
    /// <see cref="EbnfGrammar"/>), reusing the same machinery as the C# grammar API's
    /// <c>ZeroOrMore()/OneOrMore()/Optional()</c>, so EBNF text and the C# API produce identical grammars.
    /// </summary>
    public sealed class EbnfSym
    {
        public bool IsLiteral { get; set; }
        public bool IsGroup { get; set; }
        public string Text { get; set; }                        // for a literal or a name reference
        public List<List<EbnfSym>> Group { get; set; }          // for a group: its alternatives
        public EbnfQuantifier Quantifier { get; set; } = EbnfQuantifier.None;

        public static EbnfSym Literal(string value) => new EbnfSym { IsLiteral = true, Text = value };
        public static EbnfSym Ref(string name) => new EbnfSym { IsLiteral = false, Text = name };
        public static EbnfSym GroupOf(List<List<EbnfSym>> alternatives) => new EbnfSym { IsGroup = true, Group = alternatives };
    }

    /// <summary>Thrown when a grammar model cannot be turned into a grammar (e.g. an undefined symbol).</summary>
    public sealed class EbnfException : Exception
    {
        public EbnfException(string message) : base(message) { }
    }
}
