using System;
using UnityEngine;

namespace Underpin.SlotGame.Logic
{
    /// <summary>
    /// Standard playing card suits for the Double-or-Nothing gamble minigame.
    /// </summary>
    public enum CardSuit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    /// <summary>
    /// Playing card color classification.
    /// </summary>
    public enum CardColor
    {
        Red,
        Black
    }

    /// <summary>
    /// Playing card rank value.
    /// </summary>
    public enum CardRank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    /// <summary>
    /// Represents a single playing card drawn during the Gamble minigame.
    /// </summary>
    [Serializable]
    public struct GambleCard
    {
        public CardSuit Suit;
        public CardRank Rank;

        public GambleCard(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public CardColor Color => (Suit == CardSuit.Hearts || Suit == CardSuit.Diamonds) ? CardColor.Red : CardColor.Black;
        public bool IsRed => Color == CardColor.Red;

        public string SuitSymbol
        {
            get
            {
                switch (Suit)
                {
                    case CardSuit.Hearts: return "♥";
                    case CardSuit.Diamonds: return "♦";
                    case CardSuit.Clubs: return "♣";
                    case CardSuit.Spades: return "♠";
                    default: return "?";
                }
            }
        }

        public string ColorHex => IsRed ? "#FF3B30" : "#1C1C1E";

        public string RankString
        {
            get
            {
                switch (Rank)
                {
                    case CardRank.Ace: return "A";
                    case CardRank.King: return "K";
                    case CardRank.Queen: return "Q";
                    case CardRank.Jack: return "J";
                    case CardRank.Ten: return "10";
                    default: return ((int)Rank).ToString();
                }
            }
        }

        public string DisplayName => $"{RankString} of {Suit}";
        public string FormattedBadge => $"<color={ColorHex}>{RankString}{SuitSymbol}</color>";

        public static GambleCard DrawRandom(IRandomNumberGenerator rng = null)
        {
            CardSuit[] suits = (CardSuit[])Enum.GetValues(typeof(CardSuit));
            CardRank[] ranks = (CardRank[])Enum.GetValues(typeof(CardRank));

            int suitIndex = rng != null ? rng.NextRange(0, suits.Length) : UnityEngine.Random.Range(0, suits.Length);
            int rankIndex = rng != null ? rng.NextRange(0, ranks.Length) : UnityEngine.Random.Range(0, ranks.Length);

            return new GambleCard(suits[suitIndex], ranks[rankIndex]);
        }
    }
}
