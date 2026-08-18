using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TwinsDefense.Systems;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Top-level controller for the Character Selection scene. Populates the
    /// slot grid from ICharacterProgressionProvider, tracks the current
    /// selection, and drives the detail panel plus the Play/Upgrade actions.
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        [Tooltip("Must implement ICharacterProgressionProvider (e.g. StubCharacterProgressionProvider).")]
        [SerializeField] private MonoBehaviour progressionProviderSource;

        [Tooltip("12 pre-placed grid slots, in the exact order returned by GetAllSlots().")]
        [SerializeField] private CharacterSlotUI[] slotUIs;

        [SerializeField] private CharacterDetailPanelUI detailPanel;

        private ICharacterProgressionProvider provider;
        private List<CharacterSlotData> slots;
        private int selectedIndex = -1;

        private void Awake()
        {
            provider = progressionProviderSource as ICharacterProgressionProvider;
        }

        private void Start()
        {
            RefreshSlots();

            detailPanel.OnUpgradeClicked += HandleUpgradeClicked;
            detailPanel.OnPlayClicked += HandlePlayClicked;

            int defaultIndex = slots.FindIndex(s => s.isUnlocked);
            SelectIndex(defaultIndex >= 0 ? defaultIndex : 0);
        }

private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                OnBackClicked();
            }
        }


        /// <summary>Re-fetches the slot list and re-wires every grid icon to it. Must run after any change that replaces the slots list (e.g. an upgrade purchase) — CharacterSlotData has no value equality, so a CharacterSlotUI left holding a stale instance would never be found again by HandleSlotClicked's list lookup.</summary>
        private void RefreshSlots()
        {
            slots = provider.GetAllSlots();

            for (int i = 0; i < slotUIs.Length; i++)
            {
                bool hasData = i < slots.Count;
                slotUIs[i].gameObject.SetActive(hasData);
                if (hasData)
                {
                    slotUIs[i].Setup(slots[i], HandleSlotClicked);
                }
            }
        }

        private void HandleSlotClicked(CharacterSlotData data)
        {
            SelectIndex(slots.IndexOf(data));
        }

        private void SelectIndex(int index)
        {
            if (index < 0 || index >= slots.Count) return;

            selectedIndex = index;
            detailPanel.Populate(slots[index]);

            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUIs[i].SetSelected(i == index);
            }
        }

        private void HandleUpgradeClicked()
        {
            provider.RequestUpgrade(slots[selectedIndex].slotId);

            // Re-fetch so the detail panel picks up the new star count/next cost immediately,
            // and re-wire the grid icons to the fresh list (see RefreshSlots).
            RefreshSlots();
            detailPanel.Populate(slots[selectedIndex], animateStarChange: true);
        }

        private void HandlePlayClicked()
        {
            CharacterSlotData selected = slots[selectedIndex];
            SelectedRunContext.Instance.SetSelection(selected.characterId, selected.tier);
            SceneManager.LoadScene("Arena Run");
        }

        public void OnBackClicked()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
