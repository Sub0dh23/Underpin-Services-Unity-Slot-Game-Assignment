using System.Collections.Generic;
using UnityEngine;

namespace Underpin.SlotGame.Data
{
    /// <summary>
    /// Master ScriptableObject defining all symbols, paylines, bet steps, and bonus round rules.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPaytableConfig", menuName = "Slot Game/Paytable Config", order = 2)]
    public class PaytableConfig : ScriptableObject
    {
        [Header("Symbols")]
        [Tooltip("List of all available symbols in the game.")]
        [SerializeField] private List<SymbolData> symbols = new List<SymbolData>();

        [Header("Paylines")]
        [Tooltip("Configured winning lines across the 3x3 reel matrix.")]
        [SerializeField] private List<PaylineData> paylines = new List<PaylineData>();

        [Header("Economy & Betting")]
        [Tooltip("Initial credits assigned to player at start.")]
        [SerializeField] private int startingBalance = 1000;

        [Tooltip("Available discrete bet increments.")]
        [SerializeField] private int[] betAmounts = new int[] { 10, 20, 50, 100, 200, 500 };

        [Tooltip("Default selected bet amount index.")]
        [SerializeField] private int defaultBetIndex = 1;

        [Header("Bonus & Free Spin Rules")]
        [Tooltip("Minimum number of Scatter symbols anywhere on grid to trigger Free Spins.")]
        [SerializeField] private int scatterTriggerCount = 3;

        [Tooltip("Number of Free Spins awarded upon triggering.")]
        [SerializeField] private int freeSpinsAwarded = 10;

        [Tooltip("Payout multiplier applied to all wins during Free Spins.")]
        [SerializeField] private float freeSpinsWinMultiplier = 2.0f;

        [Header("Celebration Thresholds (Multiplier of Current Bet)")]
        [Tooltip("Win amount multiplier threshold for Big Win celebration.")]
        [SerializeField] private float bigWinMultiplierThreshold = 10f;

        [Tooltip("Win amount multiplier threshold for Mega Win celebration.")]
        [SerializeField] private float megaWinMultiplierThreshold = 25f;

        // Public Accessors
        public IReadOnlyList<SymbolData> Symbols => symbols;
        public IReadOnlyList<PaylineData> Paylines => paylines;
        public int StartingBalance => startingBalance;
        public int[] BetAmounts => betAmounts;
        public int DefaultBetIndex => Mathf.Clamp(defaultBetIndex, 0, betAmounts.Length - 1);
        public int ScatterTriggerCount => scatterTriggerCount;
        public int FreeSpinsAwarded => freeSpinsAwarded;
        public float FreeSpinsWinMultiplier => freeSpinsWinMultiplier;
        public float BigWinMultiplierThreshold => bigWinMultiplierThreshold;
        public float MegaWinMultiplierThreshold => megaWinMultiplierThreshold;

        /// <summary>
        /// Populates default standard 5 paylines for a 3x3 slot grid if empty.
        /// </summary>
        public void InitializeDefaultPaylines()
        {
            if (paylines != null && paylines.Count > 0) return;

            paylines = new List<PaylineData>
            {
                // Top Horizontal Line (Row 0)
                new PaylineData("Top Line", 1, new SlotCoordinate[] {
                    new SlotCoordinate(0, 0),
                    new SlotCoordinate(1, 0),
                    new SlotCoordinate(2, 0)
                }, new Color(0.2f, 0.8f, 1.0f)),

                // Middle Horizontal Line (Row 1)
                new PaylineData("Center Line", 2, new SlotCoordinate[] {
                    new SlotCoordinate(0, 1),
                    new SlotCoordinate(1, 1),
                    new SlotCoordinate(2, 1)
                }, new Color(1.0f, 0.8f, 0.2f)),

                // Bottom Horizontal Line (Row 2)
                new PaylineData("Bottom Line", 3, new SlotCoordinate[] {
                    new SlotCoordinate(0, 2),
                    new SlotCoordinate(1, 2),
                    new SlotCoordinate(2, 2)
                }, new Color(0.4f, 1.0f, 0.4f)),

                // Diagonal Top-Left to Bottom-Right
                new PaylineData("Diagonal Down", 4, new SlotCoordinate[] {
                    new SlotCoordinate(0, 0),
                    new SlotCoordinate(1, 1),
                    new SlotCoordinate(2, 2)
                }, new Color(1.0f, 0.4f, 0.8f)),

                // Diagonal Bottom-Left to Top-Right
                new PaylineData("Diagonal Up", 5, new SlotCoordinate[] {
                    new SlotCoordinate(0, 2),
                    new SlotCoordinate(1, 1),
                    new SlotCoordinate(2, 0)
                }, new Color(1.0f, 0.5f, 0.2f))
            };
        }
    }
}
