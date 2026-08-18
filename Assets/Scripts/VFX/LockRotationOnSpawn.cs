using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Zeroes out the Z rotation Projectile.Launch sets to face the travel
    /// direction, right after it runs — Start() fires after Launch, since
    /// AutoAttack calls Launch synchronously right after Instantiate, before
    /// Unity's deferred Start() pass. Purely cosmetic; attached directly to
    /// whichever projectile prefabs want a flat/upright sprite regardless of
    /// fire direction, rather than baked into Projectile.cs, which every
    /// character's projectile shares.
    /// </summary>
    public class LockRotationOnSpawn : MonoBehaviour
    {
        private void Start()
        {
            Vector3 euler = transform.eulerAngles;
            euler.z = 0f;
            transform.eulerAngles = euler;
        }
    }
}
