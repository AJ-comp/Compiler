using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Adds TypeScript class syntax to the JavaScript productions. Runtime method,
    /// field, parameter and body roles stay intact, so their existing context and
    /// early-error checks can be reused by the TypeScript validation projection.
    /// </summary>
    internal static class TypeScriptClasses
    {
        internal static string Prepare(string text)
        {
            text = TypeScriptDialect.Edit(text, "ClassElement", body => body +
                " | TSClassModifiedElement | {TSClassIndexSignatureStart()}? TSClassIndexSignature");
            text = TypeScriptDialect.Edit(text, "ClassField", body =>
                body.Replace(" TSAnnotation?", " TSClassFieldMarker? TSAnnotation?"));

            foreach (string rule in new[] { "ClassMethod", "ClassAsyncMethod", "ClassGeneratorMethod" })
                text = TypeScriptDialect.Edit(text, rule, body =>
                {
                    body = body.Replace("TSOptionalTypeParameters", "TSClassMethodOptional TSOptionalTypeParameters");
                    return body + " | " + body.Replace("FunctionBody", "TSMethodOverload");
                });

            text = TypeScriptDialect.Edit(text, "FormalParameter", body => body +
                " | {TSParameterModifiersStart()}? TSParameterModifiers BindingIdentifier TSOptional? TSAnnotation? Initializer?");
            text = TypeScriptDialect.Edit(text, "ClassTail", _ =>
                "ClassBody | ClassHeritage ClassBody | TSImplements ClassBody | ClassHeritage TSImplements ClassBody");
            text = TypeScriptDialect.Edit(text, "ClassHeritage", _ =>
                "'extends' LeftHandSideExpression TSClassHeritageArguments?");
            // Anonymous class expressions can carry type parameters too. Named
            // declarations/expressions already receive them in AddRuntimeSyntax.
            text = TypeScriptDialect.Edit(text, "ClassExpression", body =>
                body.Replace("'class' ClassTail", "'class' TSTypeParameters? ClassTail"));
            text = TypeScriptDialect.Edit(text, "TSActualDeclaration", body => body + " | TSAbstractClassDeclaration");
            text = TypeScriptDialect.Edit(text, "DefaultExportClass", body => body +
                " | 'export' 'default' TSAbstractClassExpression");
            text = TypeScriptDialect.Edit(text, "ExportDefaultExpression", body =>
                body.Replace("{DefaultExportExpression()}?", "{TSDefaultExportExpression()}?"));

            return text + @"
# A modifier prefix has its own node. Its ordinary method/field child retains
# the original JavaScript role and, in particular, its Await/Yield context.
TSClassModifiedElement : {TSClassModifiersStart()}? TSModifiers TSClassModifiedMember ;
TSClassModifiedMember : TSClassMember ;
TSClassMember : ClassMethod | ClassField | ClassAsyncMethod | ClassGeneratorMethod
               | ClassStaticBlock | {TSClassIndexSignatureStart()}? TSClassIndexSignature ;
TSModifiers : TSModifierList {TSNoClassModifierStart()}? ;
TSModifierList : TSModifier | TSModifierList TSModifier ;
TSModifier : {TSClassModifierStart()}? TSClassModifierToken ;
TSClassFieldMarker : TSOptional | TSDefinite ;
# Keep this optional marker in one named rule instead of multiplying every
# method prefix and signature/implementation alternative during normalization.
TSClassMethodOptional : TSOptional? ;
TSDefinite : NoLineTerminator TSNonNullToken ;
TSMethodOverload : TSFunctionSignatureEnd ;
TSParameterModifiers : TSParameterModifierList {TSNoParameterModifierStart()}? ;
TSParameterModifierList : TSParameterModifier | TSParameterModifierList TSParameterModifier ;
TSParameterModifier : {TSParameterModifierStart()}? TSClassModifierToken ;
TSClassModifierToken := Identifier ""(?:public|private|protected|readonly|abstract|override|declare|accessor|static)"" ;
TSImplements : 'implements' TSClassImplementsList ;
# Keep the new LHS heritage suffix separate from existing generic headers.
# Its opener must not broaden TSTypeOpen's lookahead in runtime expressions.
TSClassHeritageArguments : TSClassHeritageOpen TSTypeArgumentList TSTypeClose
                         | TSClassHeritageOpen TSTypeArgumentList ',' TSTypeClose ;
TSClassHeritageOpen := Identifier ""(?:<)(?:)"" ;
TSClassImplementsList : TSClassImplementsType | TSClassImplementsList ',' TSClassImplementsType ;
TSClassImplementsType : TSQualifiedName TSTypeArguments? ;
TSAbstractClassDeclaration : {TSAbstractClassStart()}? 'abstract' NoLineTerminator ClassDeclaration ;
# The native parser accepts this anonymous form only after export default.
TSAbstractClassExpression : 'abstract' NoLineTerminator ClassExpression ;
# readonly is carried by TSModifiers here. Reusing the type-object production's
# readonly alternative would add a third action to the member-prefix decision.
TSClassIndexSignature : '[' TSClassIndexParameters ']' TSTypeAnnotation? TSClassIndexSignatureEnd ;
TSClassIndexParameters : TSClassIndexParameter | TSClassIndexParameters ',' TSClassIndexParameter ;
TSClassIndexParameter : TSClassIndexBinding TSTypeAnnotation? ;
# Runtime BindingIdentifier supplies TypeScript's contextual/primitive words and
# the existing Await-context expansion. The type goal supplies ordinary names
# as TSTypeIdentifierName, so both token kinds are accepted without changing it.
TSClassIndexBinding : BindingIdentifier | TSTypeIdentifierName ;
TSClassIndexSignatureEnd : ';' | {TSClassIndexSignatureEnd()}? ;
";
        }

        internal static void ConfigurePredicates(EbnfReaderOptions options,
            Func<PredicateContext, IReadOnlyList<string>> tokens,
            Func<PredicateContext, string> source)
        {
            var defaultExpression = options.SnapshotPredicates()["DefaultExportExpression"];
            options.RegisterPredicate("TSDefaultExportExpression", context =>
                !IsAbstractClassDeclarationPrefix(source(context), context.NextToken.StartIndex) && defaultExpression.Evaluate(context));
            options.RegisterPredicate("TSClassModifiersStart", context =>
                HasTypeScriptModifier(context, source(context)));
            options.RegisterPredicate("TSClassModifierStart", context =>
                IsModifierAt(context, source(context), false));
            options.RegisterPredicate("TSNoClassModifierStart", context =>
                !IsModifierAt(context, source(context), false));
            options.RegisterPredicate("TSParameterModifiersStart", context =>
                IsModifierAt(context, source(context), true));
            options.RegisterPredicate("TSParameterModifierStart", context =>
                IsModifierAt(context, source(context), true));
            options.RegisterPredicate("TSNoParameterModifierStart", context =>
                !IsModifierAt(context, source(context), true));
            options.RegisterPredicate("TSAbstractClassStart", context =>
                context.NextToken.Input == "abstract" &&
                Following(context, source(context)).Text == "class" &&
                !Following(context, source(context)).LineBreak);
            options.RegisterPredicate("TSClassIndexSignatureStart", context =>
                context.NextToken.Input == "[" && TypeScriptLookahead.IndexSignature(tokens(context)));
            options.RegisterPredicate("TSClassIndexSignatureEnd", context =>
                context.NextToken.Kind is EndMarker || context.NextToken.Input == "}" || context.NextToken.HasLineBreakBefore);
        }

        // static alone remains in the JavaScript grammar. We enter this prefix
        // only when a TypeScript modifier occurs before the member's actual name.
        private static bool HasTypeScriptModifier(PredicateContext context, string source)
            => HasTypeScriptModifier(new Lexeme(context.NextToken.Input, context.NextToken.EndIndex + 1, false), source);

        internal static bool HasTypeScriptModifierPrefix(string source, int offset)
            => HasTypeScriptModifier(Read(source, offset), source);

        private static bool HasTypeScriptModifier(Lexeme current, string source)
        {
            bool found = false;
            while (IsClassModifier(current.Text))
            {
                var next = Read(source, current.End);
                if (!CanModify(current.Text, next)) break;
                found |= current.Text != "static" || next.Text == "[" && source != null &&
                    TypeScriptLookahead.IndexSignature(TypeScriptLookahead.Scan(source, current.End, 4));
                current = next;
            }
            return found;
        }

        private static bool IsModifierAt(PredicateContext context, string source, bool parameter)
        {
            string spelling = context.NextToken.Input;
            if (!(parameter ? IsParameterModifier(spelling) : IsClassModifier(spelling))) return false;
            return CanModify(spelling, Following(context, source));
        }

        private static Lexeme Following(PredicateContext context, string source)
            => Read(source, context.NextToken.EndIndex + 1);

        private static bool CanModify(string spelling, Lexeme next)
        {
            if (next.Text.Length == 0 || spelling != "static" && next.LineBreak) return false;
            // static { begins the ordinary static-block production; static must
            // remain its keyword, rather than disappearing into TSModifiers.
            if (spelling == "static" && next.Text == "{") return false;
            char first = next.Text[0];
            return TypeScriptLookahead.IsNameStart(first) || char.IsDigit(first) ||
                first == '#' || first == '\'' || first == '"' || first == '[' ||
                first == '{' || first == '*' || next.Text == "...";
        }

        private static bool IsClassModifier(string spelling)
        {
            switch (spelling)
            {
                case "public": case "private": case "protected": case "readonly":
                case "abstract": case "override": case "declare": case "accessor": case "static":
                    return true;
                default: return false;
            }
        }

        private static bool IsParameterModifier(string spelling)
            => IsClassModifier(spelling) && spelling != "accessor";

        internal static bool IsModifierPrefix(string source, int offset)
        {
            var current = Read(source, offset);
            return IsClassModifier(current.Text) && CanModify(current.Text, Read(source, current.End));
        }

        internal static bool IsModifierSpelling(string value) => IsClassModifier(value);

        internal static bool IsAbstractClassDeclarationPrefix(string source, int offset)
        {
            var first = Read(source, offset);
            if (first.Text != "abstract") return false;
            var keyword = Read(source, first.End);
            if (keyword.Text != "class" || keyword.LineBreak) return false;
            var name = Read(source, keyword.End);
            return name.Text == "{" || name.Text == "<" ||
                name.Text.Length != 0 && TypeScriptLookahead.IsNameStart(name.Text[0]);
        }

        // This lookahead reads only the next prefix token and its preceding
        // trivia. The actual token source still lexes every original character.
        private static Lexeme Read(string source, int offset)
        {
            if (source == null) return new Lexeme(string.Empty, offset, false);
            int index = Math.Max(0, offset);
            bool lineBreak = false;
            while (index < source.Length)
            {
                char value = source[index];
                if (char.IsWhiteSpace(value) || value == '\ufeff')
                { lineBreak |= IsLineBreak(value); index++; continue; }
                if (value != '/' || index + 1 >= source.Length) break;
                if (source[index + 1] == '/')
                { index += 2; while (index < source.Length && !IsLineBreak(source[index])) index++; continue; }
                if (source[index + 1] != '*') break;
                int end = source.IndexOf("*/", index + 2, StringComparison.Ordinal);
                if (end < 0) break;
                for (; index < end + 2; index++) lineBreak |= IsLineBreak(source[index]);
            }
            if (index >= source.Length) return new Lexeme(string.Empty, index, lineBreak);
            int start = index++;
            char first = source[start];
            if (TypeScriptLookahead.IsNameStart(first) || char.IsDigit(first))
            {
                while (index < source.Length && (TypeScriptLookahead.IsNameStart(source[index]) || char.IsDigit(source[index]))) index++;
            }
            else if (first == '\'' || first == '"')
            {
                while (index < source.Length)
                { char value = source[index++]; if (value == '\\' && index < source.Length) index++; else if (value == first) break; }
            }
            else if (first == '.' && index + 1 < source.Length && source[index] == '.' && source[index + 1] == '.') index += 2;
            return new Lexeme(source.Substring(start, index - start), index, lineBreak);
        }

        private static bool IsLineBreak(char value)
            => value == '\r' || value == '\n' || value == '\u2028' || value == '\u2029';

        private readonly struct Lexeme
        {
            internal Lexeme(string text, int end, bool lineBreak) { Text = text; End = end; LineBreak = lineBreak; }
            internal string Text { get; }
            internal int End { get; }
            internal bool LineBreak { get; }
        }
    }
}
