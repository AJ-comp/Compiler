using System;

namespace Janglim.FrontEnd.Parsers.LR
{
    /// <summary>
    /// Resource ceilings for a single prediction. These bounds measure work and storage,
    /// rather than a fixed lookahead k; exceeding one produces a diagnostic, never a guessed action.
    /// </summary>
    internal sealed class PredictionLimits
    {
        public int MaximumTransitions { get; }
        public int MaximumConfigurations { get; }
        public int MaximumStateDepth { get; }

        public PredictionLimits(
            int maximumTransitions = 1000000,
            int maximumConfigurations = 4096,
            int maximumStateDepth = 16384)
        {
            if (maximumTransitions < 1) throw new ArgumentOutOfRangeException(nameof(maximumTransitions));
            if (maximumConfigurations < 1) throw new ArgumentOutOfRangeException(nameof(maximumConfigurations));
            if (maximumStateDepth < 1) throw new ArgumentOutOfRangeException(nameof(maximumStateDepth));
            MaximumTransitions = maximumTransitions;
            MaximumConfigurations = maximumConfigurations;
            MaximumStateDepth = maximumStateDepth;
        }
    }
}
