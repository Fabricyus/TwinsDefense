using UnityEngine;
using UnityEngine.InputSystem;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Persistent rebinding for the game's only inputs — the 4 movement keys
    /// read by PlayerController. Arrow keys always work as a fixed fallback
    /// alongside whatever's bound here, so a bad rebind can never lock
    /// movement out entirely. Backed directly by PlayerPrefs — same
    /// placeholder-persistence rationale as PlayerWallet/CharacterProgressTracker
    /// (no project save system exists yet).
    /// </summary>
    public static class KeyBindings
    {
        private const string UpKey = "TwinsDefense.Keybind.Up";
        private const string DownKey = "TwinsDefense.Keybind.Down";
        private const string LeftKey = "TwinsDefense.Keybind.Left";
        private const string RightKey = "TwinsDefense.Keybind.Right";

        public static Key Up => (Key)PlayerPrefs.GetInt(UpKey, (int)Key.W);
        public static Key Down => (Key)PlayerPrefs.GetInt(DownKey, (int)Key.S);
        public static Key Left => (Key)PlayerPrefs.GetInt(LeftKey, (int)Key.A);
        public static Key Right => (Key)PlayerPrefs.GetInt(RightKey, (int)Key.D);

        public static void SetUp(Key key) => Set(UpKey, key);
        public static void SetDown(Key key) => Set(DownKey, key);
        public static void SetLeft(Key key) => Set(LeftKey, key);
        public static void SetRight(Key key) => Set(RightKey, key);

        private static void Set(string prefKey, Key key)
        {
            PlayerPrefs.SetInt(prefKey, (int)key);
            PlayerPrefs.Save();
        }
    }
}
