using System;
using System.Collections;
using TMPro;
using Underpin.SlotGame.Audio;
using Underpin.SlotGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Visual win dialog showing Normal Win, Big Win, Mega Win, and Free Spins awarded banners.
    /// Features count-up score tally animation and pulse juice effects.
    /// </summary>
    public class WinPopupUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupContainer;
        [SerializeField] private Image popupBannerImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI winAmountText;
        [SerializeField] private TextMeshProUGUI subDetailText;
        [SerializeField] private Button dismissButton;
        [SerializeField] private RectTransform bannerTransform;

        [Header("Animation Settings")]
        [SerializeField] private float defaultCountUpDuration = 1.2f;
        [SerializeField] private float defaultAutoDismissDelay = 2.5f;

        private Action _onDismissedCallback;
        private Action<int> _onTallyTickCallback;
        private Coroutine _displayRoutine;
        private Color _originalBannerColor = Color.white;
        private Vector3 _originalTitleScale = Vector3.one;
        private Vector3 _originalAmountScale = Vector3.one;
        private bool _isCounting = false;
        private int _currentTargetAmount = 0;

        private void Awake()
        {
            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(HandleUserClick);
            }

            if (popupBannerImage != null)
            {
                _originalBannerColor = popupBannerImage.color;
            }

            if (titleText != null)
            {
                _originalTitleScale = titleText.transform.localScale;
            }

            if (winAmountText != null)
            {
                _originalAmountScale = winAmountText.transform.localScale;
            }

            HideImmediate();
        }

        public void ShowWin(int winAmount, bool isMegaWin, bool isBigWin, Action onComplete, Action<int> onTallyTick = null)
        {
            _onDismissedCallback = onComplete;
            _onTallyTickCallback = onTallyTick;
            _currentTargetAmount = winAmount;
            _isCounting = true;

            gameObject.SetActive(true);
            if (popupContainer != null) popupContainer.SetActive(true);

            // Configure visual tier parameters
            Color bannerColor;
            string titleStr;
            string subStr;
            float countDuration;
            float dismissDelay;
            float overshootFactor;

            if (isMegaWin)
            {
                titleStr = "<size=115%><color=#FFD700>MEGA WIN!</color></size>";
                subStr = "<color=#FFE680>JACKPOT TIER COMBINATION!</color>";
                bannerColor = new Color(1.0f, 0.72f, 0.05f, 1.0f); // Radiant Gold
                countDuration = 2.0f;
                dismissDelay = 3.0f;
                overshootFactor = 3.2f;
            }
            else if (isBigWin)
            {
                titleStr = "<size=108%><color=#FFA500>BIG WIN!</color></size>";
                subStr = "<color=#FFD280>SPECTACULAR WIN!</color>";
                bannerColor = new Color(1.0f, 0.55f, 0.0f, 1.0f); // Amber Orange
                countDuration = 1.4f;
                dismissDelay = 2.2f;
                overshootFactor = 2.2f;
            }
            else
            {
                titleStr = "<color=#00FFFF>WINNER!</color>";
                subStr = "Tap anywhere to collect";
                bannerColor = new Color(0.0f, 0.75f, 1.0f, 1.0f); // Cyan Blue
                countDuration = defaultCountUpDuration;
                dismissDelay = defaultAutoDismissDelay;
                overshootFactor = 1.4f;
            }

            if (titleText != null) titleText.text = titleStr;
            if (subDetailText != null) subDetailText.text = subStr;
            if (popupBannerImage != null) popupBannerImage.color = bannerColor;

            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(AnimateWinDisplay(winAmount, isMegaWin, isBigWin, countDuration, dismissDelay, overshootFactor, bannerColor));
        }

        public void ShowFreeSpinsTrigger(int freeSpinsCount, Action onComplete)
        {
            _onDismissedCallback = onComplete;
            _isCounting = false;

            gameObject.SetActive(true);
            if (popupContainer != null) popupContainer.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "<size=115%><color=#FF00FF>FREE SPINS BONUS!</color></size>";
            }

            if (winAmountText != null)
            {
                winAmountText.text = $"+{freeSpinsCount} SPINS";
            }

            if (subDetailText != null)
            {
                subDetailText.text = "<color=#FFAAFF>All Free Spin wins are DOUBLED (2x)!</color>";
            }

            if (popupBannerImage != null)
            {
                popupBannerImage.color = new Color(0.85f, 0.15f, 1.0f, 1.0f); // Magenta
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.FreeSpinsTrigger);
            }

            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(AnimateSimpleBanner(2.8f, 2.5f));
        }

        public void HandleUserClick()
        {
            if (_isCounting)
            {
                // First click skips tally and fast-forwards directly to full amount
                _isCounting = false;
                if (winAmountText != null)
                {
                    winAmountText.text = $"+{_currentTargetAmount:N0}";
                }
                _onTallyTickCallback?.Invoke(_currentTargetAmount);
            }
            else
            {
                // Second click dismisses popup
                Dismiss();
            }
        }

        public void Dismiss()
        {
            _isCounting = false;

            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            if (_currentTargetAmount > 0)
            {
                _onTallyTickCallback?.Invoke(_currentTargetAmount);
            }

            ResetVisualTransforms();
            HideImmediate();
            _onDismissedCallback?.Invoke();
            _onDismissedCallback = null;
            _onTallyTickCallback = null;
        }

        private void ResetVisualTransforms()
        {
            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.one;
            }

            if (titleText != null)
            {
                titleText.transform.localScale = _originalTitleScale;
            }

            if (winAmountText != null)
            {
                winAmountText.transform.localScale = _originalAmountScale;
            }

            if (popupBannerImage != null)
            {
                popupBannerImage.color = _originalBannerColor;
            }
        }

        private void HideImmediate()
        {
            if (popupContainer != null && popupContainer != gameObject)
            {
                popupContainer.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateWinDisplay(int targetAmount, bool isMegaWin, bool isBigWin, float countDuration, float dismissDelay, float overshoot, Color baseColor)
        {
            // 1. Banner bounce-in using EasingHelper.EaseOutBack with dynamic overshoot
            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.zero;
            }

            float enterTimer = 0f;
            float enterDuration = 0.38f;
            while (enterTimer < enterDuration)
            {
                enterTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(enterTimer / enterDuration);
                float easeScale = EasingHelper.EaseOutBack(progress, overshoot);

                if (bannerTransform != null)
                {
                    bannerTransform.localScale = Vector3.one * easeScale;
                }
                yield return null;
            }

            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.one;
            }

            // 2. Number tally count-up with dynamic text scale pulse and sound ticks
            float countTimer = 0f;
            int lastSoundCount = 0;
            int tickStep = Mathf.Max(1, targetAmount / 12);

            while (countTimer < countDuration && _isCounting)
            {
                countTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(countTimer / countDuration);
                float easeProgress = EasingHelper.EaseOutQuad(progress);
                int currentVal = Mathf.RoundToInt(Mathf.Lerp(0, targetAmount, easeProgress));

                if (winAmountText != null)
                {
                    winAmountText.text = $"+{currentVal:N0}";

                    // Scale punch on count text
                    float textBounce = 1.0f + (0.15f * Mathf.Sin(progress * Mathf.PI * 6f));
                    winAmountText.transform.localScale = _originalAmountScale * textBounce;
                }

                // Inform listeners (HUD balance and win bar) of synchronized progress
                _onTallyTickCallback?.Invoke(currentVal);

                // Title pulse for Mega/Big wins
                if (titleText != null && (isMegaWin || isBigWin))
                {
                    float titleEase = EasingHelper.EaseInOutPingPong(countTimer * 4f);
                    float titleScale = Mathf.Lerp(1.0f, isMegaWin ? 1.2f : 1.1f, titleEase);
                    titleText.transform.localScale = _originalTitleScale * titleScale;
                }

                // Banner shimmer / color pulse for Mega Win
                if (popupBannerImage != null && isMegaWin)
                {
                    float shimmer = EasingHelper.EaseInOutPingPong(countTimer * 5f);
                    popupBannerImage.color = Color.Lerp(baseColor, new Color(1f, 0.95f, 0.6f, 1f), shimmer * 0.5f);
                }

                if (currentVal - lastSoundCount >= tickStep && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.CoinsCollect, 0.45f);
                    lastSoundCount = currentVal;
                }

                yield return null;
            }

            // Mark tally as completed & snap to exact final value
            _isCounting = false;
            _onTallyTickCallback?.Invoke(targetAmount);

            if (winAmountText != null)
            {
                winAmountText.text = $"+{targetAmount:N0}";
                winAmountText.transform.localScale = _originalAmountScale * (isMegaWin ? 1.25f : 1.12f);
            }

            if (titleText != null)
            {
                titleText.transform.localScale = _originalTitleScale * (isMegaWin ? 1.15f : 1.05f);
            }

            // 3. Celebration hold phase with subtle idle pulse
            float holdTimer = 0f;
            while (holdTimer < dismissDelay)
            {
                holdTimer += Time.deltaTime;
                if (isMegaWin && bannerTransform != null)
                {
                    float idlePulse = 1.0f + (0.04f * Mathf.Sin(holdTimer * 6f));
                    bannerTransform.localScale = Vector3.one * idlePulse;
                }
                yield return null;
            }

            Dismiss();
        }

        private IEnumerator AnimateSimpleBanner(float dismissDelay, float overshoot)
        {
            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.zero;
            }

            float enterTimer = 0f;
            float enterDuration = 0.38f;
            while (enterTimer < enterDuration)
            {
                enterTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(enterTimer / enterDuration);
                if (bannerTransform != null)
                {
                    bannerTransform.localScale = Vector3.one * EasingHelper.EaseOutBack(progress, overshoot);
                }
                yield return null;
            }

            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(dismissDelay);
            Dismiss();
        }
    }
}
