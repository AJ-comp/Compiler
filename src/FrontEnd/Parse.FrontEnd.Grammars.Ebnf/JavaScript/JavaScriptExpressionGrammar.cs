using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    internal enum JavaScriptSourceGoal { Expression, Script, Module }

    /// <summary>
    /// ECMAScript 2026 expression grammar in a strict Script context, including ordinary
    /// async/ordinary arrows, generators, classes and object/class methods.
    /// Embedded EBNF defines structure; Unicode-aware token
    /// rules and parser-driven slash/template-continuation goals provide its lexical input.
    /// <para>Parse <see cref="CreateTokenSource"/> with LALRParser, then call
    /// <see cref="Validate"/> on the successful concrete tree for static early errors.</para>
    /// <para><see href="../../../../docs/ko/javascript-expressions.html">JavaScript 표현식 문법과 사용 순서</see></para>
    /// <para><see href="../../../../docs/ko/javascript-modules.html#dynamic-import">Script 표현식의 동적 import와 별도 Module 입력</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html">async·제너레이터·메서드 문법</see></para>
    /// </summary>
    public sealed class JavaScriptExpressionGrammar
    {
        public const int Edition = 2026;
        private readonly Lexer lexer;
        private readonly JavaScriptSlashContext slashContext;

        public EbnfGrammar Grammar { get; }
        public LexicalPolicy Policy { get; }
        public string GrammarText { get; }

        public JavaScriptExpressionGrammar() : this(JavaScriptSourceGoal.Expression) { }

        internal JavaScriptExpressionGrammar(bool script) : this(script ? JavaScriptSourceGoal.Script : JavaScriptSourceGoal.Expression) { }

        internal JavaScriptExpressionGrammar(JavaScriptSourceGoal goal)
        {
            bool module = goal == JavaScriptSourceGoal.Module;
            slashContext = new JavaScriptSlashContext(goal != JavaScriptSourceGoal.Expression, module);
            GrammarText = (module ? "Module : ModuleItemList? ;\n" : goal == JavaScriptSourceGoal.Script ? "Script : StatementList? ;\n" : string.Empty) +
                Load("Expressions") + "\n" + Load("Bindings") + "\n" + Load("Statements") + "\n" + Load("Classes") + "\n" + Load("Modules") + "\n" + Load("Functions");
            GrammarText = JavaScriptFunctionGrammar.Expand(GrammarText,
                module ? "Module" : goal == JavaScriptSourceGoal.Script ? "Script" : "Expression", module);
            GrammarText = Substitute(GrammarText, "IdentifierName", JavaScriptTokenPatterns.IdentifierName);
            GrammarText = Substitute(GrammarText, "PrivateIdentifier", "#" + JavaScriptTokenPatterns.IdentifierName);
            GrammarText = Substitute(GrammarText, "NumericLiteral", JavaScriptTokenPatterns.NumericLiteral);
            GrammarText = Substitute(GrammarText, "StringLiteral", JavaScriptTokenPatterns.StringLiteral);
            GrammarText = Substitute(GrammarText, "RegularExpressionLiteral", JavaScriptTokenPatterns.RegularExpressionLiteral);
            GrammarText = Substitute(GrammarText, "Comment", JavaScriptTokenPatterns.Comment);
            GrammarText = Substitute(GrammarText, "NoSubstitutionTemplate", JavaScriptTemplateTokens.NoSubstitutionTemplate);
            GrammarText = Substitute(GrammarText, "TemplateHead", JavaScriptTemplateTokens.TemplateHead);
            GrammarText = Substitute(GrammarText, "TemplateMiddle", JavaScriptTemplateTokens.TemplateMiddle);
            GrammarText = Substitute(GrammarText, "TemplateTail", JavaScriptTemplateTokens.TemplateTail);
            var options = JavaScriptStatementPredicates.CreateOptions(slashContext.IsArrowBlockEnd,
                slashContext.CanContinueClassModifier, slashContext.IsAsyncFunctionStart);
            var read = EbnfGrammarReader.Read(GrammarText, options);
            if (!read.Success) throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));
            Grammar = read.Grammar;

            lexer = new Lexer();
            foreach (var terminal in Grammar.TerminalSet) lexer.AddTokenRule(terminal);
            // ECMAScript WhiteSpace + LineTerminator, not .NET \s (which also includes NEL).
            const string whitespace = "\t\v\f \u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u202f\u205f\u3000\ufeff\r\n\u2028\u2029";
            foreach (char ch in whitespace)
                lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, ch.ToString(), false));

            var regex = Grammar.TerminalSet.Single(terminal => terminal.Caption == "RegularExpressionLiteral");
            var divisionTokens = Grammar.TerminalSet.Where(terminal => terminal.Value == "/" || terminal.Value == "/=").ToArray();
            var closeBrace = Grammar.TerminalSet.Single(terminal => terminal.Value == "}");
            var continuationTokens = Grammar.TerminalSet.Where(terminal =>
                terminal.Caption == "TemplateMiddle" || terminal.Caption == "TemplateTail").ToArray();
            var common = Grammar.TerminalSet.Where(terminal => !(terminal is EndMarker) && !(terminal is Epsilon)
                && !ReferenceEquals(terminal, regex) && !ReferenceEquals(terminal, closeBrace)
                && !divisionTokens.Contains(terminal) && !continuationTokens.Contains(terminal)).ToArray();
            var division = new LexicalGoal(common.Concat(divisionTokens).Append(closeBrace));
            var regularExpression = new LexicalGoal(common.Append(regex).Append(closeBrace));
            var divisionOrTemplateTail = new LexicalGoal(common.Concat(divisionTokens).Concat(continuationTokens));
            var regexOrTemplateTail = new LexicalGoal(common.Append(regex).Concat(continuationTokens));
            Policy = new LexicalPolicy(new[] { regularExpression, division, regexOrTemplateTail, divisionOrTemplateTail }, context =>
            {
                // Brace depth belongs to this cursor, including speculative parser branches.
                // It decides whether a `}` closes an object or resumes template text.
                bool template = JavaScriptTemplateTokens.IsAtTemplateBoundary(context.Cursor.LexicalState);
                bool expectsRegex = context.ExpectedTerminals.Any(terminal => ReferenceEquals(terminal, regex));
                bool expectsDivision = context.ExpectedTerminals.Any(terminal => divisionTokens.Any(item => ReferenceEquals(item, terminal)));
                if (expectsRegex && expectsDivision)
                    expectsDivision = slashContext.UseDivision(context);
                if (template) return expectsDivision ? divisionOrTemplateTail : regexOrTemplateTail;
                return expectsDivision ? division : regularExpression;
            });
        }

        /// <summary>Prepares a source; tokens are read and checked lazily as the parser requests them.</summary>
        public ITokenSource CreateTokenSource(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return new CheckedSource(lexer.CreateTokenSource(input, Policy), input, slashContext);
        }

        /// <summary>Checks strict identifiers, assignment and binding patterns, declaration scopes,
        /// function/class/control-flow contexts, private-name scopes, object early errors,
        /// and untagged-template escapes.</summary>
        public IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree)
            => JavaScriptExpressionValidation.Validate(tree);

        private static string Load(string name)
        {
            using (var stream = typeof(JavaScriptExpressionGrammar).Assembly
                .GetManifestResourceStream("Janglim.JavaScript.Ecma2026." + name + ".ebnf"))
            {
                if (stream == null) throw new InvalidOperationException("The embedded JavaScript " + name + " grammar is missing.");
                using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
            }
        }

        private static string Substitute(string text, string name, string pattern)
            => text.Replace("{{" + name + "}}", pattern.Replace("\"", "\"\""));

        private sealed class CheckedSource : ITokenSource
        {
            private readonly ITokenSource source;
            private readonly string input;
            private readonly JavaScriptSlashContext slashContext;
            public TokenCursor Start => source.Start;
            public int Length => source.Length;
            public LexicalPolicy Policy => source.Policy;

            internal CheckedSource(ITokenSource source, string input, JavaScriptSlashContext slashContext)
            {
                this.source = source;
                this.input = input;
                this.slashContext = slashContext;
            }

            public TokenRead Peek(TokenCursor start, LexicalGoal goal)
            {
                var read = source.Peek(start, goal);
                var token = read.Token;
                if (token.Kind is EndMarker)
                {
                    if (start.LexicalState.Length != 0)
                        throw new JavaScriptSyntaxException("Unterminated template substitution.", input.Length, 0);
                    return read;
                }
                if (token.StartIndex + 1 < input.Length && input[token.StartIndex] == '/' && input[token.StartIndex + 1] == '*')
                    throw new JavaScriptSyntaxException("Unterminated block comment", token.StartIndex, input.Length - token.StartIndex);
                if (token.Kind is NotDefined)
                    throw new JavaScriptSyntaxException(input[token.StartIndex] == '`' ||
                        input[token.StartIndex] == '}' && JavaScriptTemplateTokens.IsAtTemplateBoundary(start.LexicalState)
                        ? "Unterminated template literal" : "Unrecognized or malformed JavaScript token", token.StartIndex, token.Input.Length);
                JavaScriptTokenPatterns.Validate(token);
                slashContext.Observe(start.PreviousToken, token, input);
                return JavaScriptTemplateTokens.WithNextState(read, start.LexicalState);
            }
        }
    }
}
