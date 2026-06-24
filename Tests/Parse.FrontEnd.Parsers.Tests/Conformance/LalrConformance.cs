using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.LR;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Runs the whole <see cref="LRParserConformance"/> suite against LALR — the engine's
/// working LR path.
/// </summary>
[Trait("Category", "Conformance")]
public class LalrConformance : LRParserConformance
{
    protected override LRParser CreateParser(Grammar grammar, bool logging)
        => new LALRParser(grammar, logging);
}

// When SLR (C0) is fixed (see Variants/SlrKnownIssueTests), uncomment this: the entire
// conformance suite then runs against SLR too, proving SLR and LALR behave identically —
// with zero duplicated test code.
//
// [Trait("Category", "Conformance")]
// public class SlrConformance : LRParserConformance
// {
//     protected override LRParser CreateParser(Grammar grammar, bool logging)
//         => new SLRParser(grammar, logging);
// }
