using System.Linq;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Parsers.Tests.Infra;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests
{
    /// <summary>
    /// Characterization ("golden master") tests that pin the CURRENT behavior of the LR
    /// parser engine on the <c>Ex1</c> example grammar. The point is not to assert what the
    /// tables "should" be — it is to lock whatever they are today so the upcoming Phase-1
    /// refactor (carving the pure parsing core out of Parse.FrontEnd) can be proven to
    /// change nothing observable.
    ///
    /// Ex1 is used because it is a small, language-agnostic grammar with quantifiers
    /// (ZeroOrMore) and an alternation, so it exercises auto-generated nonterminals,
    /// FIRST/FOLLOW, the canonical collection and the ACTION/GOTO table.
    /// </summary>
    public class GrammarEngineCharacterizationTests
    {
        private static Grammar NewEx1Grammar() => new Ex1Grammar();

        [Fact]
        public void Ex1_grammar_builds_and_exposes_root()
        {
            var grammar = NewEx1Grammar();
            Assert.NotNull(grammar.EbnfRoot);
        }

        [Fact]
        public void Ex1_grammar_ebnf_matches_snapshot()
        {
            var grammar = NewEx1Grammar();
            Snapshot.Match("Ex1.ebnf", string.Join("\n", grammar.ToEbnfString()));
        }

        [Fact]
        public void Ex1_slr_builds_nonempty_tables()
        {
            var parser = new SLRParser(NewEx1Grammar(), false);

            Assert.True(parser.ParsingTable.AllSymbols.Any(), "parsing table has no symbols");
            Assert.True(parser.Canonical.ToCanonicalLineList().Any(), "canonical collection is empty");
        }

        [Fact]
        public void Ex1_slr_first_follow_matches_snapshot()
        {
            var parser = new SLRParser(NewEx1Grammar(), false);
            Snapshot.Match("Ex1.slr.first-follow", DataTableText.ToText(parser.GetFirstAndFollow().ToTableFormat));
        }

        [Fact]
        public void Ex1_slr_parsing_table_matches_snapshot()
        {
            var parser = new SLRParser(NewEx1Grammar(), false);
            Snapshot.Match("Ex1.slr.parsing-table", DataTableText.ToText(parser.ParsingTable.ToTableFormat));
        }

        [Fact]
        public void Ex1_slr_canonical_matches_snapshot()
        {
            var parser = new SLRParser(NewEx1Grammar(), false);
            Snapshot.Match("Ex1.slr.canonical", DataTableText.ToText(parser.Canonical.ToDataTable()));
        }

        [Fact]
        public void Ex1_slr_ambiguity_matches_snapshot()
        {
            var parser = new SLRParser(NewEx1Grammar(), false);
            Snapshot.Match("Ex1.slr.ambiguity", DataTableText.ToText(parser.CheckAmbiguity().ToTableFormat));
        }

        [Fact]
        public void Ex1_lalr_builds_nonempty_tables()
        {
            var parser = new LALRParser(NewEx1Grammar(), false);
            Assert.True(parser.Canonical.ToCanonicalLineList().Any(), "canonical collection is empty");
        }
    }
}
