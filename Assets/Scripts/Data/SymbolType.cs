namespace Underpin.SlotGame.Data
{
    /// <summary>
    /// Categorization of symbols for special slot game mechanics.
    /// </summary>
    public enum SymbolType
    {
        /// <summary> Standard paytable symbol that requires matching identical symbols. </summary>
        Regular,

        /// <summary> Wild symbol substituting for any regular paying symbol to complete combinations. </summary>
        Wild,

        /// <summary> Scatter symbol that triggers bonus rewards (e.g. Free Spins) anywhere on the grid. </summary>
        Scatter,

        /// <summary> Bonus symbol that triggers instant multipliers or mini-games. </summary>
        Bonus
    }
}
