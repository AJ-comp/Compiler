using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.Properties;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// A grammar-agnostic default error handler, used by every LR parser when no custom handler was
    /// registered (see <see cref="LRParser"/>). On a parse failure it records ONE positioned error —
    /// the unexpected token, which carries its source offset — into the failing block, then reports
    /// "not recovered" so the parser stops exactly as before. Net effect: the failure now surfaces as a
    /// <see cref="ParsingErrorInfo"/> in <c>AllErrors</c> (with a position) instead of an empty result.
    ///
    /// This is the minimal, low-risk baseline. It can grow — panic-mode recovery, an "expected {…}"
    /// message, and multiple errors — without changing where it plugs in.
    /// </summary>
    public sealed class DefaultLRErrorHandler : IErrorHandlable
    {
        public ErrorHandlingResult Call(DataForRecovery dataForRecovery)
        {
            var block = dataForRecovery.CurBlock;   // the block the parser choked on
            var token = block.Token;                // the unexpected token — carries its StartIndex/EndIndex

            // "{0} is inappropriate token on this position." Guard the token: a null here would NRE and be
            // swallowed by the parser's outer catch, silently degrading back to the old empty-result — so
            // fall back to a token-less error that at least records the failure.
            var error = (token != null)
                ? ParsingErrorInfo.CreateParsingError(
                    token, nameof(AlarmCodes.CE0001), string.Format(AlarmCodes.CE0001, token.Input))
                : ParsingErrorInfo.CreateParsingError(
                    nameof(AlarmCodes.CE0001), string.Format(AlarmCodes.CE0001, string.Empty));
            block._errorInfos.Add(error);

            // record only; do NOT recover -> the parser stops at the first error, exactly as before.
            return dataForRecovery.ToErrorHandlingResult(false);
        }
    }
}
