using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwinsDefense.SaveProfiles
{
    /// <summary>
    /// Centered popup for naming a new save (max 16 characters, enforced by
    /// the TMP_InputField's own Character Limit). Confirm persists the name
    /// via SaveProfileListController's callback and closes; Cancel just closes.
    /// </summary>
    public class CreateSaveModalUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action<string> onConfirmed;

        private void Awake()
        {
            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(Hide);
            gameObject.SetActive(false);
        }

        public void Show(Action<string> confirmedCallback)
        {
            onConfirmed = confirmedCallback;
            nameInput.text = string.Empty;
            gameObject.SetActive(true);
            nameInput.Select();
            nameInput.ActivateInputField();
        }

        private void HandleConfirm()
        {
            string name = nameInput.text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            Hide();
            onConfirmed?.Invoke(name);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
