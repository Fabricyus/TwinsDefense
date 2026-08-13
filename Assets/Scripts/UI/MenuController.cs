using UnityEngine;
using UnityEngine.SceneManagement;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Main Menu button actions: Play loads Character Selection, Exit quits
    /// the application (stops Play mode instead, when running in the Editor).
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            SceneManager.LoadScene("CharacterSelection");
        }

        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
