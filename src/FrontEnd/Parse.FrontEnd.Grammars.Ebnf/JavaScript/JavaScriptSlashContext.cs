using Janglim.FrontEnd.Tokenize;
using System;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Immutable JavaScript delimiter context for slash goals, arrow-block termination
    /// and class element/modifier boundaries. Class declaration/expression headers and
    /// computed names retain their enclosing delimiter identity across nested expressions.
    /// Each source read derives a state from its previous token by reference. Weak memoization
    /// never merges different tokenizations or speculative branches at the same source offset.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#arrows">화살표 함수의 본문과 문장 경계</see></para>
    /// <para><see href="../../../../docs/ko/javascript-classes.html#fields">클래스 필드와 메서드의 경계</see></para>
    /// <para><see href="../../../../docs/ko/javascript-modules.html#exports">default 함수·클래스 선언과 표현식의 구분</see></para>
    /// </summary>
    internal sealed class JavaScriptSlashContext
    {
        private readonly bool script;
        private readonly bool module;
        private readonly State initial;
        private readonly ConditionalWeakTable<Janglim.FrontEnd.TokenData, State> states =
            new ConditionalWeakTable<Janglim.FrontEnd.TokenData, State>();
        private readonly ConditionalWeakTable<Janglim.FrontEnd.TokenData, AsyncPrefix> asyncPrefixes =
            new ConditionalWeakTable<Janglim.FrontEnd.TokenData, AsyncPrefix>();
        private readonly ConditionalWeakTable<Janglim.FrontEnd.TokenData, LetPrefix> letPrefixes =
            new ConditionalWeakTable<Janglim.FrontEnd.TokenData, LetPrefix>();

        internal JavaScriptSlashContext(bool script, bool module = false)
        {
            this.script = script;
            this.module = module;
            initial = new State(null, true, script, false, null, null, null, 0, false, null, false);
        }

        internal bool UseDivision(LexicalContext context) => !Get(context.PreviousToken).Operand;
        internal void ObserveTransparent(Janglim.FrontEnd.TokenData previous, Janglim.FrontEnd.TokenData token)
            => states.GetValue(token, _ => Get(previous));
        internal bool IsArrowBlockEnd(Janglim.FrontEnd.TokenData token) => Get(token).ArrowBlockEnd;
        internal bool IsYieldIdentifierBefore(Janglim.FrontEnd.TokenData token) => Get(token).PreviousYieldIdentifier;
        internal bool IsLetDeclarationStart(Janglim.FrontEnd.TokenData token)
            => token?.Input == "let" && letPrefixes.TryGetValue(token, out var prefix) && prefix.Binding;
        internal bool IsLetBracket(Janglim.FrontEnd.TokenData token)
            => token?.Input == "let" && letPrefixes.TryGetValue(token, out var prefix) && prefix.Bracket;
        internal void ObserveLet(Janglim.FrontEnd.TokenData token, Janglim.FrontEnd.TokenData following)
            => letPrefixes.GetValue(token, _ => new LetPrefix(following));
        internal bool IsAsyncFunctionStart(Janglim.FrontEnd.TokenData token)
            => token != null && asyncPrefixes.TryGetValue(token, out var prefix) && prefix.Function;
        internal bool CanContinueClassModifier(Janglim.FrontEnd.TokenData token)
        {
            var frame = Get(token).Frame;
            return IsClassFrame(frame?.Kind) && frame.Element == 1 && frame.Prefix >= 1 && frame.Prefix <= 3;
        }

        internal void Observe(Janglim.FrontEnd.TokenData previous, Janglim.FrontEnd.TokenData token, string input,
            bool previousContextualIdentifier = false, bool previousAwaitOperand = false)
        {
            var before = Get(previous);
            if (token.Input == "async") asyncPrefixes.GetValue(token, _ => new AsyncPrefix(StartsAsyncFunction(input, token.EndIndex + 1)));
            states.GetValue(token, _ => Advance(before, token, previousContextualIdentifier, previousAwaitOperand));
        }

        // A contextual async prefix is determined from exact source spelling and
        // trivia, before selecting the declaration/expression grammar. This small
        // lexical lookahead neither reads a slash nor executes a parser branch.
        private static bool StartsAsyncFunction(string input, int index)
        {
            while (index < input.Length)
            {
                char ch = input[index];
                if (ch == '\r' || ch == '\n' || ch == '\u2028' || ch == '\u2029') return false;
                if (char.IsWhiteSpace(ch) || ch == '\ufeff') { index++; continue; }
                if (ch == '/' && index + 1 < input.Length && input[index + 1] == '*')
                {
                    index += 2;
                    while (index + 1 < input.Length && !(input[index] == '*' && input[index + 1] == '/'))
                    {
                        ch = input[index++];
                        if (ch == '\r' || ch == '\n' || ch == '\u2028' || ch == '\u2029') return false;
                    }
                    if (index + 1 >= input.Length) return false;
                    index += 2;
                    continue;
                }
                break;
            }
            const string keyword = "function";
            if (index + keyword.Length > input.Length || string.CompareOrdinal(input, index, keyword, 0, keyword.Length) != 0) return false;
            index += keyword.Length;
            if (index == input.Length) return true;
            int point = input[index];
            if (char.IsHighSurrogate(input[index]) && index + 1 < input.Length && char.IsLowSurrogate(input[index + 1]))
                point = char.ConvertToUtf32(input[index], input[index + 1]);
            return input[index] != '\\' && !JavaScriptUnicode.IsPart(point);
        }

        private sealed class AsyncPrefix
        {
            internal AsyncPrefix(bool function) { Function = function; }
            internal bool Function { get; }
        }

        private sealed class LetPrefix
        {
            internal LetPrefix(Janglim.FrontEnd.TokenData following)
            {
                Bracket = following.Input == "[";
                Binding = following.Input == "[" || following.Input == "{" || following.Kind.Caption == "IdentifierName" ||
                    following.Input == "await" || following.Input == "async" || following.Input == "of" ||
                    following.Input == "yield" || following.Input == "let" || following.Input == "static" ||
                    following.Input == "implements" || following.Input == "interface" || following.Input == "package" ||
                    following.Input == "private" || following.Input == "protected" || following.Input == "public";
            }
            internal bool Binding { get; }
            internal bool Bracket { get; }
        }

        private State Get(Janglim.FrontEnd.TokenData token)
        {
            if (ReferenceEquals(token, null)) return initial;
            if (states.TryGetValue(token, out var state)) return state;
            throw new InvalidOperationException("JavaScript token context requires tokens from this language's token source.");
        }

        private State Advance(State before, Janglim.FrontEnd.TokenData token, bool previousContextualIdentifier,
            bool previousAwaitOperand)
        {
            Frame frame = before.Frame;
            bool operand = before.Operand, statement = before.Statement, control = before.Control;
            string function = before.Function, body = before.Body, previous = before.Previous;
            int ternary = before.Ternary;
            bool arrowBlockEnd = false;
            ClassHeader classes = before.Classes;
            string text = token.Input, caption = token.Kind.Caption;
            bool property = previous == "." || previous == "?.";
            bool classFrame = IsClassFrame(frame?.Kind);
            bool objectFrame = frame?.Kind == "object";
            // The JavaScript policy returns Div only if the current parser state
            // expects a division continuation. On the next read this distinguishes
            // an identifier `let`/`yield` from its declaration/keyword spelling.
            // The fact belongs to this read, including speculative read branches.
            if (previousContextualIdentifier) operand = false;
            // The grammar can require an AwaitExpression operand in Script async
            // code as well as Module code. Unlike an identifier `await`, that
            // keyword cannot finish a statement at the following line terminator.
            // Apply the immutable goal fact before deciding declaration vs expression.
            if (previousAwaitOperand) operand = true;
            // ASI can finish a field before a new name on the next line. Calls and
            // computed-member suffixes still continue its initializer expression.
            if (classFrame && frame.Element == 2 && token.HasLineBreakBefore &&
                (!operand || before.ArrowBlockEnd) && IsElementName(token) &&
                (before.ArrowBlockEnd || text != "in" && text != "instanceof")) frame = frame.WithElement(0);
            else if (classFrame && frame.Element == 1 && frame.Prefix == 4 && token.HasLineBreakBefore &&
                IsElementName(token)) frame = frame.WithElement(0);
            bool elementName = (classFrame || objectFrame) && frame.Element != 2;
            bool statementScope = frame == null ? script : IsStatementFrame(frame.Kind);
            bool startsStatement = text == "function" || text == "class" || text == "{" ||
                text == "async" && IsAsyncFunctionStart(token) || text == "let" && IsLetDeclarationStart(token);
            if (token.HasLineBreakBefore && statementScope &&
                (previous == "return" || previous == "break" || previous == "continue" ||
                 previous == "yield" && !previousContextualIdentifier || !operand && startsStatement)) statement = true;
            // A newline may have just ended return/yield/an expression. Determine
            // async declaration/expression role after that ASI transition, not before.
            bool asyncStatement = text == "async" && !property &&
                (statement || module && previous == "default" || token.HasLineBreakBefore && before.ArrowBlockEnd);
            if (caption == "TemplateHead")
            {
                frame = new Frame("template", frame, ternary); ternary = 0;
                operand = true; statement = false;
            }
            else if (caption == "TemplateMiddle") { ternary = 0; operand = true; statement = false; }
            else if (caption == "TemplateTail")
            {
                if (frame?.Kind == "template") { ternary = frame.Ternary; frame = frame.Parent; }
                operand = false; statement = false;
            }
            else if (caption == "NoSubstitutionTemplate") { operand = false; statement = false; }
            else if (module && text == "default" && previous == "export" && !property && !elementName)
            {
                // The default value starts in expression context (including {}).
                // Only an immediately following function/class is a declaration.
                operand = true; statement = false;
            }
            else if (text == "class" && !property && !elementName)
            {
                classes = new ClassHeader(frame?.Identity, statement || module && previous == "default", false, classes);
                operand = true; statement = false;
            }
            else if (text == "extends" && !property && classes != null &&
                     ReferenceEquals(classes.FrameIdentity, frame?.Identity))
            {
                classes = new ClassHeader(classes.FrameIdentity, classes.Declaration, true, classes.Parent);
                operand = true; statement = false;
            }
            else if (text == "function" && !property && !elementName)
            {
                function = statement || module && previous == "default" ||
                    previous == "async" && before.AsyncStatement && !token.HasLineBreakBefore ? "declaration" : "expression";
                operand = true; statement = false;
            }
            else if (!property && !elementName && (text == "if" || text == "while" || text == "for" || text == "switch" || text == "catch" || text == "with"))
            {
                control = true; operand = true; statement = text == "catch";
            }
            else if (text == "(")
            {
                frame = new Frame(elementName ? "parameters:method" : function != null ? "parameters:" + function : control ? "control" : operand ? "group" : "call", frame, ternary);
                ternary = 0; function = null; control = false; operand = true; statement = false;
            }
            else if (text == ")")
            {
                string kind = frame?.Kind ?? "group";
                ternary = frame?.Ternary ?? 0; frame = frame?.Parent;
                body = kind.StartsWith("parameters:", StringComparison.Ordinal) ? kind.Substring(11) : null;
                statement = kind == "control" || body != null; operand = statement;
            }
            else if (text == "[")
            {
                if (elementName) frame = frame.WithElement(1, 4);
                frame = new Frame("array", frame, ternary); ternary = 0;
                operand = true; statement = false;
            }
            else if (text == "]")
            {
                ternary = frame?.Ternary ?? 0; frame = frame?.Parent;
                operand = false; statement = false;
            }
            else if (text == "{")
            {
                bool classBody = body == null && previous != "=>" && classes != null &&
                    ReferenceEquals(classes.FrameIdentity, frame?.Identity) && (!classes.Heritage || !operand);
                string kind = classBody ? "class:" + (classes.Declaration ? "declaration" : "expression") :
                    previous == "=>" ? "body:arrow" : body != null ? "body:" + body :
                    elementName && previous == "static" ? "body:staticblock" : statement ? "block" : "object";
                if (classBody) classes = classes.Parent;
                frame = new Frame(kind, frame, ternary); ternary = 0; control = false;
                body = null; operand = true; statement = IsStatementFrame(kind);
            }
            else if (text == "}")
            {
                string kind = frame?.Kind ?? "block";
                ternary = frame?.Ternary ?? 0; frame = frame?.Parent;
                arrowBlockEnd = kind == "body:arrow";
                statement = kind == "block" || kind == "body:declaration" || kind == "class:declaration" || arrowBlockEnd;
                operand = statement;
                if ((kind == "body:method" || kind == "body:staticblock") && (IsClassFrame(frame?.Kind) || frame?.Kind == "object"))
                {
                    frame = frame.WithElement(0); operand = true; statement = false;
                }
            }
            else if (text == ";")
            {
                if (classFrame) frame = frame.WithElement(0);
                operand = true; statement = frame == null ? script : IsStatementFrame(frame.Kind);
            }
            else if (text == "," && objectFrame && ternary == 0)
            { frame = frame.WithElement(0); operand = true; statement = false; }
            else if (!property && (text == "else" || text == "do" || text == "try" || text == "finally"))
            { operand = true; statement = true; }
            else if (text == "=>") { operand = true; statement = false; }
            else if (text == "?") { ternary++; operand = true; statement = false; }
            else if (text == ":")
            {
                // A keyword property such as ({class: {}}) did not start a class.
                if (classes != null && !classes.Heritage && ReferenceEquals(classes.FrameIdentity, frame?.Identity))
                    classes = classes.Parent;
                bool conditional = ternary != 0;
                if (conditional) ternary--;
                statement = !conditional && (frame == null ? script : IsStatementFrame(frame.Kind));
                operand = true; function = null; control = false;
                if (objectFrame && !conditional) frame = frame.WithElement(2);
            }
            else if (text == "++" || text == "--") { statement = false; }
            else if (module && text == "await" && !property && !elementName)
            { operand = true; statement = false; }
            else if (!property && !elementName && text == "yield")
            { operand = true; statement = false; }
            else if (!property && !elementName && text == "let" && letPrefixes.TryGetValue(token, out var letPrefix) &&
                (!letPrefix.Binding || !statement))
            { operand = false; statement = false; }
            else if (!property && (IsOperator(text) || text == "return" || text == "throw" || text == "var" || text == "let" || text == "const"))
            { operand = true; statement = false; }
            else { operand = false; statement = false; }
            if ((classFrame || objectFrame) && (IsClassFrame(frame?.Kind) || frame?.Kind == "object"))
            {
                if (text == "=") frame = frame.WithElement(2);
                else if (elementName && IsElementName(token)) frame = frame.WithName(text);
            }
            return new State(frame, operand, statement, control, function, body, text, ternary, arrowBlockEnd, classes, asyncStatement,
                previous == "yield" && previousContextualIdentifier);
        }

        private static bool IsClassFrame(string kind) => kind != null && kind.StartsWith("class:", StringComparison.Ordinal);
        private static bool IsElementName(Janglim.FrontEnd.TokenData token)
            => token.Kind.Caption == "IdentifierName" || token.Kind.Caption == "PrivateIdentifier" ||
               token.Kind.Caption == "StringLiteral" || token.Kind.Caption == "NumericLiteral" ||
               token.Input.Length != 0 && (char.IsLetter(token.Input[0]) || token.Input[0] == '$' || token.Input[0] == '_');
        private static bool IsStatementFrame(string frame) => frame == "block" || frame.StartsWith("body:", StringComparison.Ordinal);
        private static bool IsOperator(string text)
            => text == "," || text == "." || text == "?." || text == "=" || text == "+" || text == "-" ||
               text == "*" || text == "/" || text == "%" || text == "**" || text == "!" || text == "~" ||
               text == "<" || text == ">" || text == "<=" || text == ">=" || text == "==" || text == "!=" ||
               text == "===" || text == "!==" || text == "<<" || text == ">>" || text == ">>>" ||
               text == "&" || text == "|" || text == "^" || text == "&&" || text == "||" || text == "??" ||
               text == "in" || text == "instanceof" || text == "new" || text == "typeof" || text == "void" || text == "delete" ||
               text.EndsWith("=", StringComparison.Ordinal);

        private sealed class Frame
        {
            internal Frame(string kind, Frame parent, int ternary, int element = 0, object identity = null, int prefix = 0)
            { Kind = kind; Parent = parent; Ternary = ternary; Element = element; Identity = identity ?? new object(); Prefix = prefix; }
            internal string Kind { get; }
            internal Frame Parent { get; }
            internal int Ternary { get; }
            // Class elements: 0 begins a member, 1 reads its names, 2 reads its initializer.
            internal int Element { get; }
            // 1: static, 2: get/set, 3: static get/set, 4: actual name completed.
            internal int Prefix { get; }
            internal object Identity { get; }
            internal Frame WithElement(int element, int prefix = 0) => new Frame(Kind, Parent, Ternary, element, Identity, prefix);
            internal Frame WithName(string name)
            {
                int prefix = Element == 0 ? name == "static" ? 1 : name == "get" || name == "set" ? 2 : 4 :
                    Prefix == 1 && (name == "get" || name == "set") ? 3 : 4;
                return WithElement(1, prefix);
            }
        }

        private sealed class ClassHeader
        {
            internal ClassHeader(object frameIdentity, bool declaration, bool heritage, ClassHeader parent)
            { FrameIdentity = frameIdentity; Declaration = declaration; Heritage = heritage; Parent = parent; }
            internal object FrameIdentity { get; }
            internal bool Declaration { get; }
            internal bool Heritage { get; }
            internal ClassHeader Parent { get; }
        }

        private sealed class State
        {
            internal State(Frame frame, bool operand, bool statement, bool control, string function, string body,
                string previous, int ternary, bool arrowBlockEnd, ClassHeader classes, bool asyncStatement, bool previousYieldIdentifier = false)
            {
                Frame = frame; Operand = operand; Statement = statement; Control = control; Function = function;
                Body = body; Previous = previous; Ternary = ternary; ArrowBlockEnd = arrowBlockEnd; Classes = classes;
                AsyncStatement = asyncStatement;
                PreviousYieldIdentifier = previousYieldIdentifier;
            }
            internal Frame Frame { get; }
            internal bool Operand { get; }
            internal bool Statement { get; }
            internal bool Control { get; }
            internal string Function { get; }
            internal string Body { get; }
            internal string Previous { get; }
            internal int Ternary { get; }
            internal bool ArrowBlockEnd { get; }
            internal ClassHeader Classes { get; }
            internal bool AsyncStatement { get; }
            internal bool PreviousYieldIdentifier { get; }
        }
    }
}
