using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Minimal, GRAMMAR-AGNOSTIC backtracking error handler — the generic core of the LGLR
/// mechanism, lifted out of AJ's <c>AJGrammarErrorHandler.TryRecoveryUsingConflictStateStack</c>.
///
/// Important finding: in this engine <c>UseBackTrackingOnConflict()</c> only sets a flag; the
/// actual backtracking is performed by whatever <see cref="IErrorHandlable"/> is attached via
/// <c>AddErrorHandler</c>. With NO handler, a conflict's losing branch is never retried.
///
/// On a parse failure this pops the conflict-state stack (<see cref="ParsingResult.BackTracking"/>)
/// and re-runs the parser with the alternative action; it returns null when no conflict point
/// remains, which the engine turns into a genuine syntax error.
/// </summary>
public class ConflictBacktrackingErrorHandler : IErrorHandlable
{
    public ErrorHandlingResult Call(DataForRecovery dataForRecovery)
    {
        if (!dataForRecovery.UseBackTracking) return null;

        var conflictInfo = dataForRecovery.ParsingResult.BackTracking();
        if (conflictInfo == null) return null;

        var parser = dataForRecovery.Parser as LRParser;
        parser.RecoveryWithSpecifiedAction(dataForRecovery, conflictInfo);

        return dataForRecovery.ToErrorHandlingResult(conflictInfo.AmbiguousBlockIndex);
    }
}
