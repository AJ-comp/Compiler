using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Grammars
{
    public class AJGrammar : Grammar
    {
        private Terminal @identifier = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", Resource.Ident);
        private Terminal @digit = new Terminal(TokenType.Identifier, "[0-9]*", Resource.Number);

        private Terminal @namespace = new Terminal(TokenType.Keyword.NormalKeyword, "namespace");
        private Terminal @private = new Terminal(TokenType.Keyword.Accessword, "private");
        private Terminal @internal = new Terminal(TokenType.Keyword.Accessword, "internal");
        private Terminal @protected = new Terminal(TokenType.Keyword.Accessword, "protected");
        private Terminal @public = new Terminal(TokenType.Keyword.Accessword, "public");

        private Terminal @class = new Terminal(TokenType.Keyword.NormalKeyword, "class");
        private Terminal @enum = new Terminal(TokenType.Keyword.NormalKeyword, "enum");

        private Terminal @const = new Terminal(TokenType.Keyword.DefinedDataType, "const");
        private Terminal @var = new Terminal(TokenType.Keyword.DefinedDataType, "var");
        private Terminal @void = new Terminal(TokenType.Keyword.DefinedDataType, "void");
        private Terminal @char = new Terminal(TokenType.Keyword.DefinedDataType, "char");
        private Terminal @short = new Terminal(TokenType.Keyword.DefinedDataType, "short");
        private Terminal @int = new Terminal(TokenType.Keyword.DefinedDataType, "int");
        private Terminal @long = new Terminal(TokenType.Keyword.DefinedDataType, "long");
        private Terminal @float = new Terminal(TokenType.Keyword.DefinedDataType, "float");
        private Terminal @double = new Terminal(TokenType.Keyword.DefinedDataType, "double");

        private Terminal @for = new Terminal(TokenType.Keyword.Repeateword, "for");
        private Terminal @if = new Terminal(TokenType.Keyword.Controlword, "if");

        private Terminal @dot = new Terminal(TokenType.Operator.NormalOperator, ".");
        private Terminal @plus = new Terminal(TokenType.Operator.NormalOperator, "+");
        private Terminal @minus = new Terminal(TokenType.Operator.NormalOperator, "-");
        private Terminal @div = new Terminal(TokenType.Operator.NormalOperator, "/");
        private Terminal @mul = new Terminal(TokenType.Operator.NormalOperator, "*");
        private Terminal @replace = new Terminal(TokenType.Operator.NormalOperator, "=");
        private Terminal @equal = new Terminal(TokenType.Operator.NormalOperator, "==");
        private Terminal @plusAndReplace = new Terminal(TokenType.Operator.NormalOperator, "+=");
        private Terminal @minusAndReplace = new Terminal(TokenType.Operator.NormalOperator, "-=");
        private Terminal @divAndReplace = new Terminal(TokenType.Operator.NormalOperator, "/=");
        private Terminal @mulAndReplace = new Terminal(TokenType.Operator.NormalOperator, "*=");
        private Terminal @plusPlus = new Terminal(TokenType.Operator.NormalOperator, "++");
        private Terminal @minusMinus = new Terminal(TokenType.Operator.NormalOperator, "--");
        private Terminal @andAnd = new Terminal(TokenType.Operator.NormalOperator, "&&");
        private Terminal @orOr = new Terminal(TokenType.Operator.NormalOperator, "||");

        private Terminal @openCurlyBrace = new Terminal(TokenType.Operator.CurlyBrace, "{");
        private Terminal @closeCurlyBrace = new Terminal(TokenType.Operator.CurlyBrace, "}");
        private Terminal @openParenthesis = new Terminal(TokenType.Operator.Parenthesis, "(");
        private Terminal @closeParenthesis = new Terminal(TokenType.Operator.Parenthesis, ")");
        private Terminal @openSquareBrace = new Terminal(TokenType.Operator.Square, "[");
        private Terminal @closeSquareBrace = new Terminal(TokenType.Operator.Square, "]");

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
