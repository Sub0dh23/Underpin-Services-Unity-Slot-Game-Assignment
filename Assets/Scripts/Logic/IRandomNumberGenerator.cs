using System.Collections.Generic;
using Underpin.SlotGame.Data;

namespace Underpin.SlotGame.Logic
{
    /// <summary>
    /// Interface for Random Number Generation in slot machine reels, enabling deterministic testing & mock simulation.
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Selects a random symbol from the provided list based on individual drop weights.
        /// </summary>
        SymbolData GetWeightedRandomSymbol(IReadOnlyList<SymbolData> availableSymbols);

        /// <summary>
        /// Generates an integer in the range [minInclusive, maxExclusive).
        /// </summary>
        int NextRange(int minInclusive, int maxExclusive);

        /// <summary>
        /// Generates a float in the range [0.0, 1.0).
        /// </summary>
        float NextFloat();

        /// <summary>
        /// Sets a specific seed for reproducible outcome sequences.
        /// </summary>
        void SetSeed(int seed);
    }
}
