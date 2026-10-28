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
    /// <summary>
    /// Prepares ECMAScript 2026 grammar, lexical input and static validation for Script,
    /// Module or a single expression. Script is the default input kind. Modules always
    /// use strict mode; Script and expression inputs have selectable initial strictness.
    /// Embedded EBNF defines structure; Unicode-aware token rules and parser-driven
    /// slash/template-continuation goals provide lexical input.
    /// <para>Parse <see cref="CreateTokenSource"/> with LALRParser, then call
    /// <see cref="Validate"/> on the successful concrete tree for static early errors.</para>
    /// <para><see href="../../../../docs/ko/javascript-expressions.html">JavaScript 표현식 문법과 사용 순서</see></para>
    /// <para><see href="../../../../docs/ko/javascript-modules.html#api">하나의 JavaScriptGrammar에서 입력 종류 선택</see></para>
    /// <para><see href="../../../../docs/ko/javascript-statements.html">Script의 함수·문장·선언 문법</see></para>
    /// <para><see href="../../../../docs/ko/javascript-classes.html">클래스와 멤버 문맥</see></para>
    /// <para><see href="../../../../docs/ko/javascript-functions.html">async·제너레이터·메서드 문법</see></para>
    /// <para><see href="../../../../docs/ko/javascript-non-strict.html">비엄격 모드와 선택적 브라우저 호환 문법</see></para>
    /// </summary>
    public sealed class JavaScriptGrammar
    {
        public const int Edition = 2026;
        private readonly Lexer lexer;
        private readonly JavaScriptSlashContext slashContext;
        private readonly LexicalGoal divisionGoal;
        private readonly LexicalGoal divisionTemplateGoal;
        private readonly LexicalGoal awaitOperandGoal;
        private readonly LexicalGoal awaitOperandTemplateGoal;
        private readonly JavaScriptDialect dialect;

        public EbnfGrammar Grammar { get; }
        public LexicalPolicy Policy { get; }
        public string GrammarText { get; }
        public JavaScriptInputKind InputKind { get; }
        public JavaScriptMode Mode { get; }
        public bool WebCompatibility { get; }

        /// <summary>Prepares strict Script input with browser compatibility disabled.</summary>
        public JavaScriptGrammar() : this(JavaScriptMode.Strict) { }

        /// <summary>
        /// Selects the input kind and prepares its grammar and token rules. Module input
        /// always uses strict mode, including when <paramref name="mode"/> is NonStrict.
        /// Browser compatibility is an independent option for every input kind.
        /// </summary>
        public JavaScriptGrammar(JavaScriptMode mode = JavaScriptMode.Strict, bool webCompatibility = false,
            JavaScriptInputKind inputKind = JavaScriptInputKind.Script)
            : this(mode, webCompatibility, inputKind, null) { }

        internal JavaScriptGrammar(JavaScriptMode mode, bool webCompatibility,
            JavaScriptInputKind inputKind, JavaScriptDialect dialect)
        {
            if (mode != JavaScriptMode.Strict && mode != JavaScriptMode.NonStrict)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (inputKind != JavaScriptInputKind.Script && inputKind != JavaScriptInputKind.Module &&
                inputKind != JavaScriptInputKind.Expression)
                throw new ArgumentOutOfRangeException(nameof(inputKind));
            InputKind = inputKind;
            this.dialect = dialect;
            Mode = inputKind == JavaScriptInputKind.Module ? JavaScriptMode.Strict : mode;
            WebCompatibility = webCompatibility;
            bool module = inputKind == JavaScriptInputKind.Module;
            slashContext = new JavaScriptSlashContext(inputKind != JavaScriptInputKind.Expression, module);
            GrammarText = (module ? "Module : ModuleItemList? ;\n" : inputKind == JavaScriptInputKind.Script ? "Script : StatementList? ;\n" : string.Empty) +
                Load("Expressions") + "\n" + Load("Bindings") + "\n" + Load("Statements") + "\n" + Load("Classes") + "\n" + Load("Modules") + "\n" + Load("Functions");
            if (Mode == JavaScriptMode.NonStrict || webCompatibility)
                GrammarText = JavaScriptNonStrictGrammar.Prepare(GrammarText, Mode, webCompatibility);
            if (dialect != null) GrammarText = dialect.PrepareGrammar(GrammarText);
            string entry = dialect?.EntryRule ?? (module ? "Module" : inputKind == JavaScriptInputKind.Script ? "Script" : "Expression");
            GrammarText = Mode == JavaScriptMode.NonStrict
                ? JavaScriptFunctionGrammar.ExpandNonStrict(GrammarText, entry)
                : JavaScriptFunctionGrammar.Expand(GrammarText, entry, module);
            GrammarText = Substitute(GrammarText, "IdentifierName", JavaScriptTokenPatterns.IdentifierName);
            GrammarText = Substitute(GrammarText, "PrivateIdentifier", "#" + JavaScriptTokenPatterns.IdentifierName);
            GrammarText = Substitute(GrammarText, "NumericLiteral", JavaScriptTokenPatterns.NumericLiteralFor(Mode));
            GrammarText = Substitute(GrammarText, "StringLiteral", JavaScriptTokenPatterns.StringLiteralFor(Mode));
            GrammarText = Substitute(GrammarText, "RegularExpressionLiteral", JavaScriptTokenPatterns.RegularExpressionLiteral);
            GrammarText = Substitute(GrammarText, "Comment", JavaScriptTokenPatterns.CommentFor(webCompatibility, module));
            GrammarText = Substitute(GrammarText, "NoSubstitutionTemplate", JavaScriptTemplateTokens.NoSubstitutionTemplate);
            GrammarText = Substitute(GrammarText, "TemplateHead", JavaScriptTemplateTokens.TemplateHead);
            GrammarText = Substitute(GrammarText, "TemplateMiddle", JavaScriptTemplateTokens.TemplateMiddle);
            GrammarText = Substitute(GrammarText, "TemplateTail", JavaScriptTemplateTokens.TemplateTail);
            var options = JavaScriptStatementPredicates.CreateOptions(slashContext.IsArrowBlockEnd,
                slashContext.CanContinueClassModifier, slashContext.IsAsyncFunctionStart,
                slashContext.IsLetDeclarationStart, slashContext.IsLetBracket, slashContext.IsYieldIdentifierBefore,
                dialect == null ? (Func<PredicateContext, bool>)null : dialect.AllowExpressionStatement,
                dialect == null ? (Func<PredicateContext, bool>)null : dialect.AllowStatementListStatement);
            dialect?.ConfigurePredicates(options);
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
                && (dialect == null || dialect.IsRuntimeTerminal(terminal))
                && !ReferenceEquals(terminal, regex) && !ReferenceEquals(terminal, closeBrace)
                && !divisionTokens.Contains(terminal) && !continuationTokens.Contains(terminal)).ToArray();
            var division = new LexicalGoal(common.Concat(divisionTokens).Append(closeBrace));
            var regularExpression = new LexicalGoal(common.Append(regex).Append(closeBrace));
            var divisionOrTemplateTail = new LexicalGoal(common.Concat(divisionTokens).Concat(continuationTokens));
            var regexOrTemplateTail = new LexicalGoal(common.Append(regex).Concat(continuationTokens));
            divisionGoal = division;
            divisionTemplateGoal = divisionOrTemplateTail;
            // These goals have the same token rules as the ordinary RegExp goals.
            // Their identity carries a branch-local fact to CheckedSource: the
            // previous `await` requires an operand, rather than ending an identifier.
            // No source-offset cache or parser/lexer API change is needed.
            awaitOperandGoal = new LexicalGoal(regularExpression.Terminals);
            awaitOperandTemplateGoal = new LexicalGoal(regexOrTemplateTail.Terminals);
            var goals = new[] { regularExpression, division, regexOrTemplateTail, divisionOrTemplateTail,
                awaitOperandGoal, awaitOperandTemplateGoal };
            Func<LexicalContext, LexicalGoal> select = context =>
            {
                // Brace depth belongs to this cursor, including speculative parser branches.
                // It decides whether a `}` closes an object or resumes template text.
                bool template = JavaScriptTemplateTokens.IsAtTemplateBoundary(context.Cursor.LexicalState);
                bool expectsRegex = context.ExpectedTerminals.Any(terminal => ReferenceEquals(terminal, regex));
                bool expectsDivision = context.ExpectedTerminals.Any(terminal => divisionTokens.Any(item => ReferenceEquals(item, terminal)));
                // In ~Yield code, `yield` and `let` may be ordinary identifiers.
                // Division is a valid continuation of an identifier, but not of a
                // bare YieldExpression. The grammar's expected terminals distinguish
                // those cases without guessing the next source characters.
                if (expectsRegex && expectsDivision && !(Mode == JavaScriptMode.NonStrict &&
                    (context.PreviousToken?.Input == "yield" || context.PreviousToken?.Input == "let")))
                    expectsDivision = slashContext.UseDivision(context);
                if (ExpectsAwaitOperand(context, expectsRegex, expectsDivision))
                    return template ? awaitOperandTemplateGoal : awaitOperandGoal;
                if (template) return expectsDivision ? divisionOrTemplateTail : regexOrTemplateTail;
                return expectsDivision ? division : regularExpression;
            };
            Policy = dialect == null ? new LexicalPolicy(goals, select) : dialect.CreatePolicy(Grammar, goals, select);
        }

        /// <summary>Prepares a source; tokens are read and checked lazily as the parser requests them.</summary>
        public ITokenSource CreateTokenSource(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return new CheckedSource(lexer.CreateTokenSource(input, Policy), input, slashContext, Mode,
                divisionGoal, divisionTemplateGoal, awaitOperandGoal, awaitOperandTemplateGoal, dialect);
        }

        private static bool ExpectsAwaitOperand(LexicalContext context, bool expectsRegex, bool expectsDivision)
        {
            // AwaitExpression requires a UnaryExpression. A RegExp may start that
            // operand, but a division continuation cannot follow a bare keyword.
            // BindingIdentifier and LabelIdentifier can also spell `await`; their
            // expected initializer/statement-end tokens distinguish those positions.
            // Check exact spelling so an escaped identifier never becomes a keyword.
            return context.PreviousToken?.Input == "await" && expectsRegex && !expectsDivision &&
                !context.ExpectedTerminals.Any(terminal => terminal is EndMarker || terminal.Value == "=" ||
                    terminal.Value == ";" || terminal.Value == "}" || terminal.Value == "return");
        }

        /// <summary>Checks context-dependent identifiers, assignment and binding patterns, declaration scopes,
        /// function/class/control-flow contexts, private-name scopes, object early errors,
        /// and untagged-template escapes.</summary>
        public IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree)
            => JavaScriptExpressionValidation.Validate(tree, Mode, WebCompatibility);

        private static string Load(string name)
        {
            using (var stream = typeof(JavaScriptGrammar).Assembly
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
            private readonly JavaScriptMode mode;
            private readonly LexicalGoal division;
            private readonly LexicalGoal divisionTemplate;
            private readonly LexicalGoal awaitOperand;
            private readonly LexicalGoal awaitOperandTemplate;
            private readonly JavaScriptDialect dialect;
            public TokenCursor Start => source.Start;
            public int Length => source.Length;
            public LexicalPolicy Policy => source.Policy;

            internal CheckedSource(ITokenSource source, string input, JavaScriptSlashContext slashContext, JavaScriptMode mode,
                LexicalGoal division, LexicalGoal divisionTemplate, LexicalGoal awaitOperand, LexicalGoal awaitOperandTemplate,
                JavaScriptDialect dialect)
            {
                this.source = source;
                this.input = input;
                this.slashContext = slashContext;
                this.mode = mode;
                this.division = division;
                this.divisionTemplate = divisionTemplate;
                this.awaitOperand = awaitOperand;
                this.awaitOperandTemplate = awaitOperandTemplate;
                this.dialect = dialect;
            }

            public TokenRead Peek(TokenCursor start, LexicalGoal goal)
            {
                var read = source.Peek(start, goal);
                var token = read.Token;
                dialect?.ObserveToken(token, input);
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
                JavaScriptTokenPatterns.Validate(token, mode);
                if (dialect != null && dialect.IsTransparent(goal))
                {
                    slashContext.ObserveTransparent(start.PreviousToken, token);
                    return JavaScriptTemplateTokens.WithNextState(read, start.LexicalState);
                }
                if (mode == JavaScriptMode.NonStrict && token.Input == "let")
                    slashContext.ObserveLet(token, source.Peek(read.Next, goal).Token);
                // Each Peek creates a separate token, so this immutable boundary fact
                // stays with the actual/speculative read that selected its goal.
                bool previousContextualIdentifier = mode == JavaScriptMode.NonStrict &&
                    (start.PreviousToken?.Input == "yield" || start.PreviousToken?.Input == "let") &&
                    (ReferenceEquals(goal, division) || ReferenceEquals(goal, divisionTemplate));
                bool previousAwaitOperand = ReferenceEquals(goal, awaitOperand) || ReferenceEquals(goal, awaitOperandTemplate);
                slashContext.Observe(start.PreviousToken, token, input, previousContextualIdentifier, previousAwaitOperand);
                return JavaScriptTemplateTokens.WithNextState(read, start.LexicalState);
            }
        }
    }
}
