using System.Collections.Generic;
using Underpin.SlotGame.Data;
using UnityEngine;

namespace Underpin.SlotGame.Logic
{
    /// <summary>
    /// Pure logic evaluator for matching symbols across paylines, computing payouts, and detecting bonus triggers.
    /// Free of Unity GameObject dependencies for testability.
    /// </summary>
    public static class WinEvaluator
    {
        /// <summary>
        /// Evaluates a 2D symbol matrix against configured paylines and rules.
        /// </summary>
        /// <param name="grid">2D array where dimension 0 is reelIndex and dimension 1 is rowIndex.</param>
        /// <param name="config">Paytable configuration containing paylines and payout rules.</param>
        /// <param name="currentBet">Base bet placed for the spin.</param>
        /// <param name="activeMultiplier">Global multiplier (e.g. 2x in Free Spins).</param>
        /// <returns>Populated WinResult containing all winnings, lines, and bonus flags.</returns>
        public static WinResult EvaluateGrid(SymbolData[,] grid, PaytableConfig config, int currentBet, float activeMultiplier = 1f)
        {
            var result = new WinResult();
            if (grid == null || config == null || currentBet <= 0) return result;

            int reelCount = grid.GetLength(0);
            int rowCount = grid.GetLength(1);

            float totalMultiplier = 0f;
            int totalWin = 0;

            // 1. Evaluate Paylines
            for (int p = 0; p < config.Paylines.Count; p++)
            {
                var payline = config.Paylines[p];
                if (payline.Coordinates == null || payline.Coordinates.Length < 2) continue;

                var lineCoords = payline.Coordinates;
                int coordsLength = lineCoords.Length;

                // Extract symbols along this payline
                SymbolData[] lineSymbols = new SymbolData[coordsLength];
                for (int i = 0; i < coordsLength; i++)
                {
                    var coord = lineCoords[i];
                    if (coord.reelIndex < reelCount && coord.rowIndex < rowCount)
                    {
                        lineSymbols[i] = grid[coord.reelIndex, coord.rowIndex];
                    }
                }

                // Check matches from left to right (standard slot evaluation)
                if (EvaluateLineMatch(lineSymbols, out SymbolData winningSymbol, out int matchCount))
                {
                    if (winningSymbol != null && matchCount >= 2)
                    {
                        float lineMultiplier = winningSymbol.GetPayoutMultiplier(matchCount) * activeMultiplier;
                        if (lineMultiplier > 0f)
                        {
                            int lineWinAmount = Mathf.RoundToInt(lineMultiplier * currentBet);
                            totalWin += lineWinAmount;
                            totalMultiplier += lineMultiplier;

                            var winningPositions = new List<SlotCoordinate>();
                            for (int i = 0; i < matchCount; i++)
                            {
                                winningPositions.Add(lineCoords[i]);
                            }

                            result.WinningPaylines.Add(new PaylineWin(
                                payline,
                                winningSymbol,
                                matchCount,
                                lineMultiplier,
                                lineWinAmount,
                                winningPositions
                            ));
                        }
                    }
                }
            }

            // 2. Evaluate Scatter symbols anywhere on grid
            int scatterCount = 0;
            var scatterCoords = new List<SlotCoordinate>();
            for (int r = 0; r < reelCount; r++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    var sym = grid[r, row];
                    if (sym != null && sym.Type == SymbolType.Scatter)
                    {
                        scatterCount++;
                        scatterCoords.Add(new SlotCoordinate(r, row));
                    }
                }
            }

            result.ScatterCount = scatterCount;
            result.ScatterPositions = scatterCoords;
            if (scatterCount >= config.ScatterTriggerCount)
            {
                result.IsFreeSpinsTriggered = true;
                result.FreeSpinsAwarded = config.FreeSpinsAwarded;
            }

            // 3. Finalize Win Totals and Celebration Tiers
            result.TotalWinAmount = totalWin;
            result.TotalPayoutMultiplier = totalMultiplier;

            if (currentBet > 0)
            {
                float relativeWin = (float)totalWin / currentBet;
                result.IsMegaWin = relativeWin >= config.MegaWinMultiplierThreshold;
                result.IsBigWin = !result.IsMegaWin && relativeWin >= config.BigWinMultiplierThreshold;
            }

            return result;
        }

        /// <summary>
        /// Evaluates a sequence of symbols from left to right with Wild substitution support.
        /// </summary>
        private static bool EvaluateLineMatch(SymbolData[] symbols, out SymbolData winningSymbol, out int matchCount)
        {
            winningSymbol = null;
            matchCount = 0;

            if (symbols == null || symbols.Length == 0) return false;

            // Find first non-wild symbol
            SymbolData targetSymbol = null;
            for (int i = 0; i < symbols.Length; i++)
            {
                var s = symbols[i];
                if (s == null) return false;

                if (s.Type != SymbolType.Wild && s.Type != SymbolType.Scatter)
                {
                    targetSymbol = s;
                    break;
                }
            }

            // If all symbols checked are Wilds, target is the first Wild
            if (targetSymbol == null)
            {
                targetSymbol = symbols[0];
            }

            winningSymbol = targetSymbol;
            matchCount = 0;

            // Match consecutive symbols from left to right
            for (int i = 0; i < symbols.Length; i++)
            {
                var current = symbols[i];
                if (current == null) break;

                // Wild matches target symbol; or identical symbol matches target
                if (current.Type == SymbolType.Wild || current.SymbolId == targetSymbol.SymbolId)
                {
                    matchCount++;
                }
                else
                {
                    break; // Left-to-right matching stops at first non-match
                }
            }

            return matchCount >= 2;
        }
    }
}
