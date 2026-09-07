using UnityEngine;

namespace Underpin.SlotGame.Data
{
    /// <summary>
    /// ScriptableObject defining symbol attributes, visual assets, payouts, and RNG weights.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSymbolData", menuName = "Slot Game/Symbol Data", order = 1)]
    public class SymbolData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier string for this symbol.")]
        [SerializeField] private string symbolId = "sym_01";

        [Tooltip("Display name of the symbol.")]
        [SerializeField] private string symbolName = "Symbol 1";

        [Tooltip("Type/Role of the symbol.")]
        [SerializeField] private SymbolType symbolType = SymbolType.Regular;

        [Header("Visuals")]
        [Tooltip("Sprite rendered on the reel.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Glow / Highlight color during win celebration.")]
        [SerializeField] private Color highlightColor = Color.yellow;

        [Header("RNG & Probability")]
        [Tooltip("Relative drop weight for RNG distribution (higher = more frequent).")]
        [Range(1, 1000)]
        [SerializeField] private int dropWeight = 100;

        [Header("Payout Multipliers (Base Bet Multiplier)")]
        [Tooltip("Payout multiplier for 3 matching symbols on an active payline.")]
        [SerializeField] private float payout3Match = 10f;

        [Tooltip("Payout multiplier for 2 matching symbols on an active payline.")]
        [SerializeField] private float payout2Match = 2f;

        // Public Properties
        public string SymbolId => symbolId;
        public string SymbolName => symbolName;
        public SymbolType Type => symbolType;
        public Sprite Icon => icon;
        public Color HighlightColor => highlightColor;
        public int DropWeight => dropWeight;
        public float Payout3Match => payout3Match;
        public float Payout2Match => payout2Match;

        /// <summary>
        /// Returns the payout multiplier corresponding to the number of matched symbols.
        /// </summary>
        public float GetPayoutMultiplier(int matchCount)
        {
            switch (matchCount)
            {
                case 3:
                    return payout3Match;
                case 2:
                    return payout2Match;
                default:
                    return 0f;
            }
        }
    }
}
