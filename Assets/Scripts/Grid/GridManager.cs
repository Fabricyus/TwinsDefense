using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwinsDefense.Grid
{
    /// <summary>
    /// A single enemy route, expressed as an ordered sequence of Path-type cell
    /// coordinates from spawn to goal. A phase can define more than one of
    /// these so waves can be routed differently.
    /// </summary>
    [System.Serializable]
    public class GridRoute
    {
        public string routeName = "Route";
        public List<Vector2Int> cells = new List<Vector2Int>();
    }

    /// <summary>
    /// Owns the cell matrix for a phase's grid/tabuleiro: dimensions, per-cell
    /// type (Path / Placement / Blocked), grid&lt;-&gt;world conversion and
    /// occupancy queries. Replaces the old fixed-3-node placement model —
    /// towers now snap to whichever Placement cell is under the cursor instead
    /// of a predefined PlacementNode.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Dimensions")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [Tooltip("World-space size of one cell. Matches the 16x16 pixel-art tiles at 16 PPU (1 world unit per tile).")]
        [SerializeField] private float cellSize = 1f;
        [Tooltip("World position of the center of cell (0, 0).")]
        [SerializeField] private Vector3 origin = Vector3.zero;

        [Header("Cells")]
        [Tooltip("Row-major (index = y * width + x). Use the context menu to (re)generate this array after changing Width/Height.")]
        [SerializeField] private CellType[] cellTypes = new CellType[0];

        [Header("Routes")]
        [Tooltip("Ordered Path-cell sequences enemies can walk, spawn to goal. A phase may define more than one for wave variety.")]
        [SerializeField] private List<GridRoute> routes = new List<GridRoute>();

        private GridCell[,] cells;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildCells();
        }

        private void BuildCells()
        {
            cells = new GridCell[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    CellType type = index < cellTypes.Length ? cellTypes[index] : CellType.Blocked;
                    cells[x, y] = new GridCell(new Vector2Int(x, y), type);
                }
            }
        }

        /// <summary>Regenerates <see cref="cellTypes"/> to match Width x Height, preserving existing values where possible.</summary>
        [ContextMenu("Resize Cell Array To Width/Height")]
        private void ResizeCellArray()
        {
            CellType[] resized = new CellType[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int newIndex = y * width + x;
                    resized[newIndex] = TryGetLegacyType(x, y, out CellType legacy) ? legacy : CellType.Blocked;
                }
            }

            cellTypes = resized;
        }

        private bool TryGetLegacyType(int x, int y, out CellType type)
        {
            // Best-effort preservation using the previous array's width isn't tracked,
            // so this only helps when width/height haven't changed since the last resize.
            int index = y * width + x;
            if (cellTypes != null && index < cellTypes.Length)
            {
                type = cellTypes[index];
                return true;
            }

            type = CellType.Blocked;
            return false;
        }

        public bool IsWithinBounds(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < width && coordinate.y >= 0 && coordinate.y < height;
        }

        public bool TryGetCell(Vector2Int coordinate, out GridCell cell)
        {
            if (cells == null)
            {
                BuildCells();
            }

            if (!IsWithinBounds(coordinate))
            {
                cell = null;
                return false;
            }

            cell = cells[coordinate.x, coordinate.y];
            return true;
        }

        /// <summary>Converts a cell coordinate to its center position in world space.</summary>
        public Vector3 GridToWorld(Vector2Int coordinate)
        {
            return origin + new Vector3(coordinate.x * cellSize, coordinate.y * cellSize, 0f);
        }

        /// <summary>Converts a world-space position to the coordinate of the cell containing it.</summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 local = worldPosition - origin;
            int x = Mathf.RoundToInt(local.x / cellSize);
            int y = Mathf.RoundToInt(local.y / cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>Finds the cell (if any) under the given world-space point.</summary>
        public bool TryGetCellAtWorldPoint(Vector3 worldPosition, out GridCell cell)
        {
            return TryGetCell(WorldToGrid(worldPosition), out cell);
        }

        public bool IsOccupied(Vector2Int coordinate)
        {
            return TryGetCell(coordinate, out GridCell cell) && cell.IsOccupied;
        }

        /// <summary>True if the cell exists, is a Placement cell, and is currently free.</summary>
        public bool CanPlaceAt(Vector2Int coordinate)
        {
            return TryGetCell(coordinate, out GridCell cell) && cell.Type == CellType.Placement && !cell.IsOccupied;
        }

        public bool TryOccupy(Vector2Int coordinate, GameObject occupant)
        {
            return TryGetCell(coordinate, out GridCell cell) && cell.TryOccupy(occupant);
        }

        public void Free(Vector2Int coordinate)
        {
            if (TryGetCell(coordinate, out GridCell cell))
            {
                cell.Clear();
            }
        }

        /// <summary>World-space waypoints for the named route, in walking order, for enemies to follow.</summary>
        public Vector3[] GetRouteWaypoints(string routeName)
        {
            GridRoute route = routes.Find(r => r.routeName == routeName);

            if (route == null)
            {
                return Array.Empty<Vector3>();
            }

            Vector3[] waypoints = new Vector3[route.cells.Count];

            for (int i = 0; i < route.cells.Count; i++)
            {
                waypoints[i] = GridToWorld(route.cells[i]);
            }

            return waypoints;
        }

        private void OnDrawGizmos()
        {
            if (cellTypes == null || cellTypes.Length == 0) return;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (index >= cellTypes.Length) continue;

                    Gizmos.color = cellTypes[index] switch
                    {
                        CellType.Path => new Color(1f, 0.85f, 0.2f, 0.35f),
                        CellType.Placement => new Color(0.2f, 0.85f, 1f, 0.35f),
                        _ => new Color(0f, 0f, 0f, 0.15f)
                    };

                    Vector3 center = GridToWorld(new Vector2Int(x, y));
                    Gizmos.DrawCube(center, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 0.01f));
                }
            }
        }
    }
}
