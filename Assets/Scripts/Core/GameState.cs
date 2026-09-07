namespace Underpin.SlotGame.Core
{
    /// <summary>
    /// Lifecycle states of the slot machine game flow.
    /// </summary>
    public enum GameState
    {
        /// <summary> Ready for player input (Spin, change bet, view paytable). </summary>
        Idle,

        /// <summary> Reels in active motion. </summary>
        Spinning,

        /// <summary> Evaluating grid symbols and payline matches. </summary>
        Evaluating,

        /// <summary> Presenting win celebration, highlight effects, and payout counting. </summary>
        WinCelebration,

        /// <summary> Executing automated Free Spins bonus sequence. </summary>
        FreeSpins,

        /// <summary> Player balance depleted below minimum bet. </summary>
        OutOfFunds
    }
}
