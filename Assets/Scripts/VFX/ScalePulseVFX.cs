using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Continuous scale pulse via iTween — ping-pongs between the object's
    /// scale at spawn (read once in Start, after anything else has already
    /// set it, e.g. Projectile.Launch's Area of Effect scaling, and after a
    /// random per-spawn multiplier in [minSpawnScale, maxSpawnScale] is
    /// applied) and that scale times pulseMultiplier, forever. Purely
    /// cosmetic; attached directly to whichever prefabs want it rather than
    /// baked into Projectile.cs, which every character's projectile shares.
    /// </summary>
    public class ScalePulseVFX : MonoBehaviour
    {
        [Tooltip("Random per-spawn scale multiplier, applied once in Start before the pulse target is computed. Leave both at 1 for no randomization.")]
        [SerializeField] private float minSpawnScale = 1f;
        [SerializeField] private float maxSpawnScale = 1f;
        [SerializeField] private float pulseMultiplier = 1.2f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private iTween.EaseType easeType = iTween.EaseType.easeInOutSine;

        private void Start()
        {
            transform.localScale *= Random.Range(minSpawnScale, maxSpawnScale);

            iTween.ScaleTo(gameObject, iTween.Hash(
                "scale", transform.localScale * pulseMultiplier,
                "time", duration,
                "easetype", easeType,
                "looptype", iTween.LoopType.pingPong
            ));
        }
    }
}
