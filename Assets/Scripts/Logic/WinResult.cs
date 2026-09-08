using System;
using System.Collections.Generic;
using Underpin.SlotGame.Data;

namespace Underpin.SlotGame.Logic
{
    /// <summary>
    /// Details for an individual payline that scored a winning combination.
    /// </summary>
    [Serializable]
    public class PaylineWin
    {
        public PaylineData Payline { get; }
        public SymbolData MatchedSymbol { get; }
        public int MatchCount { get; }
        public float PayoutMultiplier { get; }
        public int WinAmount { get; }
        public List<SlotCoordinate> WinningPositions { get; }

        public PaylineWin(PaylineData payline, SymbolData symbol, int matchCount, float multiplier, int winAmount, List<SlotCoordinate> positions)
        {
            Payline = payline;
            MatchedSymbol = symbol;
            MatchCount = matchCount;
            PayoutMultiplier = multiplier;
            WinAmount = winAmount;
            WinningPositions = positions;
        }
    }

    /// <summary>
    /// Complete evaluation result for a slot spin cycle.
    /// </summary>
    public class WinResult
    {
        public List<PaylineWin> WinningPaylines { get; } = new List<PaylineWin>();
        public int TotalWinAmount { get; set; }
        public float TotalPayoutMultiplier { get; set; }
        public bool IsFreeSpinsTriggered { get; set; }
        public int FreeSpinsAwarded { get; set; }
        public int ScatterCount { get; set; }
        public List<SlotCoordinate> ScatterPositions { get; set; } = new List<SlotCoordinate>();
        public bool IsBigWin { get; set; }
        public bool IsMegaWin { get; set; }

        public bool HasAnyWin => WinningPaylines.Count > 0 || IsFreeSpinsTriggered || TotalWinAmount > 0;

        /// <summary>
        /// Returns all unique grid coordinates participating in any win.
        /// </summary>
        public HashSet<SlotCoordinate> GetAllWinningCoordinates()
        {
            var coords = new HashSet<SlotCoordinate>();
            for (int i = 0; i < WinningPaylines.Count; i++)
            {
                var win = WinningPaylines[i];
                if (win.WinningPositions != null)
                {
                    for (int j = 0; j < win.WinningPositions.Count; j++)
                    {
                        coords.Add(win.WinningPositions[j]);
                    }
                }
            }

            if (IsFreeSpinsTriggered && ScatterPositions != null)
            {
                for (int i = 0; i < ScatterPositions.Count; i++)
                {
                    coords.Add(ScatterPositions[i]);
                }
            }

            return coords;
        }
    }
}
