using System.Collections;
using Underpin.SlotGame.Audio;
using Underpin.SlotGame.Data;
using Underpin.SlotGame.Logic;
using Underpin.SlotGame.Reel;
using Underpin.SlotGame.UI;
using UnityEngine;

namespace Underpin.SlotGame.Core
{
    /// <summary>
    /// Master orchestrator for the slot machine game. Coordinates RNG outcomes,
    /// reel physics animation, win evaluation, economy credits, UI HUD, and bonus modes.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PaytableConfig paytableConfig;

        [Header("Scene References")]
        [SerializeField] private SlotReelController reelController;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Automation & Pacing")]
        [SerializeField] private float autoSpinDelay = 1.0f;
        [SerializeField] private float winCelebrationDelay = 1.5f;

        private RNGManager _rng;
        private EconomyManager _economy;
        private GameState _currentState = GameState.Idle;
        private bool _isAutoSpinActive = false;
        private Coroutine _autoSpinRoutine;
        [System.NonSerialized] private bool _isInitialized = false;

        public GameState CurrentState => _currentState;
        public EconomyManager Economy => _economy;
        public PaytableConfig Config => paytableConfig;
        public bool CanSpin => (_currentState == GameState.Idle || _currentState == GameState.FreeSpins) && _economy != null && _economy.CanAffordSpin();

        // Spin Lifecycle Events
        public event System.Action OnSpinInitiated;
        public event System.Action<WinResult> OnSpinResultsReady;
        public event System.Action<GameState> OnStateChanged;

        private void Awake()
        {
            Application.runInBackground = true;
            InitializeGame();
        }

        private void Start()
        {
            if (!_isInitialized || _economy == null)
            {
                InitializeGame();
            }
        }

        public void InitializeGame()
        {
            if (_isInitialized && _economy != null && _rng != null) return;

            if (paytableConfig == null)
            {
                Debug.LogError("[SlotMachineController] PaytableConfig is missing!");
                return;
            }

            paytableConfig.InitializeDefaultPaylines();

            // Initialize RNG & Economy
            _rng = new RNGManager();
            _economy = new EconomyManager(paytableConfig);
            _isInitialized = true;

            // Bind UI Events
            if (uiManager != null)
            {
                uiManager.OnSpinRequested += HandleSpinRequested;
                uiManager.OnAutoSpinToggled += HandleAutoSpinToggled;
                uiManager.OnIncreaseBetRequested += () => _economy.IncreaseBet();
                uiManager.OnDecreaseBetRequested += () => _economy.DecreaseBet();
                uiManager.OnMaxBetRequested += () => _economy.SetMaxBet();

                // Economy Bindings
                _economy.OnBalanceChanged += uiManager.UpdateBalance;
                _economy.OnBetChanged += uiManager.UpdateBet;
                _economy.OnFreeSpinsChanged += uiManager.UpdateFreeSpinsDisplay;

                if (uiManager.PaytablePopup != null)
                {
                    uiManager.PaytablePopup.Initialize(paytableConfig);
                }
            }

            // Initialize Reels
            if (reelController != null)
            {
                reelController.Initialize(paytableConfig, _rng);
                reelController.OnReelClickTick += HandleReelClickTick;
                reelController.OnAnticipationStarted += HandleAnticipationStarted;
            }

            // Start Economy initial notification
            _economy.Initialize();
            SetState(GameState.Idle);
        }

        private void OnDestroy()
        {
            if (reelController != null)
            {
                reelController.OnReelClickTick -= HandleReelClickTick;
                reelController.OnAnticipationStarted -= HandleAnticipationStarted;
            }
        }

        private void HandleReelClickTick(int reelIdx)
        {
            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.ReelTick, 0.25f);
            }
        }

        private void HandleAnticipationStarted(int reelIdx)
        {
            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.Anticipation, 0.9f);
            }

            if (uiManager != null)
            {
                uiManager.SetStatusMessage("<color=#FFD700>BONUS CHANCE! SCATTER ANTICIPATION...</color>");
            }
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            UpdateControlsForState();
            OnStateChanged?.Invoke(newState);
        }

        private void UpdateControlsForState()
        {
            if (uiManager == null) return;

            switch (_currentState)
            {
                case GameState.Idle:
                    uiManager.SetSpinInteractable(_economy.CanAffordSpin());
                    uiManager.SetBetControlsInteractable(!_economy.IsInFreeSpins);
                    uiManager.SetStatusMessage("Click HANDLE or SPIN to play!");
                    break;

                case GameState.Spinning:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("Spinning...");
                    break;

                case GameState.Evaluating:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("Evaluating...");
                    break;

                case GameState.WinCelebration:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("WINNER!");
                    break;

                case GameState.FreeSpins:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("FREE SPINS BONUS ACTIVE (2X)!");
                    break;

                case GameState.OutOfFunds:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(true);
                    uiManager.SetStatusMessage("Out of credits! Adjust bet or reset balance.");
                    StopAutoSpin();
                    break;
            }
        }

        public void RequestSpin()
        {
            if (!_isInitialized || _economy == null || _rng == null)
            {
                InitializeGame();
            }

            if (_currentState != GameState.Idle && _currentState != GameState.FreeSpins)
            {
                return;
            }

            if (!_economy.CanAffordSpin())
            {
                SetState(GameState.OutOfFunds);
                return;
            }

            // Deduct bet credits
            _economy.DeductBet();

            if (_economy.IsInFreeSpins)
            {
                SetState(GameState.FreeSpins);
                _economy.ConsumeFreeSpin();
            }
            else
            {
                SetState(GameState.Spinning);
            }

            OnSpinInitiated?.Invoke();

            // Reset UI win display for new spin
            if (uiManager != null) uiManager.UpdateWin(0);

            // Determine active multiplier
            float activeMultiplier = _economy.IsInFreeSpins ? paytableConfig.FreeSpinsWinMultiplier : 1.0f;

            // Generate outcome grid via RNG
            int reelCount = reelController != null ? reelController.ReelCount : 3;
            int rowCount = 3;
            SymbolData[,] targetGrid = _rng.GenerateGrid(reelCount, rowCount, paytableConfig.Symbols);

            // Execute reel animation
            if (reelController != null)
            {
                reelController.SpinReels(targetGrid, () => HandleReelSpinComplete(targetGrid, activeMultiplier));
            }
        }

        private void HandleReelSpinComplete(SymbolData[,] grid, float activeMultiplier)
        {
            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.ReelStop);
            }

            SetState(GameState.Evaluating);

            // Evaluate grid results
            WinResult result = WinEvaluator.EvaluateGrid(grid, paytableConfig, _economy.CurrentBet, activeMultiplier);
            OnSpinResultsReady?.Invoke(result);

            StartCoroutine(ProcessSpinResult(result));
        }

        private IEnumerator ProcessSpinResult(WinResult result)
        {
            if (result.HasAnyWin)
            {
                SetState(GameState.WinCelebration);

                int preWinBalance = _economy.Balance;
                int winAmount = result.TotalWinAmount;

                // Highlight winning symbols on grid
                if (reelController != null)
                {
                    var winningCoords = result.GetAllWinningCoordinates();
                    Color glow = result.WinningPaylines.Count > 0 
                        ? result.WinningPaylines[0].MatchedSymbol.HighlightColor 
                        : (result.IsFreeSpinsTriggered ? new Color(1f, 0.85f, 0.1f, 1f) : Color.yellow);
                    reelController.HighlightWinningSymbols(winningCoords, glow);
                }

                // Sound & Win celebration
                if (audioManager != null)
                {
                    if (result.IsMegaWin)
                    {
                        audioManager.PlaySound(SoundType.WinMega);
                    }
                    else if (result.IsBigWin)
                    {
                        audioManager.PlaySound(SoundType.WinBig);
                    }
                    else if (result.TotalWinAmount > 0)
                    {
                        audioManager.PlaySound(SoundType.WinSmall);
                    }
                }

                // Trigger Free Spins Bonus Popup if triggered
                if (result.IsFreeSpinsTriggered)
                {
                    _economy.AwardFreeSpins(result.FreeSpinsAwarded);
                    _economy.AddPayout(winAmount);
                    if (uiManager != null) uiManager.UpdateWin(winAmount);

                    bool popupDone = false;
                    if (uiManager != null && uiManager.WinPopup != null)
                    {
                        uiManager.WinPopup.ShowFreeSpinsTrigger(result.FreeSpinsAwarded, () => popupDone = true);
                        while (!popupDone) yield return null;
                    }
                }
                else if (result.IsBigWin || result.IsMegaWin)
                {
                    bool popupDone = false;
                    if (uiManager != null && uiManager.WinPopup != null)
                    {
                        uiManager.WinPopup.ShowWin(winAmount, result.IsMegaWin, result.IsBigWin, 
                            () => popupDone = true,
                            (tallyVal) =>
                            {
                                if (uiManager != null)
                                {
                                    uiManager.UpdateWin(tallyVal);
                                    uiManager.UpdateBalance(preWinBalance + tallyVal);
                                }
                            });

                        while (!popupDone) yield return null;
                    }

                    _economy.AddPayout(winAmount);
                    if (uiManager != null)
                    {
                        uiManager.UpdateWin(winAmount);
                        uiManager.UpdateBalance(_economy.Balance);
                    }
                }
                else
                {
                    _economy.AddPayout(winAmount);
                    if (uiManager != null) uiManager.UpdateWin(winAmount);
                    yield return new WaitForSeconds(winCelebrationDelay);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }

            // Post-spin continuation
            if (_economy.IsInFreeSpins)
            {
                SetState(GameState.FreeSpins);
                yield return new WaitForSeconds(autoSpinDelay);
                RequestSpin();
            }
            else if (_isAutoSpinActive && _economy.CanAffordSpin())
            {
                SetState(GameState.Idle);
                yield return new WaitForSeconds(autoSpinDelay);
                if (_isAutoSpinActive) RequestSpin();
            }
            else
            {
                SetState(_economy.CanAffordSpin() ? GameState.Idle : GameState.OutOfFunds);
            }
        }

        private void HandleSpinRequested()
        {
            RequestSpin();
        }

        private void HandleAutoSpinToggled()
        {
            _isAutoSpinActive = !_isAutoSpinActive;
            if (uiManager != null)
            {
                uiManager.UpdateAutoSpinVisual(_isAutoSpinActive);
            }

            if (_isAutoSpinActive && _currentState == GameState.Idle)
            {
                RequestSpin();
            }
        }

        public void StopAutoSpin()
        {
            _isAutoSpinActive = false;
            if (uiManager != null)
            {
                uiManager.UpdateAutoSpinVisual(false);
            }
        }
    }
}
