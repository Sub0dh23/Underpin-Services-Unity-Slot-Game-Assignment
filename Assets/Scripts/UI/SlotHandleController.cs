using System;
using System.Collections;
using Underpin.SlotGame.Audio;
using Underpin.SlotGame.Core;
using Underpin.SlotGame.Logic;
using Underpin.SlotGame.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Controls the physical/visual slot machine lever handle.
    /// Handles user clicks to initiate spins, drives pull-down Mecanim animation
    /// while reels are spinning, and releases the lever back up when spin results are revealed.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlotHandleController : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Scene References")]
        [Tooltip("Master slot machine orchestrator.")]
        [SerializeField] private SlotMachineController slotMachineController;

        [Tooltip("Animator playing Handle_Idle, Handle_PullDown, Handle_HoldDown, Handle_ReleaseUp.")]
        [SerializeField] private Animator animator;

        [Tooltip("Handle Arm RectTransform (metallic stem).")]
        [SerializeField] private RectTransform handleArmRect;

        [Tooltip("Handle Knob RectTransform (red round sphere).")]
        [SerializeField] private RectTransform handleKnobRect;

        [Tooltip("Handle Knob Image component for hover tint/feedback.")]
        [SerializeField] private Image handleKnobImage;

        [Header("Juice & Hover Settings")]
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1.15f, 1f);
        [SerializeField] private Color normalColor = Color.white;

        // Animator Parameter Hashes
        private static readonly int IsPulledHash = Animator.StringToHash("IsPulled");
        private static readonly int PullHash = Animator.StringToHash("Pull");
        private static readonly int ReleaseHash = Animator.StringToHash("Release");

        private bool _isPulled = false;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private Coroutine _fallbackAnimationRoutine;

        // Default positions for fallback
        private const float DefaultKnobY = 115f;
        private const float PulledKnobY = -78f;
        private const float DefaultArmScaleY = 1.0f;
        private const float PulledArmScaleY = 0.11f;

        public bool IsPulled => _isPulled;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (handleArmRect == null)
            {
                var armTransform = transform.Find("HandleArm");
                if (armTransform != null) handleArmRect = armTransform.GetComponent<RectTransform>();
            }

            if (handleKnobRect == null)
            {
                var knobTransform = transform.Find("HandleKnob");
                if (knobTransform != null)
                {
                    handleKnobRect = knobTransform.GetComponent<RectTransform>();
                    handleKnobImage = knobTransform.GetComponent<Image>();
                }
            }

            if (slotMachineController == null)
            {
                slotMachineController = UnityEngine.Object.FindFirstObjectByType<SlotMachineController>();
            }
        }

        private void Start()
        {
            BindControllerEvents();
            EnsureRaycastTargets();
        }

        private void OnEnable()
        {
            BindControllerEvents();
        }

        private void OnDisable()
        {
            UnbindControllerEvents();
        }

        private void BindControllerEvents()
        {
            if (slotMachineController == null)
            {
                slotMachineController = UnityEngine.Object.FindFirstObjectByType<SlotMachineController>();
            }

            if (slotMachineController != null)
            {
                slotMachineController.OnSpinInitiated -= HandleSpinInitiated;
                slotMachineController.OnSpinInitiated += HandleSpinInitiated;

                slotMachineController.OnSpinResultsReady -= HandleSpinResultsReady;
                slotMachineController.OnSpinResultsReady += HandleSpinResultsReady;

                slotMachineController.OnStateChanged -= HandleStateChanged;
                slotMachineController.OnStateChanged += HandleStateChanged;
            }
        }

        private void UnbindControllerEvents()
        {
            if (slotMachineController != null)
            {
                slotMachineController.OnSpinInitiated -= HandleSpinInitiated;
                slotMachineController.OnSpinResultsReady -= HandleSpinResultsReady;
                slotMachineController.OnStateChanged -= HandleStateChanged;
            }
        }

        /// <summary>
        /// Ensures all graphic elements of the handle allow raycasting so clicks are responsive.
        /// </summary>
        private void EnsureRaycastTargets()
        {
            var parentImg = GetComponent<Image>();
            if (parentImg == null)
            {
                parentImg = gameObject.AddComponent<Image>();
                parentImg.color = Color.clear;
            }
            parentImg.raycastTarget = true;

            if (handleArmRect != null)
            {
                var armImg = handleArmRect.GetComponent<Image>();
                if (armImg != null) armImg.raycastTarget = true;
            }

            if (handleKnobRect != null)
            {
                var knobImg = handleKnobRect.GetComponent<Image>();
                if (knobImg != null) knobImg.raycastTarget = true;
            }
        }

        #region Pointer Interaction Callbacks

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            TryTriggerSpin();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isPressed = true;
            UpdateVisualFeedback();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            UpdateVisualFeedback();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisualFeedback();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            UpdateVisualFeedback();
        }

        private void UpdateVisualFeedback()
        {
            if (_isPulled) return;

            if (handleKnobImage != null)
            {
                handleKnobImage.color = _isHovered ? hoverColor : normalColor;
            }

            if (handleKnobRect != null && (animator == null || !animator.isActiveAndEnabled))
            {
                float targetScale = _isPressed ? 0.95f : (_isHovered ? hoverScale : 1.0f);
                handleKnobRect.localScale = new Vector3(targetScale, targetScale, 1f);
            }
        }

        #endregion

        #region Spin & Animation Triggers

        /// <summary>
        /// Initiates the handle pull sequence and spins the slot machine.
        /// </summary>
        public void TryTriggerSpin()
        {
            if (slotMachineController == null)
            {
                slotMachineController = UnityEngine.Object.FindFirstObjectByType<SlotMachineController>();
                if (slotMachineController == null) return;
            }

            // Verify machine state can accept a new spin
            if (!slotMachineController.CanSpin)
            {
                // Can't spin right now (already spinning, out of funds, or in win celebration)
                if (slotMachineController.Economy != null && !slotMachineController.Economy.CanAffordSpin())
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound(SoundType.ButtonClick, 0.5f);
                    }
                }
                return;
            }

            // Play handle pull sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.SpinStart, 1.0f);
            }

            // Animate handle down
            PlayPullDownAnimation();

            // Request spin on master controller
            slotMachineController.RequestSpin();
        }

        /// <summary>
        /// Triggered whenever a spin begins (either by handle click or HUD buttons).
        /// </summary>
        private void HandleSpinInitiated()
        {
            if (!_isPulled)
            {
                PlayPullDownAnimation();
            }
        }

        /// <summary>
        /// Triggered when the reels come to a stop and spin results are ready to be shown.
        /// </summary>
        private void HandleSpinResultsReady(WinResult result)
        {
            PlayReleaseUpAnimation();
        }

        /// <summary>
        /// Handles state machine changes from SlotMachineController.
        /// </summary>
        private void HandleStateChanged(GameState newState)
        {
            if (newState == GameState.Idle || newState == GameState.OutOfFunds)
            {
                if (_isPulled)
                {
                    PlayReleaseUpAnimation();
                }
            }
        }

        /// <summary>
        /// Plays the pull down animation via Animator or smooth procedural fallback.
        /// </summary>
        public void PlayPullDownAnimation()
        {
            _isPulled = true;

            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(IsPulledHash, true);
                animator.SetTrigger(PullHash);
            }
            else
            {
                if (_fallbackAnimationRoutine != null) StopCoroutine(_fallbackAnimationRoutine);
                _fallbackAnimationRoutine = StartCoroutine(ProceduralPullRoutine());
            }
        }

        /// <summary>
        /// Releases the handle back up into its resting upright position.
        /// </summary>
        public void PlayReleaseUpAnimation()
        {
            _isPulled = false;

            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(IsPulledHash, false);
                animator.SetTrigger(ReleaseHash);
            }
            else
            {
                if (_fallbackAnimationRoutine != null) StopCoroutine(_fallbackAnimationRoutine);
                _fallbackAnimationRoutine = StartCoroutine(ProceduralReleaseRoutine());
            }

            UpdateVisualFeedback();
        }

        #endregion

        #region Procedural Fallback Animation

        private IEnumerator ProceduralPullRoutine()
        {
            if (handleKnobRect == null || handleArmRect == null) yield break;

            float duration = 0.35f;
            float elapsed = 0f;
            float startKnobY = handleKnobRect.anchoredPosition.y;
            float startArmY = handleArmRect.localScale.y;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Easing curve with impact overshoot
                float curveT = EasingHelper.EaseInBack(t);
                float curKnobY = Mathf.LerpUnclamped(startKnobY, PulledKnobY, curveT);
                float curArmY = Mathf.LerpUnclamped(startArmY, PulledArmScaleY, curveT);

                handleKnobRect.anchoredPosition = new Vector2(0f, curKnobY);
                handleArmRect.localScale = new Vector3(1f, Mathf.Max(0.05f, curArmY), 1f);

                yield return null;
            }

            handleKnobRect.anchoredPosition = new Vector2(0f, PulledKnobY);
            handleArmRect.localScale = new Vector3(1f, PulledArmScaleY, 1f);

            // Subtle vibration while held down
            while (_isPulled)
            {
                float shake = Mathf.Sin(Time.time * 25f) * 1.5f;
                handleKnobRect.anchoredPosition = new Vector2(0f, PulledKnobY + shake);
                yield return null;
            }
        }

        private IEnumerator ProceduralReleaseRoutine()
        {
            if (handleKnobRect == null || handleArmRect == null) yield break;

            float duration = 0.38f;
            float elapsed = 0f;
            float startKnobY = handleKnobRect.anchoredPosition.y;
            float startArmY = handleArmRect.localScale.y;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Elastic spring overshoot bounce
                float curveT = EasingHelper.EaseOutBack(t, 2.5f);
                float curKnobY = Mathf.LerpUnclamped(startKnobY, DefaultKnobY, curveT);
                float curArmY = Mathf.LerpUnclamped(startArmY, DefaultArmScaleY, curveT);

                handleKnobRect.anchoredPosition = new Vector2(0f, curKnobY);
                handleArmRect.localScale = new Vector3(1f, Mathf.Max(0.05f, curArmY), 1f);

                yield return null;
            }

            handleKnobRect.anchoredPosition = new Vector2(0f, DefaultKnobY);
            handleArmRect.localScale = new Vector3(1f, DefaultArmScaleY, 1f);
            handleKnobRect.localScale = Vector3.one;
        }

        #endregion
    }
}
