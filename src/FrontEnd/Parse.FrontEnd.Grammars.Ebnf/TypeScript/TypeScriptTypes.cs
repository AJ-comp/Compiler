using System;
using System.IO;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Supplies TypeScript 7.0.2 type productions for composition with the existing
    /// JavaScript language grammar. Type syntax remains in TS-prefixed concrete-tree
    /// nodes; JavaScript expressions in computed names and defaults reuse its rules.
    /// The source grammar follows the official microsoft/typescript-go parser at
    /// commit 2bd066d87f5bafd315be9f40889d0a60b9e58e0b.
    /// </summary>
    internal static class TypeScriptTypes
    {
        internal const string ResourceName = "Janglim.TypeScript.Types.ebnf";

        // A continuation guard must identify its following token as well as its
        // line-break restriction. Otherwise a true NoLineBreak epsilon reduction
        // can hide the ordinary reduction that finishes the surrounding type.
        internal static void ConfigurePredicates(EbnfReaderOptions options)
        {
            void Pair(string name, Func<PredicateContext, bool> predicate)
            {
                options.RegisterPredicate("TS" + name, predicate);
                options.RegisterPredicate("TSNot" + name, context => !predicate(context));
            }
            Pair("ConditionalTypeStart", context => SameLine(context, "extends"));
            Pair("PostfixTypeStart", context => SameLine(context, "["));
            Pair("TypeArgumentsStart", context => SameLine(context, "<"));
            Pair("TypePredicateStart", context => SameLine(context, "is"));
            // The native parser allows a line break before this optional 'is'.
            // Complementary guards choose continuation before reducing the short form.
            Pair("AssertsTypePredicateIsStart", context => context.NextToken.Input == "is");
            options.RegisterPredicate("TSNoTypeReferenceSuffix", context =>
                !SameLine(context, "<") && !SameLine(context, "is"));
        }

        private static bool SameLine(PredicateContext context, string spelling)
            => context.NextToken.Input == spelling && !context.NextToken.HasLineBreakBefore;

        internal static string Load()
        {
            using (var stream = typeof(TypeScriptTypes).Assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("The embedded TypeScript type grammar is missing.");
                using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
            }
        }
    }
}
