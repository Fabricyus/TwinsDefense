using TMPro;
using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Shows the player's persistent Coins total (summed from each run's Game
    /// Over summary, spent on character star upgrades) so it's visible and
    /// stays live while browsing/upgrading characters.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CoinsWalletDisplayUI : MonoBehaviour
    {
        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            PlayerWallet.OnCoinsChanged += HandleCoinsChanged;
        }

        private void OnDisable()
        {
            PlayerWallet.OnCoinsChanged -= HandleCoinsChanged;
        }

        private void Start()
        {
            text.text = PlayerWallet.TotalCoins.ToString();
        }

        private void HandleCoinsChanged(int totalCoins)
        {
            text.text = totalCoins.ToString();
        }
    }
}
