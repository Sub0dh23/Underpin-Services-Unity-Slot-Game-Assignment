using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Underpin.SlotGame.Audio;
using Underpin.SlotGame.Logic;
using Underpin.SlotGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Interactive Double-or-Nothing Gamble Minigame UI.
    /// Allows players to gamble their current win with a 50/50 Red/Black (2x) or 1-in-4 Suit (4x) guess.
    /// Includes 3D card flipping animation, card history strip, and dynamic procedural layout builder.
    /// </summary>
    public class GambleUI : MonoBehaviour
    {
        [Header("Containers & Modals")]
        [SerializeField] private GameObject gamblePanel;
        [SerializeField] private RectTransform cardContainerTransform;
        [SerializeField] private Image cardBackgroundImage;

        [Header("Card Faces & Typography")]
        [SerializeField] private GameObject cardBackObject;
        [SerializeField] private GameObject cardFrontObject;
        [SerializeField] private TextMeshProUGUI cardRankTopText;
        [SerializeField] private TextMeshProUGUI cardSuitCenterText;
        [SerializeField] private TextMeshProUGUI cardRankBottomText;

        [Header("HUD & Information")]
        [SerializeField] private TextMeshProUGUI potAmountText;
        [SerializeField] private TextMeshProUGUI potentialWin2xText;
        [SerializeField] private TextMeshProUGUI potentialWin4xText;
        [SerializeField] private TextMeshProUGUI roundStatusText;
        [SerializeField] private TextMeshProUGUI instructionStatusText;
        [SerializeField] private TextMeshProUGUI historyCardsText;

        [Header("Action Buttons")]
        [SerializeField] private Button redButton;
        [SerializeField] private Button blackButton;
        [SerializeField] private Button heartsButton;
        [SerializeField] private Button diamondsButton;
        [SerializeField] private Button clubsButton;
        [SerializeField] private Button spadesButton;
        [SerializeField] private Button collectButton;
        [SerializeField] private Button closeButton;

        [Header("Pacing")]
        [SerializeField] private float flipDuration = 0.45f;
        [SerializeField] private int maxRounds = 5;

        private int _currentPot = 0;
        private int _currentRound = 1;
        private bool _isBusy = false;
        private Action<int> _onCollectCallback;
        private Action _onBustCallback;
        private readonly List<GambleCard> _cardHistory = new List<GambleCard>();

        public int CurrentPot => _currentPot;
        public bool IsActive => (gamblePanel != null && gamblePanel.activeSelf) || gameObject.activeSelf;

        private void Awake()
        {
            EnsureUIHierarchy();
            BindButtonEvents();
            HideImmediate();
        }

        private void BindButtonEvents()
        {
            if (redButton != null)
            {
                redButton.onClick.RemoveAllListeners();
                redButton.onClick.AddListener(() => OnColorChoiceClicked(CardColor.Red));
            }
            if (blackButton != null)
            {
                blackButton.onClick.RemoveAllListeners();
                blackButton.onClick.AddListener(() => OnColorChoiceClicked(CardColor.Black));
            }

            if (heartsButton != null)
            {
                heartsButton.onClick.RemoveAllListeners();
                heartsButton.onClick.AddListener(() => OnSuitChoiceClicked(CardSuit.Hearts));
            }
            if (diamondsButton != null)
            {
                diamondsButton.onClick.RemoveAllListeners();
                diamondsButton.onClick.AddListener(() => OnSuitChoiceClicked(CardSuit.Diamonds));
            }
            if (clubsButton != null)
            {
                clubsButton.onClick.RemoveAllListeners();
                clubsButton.onClick.AddListener(() => OnSuitChoiceClicked(CardSuit.Clubs));
            }
            if (spadesButton != null)
            {
                spadesButton.onClick.RemoveAllListeners();
                spadesButton.onClick.AddListener(() => OnSuitChoiceClicked(CardSuit.Spades));
            }

            if (collectButton != null)
            {
                collectButton.onClick.RemoveAllListeners();
                collectButton.onClick.AddListener(HandleCollectClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(HandleCollectClicked);
            }
        }

        public void OpenGamble(int initialWinAmount, int maxGambleRounds, Action<int> onCollect, Action onBust)
        {
            EnsureUIHierarchy();
            BindButtonEvents();

            _currentPot = initialWinAmount;
            maxRounds = Mathf.Max(1, maxGambleRounds);
            _currentRound = 1;
            _isBusy = false;
            _onCollectCallback = onCollect;
            _onBustCallback = onBust;

            gameObject.SetActive(true);
            if (gamblePanel != null) gamblePanel.SetActive(true);

            ResetCardFaceToBack();
            UpdatePotAndStatusUI();
            SetButtonsInteractable(true);

            if (instructionStatusText != null)
            {
                instructionStatusText.text = "<color=#FFD700>GUESS RED/BLACK (2X) OR SUIT (4X)!</color>";
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }

        private void OnColorChoiceClicked(CardColor chosenColor)
        {
            if (_isBusy || _currentPot <= 0) return;
            _isBusy = true;
            SetButtonsInteractable(false);

            GambleCard drawnCard = GambleCard.DrawRandom();
            bool isWin = (drawnCard.Color == chosenColor);
            int multiplier = 2;

            StartCoroutine(ExecuteGambleResolution(drawnCard, isWin, multiplier));
        }

        private void OnSuitChoiceClicked(CardSuit chosenSuit)
        {
            if (_isBusy || _currentPot <= 0) return;
            _isBusy = true;
            SetButtonsInteractable(false);

            GambleCard drawnCard = GambleCard.DrawRandom();
            bool isWin = (drawnCard.Suit == chosenSuit);
            int multiplier = 4;

            StartCoroutine(ExecuteGambleResolution(drawnCard, isWin, multiplier));
        }

        private IEnumerator ExecuteGambleResolution(GambleCard drawnCard, bool isWin, int multiplier)
        {
            if (instructionStatusText != null)
            {
                instructionStatusText.text = "<color=#00FFFF>FLIPPING CARD...</color>";
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.GambleCardFlip);
            }

            // Animate 3D Card Flip (horizontal scale)
            yield return AnimateCardFlip(drawnCard);

            // Record History
            _cardHistory.Insert(0, drawnCard);
            if (_cardHistory.Count > 6) _cardHistory.RemoveAt(_cardHistory.Count - 1);
            UpdateHistoryUI();

            if (isWin)
            {
                int wonAmount = _currentPot * multiplier;
                _currentPot = wonAmount;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.GambleWin);
                }

                if (instructionStatusText != null)
                {
                    instructionStatusText.text = $"<color=#00FF66>WINNER! +{wonAmount:N0} CREDITS ({multiplier}X)</color>";
                }

                UpdatePotAndStatusUI();

                if (potAmountText != null)
                {
                    StartCoroutine(PunchScale(potAmountText.transform, 1.25f, 0.3f));
                }

                yield return new WaitForSeconds(1.0f);

                _currentRound++;
                if (_currentRound > maxRounds)
                {
                    if (instructionStatusText != null)
                    {
                        instructionStatusText.text = "<color=#FFD700>MAX GAMBLE ROUNDS REACHED! COLLECTING...</color>";
                    }
                    yield return new WaitForSeconds(1.2f);
                    HandleCollectClicked();
                }
                else
                {
                    ResetCardFaceToBack();
                    UpdatePotAndStatusUI();
                    _isBusy = false;
                    SetButtonsInteractable(true);
                    if (instructionStatusText != null)
                    {
                        instructionStatusText.text = "<color=#FFD700>DOUBLE AGAIN OR COLLECT WIN?</color>";
                    }
                }
            }
            else
            {
                // BUST
                _currentPot = 0;
                UpdatePotAndStatusUI();

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.GambleLose);
                }

                if (instructionStatusText != null)
                {
                    instructionStatusText.text = "<color=#FF3B30>DEALER WINS - BETTER LUCK NEXT TIME!</color>";
                }

                yield return new WaitForSeconds(1.6f);
                CloseOnBust();
            }
        }

        private IEnumerator AnimateCardFlip(GambleCard drawnCard)
        {
            if (cardContainerTransform == null) yield break;

            float halfDuration = flipDuration * 0.5f;
            float timer = 0f;

            // Half 1: Rotate / Scale down horizontally (1 -> 0)
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / halfDuration);
                float scaleX = Mathf.Lerp(1f, 0f, EasingHelper.EaseInQuad(t));
                cardContainerTransform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }

            // Swap card visual to front face
            ApplyCardFace(drawnCard);

            // Half 2: Scale up horizontally (0 -> 1)
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / halfDuration);
                float scaleX = Mathf.Lerp(0f, 1f, EasingHelper.EaseOutBack(t, 1.4f));
                cardContainerTransform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }

            cardContainerTransform.localScale = Vector3.one;
        }

        private void ResetCardFaceToBack()
        {
            if (cardBackObject != null) cardBackObject.SetActive(true);
            if (cardFrontObject != null) cardFrontObject.SetActive(false);
            if (cardContainerTransform != null) cardContainerTransform.localScale = Vector3.one;
        }

        private void ApplyCardFace(GambleCard card)
        {
            if (cardBackObject != null) cardBackObject.SetActive(false);
            if (cardFrontObject != null) cardFrontObject.SetActive(true);

            string colorHex = card.ColorHex;
            string rankStr = card.RankString;
            string suitSym = card.SuitSymbol;

            if (cardRankTopText != null)
            {
                cardRankTopText.text = $"<color={colorHex}>{rankStr}\n{suitSym}</color>";
            }

            if (cardSuitCenterText != null)
            {
                cardSuitCenterText.text = $"<size=150%><color={colorHex}>{suitSym}</color></size>";
            }

            if (cardRankBottomText != null)
            {
                cardRankBottomText.text = $"<color={colorHex}>{rankStr}\n{suitSym}</color>";
            }
        }

        private void UpdatePotAndStatusUI()
        {
            if (potAmountText != null)
            {
                potAmountText.text = $"GAMBLE POT: <color=#FFD700>{_currentPot:N0}</color>";
            }

            if (potentialWin2xText != null)
            {
                potentialWin2xText.text = $"2X RED/BLACK: <color=#00FF66>{(_currentPot * 2):N0}</color>";
            }

            if (potentialWin4xText != null)
            {
                potentialWin4xText.text = $"4X SUIT: <color=#FFAA00>{(_currentPot * 4):N0}</color>";
            }

            if (roundStatusText != null)
            {
                roundStatusText.text = $"ROUND {_currentRound} / {maxRounds}";
            }

            if (collectButton != null)
            {
                var collectTxt = collectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (collectTxt != null)
                {
                    collectTxt.text = $"COLLECT <color=#FFD700>+{_currentPot:N0}</color>";
                }
            }
        }

        private void UpdateHistoryUI()
        {
            if (historyCardsText == null) return;

            if (_cardHistory.Count == 0)
            {
                historyCardsText.text = "HISTORY: <color=#888888>--</color>";
                return;
            }

            string hist = "HISTORY: ";
            for (int i = 0; i < _cardHistory.Count; i++)
            {
                hist += $"[{_cardHistory[i].FormattedBadge}] ";
            }
            historyCardsText.text = hist.Trim();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (redButton != null) redButton.interactable = interactable;
            if (blackButton != null) blackButton.interactable = interactable;
            if (heartsButton != null) heartsButton.interactable = interactable;
            if (diamondsButton != null) diamondsButton.interactable = interactable;
            if (clubsButton != null) clubsButton.interactable = interactable;
            if (spadesButton != null) spadesButton.interactable = interactable;
            if (collectButton != null) collectButton.interactable = interactable && _currentPot > 0;
            if (closeButton != null) closeButton.interactable = interactable;
        }

        public void HandleCollectClicked()
        {
            if (_isBusy && _currentPot > 0 && _currentRound <= maxRounds) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.CoinsCollect);
            }

            int finalCollect = _currentPot;
            HideImmediate();
            _onCollectCallback?.Invoke(finalCollect);
            _onCollectCallback = null;
            _onBustCallback = null;
        }

        private void CloseOnBust()
        {
            HideImmediate();
            _onBustCallback?.Invoke();
            _onCollectCallback = null;
            _onBustCallback = null;
        }

        public void HideImmediate()
        {
            _isBusy = false;
            if (gamblePanel != null && gamblePanel != gameObject)
            {
                gamblePanel.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        private IEnumerator PunchScale(Transform target, float punchMultiplier, float duration)
        {
            if (target == null) yield break;
            Vector3 originalScale = target.localScale;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float ease = Mathf.Sin(progress * Mathf.PI);
                target.localScale = originalScale * (1f + (punchMultiplier - 1f) * ease);
                yield return null;
            }
            target.localScale = originalScale;
        }

        /// <summary>
        /// Ensures all essential Canvas UI nodes exist dynamically with styled casino layout.
        /// </summary>
        private void EnsureUIHierarchy()
        {
            if (gamblePanel != null && cardBackObject != null && redButton != null && collectButton != null)
            {
                return;
            }

            // Root overlay panel
            gamblePanel = gameObject;
            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image bgOverlay = GetComponent<Image>();
            if (bgOverlay == null) bgOverlay = gameObject.AddComponent<Image>();
            bgOverlay.color = new Color(0.02f, 0.04f, 0.08f, 0.92f);

            // Modal Card Panel Window
            Transform existingModal = transform.Find("GambleModalWindow");
            GameObject modalObj = existingModal != null ? existingModal.gameObject : new GameObject("GambleModalWindow");
            modalObj.transform.SetParent(transform, false);

            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            if (modalRect == null) modalRect = modalObj.AddComponent<RectTransform>();
            modalRect.sizeDelta = new Vector2(760, 530);
            modalRect.anchoredPosition = Vector2.zero;

            Image modalBg = modalObj.GetComponent<Image>();
            if (modalBg == null) modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

            // Title Header
            GameObject headerObj = GetOrCreateChild(modalObj.transform, "TitleHeader", new Vector2(0, 225), new Vector2(680, 45));
            GetOrCreateTMP(headerObj, "<size=120%><color=#FFD700>DOUBLE OR NOTHING</color></size>", 26, TextAlignmentOptions.Center);

            // Sub Header / Status
            GameObject statusObj = GetOrCreateChild(modalObj.transform, "StatusHeader", new Vector2(0, 185), new Vector2(680, 32));
            instructionStatusText = GetOrCreateTMP(statusObj, "GUESS RED/BLACK (2X) OR SUIT (4X)!", 17, TextAlignmentOptions.Center);

            // Pot & Round Row
            GameObject potObj = GetOrCreateChild(modalObj.transform, "PotRow", new Vector2(-160, 140), new Vector2(320, 36));
            potAmountText = GetOrCreateTMP(potObj, "GAMBLE POT: <color=#FFD700>0</color>", 20, TextAlignmentOptions.Left);

            GameObject roundObj = GetOrCreateChild(modalObj.transform, "RoundRow", new Vector2(160, 140), new Vector2(300, 36));
            roundStatusText = GetOrCreateTMP(roundObj, "ROUND 1 / 5", 18, TextAlignmentOptions.Right);

            // Center Playing Card Deck View
            GameObject cardObj = GetOrCreateChild(modalObj.transform, "CardContainer", new Vector2(0, 25), new Vector2(140, 195));
            cardContainerTransform = cardObj.GetComponent<RectTransform>();

            // Card Back (Facedown)
            GameObject cBack = GetOrCreateChild(cardObj.transform, "CardBack", Vector2.zero, new Vector2(140, 195));
            cardBackObject = cBack;
            Image cBackImg = cBack.GetComponent<Image>();
            if (cBackImg == null) cBackImg = cBack.AddComponent<Image>();
            cBackImg.color = new Color(0.72f, 0.12f, 0.16f, 1f); // Casino Crimson

            GameObject cBackInner = GetOrCreateChild(cBack.transform, "InnerBorder", Vector2.zero, new Vector2(120, 175));
            Image cInnerImg = cBackInner.GetComponent<Image>();
            if (cInnerImg == null) cInnerImg = cBackInner.AddComponent<Image>();
            cInnerImg.color = new Color(0.95f, 0.75f, 0.2f, 0.4f);

            GameObject cBackTextChild = GetOrCreateChild(cBackInner.transform, "TextLabel", Vector2.zero, new Vector2(120, 175));
            GetOrCreateTMP(cBackTextChild, "SLOTS", 24, TextAlignmentOptions.Center);

            // Card Front (Faceup)
            GameObject cFront = GetOrCreateChild(cardObj.transform, "CardFront", Vector2.zero, new Vector2(140, 195));
            cardFrontObject = cFront;
            Image cFrontImg = cFront.GetComponent<Image>();
            if (cFrontImg == null) cFrontImg = cFront.AddComponent<Image>();
            cFrontImg.color = Color.white;

            GameObject topCorner = GetOrCreateChild(cFront.transform, "TopCorner", new Vector2(-45, 68), new Vector2(40, 45));
            cardRankTopText = GetOrCreateTMP(topCorner, "A\n♥", 16, TextAlignmentOptions.Center);

            GameObject centerSuit = GetOrCreateChild(cFront.transform, "CenterSuit", Vector2.zero, new Vector2(70, 70));
            cardSuitCenterText = GetOrCreateTMP(centerSuit, "♥", 42, TextAlignmentOptions.Center);

            GameObject btmCorner = GetOrCreateChild(cFront.transform, "BtmCorner", new Vector2(45, -68), new Vector2(40, 45));
            cardRankBottomText = GetOrCreateTMP(btmCorner, "A\n♥", 16, TextAlignmentOptions.Center);

            // Red Button (Left)
            GameObject redBtnObj = GetOrCreateChild(modalObj.transform, "Btn_Red", new Vector2(-190, 45), new Vector2(165, 70));
            redButton = SetupButton(redBtnObj, new Color(0.85f, 0.15f, 0.15f, 1f), "<color=#FFFFFF><b>RED (2X)</b>\n<size=75%>HEARTS / DIAMONDS</size></color>");

            // Black Button (Right)
            GameObject blackBtnObj = GetOrCreateChild(modalObj.transform, "Btn_Black", new Vector2(190, 45), new Vector2(165, 70));
            blackButton = SetupButton(blackBtnObj, new Color(0.18f, 0.20f, 0.26f, 1f), "<color=#FFFFFF><b>BLACK (2X)</b>\n<size=75%>SPADES / CLUBS</size></color>");

            // Suit Choice Buttons (4X Row)
            GameObject suitHObj = GetOrCreateChild(modalObj.transform, "Btn_Hearts", new Vector2(-225, -45), new Vector2(80, 50));
            heartsButton = SetupButton(suitHObj, new Color(0.85f, 0.15f, 0.15f, 1f), "<color=#FFFFFF><b>HEART</b>\n<size=70%>4X</size></color>");

            GameObject suitDObj = GetOrCreateChild(modalObj.transform, "Btn_Diamonds", new Vector2(-135, -45), new Vector2(80, 50));
            diamondsButton = SetupButton(suitDObj, new Color(0.85f, 0.15f, 0.15f, 1f), "<color=#FFFFFF><b>DIAMOND</b>\n<size=70%>4X</size></color>");

            GameObject suitCObj = GetOrCreateChild(modalObj.transform, "Btn_Clubs", new Vector2(135, -45), new Vector2(80, 50));
            clubsButton = SetupButton(suitCObj, new Color(0.18f, 0.20f, 0.26f, 1f), "<color=#FFFFFF><b>CLUB</b>\n<size=70%>4X</size></color>");

            GameObject suitSObj = GetOrCreateChild(modalObj.transform, "Btn_Spades", new Vector2(225, -45), new Vector2(80, 50));
            spadesButton = SetupButton(suitSObj, new Color(0.18f, 0.20f, 0.26f, 1f), "<color=#FFFFFF><b>SPADE</b>\n<size=70%>4X</size></color>");

            // Potential Multipliers Display Info
            GameObject pot2xObj = GetOrCreateChild(modalObj.transform, "Pot2xInfo", new Vector2(-190, -100), new Vector2(200, 26));
            potentialWin2xText = GetOrCreateTMP(pot2xObj, "2X RED/BLACK: 0", 14, TextAlignmentOptions.Center);

            GameObject pot4xObj = GetOrCreateChild(modalObj.transform, "Pot4xInfo", new Vector2(190, -100), new Vector2(200, 26));
            potentialWin4xText = GetOrCreateTMP(pot4xObj, "4X SUIT: 0", 14, TextAlignmentOptions.Center);

            // History Bar
            GameObject histObj = GetOrCreateChild(modalObj.transform, "HistoryRow", new Vector2(0, -145), new Vector2(620, 28));
            historyCardsText = GetOrCreateTMP(histObj, "HISTORY: --", 15, TextAlignmentOptions.Center);

            // Bottom Collect Button
            GameObject collectObj = GetOrCreateChild(modalObj.transform, "Btn_Collect", new Vector2(0, -205), new Vector2(320, 55));
            collectButton = SetupButton(collectObj, new Color(0.12f, 0.68f, 0.32f, 1f), "<size=115%><b>COLLECT WIN</b></size>");
        }

        private GameObject GetOrCreateChild(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            Transform t = parent.Find(name);
            GameObject obj = t != null ? t.gameObject : new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return obj;
        }

        private TextMeshProUGUI GetOrCreateTMP(GameObject target, string initialText, float fontSize, TextAlignmentOptions alignment)
        {
            if (target == null) return null;

            TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                // If target already has an Image/Graphic, instantiate a dedicated text child
                if (target.GetComponent<Graphic>() != null)
                {
                    Transform existingChild = target.transform.Find("TextLabel");
                    GameObject textChild = existingChild != null ? existingChild.gameObject : new GameObject("TextLabel");
                    textChild.transform.SetParent(target.transform, false);

                    RectTransform rt = textChild.GetComponent<RectTransform>();
                    if (rt == null) rt = textChild.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    tmp = textChild.GetComponent<TextMeshProUGUI>();
                    if (tmp == null) tmp = textChild.AddComponent<TextMeshProUGUI>();
                }
                else
                {
                    tmp = target.AddComponent<TextMeshProUGUI>();
                }
            }

            tmp.text = initialText;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Button SetupButton(GameObject target, Color baseColor, string labelText)
        {
            Image img = target.GetComponent<Image>();
            if (img == null) img = target.AddComponent<Image>();
            img.color = baseColor;

            Button btn = target.GetComponent<Button>();
            if (btn == null) btn = target.AddComponent<Button>();

            var colors = btn.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = baseColor * 1.18f;
            colors.pressedColor = baseColor * 0.85f;
            colors.disabledColor = new Color(baseColor.r * 0.4f, baseColor.g * 0.4f, baseColor.b * 0.4f, 0.6f);
            btn.colors = colors;

            GameObject textChild = GetOrCreateChild(target.transform, "Label", Vector2.zero, target.GetComponent<RectTransform>().sizeDelta);
            TextMeshProUGUI tmp = GetOrCreateTMP(textChild, labelText, 15, TextAlignmentOptions.Center);
            tmp.raycastTarget = false;
            return btn;
        }
    }
}
