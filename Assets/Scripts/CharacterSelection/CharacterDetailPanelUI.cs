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
        [Tooltip("Raw stat-magnitude bars (baseStats.damage/defense divided by 5) — see SetMagnitudeTrack. Independent of starTrack below, which shows purchased Attack Star progress.")]
        [SerializeField] private Image[] attackPips;
        [SerializeField] private Image[] defensePips;
        [SerializeField] private Color filledPipColor = Color.white;

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

            SetMagnitudeTrack(attackPips, data.attackPipCount);
            SetMagnitudeTrack(defensePips, data.defensePipCount);
            SetStarTrack(data.attackStars, animateStarChange);

            bool isMaxed = data.upgradeCost < 0;
            upgradeButton.interactable = data.isUnlocked && !isMaxed;
            upgradeCostText.text = !data.isUnlocked ? "LOCKED" : isMaxed ? "MAXED" : $"UPGRADE\n{data.upgradeCost}";

            playButton.interactable = data.isUnlocked;
        }

        /// <summary>Shows exactly count pips, all filled — a flat raw-stat magnitude bar (baseStats.damage/defense divided by 5), independent of the Star Upgrade progress shown by starTrack. Any wired pip beyond count is hidden.</summary>
        private void SetMagnitudeTrack(Image[] pips, int count)
        {
            for (int i = 0; i < pips.Length; i++)
            {
                if (pips[i] == null) continue; // track can have fewer pip icons wired up than the highest count among all characters

                bool withinCount = i < count;
                pips[i].gameObject.SetActive(withinCount);
                if (withinCount)
                {
                    pips[i].color = filledPipColor;
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
