using System;
using System.Collections;
using TMPro;
using Underpin.SlotGame.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Master HUD controller managing player displays, bet selectors, spin triggers, and dialogs.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Display Texts")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI currentBetText;
        [SerializeField] private TextMeshProUGUI lastWinText;
        [SerializeField] private TextMeshProUGUI statusMessageText;

        [Header("Free Spins Indicator")]
        [SerializeField] private GameObject freeSpinsBanner;
        [SerializeField] private TextMeshProUGUI freeSpinsCountText;

        [Header("Primary Action Buttons")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button autoSpinButton;
        [SerializeField] private TextMeshProUGUI autoSpinButtonText;

        [Header("Bet Adjustment Controls")]
        [SerializeField] private Button betIncreaseButton;
        [SerializeField] private Button betDecreaseButton;
        [SerializeField] private Button maxBetButton;

        [Header("Utility Controls")]
        [SerializeField] private Button paytableButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private TextMeshProUGUI soundToggleText;
        [SerializeField] private Button mainMenuButton;

        [Header("Popups & Minigames")]
        [SerializeField] private WinPopupUI winPopup;
        [SerializeField] private PaytableUI paytablePopup;
        [SerializeField] private MainMenuUI mainMenuPopup;
        [SerializeField] private GambleUI gamblePopup;
        [SerializeField] private Button gambleHudButton;

        // Events
        public event Action OnSpinRequested;
        public event Action OnAutoSpinToggled;
        public event Action OnIncreaseBetRequested;
        public event Action OnDecreaseBetRequested;
        public event Action OnMaxBetRequested;
        public event Action OnGambleRequested;

        public WinPopupUI WinPopup => winPopup;
        public PaytableUI PaytablePopup => paytablePopup;
        public MainMenuUI MainMenuPopup => mainMenuPopup;
        public GambleUI GamblePopup => gamblePopup;

        private void Awake()
        {
            EnsureGambleUI();
            RegisterButtonCallbacks();

            if (freeSpinsBanner != null)
            {
                freeSpinsBanner.SetActive(false);
            }

            if (gambleHudButton != null)
            {
                gambleHudButton.gameObject.SetActive(false);
            }
        }

        private void RegisterButtonCallbacks()
        {
            if (spinButton != null)
            {
                spinButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.SpinStart);
                    OnSpinRequested?.Invoke();
                });
            }

            if (autoSpinButton != null)
            {
                autoSpinButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
                    OnAutoSpinToggled?.Invoke();
                });
            }

            if (betIncreaseButton != null)
            {
                betIncreaseButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.BetChange);
                    OnIncreaseBetRequested?.Invoke();
                });
            }

            if (betDecreaseButton != null)
            {
                betDecreaseButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.BetChange);
                    OnDecreaseBetRequested?.Invoke();
                });
            }

            if (maxBetButton != null)
            {
                maxBetButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.BetChange);
                    OnMaxBetRequested?.Invoke();
                });
            }

            if (paytableButton != null)
            {
                paytableButton.onClick.AddListener(() =>
                {
                    if (paytablePopup != null) paytablePopup.Toggle();
                });
            }

            if (soundToggleButton != null)
            {
                soundToggleButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.ToggleMute();
                        UpdateSoundButtonVisuals();
                    }
                });
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(() =>
                {
                    if (mainMenuPopup != null) mainMenuPopup.Toggle();
                });
            }

            if (mainMenuPopup != null)
            {
                mainMenuPopup.OnPaytableClicked += () =>
                {
                    if (paytablePopup != null) paytablePopup.Show();
                };
            }

            if (gambleHudButton != null)
            {
                gambleHudButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
                    OnGambleRequested?.Invoke();
                });
            }
        }

        private void EnsureGambleUI()
        {
            if (gamblePopup == null)
            {
                gamblePopup = GetComponentInChildren<GambleUI>(true);
            }

            if (gamblePopup == null)
            {
                GameObject gambleObj = new GameObject("GamblePopup_AutoCreated", typeof(RectTransform), typeof(GambleUI));
                gambleObj.transform.SetParent(transform, false);
                gamblePopup = gambleObj.GetComponent<GambleUI>();
                gambleObj.SetActive(false);
            }
            else
            {
                gamblePopup.gameObject.SetActive(false);
            }

            if (gambleHudButton == null)
            {
                Transform parentT = lastWinText != null ? lastWinText.transform.parent : transform;
                GameObject btnObj = new GameObject("Btn_Gamble_HUD", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(parentT, false);

                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(230, 48);
                if (lastWinText != null)
                {
                    rt.anchoredPosition = new Vector2(0, 80);
                }
                else
                {
                    rt.anchoredPosition = new Vector2(0, -180);
                }

                Image img = btnObj.GetComponent<Image>();
                img.color = new Color(0.94f, 0.58f, 0.04f, 1.0f); // Radiant Gold

                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "<color=#FFFFFF><b>GAMBLE (2X)</b></color>";
                tmp.fontSize = 17;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.raycastTarget = false;

                RectTransform txtRt = txtObj.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                gambleHudButton = btnObj.GetComponent<Button>();
                var colors = gambleHudButton.colors;
                colors.normalColor = img.color;
                colors.highlightedColor = new Color(1f, 0.85f, 0.25f, 1f);
                colors.pressedColor = new Color(0.72f, 0.42f, 0.02f, 1f);
                gambleHudButton.colors = colors;

                gambleHudButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
                    OnGambleRequested?.Invoke();
                });

                btnObj.SetActive(false);
            }
        }

        private Coroutine _gambleButtonPulseRoutine;

        public void SetGambleButtonVisible(bool visible)
        {
            if (gambleHudButton != null)
            {
                gambleHudButton.gameObject.SetActive(visible);
                if (_gambleButtonPulseRoutine != null)
                {
                    StopCoroutine(_gambleButtonPulseRoutine);
                    _gambleButtonPulseRoutine = null;
                }

                if (visible)
                {
                    _gambleButtonPulseRoutine = StartCoroutine(PulseGambleButton(gambleHudButton.transform));
                }
                else
                {
                    gambleHudButton.transform.localScale = Vector3.one;
                }
            }
        }

        private IEnumerator PulseGambleButton(Transform target)
        {
            if (target == null) yield break;
            while (target.gameObject.activeSelf)
            {
                float t = Mathf.Sin(Time.time * 6f) * 0.5f + 0.5f;
                float scale = Mathf.Lerp(1.0f, 1.08f, t);
                target.localScale = Vector3.one * scale;
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        public void ShowGamble(int winAmount, int maxRounds, Action<int> onCollect, Action onBust, Underpin.SlotGame.Logic.IRandomNumberGenerator rng = null)
        {
            EnsureGambleUI();
            if (gamblePopup != null)
            {
                gamblePopup.OpenGamble(winAmount, maxRounds, onCollect, onBust, rng);
            }
        }

        public void UpdateBalance(int balance)
        {
            if (balanceText != null)
            {
                balanceText.text = $"CREDITS: {balance:N0}";
            }
        }

        public void UpdateBet(int betAmount, bool isMin, bool isMax)
        {
            if (currentBetText != null)
            {
                currentBetText.text = $"BET: {betAmount:N0}";
            }

            if (betDecreaseButton != null) betDecreaseButton.interactable = !isMin;
            if (betIncreaseButton != null) betIncreaseButton.interactable = !isMax;
            if (maxBetButton != null) maxBetButton.interactable = !isMax;
        }

        public void UpdateWin(int winAmount)
        {
            if (lastWinText != null)
            {
                lastWinText.text = winAmount > 0 ? $"WIN: {winAmount:N0}" : "WIN: 0";
            }
        }

        public void SetStatusMessage(string message)
        {
            if (statusMessageText != null)
            {
                statusMessageText.text = message;
            }
        }

        public void UpdateFreeSpinsDisplay(int remaining, int totalWon)
        {
            bool inFreeSpins = remaining > 0;
            if (freeSpinsBanner != null)
            {
                freeSpinsBanner.SetActive(inFreeSpins);
            }

            if (freeSpinsCountText != null && inFreeSpins)
            {
                freeSpinsCountText.text = $"FREE SPINS LEFT: {remaining}  |  WON: {totalWon:N0}";
            }
        }

        public void SetSpinInteractable(bool canSpin)
        {
            if (spinButton != null)
            {
                spinButton.interactable = canSpin;
            }
        }

        public void SetBetControlsInteractable(bool interactable)
        {
            if (betIncreaseButton != null) betIncreaseButton.interactable = interactable;
            if (betDecreaseButton != null) betDecreaseButton.interactable = interactable;
            if (maxBetButton != null) maxBetButton.interactable = interactable;
        }

        public void UpdateAutoSpinVisual(bool isAutoActive)
        {
            if (autoSpinButtonText != null)
            {
                autoSpinButtonText.text = isAutoActive ? "<color=#FF5555>STOP AUTO</color>" : "AUTO SPIN";
            }
        }

        private void UpdateSoundButtonVisuals()
        {
            if (soundToggleText != null && AudioManager.Instance != null)
            {
                soundToggleText.text = AudioManager.Instance.IsMuted ? "SOUND: OFF" : "SOUND: ON";
            }
        }
    }
}
