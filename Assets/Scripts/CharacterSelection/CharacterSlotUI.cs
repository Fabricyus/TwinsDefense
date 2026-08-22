using System;
using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Systems;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// A single grid icon (one character tier). Shows the unlocked portrait or
    /// locked silhouette on its own Image, and reports clicks to the controller.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CharacterSlotUI : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private Color normalColor = Color.white;

        [Tooltip("Applied to the portrait when the slot is locked (e.g. a white-flash silhouette material). Unlocked slots use the default UI material.")]
        [SerializeField] private Material lockedMaterial;

        [Tooltip("Sibling Image sitting behind the portrait, same icon sprite, with the Rainbow Aura material. Shown only for this EXACT slot's tier once it reached auraRequiredLevel — see PlayerRainbowAuraVFX for the same per-tier gating on the player itself.")]
        [SerializeField] private Image auraImage;
        [SerializeField] private int auraRequiredLevel = 100;

        [Tooltip("Child badge Image (star sprite) shown only once this slot's character has purchased every Attack Star (see CharacterStarUpgrades.MaxStars).")]
        [SerializeField] private Image starImage;

        private Button button;
        private CharacterSlotData assignedData;
        private Action<CharacterSlotData> onClicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (portraitImage == null)
            {
                portraitImage = GetComponent<Image>();
            }

            button.onClick.AddListener(HandleClick);
        }

        public void Setup(CharacterSlotData data, Action<CharacterSlotData> clickedCallback)
        {
            assignedData = data;
            onClicked = clickedCallback;
            portraitImage.sprite = data.icon;
            portraitImage.material = data.isUnlocked ? null : lockedMaterial;

            if (auraImage != null)
            {
                bool hasAura = CharacterProgressTracker.Instance.GetHighestLevelForTier(data.characterId, data.tier) >= auraRequiredLevel;
                auraImage.gameObject.SetActive(hasAura);

                if (hasAura)
                {
                    auraImage.sprite = data.icon;
                }
            }

            if (starImage != null)
            {
                bool hasMaxStars = CharacterStarUpgrades.Instance.GetStars(data.slotId) >= CharacterStarUpgrades.MaxStars;
                starImage.gameObject.SetActive(hasMaxStars);
            }
        }

        public void SetSelected(bool selected)
        {
            portraitImage.color = selected ? selectedColor : normalColor;
        }

        private void HandleClick()
        {
            onClicked?.Invoke(assignedData);
        }
    }
}
