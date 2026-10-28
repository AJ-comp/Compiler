using Janglim.FrontEnd.Tokenize;
using System;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Immutable JavaScript delimiter context for slash goals and arrow-block termination.
    /// Each source read derives a state from its previous token by reference. Weak memoization
    /// never merges different tokenizations or speculative branches at the same source offset.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#arrows">화살표 함수의 본문과 문장 경계</see></para>
    /// </summary>
    internal sealed class JavaScriptSlashContext
    {
        private readonly bool script;
        private readonly State initial;
        private readonly ConditionalWeakTable<Janglim.FrontEnd.TokenData, State> states =
            new ConditionalWeakTable<Janglim.FrontEnd.TokenData, State>();

        internal JavaScriptSlashContext(bool script)
        {
            this.script = script;
            initial = new State(null, true, script, false, null, null, null, 0, false);
        }

        internal bool UseDivision(LexicalContext context) => !Get(context.PreviousToken).Operand;
        internal bool IsArrowBlockEnd(Janglim.FrontEnd.TokenData token) => Get(token).ArrowBlockEnd;

        internal void Observe(Janglim.FrontEnd.TokenData previous, Janglim.FrontEnd.TokenData token)
        {
            var before = Get(previous);
            states.GetValue(token, _ => Advance(before, token));
        }

        private State Get(Janglim.FrontEnd.TokenData token)
        {
            if (ReferenceEquals(token, null)) return initial;
            if (states.TryGetValue(token, out var state)) return state;
            throw new InvalidOperationException("JavaScript token context requires tokens from this language's token source.");
        }

        private State Advance(State before, Janglim.FrontEnd.TokenData token)
        {
            Frame frame = before.Frame;
            bool operand = before.Operand, statement = before.Statement, control = before.Control;
            string function = before.Function, body = before.Body, previous = before.Previous;
            int ternary = before.Ternary;
            bool arrowBlockEnd = false;
            string text = token.Input, caption = token.Kind.Caption;
            bool property = previous == "." || previous == "?.";
            bool statementScope = frame == null ? script : IsStatementFrame(frame.Kind);
            if (token.HasLineBreakBefore && statementScope &&
                (previous == "return" || previous == "break" || previous == "continue" ||
                 !operand && (text == "function" || text == "{"))) statement = true;
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
            else if (text == "function" && !property)
            {
                function = statement ? "declaration" : "expression";
                operand = true; statement = false;
            }
            else if (!property && (text == "if" || text == "while" || text == "for" || text == "switch" || text == "catch"))
            {
                control = true; operand = true; statement = text == "catch";
            }
            else if (text == "(")
            {
                frame = new Frame(function != null ? "parameters:" + function : control ? "control" : operand ? "group" : "call", frame, ternary);
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
                string kind = previous == "=>" ? "body:arrow" : body != null ? "body:" + body : statement ? "block" : "object";
                frame = new Frame(kind, frame, ternary); ternary = 0; control = false;
                body = null; operand = true; statement = IsStatementFrame(kind);
            }
            else if (text == "}")
            {
                string kind = frame?.Kind ?? "block";
                ternary = frame?.Ternary ?? 0; frame = frame?.Parent;
                arrowBlockEnd = kind == "body:arrow";
                statement = kind == "block" || kind == "body:declaration" || arrowBlockEnd;
                operand = statement;
            }
            else if (text == ";")
            {
                operand = true; statement = frame == null ? script : IsStatementFrame(frame.Kind);
            }
            else if (!property && (text == "else" || text == "do" || text == "try" || text == "finally"))
            { operand = true; statement = true; }
            else if (text == "=>") { operand = true; statement = false; }
            else if (text == "?") { ternary++; operand = true; statement = false; }
            else if (text == ":")
            {
                bool conditional = ternary != 0;
                if (conditional) ternary--;
                statement = !conditional && (frame == null ? script : IsStatementFrame(frame.Kind));
                operand = true; function = null; control = false;
            }
            else if (text == "++" || text == "--") { statement = false; }
            else if (!property && (IsOperator(text) || text == "return" || text == "throw" || text == "var" || text == "let" || text == "const"))
            { operand = true; statement = false; }
            else { operand = false; statement = false; }
            return new State(frame, operand, statement, control, function, body, text, ternary, arrowBlockEnd);
        }

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
            internal Frame(string kind, Frame parent, int ternary) { Kind = kind; Parent = parent; Ternary = ternary; }
            internal string Kind { get; }
            internal Frame Parent { get; }
            internal int Ternary { get; }
        }

        private sealed class State
        {
            internal State(Frame frame, bool operand, bool statement, bool control, string function, string body,
                string previous, int ternary, bool arrowBlockEnd)
            {
                Frame = frame; Operand = operand; Statement = statement; Control = control; Function = function;
                Body = body; Previous = previous; Ternary = ternary; ArrowBlockEnd = arrowBlockEnd;
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
        }
    }
}
