using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Esc-triggered pause menu for the Arena Run scene. Offers a single
    /// action — Back to Menu — gated behind a confirmation panel warning the
    /// run's progress won't be saved. Esc toggles the pause panel open/closed
    /// like a normal pause menu; while the level-up cards panel is showing
    /// (its own separate pause), Esc is ignored so the two don't fight over
    /// Time.timeScale.
    /// </summary>
    public class ArenaPauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject confirmPanel;
        [Tooltip("Esc is ignored while this panel is active, so pause can't fight the level-up draft over Time.timeScale.")]
        [SerializeField] private GameObject levelUpCardsPanel;

        [Header("Buttons")]
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [SerializeField] private string menuSceneName = "Menu";

        private void Awake()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);

            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(HandleBackToMenuClicked);
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(HandleConfirmYesClicked);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(HandleConfirmNoClicked);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (levelUpCardsPanel != null && levelUpCardsPanel.activeSelf) return;

            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                HandleConfirmNoClicked();
                return;
            }

            TogglePause();
        }

        private void TogglePause()
        {
            if (pausePanel == null) return;

            if (pausePanel.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }

        private void Resume()
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        private void HandleBackToMenuClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        private void HandleConfirmYesClicked()
        {
            // Resets timeScale (frozen at 0 while paused) before leaving, so Menu doesn't load paused.
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        private void HandleConfirmNoClicked()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }
    }
}
