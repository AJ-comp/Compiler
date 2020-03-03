using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars
{
    public class AJGrammar : Grammar
    {
        private Terminal @identifier = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident");
        private Terminal @digit = new Terminal(TokenType.Identifier, "[0-9]*", "digit");

        private Terminal @namespace = new Terminal(TokenType.Keyword, "namespace");
        private Terminal @private = new Terminal(TokenType.Keyword, "private");
        private Terminal @internal = new Terminal(TokenType.Keyword, "internal");
        private Terminal @protected = new Terminal(TokenType.Keyword, "protected");
        private Terminal @public = new Terminal(TokenType.Keyword, "public");

        private Terminal @class = new Terminal(TokenType.Keyword, "class");
        private Terminal @enum = new Terminal(TokenType.Keyword, "enum");

        private Terminal @const = new Terminal(TokenType.Keyword, "const");
        private Terminal @var = new Terminal(TokenType.Keyword, "var");
        private Terminal @void = new Terminal(TokenType.Keyword, "void");
        private Terminal @char = new Terminal(TokenType.Keyword, "char");
        private Terminal @short = new Terminal(TokenType.Keyword, "short");
        private Terminal @int = new Terminal(TokenType.Keyword, "int");
        private Terminal @long = new Terminal(TokenType.Keyword, "long");
        private Terminal @float = new Terminal(TokenType.Keyword, "float");
        private Terminal @double = new Terminal(TokenType.Keyword, "double");

        private Terminal @for = new Terminal(TokenType.Keyword, "for");
        private Terminal @if = new Terminal(TokenType.Keyword, "if");

        private Terminal @dot = new Terminal(TokenType.Operator, ".");
        private Terminal @plus = new Terminal(TokenType.Operator, "+");
        private Terminal @minus = new Terminal(TokenType.Operator, "-");
        private Terminal @div = new Terminal(TokenType.Operator, "/");
        private Terminal @mul = new Terminal(TokenType.Operator, "*");
        private Terminal @replace = new Terminal(TokenType.Operator, "=");
        private Terminal @equal = new Terminal(TokenType.Operator, "==");
        private Terminal @plusAndReplace = new Terminal(TokenType.Operator, "+=");
        private Terminal @minusAndReplace = new Terminal(TokenType.Operator, "-=");
        private Terminal @divAndReplace = new Terminal(TokenType.Operator, "/=");
        private Terminal @mulAndReplace = new Terminal(TokenType.Operator, "*=");
        private Terminal @plusPlus = new Terminal(TokenType.Operator, "++");
        private Terminal @minusMinus = new Terminal(TokenType.Operator, "--");
        private Terminal @andAnd = new Terminal(TokenType.Operator, "&&");
        private Terminal @orOr = new Terminal(TokenType.Operator, "||");

        private Terminal @openCurlyBrace = new Terminal(TokenType.Operator, "{");
        private Terminal @closeCurlyBrace = new Terminal(TokenType.Operator, "}");
        private Terminal @openParenthesis = new Terminal(TokenType.Operator, "(");
        private Terminal @closeParenthesis = new Terminal(TokenType.Operator, ")");
        private Terminal @openSquareBrace = new Terminal(TokenType.Operator, "[");
        private Terminal @closeSquareBrace = new Terminal(TokenType.Operator, "]");

        private NonTerminal namespaceStatement = new NonTerminal("NamespaceStatement", true);
        private NonTerminal classStatement = new NonTerminal("ClassStatement");
        private NonTerminal enumStatement = new NonTerminal("EnumStatement");
        private NonTerminal varStatement = new NonTerminal("VarStatement");
        private NonTerminal funcStatement = new NonTerminal("FuncStatement");

        private NonTerminal accessor = new NonTerminal("Accessor");
        private NonTerminal varType = new NonTerminal("VarType");

        public override Sdts SDTS { get; }


        public AJGrammar()
        {
            this.namespaceStatement.SetItem(@namespace + @identifier + (dot + @identifier).ZeroOrMore() + @openCurlyBrace + classStatement + @closeCurlyBrace);
            this.classStatement.SetItem(accessor.Optional() + @class + @identifier + @openCurlyBrace + (enumStatement | varStatement | funcStatement).ZeroOrMore() + @closeCurlyBrace);
            this.enumStatement.SetItem(accessor.Optional() + @enum + @identifier + @openCurlyBrace + (@identifier + (@replace + @digit).ZeroOrMore()).ZeroOrMore() + @closeCurlyBrace);
            //            this.varStatement.Production(accessor.Optional() + varType + @identifier + )

            this.accessor.SetItem(@private | @internal | @protected | @public);
            this.varType.SetItem(@char | @short | @int | @long | @float | @double);

            this.Optimization();
        }
    }
}
