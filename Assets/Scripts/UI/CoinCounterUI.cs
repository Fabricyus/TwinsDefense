using TMPro;
using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Keeps coinTxt in sync with CoinManager.CurrentCoins. Intended to sit on
    /// the coinTxt GameObject itself, driven by CoinManager.OnCoinsChanged
    /// instead of polling every frame.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CoinCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinTxt;

        private void Awake()
        {
            if (coinTxt == null)
            {
                coinTxt = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            // Start (unlike OnEnable) is guaranteed to run after every
            // GameObject's Awake in the scene, so CoinManager.Instance is
            // reliably set by the time this subscribes.
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.OnCoinsChanged += HandleCoinsChanged;
                HandleCoinsChanged(CoinManager.Instance.CurrentCoins);
            }
        }

        private void OnDisable()
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void HandleCoinsChanged(int currentCoins)
        {
            coinTxt.text = currentCoins.ToString();
        }
    }
}
