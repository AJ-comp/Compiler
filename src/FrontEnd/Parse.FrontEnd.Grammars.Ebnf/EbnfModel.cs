using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.Ebnf
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

    /// <summary>One symbol inside a production body: a literal (<c>'x'</c>) or a reference to a rule/token by name.</summary>
    public sealed class EbnfSym
    {
        public bool IsLiteral { get; set; }
        public string Text { get; set; }

        public static EbnfSym Literal(string value) => new EbnfSym { IsLiteral = true, Text = value };
        public static EbnfSym Ref(string name) => new EbnfSym { IsLiteral = false, Text = name };
    }

    /// <summary>Thrown when a grammar model cannot be turned into a grammar (e.g. an undefined symbol).</summary>
    public sealed class EbnfException : Exception
    {
        public EbnfException(string message) : base(message) { }
    }
}
