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
        [SerializeField] private float countUpDuration = 1.2f;
        [SerializeField] private float autoDismissDelay = 2.5f;

        private Action _onDismissedCallback;
        private Coroutine _displayRoutine;

        private void Awake()
        {
            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(Dismiss);
            }

            HideImmediate();
        }

        public void ShowWin(int winAmount, bool isMegaWin, bool isBigWin, Action onComplete)
        {
            _onDismissedCallback = onComplete;
            gameObject.SetActive(true);
            if (popupContainer != null) popupContainer.SetActive(true);

            if (titleText != null)
            {
                if (isMegaWin)
                    titleText.text = "<color=#FFD700>MEGA WIN!</color>";
                else if (isBigWin)
                    titleText.text = "<color=#FFA500>BIG WIN!</color>";
                else
                    titleText.text = "<color=#00FFFF>WINNER!</color>";
            }

            if (subDetailText != null)
            {
                subDetailText.text = "Tap anywhere to collect";
            }

            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(AnimateWinDisplay(winAmount));
        }

        public void ShowFreeSpinsTrigger(int freeSpinsCount, Action onComplete)
        {
            _onDismissedCallback = onComplete;
            gameObject.SetActive(true);
            if (popupContainer != null) popupContainer.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "<color=#FF00FF>FREE SPINS BONUS!</color>";
            }

            if (winAmountText != null)
            {
                winAmountText.text = $"+{freeSpinsCount} SPINS";
            }

            if (subDetailText != null)
            {
                subDetailText.text = "All Free Spin wins are DOUBLED (2x)!";
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.FreeSpinsTrigger);
            }

            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(AnimateSimpleBanner());
        }

        public void Dismiss()
        {
            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            HideImmediate();
            _onDismissedCallback?.Invoke();
            _onDismissedCallback = null;
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

        private IEnumerator AnimateWinDisplay(int targetAmount)
        {
            // Banner bounce-in
            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.zero;
            }

            float enterTimer = 0f;
            float enterDuration = 0.35f;
            while (enterTimer < enterDuration)
            {
                enterTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(enterTimer / enterDuration);
                if (bannerTransform != null)
                {
                    bannerTransform.localScale = Vector3.one * EasingHelper.EaseOutBack(progress);
                }
                yield return null;
            }

            // Number tally count-up
            float countTimer = 0f;
            int lastSoundCount = 0;
            while (countTimer < countUpDuration)
            {
                countTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(countTimer / countUpDuration);
                int currentVal = Mathf.RoundToInt(Mathf.Lerp(0, targetAmount, progress));

                if (winAmountText != null)
                {
                    winAmountText.text = $"+{currentVal:N0}";
                }

                if (currentVal - lastSoundCount > targetAmount / 10 && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.CoinsCollect, 0.4f);
                    lastSoundCount = currentVal;
                }

                yield return null;
            }

            if (winAmountText != null)
            {
                winAmountText.text = $"+{targetAmount:N0}";
            }

            yield return new WaitForSeconds(autoDismissDelay);
            Dismiss();
        }

        private IEnumerator AnimateSimpleBanner()
        {
            if (bannerTransform != null)
            {
                bannerTransform.localScale = Vector3.zero;
            }

            float enterTimer = 0f;
            float enterDuration = 0.35f;
            while (enterTimer < enterDuration)
            {
                enterTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(enterTimer / enterDuration);
                if (bannerTransform != null)
                {
                    bannerTransform.localScale = Vector3.one * EasingHelper.EaseOutBack(progress);
                }
                yield return null;
            }

            yield return new WaitForSeconds(autoDismissDelay);
            Dismiss();
        }
    }
}
