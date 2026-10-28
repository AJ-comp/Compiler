using Janglim.FrontEnd.Tokenize;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Classifies consumed JavaScript delimiters when a merged LR row admits both
    /// slash goals. This language-only scan is pure; each prediction uses its own
    /// consumed history. Token text inside strings, comments and regexes is never scanned.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#slash">문맥에 따른 슬래시 읽기 방식</see></para>
    /// </summary>
    internal static class JavaScriptSlashContext
    {
        internal static bool UseDivision(LexicalContext context, bool script)
        {
            var frames = new Stack<string>();
            var conditionals = new Stack<int>();
            bool operand = true, statement = script, control = false;
            string function = null, body = null, previous = null;
            int ternary = 0;
            foreach (var token in context.ConsumedTokens)
            {
                string text = token.Input, caption = token.Kind.Caption;
                bool property = previous == "." || previous == "?.";
                bool statementScope = frames.Count == 0 ? script : IsStatementFrame(frames.Peek());
                if (token.HasLineBreakBefore && statementScope &&
                    (previous == "return" || previous == "break" || previous == "continue" ||
                     !operand && (text == "function" || text == "{"))) statement = true;
                if (caption == "TemplateHead")
                {
                    conditionals.Push(ternary); ternary = 0; frames.Push("template"); operand = true; statement = false;
                }
                else if (caption == "TemplateMiddle") { ternary = 0; operand = true; statement = false; }
                else if (caption == "TemplateTail")
                {
                    if (frames.Count != 0 && frames.Peek() == "template") { frames.Pop(); ternary = conditionals.Pop(); }
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
                    conditionals.Push(ternary); ternary = 0;
                    frames.Push(function != null ? "parameters:" + function : control ? "control" : operand ? "group" : "call");
                    function = null; control = false; operand = true; statement = false;
                }
                else if (text == ")")
                {
                    string frame = frames.Count == 0 ? "group" : frames.Pop();
                    ternary = conditionals.Count == 0 ? 0 : conditionals.Pop();
                    body = frame.StartsWith("parameters:") ? frame.Substring(11) : null;
                    statement = frame == "control" || body != null;
                    operand = statement;
                }
                else if (text == "[") { conditionals.Push(ternary); ternary = 0; frames.Push("array"); operand = true; statement = false; }
                else if (text == "]") { if (frames.Count != 0) { frames.Pop(); ternary = conditionals.Pop(); } operand = false; statement = false; }
                else if (text == "{")
                {
                    conditionals.Push(ternary); ternary = 0; control = false;
                    frames.Push(body != null ? "body:" + body : statement ? "block" : "object");
                    body = null; operand = true; statement = IsStatementFrame(frames.Peek());
                }
                else if (text == "}")
                {
                    string frame = frames.Count == 0 ? "block" : frames.Pop();
                    ternary = conditionals.Count == 0 ? 0 : conditionals.Pop();
                    statement = frame == "block" || frame == "body:declaration";
                    operand = statement;
                }
                else if (text == ";")
                {
                    operand = true; statement = frames.Count == 0 ? script : IsStatementFrame(frames.Peek());
                }
                else if (!property && (text == "else" || text == "do" || text == "try" || text == "finally"))
                { operand = true; statement = true; }
                else if (text == "?") { ternary++; operand = true; statement = false; }
                else if (text == ":")
                {
                    bool conditional = ternary != 0;
                    if (conditional) ternary--;
                    statement = !conditional && (frames.Count == 0 ? script : IsStatementFrame(frames.Peek()));
                    operand = true; function = null; control = false;
                }
                else if (text == "++" || text == "--") { statement = false; }
                else if (!property && (IsOperator(text) || text == "return" || text == "throw" || text == "var" || text == "let" || text == "const"))
                { operand = true; statement = false; }
                else { operand = false; statement = false; }
                previous = text;
            }
            return !operand;
        }

        private static bool IsStatementFrame(string frame) => frame == "block" || frame.StartsWith("body:");
        private static bool IsOperator(string text)
            => text == "," || text == "." || text == "?." || text == "=" || text == "+" || text == "-" ||
               text == "*" || text == "/" || text == "%" || text == "**" || text == "!" || text == "~" ||
               text == "<" || text == ">" || text == "<=" || text == ">=" || text == "==" || text == "!=" ||
               text == "===" || text == "!==" || text == "<<" || text == ">>" || text == ">>>" ||
               text == "&" || text == "|" || text == "^" || text == "&&" || text == "||" || text == "??" ||
               text == "in" || text == "instanceof" || text == "new" || text == "typeof" || text == "void" || text == "delete" ||
               text.EndsWith("=");
    }
}
