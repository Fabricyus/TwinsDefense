namespace TwinsDefense.Grid
{
    /// <summary>
    /// Classifies what a single grid cell may be used for.
    /// </summary>
    public enum CellType
    {
        /// <summary>Obstacle/decoration. Unusable by enemies or towers.</summary>
        Blocked,
        /// <summary>Walkable by enemies as part of a route.</summary>
        Path,
        /// <summary>Valid tower placement spot.</summary>
        Placement
    }
}
