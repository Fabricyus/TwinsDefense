using UnityEngine;

namespace TwinsDefense.Grid
{
    /// <summary>
    /// A single cell of the phase's grid/tabuleiro: its coordinate, type
    /// (Path / Placement / Blocked) and whatever currently occupies it.
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        [SerializeField] private Vector2Int coordinate;
        [SerializeField] private CellType type;

        /// <summary>The GameObject currently placed on this cell (a tower), if any.</summary>
        public GameObject Occupant { get; private set; }

        public Vector2Int Coordinate => coordinate;
        public CellType Type => type;
        public bool IsOccupied => Occupant != null;

        public GridCell(Vector2Int coordinate, CellType type)
        {
            this.coordinate = coordinate;
            this.type = type;
        }

        public bool TryOccupy(GameObject occupant)
        {
            if (IsOccupied || occupant == null)
            {
                return false;
            }

            Occupant = occupant;
            return true;
        }

        public void Clear()
        {
            Occupant = null;
        }
    }
}
