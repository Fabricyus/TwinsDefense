using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TwinsDefense.Systems;

namespace TwinsDefense.SaveProfiles
{
    /// <summary>
    /// Top-level controller for the Save (profile list) screen — the player's
    /// save management screen reached from the Menu's Play button. Populates
    /// the 3 SaveSlotUI boxes, routes empty-slot clicks to the create-name
    /// modal and occupied-slot clicks to selection (highlight + enable
    /// Play/Delete), and drives Play (-> Character Selection, with the
    /// selected save made active) and Delete (-> confirm modal -> wipe save).
    /// </summary>
    public class SaveProfileListController : MonoBehaviour
    {
        [SerializeField] private SaveSlotUI[] slots;
        [SerializeField] private Button playButton;
        [SerializeField] private Button removeProfileListBtn;
        [SerializeField] private CreateSaveModalUI createModal;
        [SerializeField] private DeleteConfirmModalUI deleteModal;
        [Tooltip("Same back button style as Character Selection's — returns to the Menu.")]
        [SerializeField] private Button backButton;

        private int selectedIndex = -1;

        private void Awake()
        {
            playButton.onClick.AddListener(HandlePlayClicked);
            removeProfileListBtn.onClick.AddListener(HandleDeleteClicked);
            if (backButton != null) backButton.onClick.AddListener(HandleBackClicked);
        }

        private void Start()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            selectedIndex = -1;

            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                slots[i].Refresh(index, HandleSlotClicked);
            }

            UpdateActionButtons();
        }

        private void HandleSlotClicked(int index)
        {
            if (!slots[index].HasSave)
            {
                createModal.Show(name => HandleSaveCreated(index, name));
                return;
            }

            selectedIndex = index;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetSelected(i == index);
            }

            UpdateActionButtons();
        }

        private void HandleSaveCreated(int index, string profileName)
        {
            SaveProfileManager.CreateProfile(index, profileName);
            RefreshAll();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = selectedIndex >= 0;
            playButton.interactable = hasSelection;
            removeProfileListBtn.interactable = hasSelection;
        }

        private void HandlePlayClicked()
        {
            if (selectedIndex < 0) return;

            SaveProfileManager.SetActiveProfile(selectedIndex);
            SceneManager.LoadScene("CharacterSelection");
        }

        private void HandleDeleteClicked()
        {
            if (selectedIndex < 0) return;

            deleteModal.Show(HandleDeleteConfirmed);
        }

        private void HandleDeleteConfirmed()
        {
            SaveProfileManager.DeleteProfile(selectedIndex);
            RefreshAll();
        }

        private void HandleBackClicked()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
