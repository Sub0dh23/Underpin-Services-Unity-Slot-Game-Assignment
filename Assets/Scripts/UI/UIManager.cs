using System;
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

        [Header("Popups")]
        [SerializeField] private WinPopupUI winPopup;
        [SerializeField] private PaytableUI paytablePopup;
        [SerializeField] private MainMenuUI mainMenuPopup;

        // Events
        public event Action OnSpinRequested;
        public event Action OnAutoSpinToggled;
        public event Action OnIncreaseBetRequested;
        public event Action OnDecreaseBetRequested;
        public event Action OnMaxBetRequested;

        public WinPopupUI WinPopup => winPopup;
        public PaytableUI PaytablePopup => paytablePopup;
        public MainMenuUI MainMenuPopup => mainMenuPopup;

        private void Awake()
        {
            RegisterButtonCallbacks();

            if (freeSpinsBanner != null)
            {
                freeSpinsBanner.SetActive(false);
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
