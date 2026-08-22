using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Systems;

namespace TwinsDefense.SaveProfiles
{
    /// <summary>
    /// One of the 3 boxes on the Save (profile list) screen. Shows either
    /// "+ create new save" or the stored profile name, and reports clicks
    /// (empty -> open the create-name modal, occupied -> select it) up to
    /// SaveProfileListController. The character portrait Image sitting
    /// alongside the name is fixed per slot position (Izzy/Court/Ralph) —
    /// purely decorative, not tied to save data, so it isn't touched here.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SaveSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [Tooltip("nameText's color once this slot has a save — matches the color originally set on the first save box's playerName text.")]
        [SerializeField] private Color savedNameColor = new Color(0.9803066f, 1f, 0.495283f, 1f);
        [Tooltip("nameText's color while this slot is still empty (\"+ create new save\").")]
        [SerializeField] private Color emptyNameColor = Color.white;

        [Tooltip("Shows this save's own achievement completion (e.g. \"32%\"), read via SaveProfileManager.PeekAchievementPercent without needing to select the save first. Blank on an empty slot.")]
        [SerializeField] private TextMeshProUGUI achievementPercentText;

        [Tooltip("Sibling Image sitting behind this box's own bars_1 sprite, same sprite, with the Rainbow Aura material — same pattern as CharacterSlotUI's auraImage. Shown only once this save has 100% achievement completion.")]
        [SerializeField] private Image rainbowAuraImage;

        [Header("Selection scale-up")]
        [SerializeField] private float selectedScale = 1.1f;
        [SerializeField] private float scaleAnimDuration = 0.12f;

        private Button button;
        private Vector3 baseScale;
        private Coroutine scaleRoutine;
        private int slotIndex;
        private Action<int> onClicked;

        public bool HasSave { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
            button.onClick.AddListener(HandleClick);
        }

        /// <summary>Re-reads this slot's current save data and resets its visuals — called whenever the profile list needs to reflect a create/delete.</summary>
        public void Refresh(int index, Action<int> clickedCallback)
        {
            slotIndex = index;
            onClicked = clickedCallback;

            SaveProfileSlot slot = SaveProfileManager.GetSlot(index);
            HasSave = slot.exists;
            nameText.text = HasSave ? slot.profileName : "+ create new save";
            nameText.color = HasSave ? savedNameColor : emptyNameColor;

            int achievementPercent = HasSave ? SaveProfileManager.PeekAchievementPercent(index) : 0;

            if (achievementPercentText != null)
            {
                achievementPercentText.text = HasSave ? $"{achievementPercent}%" : string.Empty;
            }

            if (rainbowAuraImage != null)
            {
                rainbowAuraImage.gameObject.SetActive(HasSave && achievementPercent >= 100);
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            SetScaleTarget(selected ? baseScale * selectedScale : baseScale);
        }

        private void HandleClick()
        {
            onClicked?.Invoke(slotIndex);
        }

        private void SetScaleTarget(Vector3 targetScale)
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(AnimateScale(targetScale));
        }

        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float t = 0f;

            while (t < scaleAnimDuration)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, scaleAnimDuration <= 0f ? 1f : t / scaleAnimDuration);
                yield return null;
            }

            transform.localScale = targetScale;
            scaleRoutine = null;
        }
    }
}
