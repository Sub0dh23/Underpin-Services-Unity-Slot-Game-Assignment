using System;
using UnityEngine;

namespace Underpin.SlotGame.Data
{
    /// <summary>
    /// Represents a grid coordinate on the slot machine matrix (reel index and row index).
    /// </summary>
    [Serializable]
    public struct SlotCoordinate
    {
        [Tooltip("Index of the reel (0 = leftmost).")]
        public int reelIndex;

        [Tooltip("Index of the visible row (0 = top, 1 = middle, 2 = bottom).")]
        public int rowIndex;

        public SlotCoordinate(int reelIndex, int rowIndex)
        {
            this.reelIndex = reelIndex;
            this.rowIndex = rowIndex;
        }

        public override string ToString() => $"[Reel {reelIndex}, Row {rowIndex}]";
    }

    /// <summary>
    /// Configuration data for an individual payline pattern across the reels.
    /// </summary>
    [Serializable]
    public class PaylineData
    {
        [Tooltip("Display name or identifier for the payline (e.g. 'Line 1 - Center').")]
        [SerializeField] private string lineName = "Line 1";

        [Tooltip("Line index number.")]
        [SerializeField] private int lineId = 1;

        [Tooltip("Path coordinates across reels from left to right.")]
        [SerializeField] private SlotCoordinate[] coordinates;

        [Tooltip("Color used to draw the winning payline overlay.")]
        [SerializeField] private Color lineColor = Color.cyan;

        public string LineName => lineName;
        public int LineId => lineId;
        public SlotCoordinate[] Coordinates => coordinates;
        public Color LineColor => lineColor;

        public PaylineData(string name, int id, SlotCoordinate[] coords, Color color)
        {
            lineName = name;
            lineId = id;
            coordinates = coords;
            lineColor = color;
        }
    }
}
