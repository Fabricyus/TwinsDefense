using System;
using UnityEngine;
using UnityEngine.UI;

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
