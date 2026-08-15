using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.UI;

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
        [Tooltip("Punch-scale played on a star icon the moment its sprite flips to unlocked (see Populate's animateStarChange).")]
        [SerializeField] private Vector3 starPunchScaleAmount = new Vector3(0.4f, 0.4f, 0f);
        [SerializeField] private float starPunchDuration = 0.3f;

        [Header("Upgrade")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        [Header("Play")]
        [SerializeField] private Button playButton;

        [Header("Upgrade Tooltip")]
        [Tooltip("Shows a preview of the next star's rewards while hovering the Upgrade button.")]
        [SerializeField] private StarUpgradeTooltipUI upgradeTooltip;
        [Tooltip("Pointer relay on the Upgrade button GameObject (CharacterDetailPanelUI doesn't sit on that GameObject, so it can't implement IPointerEnter/ExitHandler directly).")]
        [SerializeField] private PointerHoverRelay upgradeButtonHoverRelay;

        public event Action OnUpgradeClicked;
        public event Action OnPlayClicked;

        private CharacterSlotData currentData;

        private void Awake()
        {
            upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());

            if (upgradeButtonHoverRelay != null)
            {
                upgradeButtonHoverRelay.OnEnter += () => upgradeTooltip?.Show(currentData);
                upgradeButtonHoverRelay.OnExit += () => upgradeTooltip?.Hide();
            }
        }

        /// <summary>Locked tiers still show name/description (to preview the unlock) but disable Upgrade and Play. Set animateStarChange when this Populate follows an actual star purchase, so the newly-unlocked star punches instead of just switching sprite (a plain slot-selection refresh should stay silent).</summary>
        public void Populate(CharacterSlotData data, bool animateStarChange = false)
        {
            currentData = data;

            nameText.text = data.displayName;
            descriptionText.text = data.description;
            portraitImage.sprite = data.icon;
            portraitImage.material = data.isUnlocked ? null : lockedMaterial;

            SetPipTrack(attackPips, data.attackStars, data.attackStarsMax);
            SetPipTrack(defensePips, data.defenseStars, data.defenseStarsMax);
            SetStarTrack(data.attackStars, animateStarChange);

            bool isMaxed = data.upgradeCost < 0;
            upgradeButton.interactable = data.isUnlocked && !isMaxed;
            upgradeCostText.text = !data.isUnlocked ? "LOCKED" : isMaxed ? "MAXED" : $"UPGRADE\n{data.upgradeCost}";

            playButton.interactable = data.isUnlocked;
        }

        private void SetPipTrack(Image[] pips, int current, int max)
        {
            for (int i = 0; i < pips.Length; i++)
            {
                if (pips[i] == null) continue; // track can have fewer pip icons wired up than the stat's max

                bool withinMax = i < max;
                pips[i].gameObject.SetActive(withinMax);
                if (withinMax)
                {
                    pips[i].color = i < current ? filledPipColor : emptyPipColor;
                }
            }
        }

        /// <summary>Swaps each of the 5 star icons' source sprite between locked/unlocked based on purchased Attack Stars, punch-scaling any star that flips to unlocked this call while animate is true.</summary>
        private void SetStarTrack(int purchasedStars, bool animate)
        {
            for (int i = 0; i < starTrack.Length; i++)
            {
                bool shouldBeUnlocked = i < purchasedStars;
                bool wasUnlocked = starTrack[i].sprite == unlockedStarSprite;

                starTrack[i].sprite = shouldBeUnlocked ? unlockedStarSprite : lockedStarSprite;

                if (animate && shouldBeUnlocked && !wasUnlocked)
                {
                    iTween.PunchScale(starTrack[i].gameObject, iTween.Hash(
                        "amount", starPunchScaleAmount,
                        "time", starPunchDuration
                    ));
                }
            }
        }
    }
}
