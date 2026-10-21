using System;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>A prediction error keeps its position in the supplied input sequence for the parser's diagnostic.</summary>
    internal abstract class PredictionException : InvalidOperationException
    {
        public int State { get; }
        public int TokenIndex { get; }
        // Token ordinal and UTF-16 source offset differ under contextual tokenization.
        internal int? SourceOffset { get; set; }
        public override string Message => base.Message + (SourceOffset.HasValue ? " Source offset " + SourceOffset.Value + "." : string.Empty);

        protected PredictionException(string message, int state, int tokenIndex)
            : base(message + " (state " + state + ", token index " + tokenIndex + ").")
        {
            State = state;
            TokenIndex = tokenIndex;
        }
    }

    internal sealed class PredictionFailedException : PredictionException
    {
        public PredictionFailedException(int state, int tokenIndex)
            : base("No candidate can continue through the predicate conflict", state, tokenIndex) { }
    }

    internal sealed class AmbiguousPredictionException : PredictionException
    {
        public AmbiguousPredictionException(int state, int tokenIndex)
            : base("More than one initial action accepts the same complete input", state, tokenIndex) { }
    }

    internal sealed class PredictionLimitException : PredictionException
    {
        public PredictionLimitException(string reason, int state, int tokenIndex)
            : base("Predicate conflict prediction exceeded its " + reason, state, tokenIndex) { }
    }

    internal sealed class PredictionCycleException : PredictionException
    {
        public PredictionCycleException(int state, int tokenIndex)
            : base("Predicate conflict prediction found a non-consuming reduction cycle", state, tokenIndex) { }
    }

    internal sealed class InvalidPredictionTableException : PredictionException
    {
        public InvalidPredictionTableException(string reason, int state, int tokenIndex)
            : base("Invalid prediction table: " + reason, state, tokenIndex) { }
    }
}
