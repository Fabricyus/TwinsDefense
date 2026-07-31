using System;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Economy;

namespace TwinsDefense.Towers
{
    /// <summary>
    /// Runtime star-upgrade state for a single placed tower instance.
    /// TowerData.starLevels holds one entry per star ABOVE 1 (index 0 = star 2,
    /// ... index 3 = star 5), so star 1 itself needs no table entry.
    /// </summary>
    public class TowerStarUpgrade : MonoBehaviour
    {
        [SerializeField] private TowerData towerData;
        [SerializeField] private Tower tower;

        public int CurrentStar { get; private set; } = 1;

        public event Action<int> OnStarChanged;

        public bool CanUpgrade => CurrentStar < 5;

        private void Awake()
        {
            if (tower == null)
            {
                tower = GetComponent<Tower>();
            }
        }

        public int NextStarCost()
        {
            if (!CanUpgrade) return -1;

            return towerData.starLevels[CurrentStar - 1].gemCost;
        }

        public bool TryUpgrade()
        {
            if (!CanUpgrade) return false;

            int cost = NextStarCost();

            if (GemsManager.Instance == null || !GemsManager.Instance.HasEnough(cost))
            {
                return false;
            }

            GemsManager.Instance.Spend(cost);
            CurrentStar++;
            ApplyStatsForCurrentStar();
            OnStarChanged?.Invoke(CurrentStar);

            return true;
        }

        private void ApplyStatsForCurrentStar()
        {
            if (CurrentStar <= 1 || tower == null) return;

            StarLevel level = towerData.starLevels[CurrentStar - 2];
            tower.SetStarMultipliers(level.damageMultiplier, level.rangeMultiplier, level.fireRateMultiplier);
        }
    }
}
