using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Pure token-boundary conditions for strict JavaScript statements. Separate EBNF
    /// termination rules distinguish completed expressions, bare bindings, restricted
    /// statements, and do/while; no synthetic token is inserted into the engine input.
    /// <para><see href="../../../../docs/ko/javascript-statements.html#semicolon">자동 세미콜론과 문장 종료 조건</see></para>
    /// <para><see href="../../../../docs/ko/javascript-classes.html#fields">클래스 필드와 문맥상 수식어의 경계</see></para>
    /// <para><see href="../../../../docs/ko/javascript-modules.html#exports">Module 선언의 종료와 default 구문</see></para>
    /// </summary>
    internal static class JavaScriptStatementPredicates
    {
        private static readonly HashSet<string> Continuations = new HashSet<string>
        {
            "(", "[", ".", "?.", ",", "?", "=", "+", "-", "*", "/", "%", "**",
            "<", ">", "<=", ">=", "==", "!=", "===", "!==", "<<", ">>", ">>>",
            "&", "|", "^", "&&", "||", "??", "in", "instanceof", "*=", "/=", "%=",
            "+=", "-=", "<<=", ">>=", ">>>=", "&=", "|=", "^=", "**=", "&&=", "||=", "??="
        };

        internal static EbnfReaderOptions CreateOptions(Func<Janglim.FrontEnd.TokenData, bool> isArrowBlockEnd,
            Func<Janglim.FrontEnd.TokenData, bool> canContinueClassModifier,
            Func<Janglim.FrontEnd.TokenData, bool> isAsyncFunctionStart)
            => new EbnfReaderOptions()
                .RegisterPredicate("NoLineBreak", context => !context.NextToken.HasLineBreakBefore)
                .RegisterPredicate("NoPostfix", context => context.NextToken.HasLineBreakBefore ||
                    context.NextToken.Input != "++" && context.NextToken.Input != "--" && context.NextToken.Input != "=>" &&
                    !(context.PreviousToken?.Input == "async" && IsIdentifier(context.NextToken)))
                .RegisterPredicate("AsyncFunctionStart", context => isAsyncFunctionStart(context.NextToken))
                .RegisterPredicate("ArrowExpressionBody", context => context.NextToken.Input != "{")
                .RegisterPredicate("AsyncModifier", context => context.PreviousToken?.Input == "async" && !context.NextToken.HasLineBreakBefore)
                .RegisterPredicate("StaticGeneratorModifier", context => context.PreviousToken?.Input == "static")
                .RegisterPredicate("ObjectAccessor", context => context.PreviousToken?.Input == "get" || context.PreviousToken?.Input == "set")
                .RegisterPredicate("YieldEnd", context => context.IsEndOfInput || context.NextToken.HasLineBreakBefore ||
                    context.NextToken.Input == ";" || context.NextToken.Input == ")" || context.NextToken.Input == "]" ||
                    context.NextToken.Input == "}" || context.NextToken.Input == "," || context.NextToken.Input == ":")
                .RegisterPredicate("ClassModifierName", context => canContinueClassModifier(context.PreviousToken))
                .RegisterPredicate("ClassStaticBlock", context => context.PreviousToken?.Input == "static")
                .RegisterPredicate("ClassFieldEnd", context => CanEndClassField(context, canContinueClassModifier))
                .RegisterPredicate("ClassFieldExpressionEnd", context => context.NextToken.Input != "," && CanEndExpression(context, isArrowBlockEnd))
                .RegisterPredicate("ModuleFrom", context => context.NextToken.Input == "from")
                .RegisterPredicate("ModuleAs", context => context.NextToken.Input == "as")
                .RegisterPredicate("DefaultExportExpression", context => context.NextToken.Input != "function" && context.NextToken.Input != "class" && !isAsyncFunctionStart(context.NextToken))
                .RegisterPredicate("ModuleEnd", CanEnd)
                .RegisterPredicate("ExportNamedEnd", context => context.NextToken.Input != "from" && CanEnd(context))
                .RegisterPredicate("ExpressionStatementStart", context =>
                    context.NextToken.Input != "{" && context.NextToken.Input != "function" && context.NextToken.Input != "class" && !isAsyncFunctionStart(context.NextToken))
                .RegisterPredicate("ExpressionEnd", context => CanEndExpression(context, isArrowBlockEnd))
                .RegisterPredicate("BindingEnd", context => CanEnd(context) &&
                    context.NextToken.Input != "=" && context.NextToken.Input != ",")
                .RegisterPredicate("RestrictedEnd", CanEnd)
                .RegisterPredicate("StatementEnd", CanEnd)
                .RegisterPredicate("DoWhileEnd", context => context.NextToken.Input != ";");

        private static bool IsIdentifier(Janglim.FrontEnd.TokenData token)
            => token.Kind.Caption == "IdentifierName" || token.Input == "async" || token.Input == "await" || token.Input == "of";

        private static bool CanEnd(PredicateContext context)
            => context.NextToken.Input != ";" &&
               (context.IsEndOfInput || context.NextToken.Input == "}" || context.NextToken.HasLineBreakBefore);

        private static bool CanEndClassField(PredicateContext context, Func<Janglim.FrontEnd.TokenData, bool> canContinueClassModifier)
        {
            if (!CanEnd(context) || context.NextToken.Input == "=" || context.NextToken.Input == "(") return false;
            // A newline does not split a contextual modifier from its following name:
            // get\nvalue() {} is an accessor, and static\nvalue is a static field.
            if (canContinueClassModifier(context.PreviousToken))
            {
                string caption = context.NextToken.Kind.Caption;
                string next = context.NextToken.Input;
                if (caption == "IdentifierName" || caption == "PrivateIdentifier" ||
                    caption == "StringLiteral" || caption == "NumericLiteral" || next == "[" ||
                    next.Length != 0 && (char.IsLetter(next[0]) || next[0] == '$' || next[0] == '_')) return false;
            }
            return true;
        }

        private static bool CanEndExpression(PredicateContext context, Func<Janglim.FrontEnd.TokenData, bool> isArrowBlockEnd)
        {
            if (context.IsEndOfInput || context.NextToken.Input == "}") return true;
            if (!context.NextToken.HasLineBreakBefore || context.NextToken.Input == ";") return false;
            string next = context.NextToken.Input;
            // A completed arrow block is an AssignmentExpression, with no call/member
            // suffix. A comma can still continue an enclosing Expression sequence.
            if (isArrowBlockEnd(context.PreviousToken) || context.PreviousToken?.Input == "yield") return next != ",";
            bool template = context.NextToken.Kind.Caption == "NoSubstitutionTemplate" || context.NextToken.Kind.Caption == "TemplateHead";
            // Postfix updates cannot acquire a call, member, or tagged-template suffix.
            // The same next token continues a normal left-hand-side expression instead.
            if (next == "(" || next == "[" || template)
                return context.PreviousToken?.Input == "++" || context.PreviousToken?.Input == "--";
            return !Continuations.Contains(next);
        }
    }
}
