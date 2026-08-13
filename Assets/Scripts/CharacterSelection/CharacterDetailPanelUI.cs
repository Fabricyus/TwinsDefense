using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Right-side detail panel: name, description, portrait, attack/defense
    /// star tracks, and the Upgrade/Play buttons for the currently selected slot.
    /// </summary>
    public class CharacterDetailPanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image portraitImage;

        [Tooltip("Applied to the portrait when the slot is locked (e.g. a white-flash silhouette material). Unlocked slots use the default UI material.")]
        [SerializeField] private Material lockedMaterial;

        [Header("Star Tracks")]
        [SerializeField] private Image[] attackPips;
        [SerializeField] private Image[] defensePips;
        [SerializeField] private Color filledPipColor = Color.white;
        [SerializeField] private Color emptyPipColor = new Color(1f, 1f, 1f, 0.35f);

        [Tooltip("5 star icons (star, star (1)...star (4)) whose source sprite swaps between lockedStarSprite and unlockedStarSprite based on purchased Attack Stars.")]
        [SerializeField] private Image[] starTrack;
        [SerializeField] private Sprite lockedStarSprite;
        [SerializeField] private Sprite unlockedStarSprite;

        [Header("Upgrade")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        [Header("Play")]
        [SerializeField] private Button playButton;

        public event Action OnUpgradeClicked;
        public event Action OnPlayClicked;

        private void Awake()
        {
            upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        }

        /// <summary>Locked tiers still show name/description (to preview the unlock) but disable Upgrade and Play.</summary>
        public void Populate(CharacterSlotData data)
        {
            nameText.text = data.displayName;
            descriptionText.text = data.description;
            portraitImage.sprite = data.icon;
            portraitImage.material = data.isUnlocked ? null : lockedMaterial;

            SetPipTrack(attackPips, data.attackStars, data.attackStarsMax);
            SetPipTrack(defensePips, data.defenseStars, data.defenseStarsMax);
            SetStarTrack(data.attackStars);

            bool isMaxed = data.upgradeCost < 0;
            upgradeButton.interactable = data.isUnlocked && !isMaxed;
            upgradeCostText.text = !data.isUnlocked ? "LOCKED" : isMaxed ? "MAXED" : $"UPGRADE\n{data.upgradeCost}";

            playButton.interactable = data.isUnlocked;
        }

        private void SetPipTrack(Image[] pips, int current, int max)
        {
            for (int i = 0; i < pips.Length; i++)
            {
                bool withinMax = i < max;
                pips[i].gameObject.SetActive(withinMax);
                if (withinMax)
                {
                    pips[i].color = i < current ? filledPipColor : emptyPipColor;
                }
            }
        }

        /// <summary>Swaps each of the 5 star icons' source sprite between locked/unlocked based on purchased Attack Stars.</summary>
        private void SetStarTrack(int purchasedStars)
        {
            for (int i = 0; i < starTrack.Length; i++)
            {
                starTrack[i].sprite = i < purchasedStars ? unlockedStarSprite : lockedStarSprite;
            }
        }
    }
}
