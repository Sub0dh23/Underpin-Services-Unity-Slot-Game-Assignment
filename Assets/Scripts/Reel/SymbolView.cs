using System.Collections;
using Underpin.SlotGame.Data;
using Underpin.SlotGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.Reel
{
    /// <summary>
    /// Visual component representing a single symbol slot on a reel.
    /// Handles sprite rendering, win celebration pulsing, and visual effects.
    /// </summary>
    public class SymbolView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image glowOutline;
        [SerializeField] private RectTransform rectTransform;

        [Header("Animation Settings")]
        [SerializeField] private float pulseScale = 1.18f;
        [SerializeField] private float pulseSpeed = 4f;

        private SymbolData _currentSymbol;
        private Coroutine _winAnimationRoutine;
        private Vector3 _originalScale;
        private Color _originalImageColor = Color.white;
        private Color _baseGlowColor = Color.yellow;
        private bool _isHighlighted;

        public SymbolData CurrentSymbol => _currentSymbol;
        public RectTransform Rect => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());
        public bool IsHighlighted => _isHighlighted;

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            _originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

            if (symbolImage != null)
            {
                _originalImageColor = symbolImage.color;
            }

            if (glowOutline != null)
            {
                glowOutline.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Binds and displays the given SymbolData.
        /// </summary>
        public void SetSymbol(SymbolData symbol)
        {
            _currentSymbol = symbol;
            if (symbolImage != null)
            {
                if (symbol != null && symbol.Icon != null)
                {
                    symbolImage.sprite = symbol.Icon;
                    symbolImage.enabled = true;
                }
                else
                {
                    symbolImage.enabled = false;
                }
            }

            ResetHighlight();
        }

        /// <summary>
        /// Triggers win highlight effect (glow outline + EasingHelper scale-pulse & color-flash tween).
        /// </summary>
        public void PlayWinHighlight(Color customGlowColor)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            _baseGlowColor = customGlowColor;

            if (glowOutline != null)
            {
                glowOutline.color = _baseGlowColor;
                glowOutline.gameObject.SetActive(true);
            }

            if (_winAnimationRoutine != null) StopCoroutine(_winAnimationRoutine);
            _winAnimationRoutine = StartCoroutine(AnimateWinPulseRoutine(pulseScale, pulseSpeed));
        }

        /// <summary>
        /// Triggers rapid anticipation pulse for landed scatters when bonus is within reach.
        /// </summary>
        public void PlayAnticipationPulse(Color? customColor = null)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            _baseGlowColor = customColor ?? new Color(1f, 0.85f, 0.1f, 1f); // Vibrant gold

            if (glowOutline != null)
            {
                glowOutline.color = _baseGlowColor;
                glowOutline.gameObject.SetActive(true);
            }

            if (_winAnimationRoutine != null) StopCoroutine(_winAnimationRoutine);
            _winAnimationRoutine = StartCoroutine(AnimateWinPulseRoutine(1.22f, 6.0f));
        }

        /// <summary>
        /// Resets visual state to normal idle.
        /// </summary>
        public void ResetHighlight()
        {
            _isHighlighted = false;
            if (_winAnimationRoutine != null)
            {
                StopCoroutine(_winAnimationRoutine);
                _winAnimationRoutine = null;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = _originalScale;
            }

            if (symbolImage != null)
            {
                symbolImage.color = _originalImageColor;
            }

            if (glowOutline != null)
            {
                glowOutline.gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateWinPulseRoutine(float maxScale, float speed)
        {
            float elapsed = 0f;

            while (_isHighlighted)
            {
                elapsed += Time.deltaTime * speed;
                float easeFactor = EasingHelper.EaseInOutPingPong(elapsed);
                float currentScale = Mathf.Lerp(1.0f, maxScale, easeFactor);

                if (rectTransform != null)
                {
                    rectTransform.localScale = _originalScale * currentScale;
                }

                if (glowOutline != null)
                {
                    Color gc = _baseGlowColor;
                    gc.a = Mathf.Lerp(0.35f, 1.0f, easeFactor);
                    glowOutline.color = gc;
                }

                if (symbolImage != null)
                {
                    // Subtle color-flash brightness pulse
                    Color flashColor = Color.Lerp(_originalImageColor, Color.Lerp(_originalImageColor, _baseGlowColor, 0.45f), easeFactor);
                    symbolImage.color = flashColor;
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = _originalScale;
            }

            if (symbolImage != null)
            {
                symbolImage.color = _originalImageColor;
            }
        }
    }
}
