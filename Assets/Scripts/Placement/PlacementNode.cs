using System;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Placement
{
    /// <summary>
    /// A single fixed slot where one tower of a specific character (Izzy, Court
    /// or Ralph) can be placed, per the game's "1 Izzy + 1 Court + 1 Ralph" rule.
    /// </summary>
    public class PlacementNode : MonoBehaviour
    {
        [Header("Slot")]
        public TowerCharacter allowedCharacter;

        [Header("Placeholder Visual")]
        [Tooltip("Child object toggled on while this node is the tutorial's highlighted/available slot.")]
        [SerializeField] private GameObject highlightVisual;

        public bool IsOccupied { get; private set; }
        public GameObject PlacedTowerInstance { get; private set; }

        /// <summary>Raised whenever any PlacementNode successfully places a tower.</summary>
        public static event Action<PlacementNode> OnTowerPlaced;

        public void SetHighlighted(bool highlighted)
        {
            if (highlightVisual != null)
            {
                highlightVisual.SetActive(highlighted);
            }
        }

        /// <summary>Instantiates the given tower's prefab on this node and marks it occupied.</summary>
        public GameObject PlaceTower(TowerData data)
        {
            if (IsOccupied || data == null || data.towerPrefab == null)
            {
                return null;
            }

            PlacedTowerInstance = Instantiate(data.towerPrefab, transform.position, Quaternion.identity);
            IsOccupied = true;
            SetHighlighted(false);

            OnTowerPlaced?.Invoke(this);

            return PlacedTowerInstance;
        }
    }
}
