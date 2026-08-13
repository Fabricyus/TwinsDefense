using UnityEngine;

namespace TwinsDefense.Progression
{
    /// <summary>
    /// Tallies simple run stats (kills, coin pickups) shown on the Game Over
    /// summary screen. One instance lives in the Arena Run scene.
    /// </summary>
    public class RunStats : MonoBehaviour
    {
        public static RunStats Instance { get; private set; }

        public int MonstersKilled { get; private set; }
        public int CoinsCollected { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>Called once per enemy defeated.</summary>
        public void RegisterKill()
        {
            MonstersKilled++;
        }

        /// <summary>Called once per coin pickup collected (a count of pickups, not their currency value).</summary>
        public void RegisterCoinCollected()
        {
            CoinsCollected++;
        }
    }
}
