using System;
using System.Collections;
using System.Collections.Generic;
using Underpin.SlotGame.Data;
using Underpin.SlotGame.Logic;
using UnityEngine;

namespace Underpin.SlotGame.Reel
{
    /// <summary>
    /// Master coordinator for all reels on the slot machine grid.
    /// Manages staggered launch, staggered stop sequencing, and symbol highlight effects.
    /// </summary>
    public class SlotReelController : MonoBehaviour
    {
        [Header("Reels Setup")]
        [Tooltip("Ordered array of ReelStrip components from left to right.")]
        [SerializeField] private ReelStrip[] reels;

        [Header("Timing Configuration")]
        [Tooltip("Delay in seconds between starting consecutive reels.")]
        [SerializeField] private float spinStartStagger = 0.08f;

        [Tooltip("Minimum duration in seconds before the first reel begins stopping.")]
        [SerializeField] private float minSpinDuration = 1.0f;

        [Tooltip("Delay in seconds between stopping consecutive reels.")]
        [SerializeField] private float reelStopStagger = 0.32f;

        [Tooltip("Extra spin tension duration in seconds when 2+ scatters land before the final reel stops.")]
        [SerializeField] private float anticipationSpinDuration = 1.25f;

        // Events
        public event Action OnAllReelsSpinStarted;
        public event Action OnAllReelsSpinCompleted;
        public event Action<int> OnReelClickTick;
        public event Action<int> OnAnticipationStarted;

        private bool _isSpinning;
        private int _reelsStoppedCount;

        public bool IsSpinning => _isSpinning;
        public int ReelCount => reels != null ? reels.Length : 0;

        /// <summary>
        /// Initializes all reels with symbol configurations and RNG provider.
        /// </summary>
        public void Initialize(PaytableConfig config, IRandomNumberGenerator rng)
        {
            if (reels == null || reels.Length == 0)
            {
                reels = GetComponentsInChildren<ReelStrip>();
            }

            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].Initialize(i, config.Symbols, rng);
                reels[i].OnSymbolTick += HandleSymbolTick;
            }
        }

        private void OnDestroy()
        {
            if (reels != null)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                    {
                        reels[i].OnSymbolTick -= HandleSymbolTick;
                    }
                }
            }
        }

        private void HandleSymbolTick(int reelIdx)
        {
            OnReelClickTick?.Invoke(reelIdx);
        }

        /// <summary>
        /// Triggers the full staggered spin sequence and delivers target outcomes to each reel.
        /// </summary>
        public void SpinReels(SymbolData[,] targetGrid, Action onComplete)
        {
            if (_isSpinning) return;
            _isSpinning = true;
            _reelsStoppedCount = 0;

            ResetAllHighlights();
            StartCoroutine(SpinSequenceRoutine(targetGrid, onComplete));
        }

        private IEnumerator SpinSequenceRoutine(SymbolData[,] targetGrid, Action onComplete)
        {
            OnAllReelsSpinStarted?.Invoke();

            // 1. Staggered Start
            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].StartSpin();
                if (spinStartStagger > 0f)
                {
                    yield return new WaitForSeconds(spinStartStagger);
                }
            }

            // 2. Minimum spin duration
            yield return new WaitForSeconds(minSpinDuration);

            // 3. Staggered Stop with Anticipation Check
            int rowCount = targetGrid.GetLength(1);
            int landedScatters = 0;

            for (int i = 0; i < reels.Length; i++)
            {
                // Anticipation Near-Miss Check: If 2+ scatters already landed and remaining reels are spinning
                if (i > 0 && landedScatters >= 2)
                {
                    HighlightLandedScatters(targetGrid, i);
                    OnAnticipationStarted?.Invoke(i);

                    // Add tension slowdown delay before stopping this reel
                    if (anticipationSpinDuration > 0f)
                    {
                        yield return new WaitForSeconds(anticipationSpinDuration);
                    }
                }

                // Extract target symbols for this reel column
                SymbolData[] reelTargets = new SymbolData[rowCount];
                for (int row = 0; row < rowCount; row++)
                {
                    reelTargets[row] = targetGrid[i, row];
                    if (reelTargets[row] != null && reelTargets[row].Type == SymbolType.Scatter)
                    {
                        landedScatters++;
                    }
                }

                int reelIndex = i;
                reels[i].OnReelSpinStopped += HandleIndividualReelStopped;
                reels[i].StopSpin(reelTargets);

                if (reelStopStagger > 0f && i < reels.Length - 1)
                {
                    yield return new WaitForSeconds(reelStopStagger);
                }
            }

            // Wait until all reels have completed landing
            while (_reelsStoppedCount < reels.Length)
            {
                yield return null;
            }

            _isSpinning = false;
            OnAllReelsSpinCompleted?.Invoke();
            onComplete?.Invoke();
        }

        private void HighlightLandedScatters(SymbolData[,] grid, int upToReelExclusive)
        {
            int rowCount = grid.GetLength(1);
            for (int r = 0; r < upToReelExclusive && r < reels.Length; r++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (grid[r, row] != null && grid[r, row].Type == SymbolType.Scatter)
                    {
                        var view = reels[r].GetVisibleSymbolView(row);
                        if (view != null)
                        {
                            view.PlayAnticipationPulse(new Color(1f, 0.85f, 0.1f, 1f));
                        }
                    }
                }
            }
        }

        private void HandleIndividualReelStopped(int reelIdx)
        {
            reels[reelIdx].OnReelSpinStopped -= HandleIndividualReelStopped;
            _reelsStoppedCount++;
        }

        /// <summary>
        /// Highlights specific winning symbol slots on the matrix.
        /// </summary>
        public void HighlightWinningSymbols(IEnumerable<SlotCoordinate> winningCoords, Color glowColor)
        {
            if (winningCoords == null) return;

            foreach (var coord in winningCoords)
            {
                if (coord.reelIndex >= 0 && coord.reelIndex < reels.Length)
                {
                    var view = reels[coord.reelIndex].GetVisibleSymbolView(coord.rowIndex);
                    if (view != null)
                    {
                        view.PlayWinHighlight(glowColor);
                    }
                }
            }
        }

        /// <summary>
        /// Resets all symbol highlight effects across all reels.
        /// </summary>
        public void ResetAllHighlights()
        {
            if (reels == null) return;
            for (int r = 0; r < reels.Length; r++)
            {
                if (reels[r] != null && reels[r].VisibleSymbolViews != null)
                {
                    foreach (var view in reels[r].VisibleSymbolViews)
                    {
                        if (view != null) view.ResetHighlight();
                    }
                }
            }
        }
    }
}
