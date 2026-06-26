using System.Linq;
using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// MeaningUnit must survive the global flattening pass (the normalization that moved out of
/// AddItem/SetItem into Grammar.Optimization). Ex8_10 attaches "Add"/"Mul" to its recursive
/// productions; after the grammar is built and flattened, those productions must still carry
/// them — otherwise semantic actions (e.g. AJ's) would silently lose their tags.
/// </summary>
[Trait("Category", "GrammarNormalization")]
public class MeaningUnitPreservationTests
{
    [Theory]
    [InlineData("E", "Add")]   // E -> E + T  (Add)
    [InlineData("T", "Mul")]   // T -> T * F  (Mul)
    public void Flatten_keeps_the_meaning_unit_on_the_production(string nonTerminal, string meaningUnit)
    {
        var g = new Ex8_10Grammar();   // building it runs Optimization() -> FlattenAll

        var nt = g.NonTerminalMultiples.First(n => n.Name == nonTerminal);
        var carriedMeaningUnits = Enumerable.Range(0, nt.Count)
                                            .Select(i => nt.ElementAt(i))
                                            .Where(c => c.MeaningUnit != null)
                                            .Select(c => c.MeaningUnit.Name)
                                            .ToList();

        Assert.Contains(meaningUnit, carriedMeaningUnits);
    }
}
