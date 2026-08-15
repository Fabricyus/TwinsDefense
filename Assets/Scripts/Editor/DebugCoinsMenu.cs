using UnityEditor;
using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// Editor-only shortcuts for testing Character Selection's star upgrades
    /// without having to grind runs for Coins. Works with the game closed —
    /// PlayerWallet is plain PlayerPrefs, no Play Mode required.
    /// </summary>
    public static class DebugCoinsMenu
    {
        [MenuItem("Tools/TwinsDefense/Debug/Add 10000 Coins")]
        public static void Add10000()
        {
            PlayerWallet.AddCoins(10000);
            Debug.Log($"DebugCoinsMenu: +10000 Coins — total now {PlayerWallet.TotalCoins}.");
        }

        [MenuItem("Tools/TwinsDefense/Debug/Add 50000 Coins")]
        public static void Add50000()
        {
            PlayerWallet.AddCoins(50000);
            Debug.Log($"DebugCoinsMenu: +50000 Coins — total now {PlayerWallet.TotalCoins}.");
        }

        [MenuItem("Tools/TwinsDefense/Debug/Reset Coins to 0")]
        public static void ResetToZero()
        {
            PlayerWallet.SpendCoins(PlayerWallet.TotalCoins);
            Debug.Log("DebugCoinsMenu: Coins reset to 0.");
        }

        [MenuItem("Tools/TwinsDefense/Debug/Log Current Coins")]
        public static void LogCurrent()
        {
            Debug.Log($"DebugCoinsMenu: current total = {PlayerWallet.TotalCoins} Coins.");
        }
    }
}
