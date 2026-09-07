using System;
using Underpin.SlotGame.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Controller for the Royal Vegas Slots Main Menu modal panel and its navigation buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu Containers")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private CanvasGroup menuCanvasGroup;

        [Header("Navigation Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button paytableButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button closeButton;

        [Header("Sub-Dialogs (Optional)")]
        [SerializeField] private GameObject creditsDialog;

        // Public Events
        public event Action OnPlayClicked;
        public event Action OnPaytableClicked;
        public event Action OnSettingsClicked;
        public event Action OnStatsClicked;
        public event Action OnExitClicked;

        public bool IsOpen => menuPanel != null && menuPanel.activeSelf;

        private void Awake()
        {
            RegisterButtonListeners();

            if (creditsDialog != null)
            {
                creditsDialog.SetActive(false);
            }
        }

        private void RegisterButtonListeners()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    Hide();
                    OnPlayClicked?.Invoke();
                });
            }

            if (paytableButton != null)
            {
                paytableButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    Hide();
                    OnPaytableClicked?.Invoke();
                });
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.ToggleMute();
                    }
                    OnSettingsClicked?.Invoke();
                });
            }

            if (statsButton != null)
            {
                statsButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    OnStatsClicked?.Invoke();
                });
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    if (creditsDialog != null)
                    {
                        creditsDialog.SetActive(!creditsDialog.activeSelf);
                    }
                });
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    OnExitClicked?.Invoke();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    PlayButtonAudio();
                    Hide();
                });
            }
        }

        public void Show()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 1f;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }

            PlayButtonAudio();
        }

        public void Hide()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
            }

            if (creditsDialog != null)
            {
                creditsDialog.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void PlayButtonAudio()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }
    }
}
