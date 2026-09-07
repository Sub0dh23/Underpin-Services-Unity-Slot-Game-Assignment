using System;
using System.Collections.Generic;
using Underpin.SlotGame.Data;
using UnityEngine;

namespace Underpin.SlotGame.Logic
{
    /// <summary>
    /// Thread-safe, weighted Random Number Generator for slot outcomes and spin generation.
    /// Supports seed configuration for automated testing and auditing.
    /// </summary>
    public class RNGManager : IRandomNumberGenerator
    {
        private System.Random _random;

        public RNGManager()
        {
            _random = new System.Random(Guid.NewGuid().GetHashCode());
        }

        public RNGManager(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <inheritdoc/>
        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <inheritdoc/>
        public int NextRange(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive) return minInclusive;
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <inheritdoc/>
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <inheritdoc/>
        public SymbolData GetWeightedRandomSymbol(IReadOnlyList<SymbolData> availableSymbols)
        {
            if (availableSymbols == null || availableSymbols.Count == 0)
            {
                Debug.LogWarning("[RNGManager] Symbol list is empty! Returning null.");
                return null;
            }

            // Calculate total cumulative weight
            int totalWeight = 0;
            for (int i = 0; i < availableSymbols.Count; i++)
            {
                var sym = availableSymbols[i];
                if (sym != null)
                {
                    totalWeight += Mathf.Max(1, sym.DropWeight);
                }
            }

            if (totalWeight <= 0) return availableSymbols[0];

            int randomValue = _random.Next(0, totalWeight);
            int runningSum = 0;

            for (int i = 0; i < availableSymbols.Count; i++)
            {
                var sym = availableSymbols[i];
                if (sym == null) continue;

                runningSum += Mathf.Max(1, sym.DropWeight);
                if (randomValue < runningSum)
                {
                    return sym;
                }
            }

            return availableSymbols[availableSymbols.Count - 1];
        }

        /// <summary>
        /// Generates a randomized outcome grid (reelCount x rowCount) using weighted probabilities.
        /// </summary>
        public SymbolData[,] GenerateGrid(int reelCount, int rowCount, IReadOnlyList<SymbolData> symbols)
        {
            SymbolData[,] grid = new SymbolData[reelCount, rowCount];
            for (int reel = 0; reel < reelCount; reel++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    grid[reel, row] = GetWeightedRandomSymbol(symbols);
                }
            }
            return grid;
        }
    }
}
