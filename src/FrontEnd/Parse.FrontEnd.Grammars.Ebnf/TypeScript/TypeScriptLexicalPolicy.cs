using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    // A type argument's '>' must be split, while the following runtime '>>' must
    // retain longest-match lexing. The immutable delimiter chain belongs to the
    // previously consumed token, so lookahead and prediction cannot mutate it.
    internal sealed class TypeScriptLexicalPolicy
    {
        private enum Domain { Runtime, Type, NonNull, Modifier }
        private readonly ConditionalWeakTable<object, Text> inputs = new ConditionalWeakTable<object, Text>();
        private readonly ConditionalWeakTable<TokenData, State> states = new ConditionalWeakTable<TokenData, State>();
        private readonly Dictionary<LexicalGoal, Domain> domains = new Dictionary<LexicalGoal, Domain>();
        private readonly IReadOnlyList<LexicalGoal> runtime;
        private readonly LexicalGoal[] types;
        private readonly LexicalGoal[] nonNull;
        private readonly LexicalGoal[] modifiers;
        private readonly LexicalGoal[] staticModifiers;
        private readonly LexicalGoal[] heritage;
        private readonly Func<LexicalContext, LexicalGoal> select;
        private readonly Terminal runtimeIdentifier;
        private readonly Terminal identifier;
        private readonly Terminal open;
        private readonly Terminal close;
        private readonly Terminal postfix;
        private readonly Terminal modifier;
        private readonly Terminal heritageOpen;
        private readonly TypeScriptInputKind kind;

        internal TypeScriptLexicalPolicy(Ebnf.EbnfGrammar grammar, TypeScriptInputKind kind,
            IReadOnlyList<LexicalGoal> goals, Func<LexicalContext, LexicalGoal> select,
            HashSet<LexicalGoal> transparent)
        {
            this.kind = kind;
            runtime = goals;
            this.select = select;
            runtimeIdentifier = grammar.TerminalSet.Single(t => t.Caption == "IdentifierName");
            identifier = grammar.TerminalSet.Single(t => t.Caption == "TSTypeIdentifierName");
            open = grammar.TerminalSet.Single(t => t.Caption == "TSTypeOpen");
            close = grammar.TerminalSet.Single(t => t.Caption == "TSTypeClose");
            postfix = grammar.TerminalSet.SingleOrDefault(t => t.Caption == "TSNonNullToken");
            modifier = grammar.TerminalSet.SingleOrDefault(t => t.Caption == "TSClassModifierToken");
            heritageOpen = grammar.TerminalSet.SingleOrDefault(t => t.Caption == "TSClassHeritageOpen");
            types = goals.Select(goal => new LexicalGoal(goal.Terminals.Where(t =>
                t.Caption != "IdentifierName" && t.Value != "<" && t.Value != "<<" && t.Value != "<<=" && t.Value != ">" &&
                t.Value != ">>" && t.Value != ">>>" && t.Value != ">=" &&
                t.Value != ">>=" && t.Value != ">>>=")
                .Append(identifier).Append(open).Append(close))).ToArray();
            heritage = heritageOpen == null ? Array.Empty<LexicalGoal>() : types.Select(goal =>
                new LexicalGoal(goal.Terminals.Where(t => !ReferenceEquals(t, open)).Append(heritageOpen))).ToArray();
            nonNull = postfix == null ? Array.Empty<LexicalGoal>() : goals.Select(goal =>
                new LexicalGoal(goal.Terminals.Where(t => t.Value != "!").Append(postfix))).ToArray();
            modifiers = modifier == null ? Array.Empty<LexicalGoal>() : goals.Select(goal =>
                new LexicalGoal(goal.Terminals.Where(t => t.Caption != "IdentifierName" &&
                    !TypeScriptClasses.IsModifierSpelling(t.Value)).Append(modifier))).ToArray();
            staticModifiers = modifiers.Select(goal => new LexicalGoal(goal.Terminals)).ToArray();
            RegisterGoals(goals, Domain.Runtime, transparent);
            RegisterGoals(types, Domain.Type, transparent);
            RegisterGoals(heritage, Domain.Type, transparent);
            RegisterGoals(nonNull, Domain.NonNull, transparent);
            RegisterGoals(modifiers, Domain.Modifier, transparent);
            // static remains a runtime modifier: it establishes the JavaScript
            // class element's staticness even when followed by TypeScript modifiers.
            RegisterGoals(staticModifiers, Domain.Runtime, transparent);
            Policy = new LexicalPolicy(goals.Concat(types).Concat(heritage).Concat(nonNull).Concat(modifiers).Concat(staticModifiers), Resolve);
        }

        internal LexicalPolicy Policy { get; }
        internal void Register(object identity, string input) => inputs.GetValue(identity, _ => new Text(input));

        internal void Record(TokenCursor start, LexicalGoal goal, TokenRead read)
        {
            if (read.Token.Kind is EndMarker) return;
            var previous = Previous(start.PreviousToken);
            var domain = domains[goal];
            string input = inputs.GetValue(start.SourceIdentity, _ => new Text(string.Empty)).Value;
            bool declaration = StartsDeclaration(read.Token, domain, input);
            states.GetValue(read.Token, _ => previous.Advance(read.Token, domain, declaration));
        }

        private static bool StartsDeclaration(TokenData token, Domain domain, string input)
        {
            if (domain != Domain.Runtime) return false;
            string keyword = token.Input;
            if (keyword != "interface" && keyword != "namespace" && keyword != "module" && keyword != "enum") return false;
            var next = Next(input, token.EndIndex + 1);
            if (keyword != "enum" && next.LineBreak) return false;
            if (keyword == "module" && (next.First == '\'' || next.First == '"')) return true;
            // A keyword spelling used as a parameter, property or expression
            // has punctuation here, rather than the name of a declaration.
            return next.First != '\0' && TypeScriptLookahead.IsNameStart(next.First);
        }

        private void RegisterGoals(IEnumerable<LexicalGoal> goals, Domain domain, HashSet<LexicalGoal> transparent)
        {
            foreach (var goal in goals)
            {
                domains.Add(goal, domain);
                if (domain != Domain.Runtime) transparent.Add(goal);
            }
        }

        private LexicalGoal Resolve(LexicalContext context)
        {
            var selected = select(context);
            int index = 0;
            while (!ReferenceEquals(runtime[index], selected)) index++;
            var state = Previous(context.PreviousToken);
            string input = inputs.GetValue(context.Cursor.SourceIdentity, _ => new Text(string.Empty)).Value;
            var next = Next(input, context.SourceOffset);
            bool expectsOpen = Expects(context, open);
            // A row can also allow optional type parameters after a runtime
            // function name. That alone does not turn its following '(' or '='
            // into type syntax. Dedicated angle tokens are selected only at the
            // actual angle boundary below.
            bool expectsType = Expects(context, identifier);

            // New assertion continuations broaden the LR row's FOLLOW tokens.
            // A completed declaration nevertheless starts a fresh statement.
            if (state.AfterDeclaration && next.First == '/') return runtime[index == 3 || index == 2 ? 2 : 0];
            if (TypeScriptClasses.IsAbstractClassDeclarationPrefix(input, context.SourceOffset)) return selected;

            // The dedicated terminal is expected only on a TypeScript modifier
            // path. Identical words used as member names keep the ordinary goal.
            if (modifier != null && Expects(context, modifier) && TypeScriptClasses.IsModifierPrefix(input, context.SourceOffset) &&
                (next.Word != "static" || state.InModifiers || TypeScriptClasses.HasTypeScriptModifierPrefix(input, context.SourceOffset)))
                return next.Word == "static" ? staticModifiers[index] : modifiers[index];
            if (next.First == '[' && TypeScriptLookahead.IndexSignature(TypeScriptLookahead.Scan(input, context.SourceOffset, 4)))
                return types[index];

            // Only a same-line postfix token is transparent. Prefix ! and !=/!==
            // retain the ordinary JavaScript rules and their slash context.
            if (postfix != null && Expects(context, postfix) && !next.LineBreak &&
                next.First == '!' && next.Second != '=') return nonNull[index];

            // The class-only opener has its own terminal. Offering that suffix
            // after an LHS therefore leaves existing generic headers unchanged.
            if (next.First == '<' && next.Second != '=' && !next.LineBreak &&
                heritageOpen != null && Expects(context, heritageOpen) && IsClassHeritage(context.ConsumedTokens))
                return heritage[index];
            if (next.First == '<' && next.Second != '=' && expectsOpen && !next.LineBreak &&
                (!state.Type || state.CanTypeArguments) &&
                (state.Type || !IsPropertyAccess(context.ConsumedTokens))) return types[index];
            if (next.First == '>' && state.Frame?.Close == '>') return types[index];
            if (state.Frame != null && next.First == state.Frame.Close)
                return state.Frame.Type ? types[index] : selected;
            // An optional generic header does not turn a runtime namespace body
            // into a type object. Computed type names likewise contain ordinary
            // JavaScript expressions, whose identifiers keep their runtime kind.
            if (Expects(context, runtimeIdentifier) && (!state.Type ||
                !expectsType && state.Frame?.Close == ']' && state.Last == "[")) return selected;
            if (state.TypeEnd && next.First == '[' && !next.LineBreak) return types[index];
            if (state.TypeEnd && IsRuntimeContinuation(next, state, expectsOpen)) return selected;
            if (expectsType || kind == TypeScriptInputKind.Type && context.TokenIndex == 0 ||
                state.Type && state.Frame?.Type == true) return types[index];
            return selected;
        }

        private static bool Expects(LexicalContext context, Terminal terminal)
            => context.ExpectedTerminals.Any(item => ReferenceEquals(item, terminal));

        private static bool IsPropertyAccess(IReadOnlyList<TokenData> tokens)
            // PropertyIdentifierName is shared with generic method names. After
            // a runtime dot, its '<' continues an expression, not a method header.
            => tokens.Count >= 2 && (tokens[tokens.Count - 2].Input == "." ||
                tokens[tokens.Count - 2].Input == "?.");

        private static bool IsClassHeritage(IReadOnlyList<TokenData> tokens)
        {
            bool extends = false;
            for (int index = tokens.Count - 1; index >= 0; index--)
            {
                string value = tokens[index].Input;
                if (IsClosingGroup(tokens[index]))
                {
                    int open = FindGroupStart(tokens, index);
                    if (open < 0) return false;
                    // A completed class/function body also closes its header.
                    // Skip both, so a previous declaration's extends clause cannot
                    // be mistaken for the current expression's lexical context.
                    int header = value == "}" ? FindBodyHeaderStart(tokens, open) : -1;
                    index = (header >= 0 ? header : open);
                    continue;
                }
                // An unfinished group places the cursor inside an argument,
                // computed key or class body, rather than in the heritage suffix.
                if (value == "(" || value == "[" || value == "{" || value == ";" ||
                    IsTypeOpen(tokens[index])) return false;
                if (IsPropertyKeyword(tokens, index)) continue;
                if (value == "extends") extends = true;
                else if (value == "implements") return false;
                else if (value == "class") return extends;
            }
            return false;
        }

        private static bool IsClosingGroup(TokenData token)
            => token.Input == ")" || token.Input == "]" || token.Input == "}" ||
                token.Kind.Caption == "TSTypeClose";

        private static bool IsTypeOpen(TokenData token)
            => token.Kind.Caption == "TSTypeOpen" || token.Kind.Caption == "TSClassHeritageOpen";

        private static int FindGroupStart(IReadOnlyList<TokenData> tokens, int end)
        {
            string close = tokens[end].Input;
            string open = close == ")" ? "(" : close == "]" ? "[" : close == "}" ? "{" : "<";
            bool type = tokens[end].Kind.Caption == "TSTypeClose";
            int depth = 0;
            for (int index = end; index >= 0; index--)
            {
                var token = tokens[index];
                if (type)
                {
                    if (token.Kind.Caption == "TSTypeClose") depth++;
                    else if (IsTypeOpen(token) && --depth == 0) return index;
                }
                else if (token.Input == close) depth++;
                else if (token.Input == open && --depth == 0) return index;
            }
            return -1;
        }

        private static int FindBodyHeaderStart(IReadOnlyList<TokenData> tokens, int body)
        {
            // `extends { ... }` is an object expression used as the base. Its
            // brace belongs to that expression, rather than to a declaration.
            if (body > 0 && tokens[body - 1].Input == "extends" && !IsPropertyKeyword(tokens, body - 1)) return -1;
            for (int index = body - 1; index >= 0; index--)
            {
                string value = tokens[index].Input;
                if (IsClosingGroup(tokens[index]))
                {
                    int open = FindGroupStart(tokens, index);
                    if (open < 0) return -1;
                    int nested = value == "}" ? FindBodyHeaderStart(tokens, open) : -1;
                    index = nested >= 0 ? nested : open;
                    continue;
                }
                if (!IsPropertyKeyword(tokens, index) && (value == "class" || value == "function")) return index;
                if (value == "{" || value == "(" || value == "[" || value == ";" || value == "=" ||
                    value == ":" || value == "," || value == "=>" || value == "return" ||
                    value == "throw" || value == "const" || value == "let" || value == "var" ||
                    IsTypeOpen(tokens[index])) return -1;
            }
            return -1;
        }

        private static bool IsPropertyKeyword(IReadOnlyList<TokenData> tokens, int index)
            => index > 0 && (tokens[index - 1].Input == "." || tokens[index - 1].Input == "?.");

        private State Previous(TokenData token)
            => token != null && states.TryGetValue(token, out var state) ? state : State.Empty;

        private static bool IsRuntimeContinuation(Look next, State state, bool expectsOpen)
        {
            char first = next.First, second = next.Second;
            if (first == '|' || first == '&') return second == first;
            if (first == '[') return next.LineBreak;
            if (first == '<') return !expectsOpen || second == '<' || second == '=';
            if (first == '?' || first == ':') return state.Conditionals == 0 && state.Frame?.Type != true;
            if (first == '=' && second == '>' && state.Last == ")" && state.Type) return false;
            if (first == '=' && state.Frame?.Close == '>') return false;
            if ((first == '+' || first == '-') && (second == '?' || next.Word == "readonly")) return false;
            if (first == ',' || first == ';' || first == '(') return state.Frame?.Type != true;
            if (first == '{' || first == '+' || first == '-' || first == '*' || first == '/' ||
                first == '%' || first == '>' || first == '=' || first == '!') return true;
            return next.Word == "as" || next.Word == "satisfies" || next.Word == "in" || next.Word == "instanceof";
        }

        private static Look Next(string input, int start)
        {
            int index = start;
            bool lineBreak = false;
            while (index < input.Length)
            {
                char value = input[index];
                if (char.IsWhiteSpace(value) || value == '\ufeff')
                { lineBreak |= IsLineBreak(value); index++; continue; }
                if (value != '/' || index + 1 >= input.Length) break;
                if (input[index + 1] == '/')
                { index += 2; while (index < input.Length && !IsLineBreak(input[index])) index++; continue; }
                if (input[index + 1] != '*') break;
                int end = input.IndexOf("*/", index + 2, StringComparison.Ordinal);
                if (end < 0) break;
                for (; index < end + 2; index++) lineBreak |= IsLineBreak(input[index]);
            }
            char first = index < input.Length ? input[index] : '\0';
            char second = index + 1 < input.Length ? input[index + 1] : '\0';
            int begin = index;
            while (index < input.Length && (char.IsLetter(input[index]) || input[index] == '_')) index++;
            return new Look(first, second, lineBreak, input.Substring(begin, index - begin));
        }

        private static bool IsLineBreak(char value)
            => value == '\r' || value == '\n' || value == '\u2028' || value == '\u2029';

        private sealed class Text
        { internal Text(string value) { Value = value; } internal string Value { get; } }

        private readonly struct Look
        {
            internal Look(char first, char second, bool lineBreak, string word)
            { First = first; Second = second; LineBreak = lineBreak; Word = word; }
            internal char First { get; }
            internal char Second { get; }
            internal bool LineBreak { get; }
            internal string Word { get; }
        }

        private sealed class Frame
        {
            internal Frame(char close, bool type, Frame parent, int conditionals, bool declaration = false)
            {
                Close = close; Type = type; Parent = parent; Conditionals = conditionals; Declaration = declaration;
                Depth = (parent?.Depth ?? 0) + 1;
            }
            internal char Close { get; }
            internal bool Type { get; }
            internal Frame Parent { get; }
            internal int Conditionals { get; }
            internal bool Declaration { get; }
            internal int Depth { get; }
        }

        private sealed class Header
        {
            internal Header(int depth) { Depth = depth; }
            internal int Depth { get; }
        }

        private sealed class State
        {
            internal static readonly State Empty = new State(null, false, false, string.Empty, 0, null, false, false);
            private State(Frame frame, bool type, bool typeEnd, string last, int conditionals,
                Header header, bool afterDeclaration, bool canTypeArguments, bool inModifiers = false)
            {
                Frame = frame; Type = type; TypeEnd = typeEnd; Last = last; Conditionals = conditionals;
                Header = header; AfterDeclaration = afterDeclaration; CanTypeArguments = canTypeArguments;
                InModifiers = inModifiers;
            }
            internal Frame Frame { get; }
            internal bool Type { get; }
            internal bool TypeEnd { get; }
            internal string Last { get; }
            internal int Conditionals { get; }
            internal Header Header { get; }
            internal bool AfterDeclaration { get; }
            internal bool CanTypeArguments { get; }
            internal bool InModifiers { get; }

            internal State Advance(TokenData token, Domain domain, bool declarationStart)
            {
                bool type = domain == Domain.Type;
                var frame = Frame;
                string value = token.Input;
                int conditionals = Conditionals;
                var header = NextHeader(frame, value, declarationStart);
                bool afterDeclaration = false;
                // A computed name is a runtime expression, unlike an index
                // signature or indexed-access type that starts with a type token.
                if (!type && frame?.Type == true && frame.Close == ']' && Last == "[")
                    frame = new Frame(frame.Close, false, frame.Parent, frame.Conditionals, frame.Declaration);
                if (IsTypeOpen(token)) frame = new Frame('>', true, frame, conditionals);
                else if (value == "(" || value == "[" || value == "{")
                {
                    bool declaration = value == "{" && header != null && (frame?.Depth ?? 0) == header.Depth;
                    if (declaration) header = null;
                    frame = new Frame(value == "(" ? ')' : value == "[" ? ']' : '}', type, frame, conditionals, declaration);
                }
                else if (value.Length == 1 && frame != null && value[0] == frame.Close)
                {
                    conditionals = frame.Conditionals;
                    afterDeclaration = frame.Declaration;
                    frame = frame.Parent;
                }
                if (type && value == "extends" && Last != ".") conditionals++;
                if (type && value == ":" && conditionals != 0) conditionals--;
                bool end = type && (value == ")" || value == "]" || value == "}" ||
                    token.Kind.Caption == "TSTypeClose" || IsTypeValue(value));
                bool canArguments = type && end && IsTypeName(value, token.Kind.Caption, Last);
                return new State(frame, type, end, value, conditionals, header, afterDeclaration, canArguments,
                    token.Kind.Caption == "TSClassModifierToken");
            }

            private Header NextHeader(Frame frame, string value, bool declarationStart)
            {
                int depth = frame?.Depth ?? 0;
                if (declarationStart && Last != "." && (value == "enum" ||
                    Last != "const" && Last != "let" && Last != "var")) return new Header(depth);
                if (Header == null || depth != Header.Depth) return Header;
                return StopsHeader(value) ? null : Header;
            }

            private static bool StopsHeader(string value)
            {
                if (value.Length == 0) return false;
                switch (value[0])
                {
                    case ';': case '=': case ':': case '+': case '-': case '*': case '/': case '%':
                    case '!': case '?': case '|': case '&': case '^': case '(': case ')': case ']': case '}':
                        return true;
                    default: return false;
                }
            }

            private static bool IsTypeName(string value, string caption, string previous)
            {
                if (caption == "TSTypeIdentifierName") return true;
                if (value.Length == 0 || !char.IsLetter(value[0])) return false;
                if (previous == ".") return true;
                switch (value)
                {
                    case "any": case "unknown": case "string": case "number": case "bigint": case "symbol":
                    case "boolean": case "undefined": case "never": case "object": case "void": case "intrinsic":
                    case "this": case "null": case "true": case "false": return false;
                    default: return true;
                }
            }

            private static bool IsTypeValue(string value)
            {
                if (value.Length == 0 || value == "extends" || value == "keyof" || value == "readonly" ||
                    value == "unique" || value == "infer" || value == "is") return false;
                char first = value[0];
                return char.IsLetterOrDigit(first) || first == '_' || first == '$' || first == '\\' ||
                    first == '\'' || first == '"' || first == '`';
            }
        }
    }
}
