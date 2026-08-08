using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Grid
{
    /// <summary>
    /// Grid-based replacement for the old fixed-node placement flow. Validates
    /// tower placement against the GridManager (cell is Placement type and
    /// unoccupied) instead of a predefined PlacementNode, and still enforces
    /// the "1 Izzy + 1 Court + 1 Ralph" field-wide rule.
    /// </summary>
    public class TowerPlacementValidator : MonoBehaviour
    {
        public static TowerPlacementValidator Instance { get; private set; }

        [SerializeField] private GridManager gridManager;

        private readonly Dictionary<TowerCharacter, GameObject> activeTowers = new Dictionary<TowerCharacter, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridManager>();
            }
        }

        public bool HasActiveTowerOfCharacter(TowerCharacter character)
        {
            return activeTowers.ContainsKey(character);
        }

        /// <summary>Finds the grid cell (if any) under the given screen-space point.</summary>
        public bool TryGetCellUnderScreenPoint(Vector2 screenPoint, Camera camera, out GridCell cell)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null || gridManager == null)
            {
                cell = null;
                return false;
            }

            Vector3 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            return gridManager.TryGetCellAtWorldPoint(worldPoint, out cell);
        }

        /// <summary>True if this tower could be placed on this cell right now (type, occupancy, one-per-character rule).</summary>
        public bool CanPlace(GridCell cell, TowerData data)
        {
            return cell != null
                && data != null
                && cell.Type == CellType.Placement
                && !cell.IsOccupied
                && !HasActiveTowerOfCharacter(data.character);
        }

        /// <summary>Instantiates the tower's prefab snapped to the cell's center and marks the cell occupied.</summary>
        public GameObject PlaceTower(GridCell cell, TowerData data)
        {
            if (!CanPlace(cell, data) || data.towerPrefab == null)
            {
                return null;
            }

            Vector3 worldPosition = gridManager.GridToWorld(cell.Coordinate);
            GameObject instance = Instantiate(data.towerPrefab, worldPosition, Quaternion.identity);

            gridManager.TryOccupy(cell.Coordinate, instance);
            activeTowers[data.character] = instance;

            return instance;
        }

        /// <summary>Frees the cell and clears the one-per-character slot for this tower, e.g. on sell/despawn.</summary>
        public void RemoveTower(TowerCharacter character)
        {
            if (!activeTowers.TryGetValue(character, out GameObject instance))
            {
                return;
            }

            Vector2Int coordinate = gridManager.WorldToGrid(instance.transform.position);
            gridManager.Free(coordinate);
            activeTowers.Remove(character);
        }
    }
}
