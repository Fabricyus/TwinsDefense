using System.Collections.Generic;
using UnityEngine;
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

            detailPanel.OnUpgradeClicked += HandleUpgradeClicked;
            detailPanel.OnPlayClicked += HandlePlayClicked;

            int defaultIndex = slots.FindIndex(s => s.isUnlocked);
            SelectIndex(defaultIndex >= 0 ? defaultIndex : 0);
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

            // Re-fetch so the detail panel picks up the new star count/next cost immediately.
            slots = provider.GetAllSlots();
            detailPanel.Populate(slots[selectedIndex]);
        }

        private void HandlePlayClicked()
        {
            CharacterSlotData selected = slots[selectedIndex];
            SelectedRunContext.Instance.SetSelection(selected.characterId, selected.tier);
            SceneManager.LoadScene("Arena Run");
        }
    }
}
