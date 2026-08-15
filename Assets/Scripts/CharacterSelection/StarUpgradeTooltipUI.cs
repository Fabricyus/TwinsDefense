using System.Text;
using TMPro;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Hover tooltip shown over the Upgrade button in Character Selection,
    /// previewing what the NEXT star purchase (data.attackStars + 1) grants.
    /// Per-star rewards mirror PlayerCharacterData.ApplyPurchasedStars — every
    /// star (1 through 5) grants +1 Star Projectile 1:1 with purchasedStars;
    /// the Exclusive Card line is resolved live by scanning cardPool for any
    /// card restricted to this slot with minStarsRequired == nextStar.
    /// </summary>
    public class StarUpgradeTooltipUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private CardPoolConfig cardPool;

        private void Awake()
        {
            Hide();
        }

        public void Show(CharacterSlotData data)
        {
            if (data == null || panelRoot == null) return;

            bool isMaxed = data.upgradeCost < 0;
            if (!data.isUnlocked || isMaxed)
            {
                panelRoot.SetActive(false);
                return;
            }

            int nextStar = data.attackStars + 1;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Star {nextStar}</b>");
            sb.AppendLine("+1 Damage");
            sb.AppendLine("+15% Attack Fire Rate");
            sb.AppendLine("+2% Passive Proc Chance");
            sb.AppendLine("+8% Passive Magnitude");
            sb.AppendLine("+1 Star Projectile");

            if (nextStar == 3)
            {
                sb.AppendLine("Unlocks: Cast Trail Cosmetic");
            }

            string exclusiveCardName = FindExclusiveCardName(data.slotId, nextStar);
            if (!string.IsNullOrEmpty(exclusiveCardName))
            {
                sb.AppendLine($"Unlocks Card: {exclusiveCardName}");
            }

            bodyText.text = sb.ToString().TrimEnd();
            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private string FindExclusiveCardName(string slotId, int nextStar)
        {
            if (cardPool == null || cardPool.allCards == null || string.IsNullOrEmpty(slotId)) return null;

            foreach (CardData card in cardPool.allCards)
            {
                if (card == null || card.minStarsRequired != nextStar || card.restrictedToCharacterIds == null) continue;

                foreach (string restrictedId in card.restrictedToCharacterIds)
                {
                    if (restrictedId == slotId) return card.displayName;
                }
            }

            return null;
        }
    }
}
