using UnityEngine;

namespace TwinsDefense.Economy
{
    /// <summary>Implemented by pickups (Coin, Exp) that a magnet can pull toward a target.</summary>
    public interface IAttractable
    {
        void Attract(Transform target);
    }
}
