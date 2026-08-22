using System;
using UnityEngine;
using UnityEngine.UI;

namespace TwinsDefense.SaveProfiles
{
    /// <summary>
    /// Centered "are you sure?" popup shown by removeProfileListBtn before a
    /// save is actually deleted. Yes runs the callback (SaveProfileListController
    /// deletes the selected save and refreshes the list); No just closes.
    /// </summary>
    public class DeleteConfirmModalUI : MonoBehaviour
    {
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action onConfirmed;

        private void Awake()
        {
            yesButton.onClick.AddListener(HandleYes);
            noButton.onClick.AddListener(Hide);
            gameObject.SetActive(false);
        }

        public void Show(Action confirmedCallback)
        {
            onConfirmed = confirmedCallback;
            gameObject.SetActive(true);
        }

        private void HandleYes()
        {
            Hide();
            onConfirmed?.Invoke();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
