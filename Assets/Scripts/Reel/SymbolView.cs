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
        private bool _isHighlighted;

        public SymbolData CurrentSymbol => _currentSymbol;
        public RectTransform Rect => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            _originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

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

        private Color _baseGlowColor = Color.yellow;

        /// <summary>
        /// Triggers win highlight effect (glow + pulse animation).
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
            _winAnimationRoutine = StartCoroutine(AnimateWinPulse());
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

            if (glowOutline != null)
            {
                glowOutline.gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateWinPulse()
        {
            float timer = 0f;
            while (_isHighlighted)
            {
                timer += Time.deltaTime * pulseSpeed;
                float sinFactor = (Mathf.Sin(timer) + 1f) * 0.5f; // 0 to 1
                float scale = Mathf.Lerp(1.0f, pulseScale, sinFactor);

                if (rectTransform != null)
                {
                    rectTransform.localScale = _originalScale * scale;
                }

                if (glowOutline != null)
                {
                    Color c = _baseGlowColor;
                    c.a = Mathf.Lerp(0.45f, 1.0f, sinFactor);
                    glowOutline.color = c;
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = _originalScale;
            }
        }
    }
}
