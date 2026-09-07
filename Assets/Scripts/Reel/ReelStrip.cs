using System;
using System.Collections;
using System.Collections.Generic;
using Underpin.SlotGame.Data;
using Underpin.SlotGame.Logic;
using Underpin.SlotGame.Utils;
using UnityEngine;

namespace Underpin.SlotGame.Reel
{
    /// <summary>
    /// Controls a single vertical reel column. Manages pooled symbol positioning,
    /// anticipation easing, high-speed continuous scrolling loop, and realistic landing bounce.
    /// </summary>
    public class ReelStrip : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [Tooltip("Index of this reel (0 = left, 1 = middle, 2 = right).")]
        [SerializeField] private int reelIndex = 0;

        [Tooltip("Number of visible symbol rows on the reel.")]
        [SerializeField] private int visibleRowCount = 3;

        [Tooltip("Vertical height allocated per symbol slot in UI units.")]
        [SerializeField] private float symbolHeight = 120f;

        [Tooltip("Vertical spacing between symbols.")]
        [SerializeField] private float symbolSpacing = 15f;

        [Header("Prefabs & Containers")]
        [Tooltip("Prefab for instantiating SymbolView instances.")]
        [SerializeField] private SymbolView symbolPrefab;

        [Tooltip("Container Transform under which symbol instances reside.")]
        [SerializeField] private RectTransform symbolsContainer;

        [Header("Animation Tuning")]
        [Tooltip("Anticipation pull-up distance in pixels.")]
        [SerializeField] private float anticipationOffset = 35f;

        [Tooltip("Anticipation pull-up duration in seconds.")]
        [SerializeField] private float anticipationDuration = 0.18f;

        [Tooltip("Spin scrolling speed in pixels per second.")]
        [SerializeField] private float spinSpeed = 1600f;

        [Tooltip("Duration of final deceleration and bounce landing in seconds.")]
        [SerializeField] private float landingDuration = 0.38f;

        [Tooltip("Overshoot bounce distance factor.")]
        [SerializeField] private float overshootFactor = 1.35f;

        // Events
        public event Action<int> OnReelSpinStarted;
        public event Action<int> OnReelSpinStopped;
        public event Action<int> OnSymbolTick;

        // State & Pooling
        private readonly List<SymbolView> _symbolViews = new List<SymbolView>();
        private IRandomNumberGenerator _rng;
        private IReadOnlyList<SymbolData> _availableSymbols;
        private SymbolData[] _targetOutcomeSymbols;
        private Coroutine _spinCoroutine;
        private bool _isSpinning;
        private float _slotStep;
        private float _totalHeight;
        private float _topY;
        private float _bottomY;
        private float _minY;
        private float _scrollOffset;

        public int ReelIndex => reelIndex;
        public bool IsSpinning => _isSpinning;
        public IReadOnlyList<SymbolView> VisibleSymbolViews => _symbolViews;

        private void Awake()
        {
            if (symbolsContainer == null)
            {
                symbolsContainer = GetComponent<RectTransform>();
            }

            if (symbolsContainer != null && symbolsContainer.rect.height > 10f)
            {
                _slotStep = symbolsContainer.rect.height / visibleRowCount;
                symbolHeight = _slotStep * 0.94f;
                symbolSpacing = _slotStep * 0.06f;
            }
            else
            {
                _slotStep = symbolHeight + symbolSpacing;
            }
        }

        /// <summary>
        /// Initializes the reel with symbols, RNG generator, and creates the pooled symbol views.
        /// </summary>
        public void Initialize(int index, IReadOnlyList<SymbolData> symbols, IRandomNumberGenerator rng)
        {
            reelIndex = index;
            _availableSymbols = symbols;
            _rng = rng;

            if (symbolsContainer != null && symbolsContainer.rect.height > 10f)
            {
                _slotStep = symbolsContainer.rect.height / visibleRowCount;
                symbolHeight = _slotStep * 0.94f;
                symbolSpacing = _slotStep * 0.06f;
            }
            else
            {
                _slotStep = symbolHeight + symbolSpacing;
            }

            // Visible rows (e.g. 3) + 2 buffer symbols (1 above, 1 below) = 5 pooled items
            int totalPooled = visibleRowCount + 2;
            _totalHeight = totalPooled * _slotStep;

            // Center visible rows around Y = 0
            // For 3 rows: row 0 is +_slotStep, row 1 is 0, row 2 is -_slotStep
            _topY = (_slotStep * (visibleRowCount - 1) * 0.5f) + _slotStep;
            _bottomY = -((_slotStep * (visibleRowCount - 1) * 0.5f) + _slotStep);
            _minY = _bottomY - (_slotStep * 0.5f);
            _scrollOffset = 0f;

            // Clean up existing children if any
            if (symbolsContainer != null)
            {
                for (int i = symbolsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(symbolsContainer.GetChild(i).gameObject);
                }
            }
            _symbolViews.Clear();

            // Spawn pooled symbol views
            for (int i = 0; i < totalPooled; i++)
            {
                SymbolView view = Instantiate(symbolPrefab, symbolsContainer);
                view.gameObject.name = $"SymbolSlot_{i}";
                float yPos = _topY - (i * _slotStep);
                view.Rect.anchoredPosition = new Vector2(0, yPos);

                // Assign initial random symbol
                SymbolData randSym = _rng != null && _availableSymbols != null && _availableSymbols.Count > 0
                    ? _rng.GetWeightedRandomSymbol(_availableSymbols)
                    : (_availableSymbols != null && _availableSymbols.Count > 0 ? _availableSymbols[0] : null);
                view.SetSymbol(randSym);
                _symbolViews.Add(view);
            }
        }

        /// <summary>
        /// Sets initial display symbols without animation.
        /// </summary>
        public void SetDisplaySymbols(SymbolData[] initialSymbols)
        {
            if (initialSymbols == null) return;
            // Visible symbols start at index 1 in the pooled list (index 0 is top buffer)
            for (int r = 0; r < visibleRowCount && r < initialSymbols.Length; r++)
            {
                if (r + 1 < _symbolViews.Count)
                {
                    _symbolViews[r + 1].SetSymbol(initialSymbols[r]);
                }
            }
        }

        /// <summary>
        /// Gets the visible SymbolView corresponding to a specific row index (0 = top, 1 = middle, 2 = bottom).
        /// </summary>
        public SymbolView GetVisibleSymbolView(int rowIndex)
        {
            int targetIdx = rowIndex + 1; // offset by 1 for top buffer
            if (targetIdx >= 0 && targetIdx < _symbolViews.Count)
            {
                return _symbolViews[targetIdx];
            }
            return null;
        }

        /// <summary>
        /// Begins the spin sequence.
        /// </summary>
        public void StartSpin()
        {
            if (_isSpinning) return;
            _isSpinning = true;
            OnReelSpinStarted?.Invoke(reelIndex);

            if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
            _spinCoroutine = StartCoroutine(SpinRoutine());
        }

        /// <summary>
        /// Signals the reel to decelerate and land on the specified outcome symbols.
        /// </summary>
        /// <param name="targetSymbols">Array of visible symbols from top to bottom (length = visibleRowCount).</param>
        public void StopSpin(SymbolData[] targetSymbols)
        {
            _targetOutcomeSymbols = targetSymbols;
        }

        private IEnumerator SpinRoutine()
        {
            _targetOutcomeSymbols = null;

            // 1. Anticipation Phase (Pull-up slightly)
            float timer = 0f;
            while (timer < anticipationDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / anticipationDuration);
                float easeOffset = -EasingHelper.EaseInBack(progress) * anticipationOffset;
                ApplyScrollOffset(_scrollOffset + easeOffset);
                yield return null;
            }

            // 2. High-Speed Looping Phase
            float previousScroll = _scrollOffset;
            while (_targetOutcomeSymbols == null)
            {
                _scrollOffset += spinSpeed * Time.deltaTime;
                CheckTicks(previousScroll, _scrollOffset);
                previousScroll = _scrollOffset;
                ApplyScrollOffset(_scrollOffset);
                yield return null;
            }

            // 3. Load target symbols onto the views
            if (_targetOutcomeSymbols != null)
            {
                for (int r = 0; r < visibleRowCount && r < _targetOutcomeSymbols.Length; r++)
                {
                    int viewIdx = r + 1;
                    if (viewIdx < _symbolViews.Count)
                    {
                        _symbolViews[viewIdx].SetSymbol(_targetOutcomeSymbols[r]);
                    }
                }
                if (_symbolViews.Count > 0 && _rng != null && _availableSymbols != null)
                {
                    _symbolViews[0].SetSymbol(_rng.GetWeightedRandomSymbol(_availableSymbols));
                    _symbolViews[_symbolViews.Count - 1].SetSymbol(_rng.GetWeightedRandomSymbol(_availableSymbols));
                }
            }

            // 4. Smooth Landing with Overshoot Bounce
            float startOffset = _scrollOffset;
            float targetOffset = Mathf.Ceil((startOffset + _slotStep * 2f) / _totalHeight) * _totalHeight;
            float totalDist = targetOffset - startOffset;

            float landTimer = 0f;
            while (landTimer < landingDuration)
            {
                landTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(landTimer / landingDuration);
                float easeVal = EasingHelper.EaseOutBack(progress, overshootFactor);
                float currentOffset = startOffset + (totalDist * easeVal);

                CheckTicks(previousScroll, currentOffset);
                previousScroll = currentOffset;
                ApplyScrollOffset(currentOffset);
                yield return null;
            }

            // Exact snap to final rest positions
            _scrollOffset = 0f;
            ApplyScrollOffset(0f);

            _isSpinning = false;
            OnReelSpinStopped?.Invoke(reelIndex);
        }

        private void ApplyScrollOffset(float offset)
        {
            for (int i = 0; i < _symbolViews.Count; i++)
            {
                if (_symbolViews[i] == null || _symbolViews[i].Rect == null) continue;
                float baseY = _topY - (i * _slotStep);
                float y = Mathf.Repeat(baseY - offset - _minY, _totalHeight) + _minY;
                _symbolViews[i].Rect.anchoredPosition = new Vector2(0, y);
            }
        }

        private void CheckTicks(float fromOffset, float toOffset)
        {
            int prevStep = Mathf.FloorToInt(fromOffset / _slotStep);
            int currStep = Mathf.FloorToInt(toOffset / _slotStep);
            if (currStep > prevStep)
            {
                OnSymbolTick?.Invoke(reelIndex);
            }
        }
    }
}
