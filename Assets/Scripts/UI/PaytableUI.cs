using System;
using System.Collections.Generic;
using TMPro;
using Underpin.SlotGame.Audio;
using Underpin.SlotGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Underpin.SlotGame.UI
{
    /// <summary>
    /// Modal dialog displaying all symbol payouts, Wild / Scatter bonus rules, and payline configurations.
    /// </summary>
    public class PaytableUI : MonoBehaviour
    {
        [Header("UI Containers")]
        [SerializeField] private GameObject paytablePanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform symbolsContentParent;

        [Header("Prefabs & Templates")]
        [SerializeField] private GameObject symbolEntryTemplate;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (symbolEntryTemplate != null)
            {
                symbolEntryTemplate.SetActive(false);
            }

            HideImmediate();
        }

        public void Initialize(PaytableConfig config)
        {
            if (config == null || symbolsContentParent == null || symbolEntryTemplate == null) return;

            // Clear old entries
            foreach (Transform child in symbolsContentParent)
            {
                if (child.gameObject != symbolEntryTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            // Populate symbol cards
            foreach (var sym in config.Symbols)
            {
                if (sym == null) continue;

                GameObject entry = Instantiate(symbolEntryTemplate, symbolsContentParent);
                entry.SetActive(true);

                var images = entry.GetComponentsInChildren<Image>(true);
                Image iconImg = null;
                foreach (var im in images)
                {
                    if (im.gameObject.name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0 || im.gameObject != entry)
                    {
                        iconImg = im;
                        break;
                    }
                }
                if (iconImg == null && images.Length > 0) iconImg = images[0];

                if (iconImg != null && sym.Icon != null)
                {
                    iconImg.sprite = sym.Icon;
                    iconImg.preserveAspect = true;
                }

                var texts = entry.GetComponentsInChildren<TextMeshProUGUI>(true);
                TextMeshProUGUI nameTxt = null;
                TextMeshProUGUI payoutTxt = null;

                foreach (var t in texts)
                {
                    if (t.gameObject.name.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0) nameTxt = t;
                    else if (t.gameObject.name.IndexOf("Payout", StringComparison.OrdinalIgnoreCase) >= 0 || t.gameObject.name.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0) payoutTxt = t;
                }

                if (nameTxt == null && texts.Length >= 1) nameTxt = texts[0];
                if (payoutTxt == null && texts.Length >= 2) payoutTxt = texts[1];

                if (nameTxt != null) nameTxt.text = sym.SymbolName;
                if (payoutTxt != null)
                {
                    if (sym.Type == SymbolType.Wild)
                    {
                        payoutTxt.text = "<color=#FFD700>WILD</color>\nSubstitutes symbols\n3x: " + sym.Payout3Match + "x";
                    }
                    else if (sym.Type == SymbolType.Scatter)
                    {
                        payoutTxt.text = $"<color=#FF00FF>SCATTER</color>\n3+ triggers {config.FreeSpinsAwarded} FS (2x)";
                    }
                    else
                    {
                        payoutTxt.text = $"3x: {sym.Payout3Match}x\n2x: {sym.Payout2Match}x";
                    }
                }
            }

            // Populate Gamble / Double-or-Nothing Bonus Info Card
            if (config.IsGambleEnabled)
            {
                GameObject gambleCard = Instantiate(symbolEntryTemplate, symbolsContentParent);
                gambleCard.SetActive(true);

                var texts = gambleCard.GetComponentsInChildren<TextMeshProUGUI>(true);
                TextMeshProUGUI nameTxt = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI payoutTxt = texts.Length > 1 ? texts[1] : null;

                if (nameTxt != null) nameTxt.text = "<color=#FFD700>GAMBLE FEATURE</color>";
                if (payoutTxt != null)
                {
                    payoutTxt.text = $"<color=#00FFFF>Double or Nothing</color>\nRed/Black: 2x | Suit: 4x\nUp to {config.MaxGambleRounds} Rounds";
                }
            }
        }

        public void Show()
        {
            if (paytablePanel != null)
            {
                paytablePanel.SetActive(true);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }

        public void Hide()
        {
            if (paytablePanel != null)
            {
                paytablePanel.SetActive(false);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }

        public void Toggle()
        {
            if (paytablePanel != null)
            {
                if (paytablePanel.activeSelf) Hide();
                else Show();
            }
        }

        private void HideImmediate()
        {
            if (paytablePanel != null)
            {
                paytablePanel.SetActive(false);
            }
        }
    }
}
