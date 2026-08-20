using UnityEngine;
using TwinsDefense.Systems;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Scales a baked-in TrailRenderer's colorGradient alpha by the player's
    /// projectile-opacity preference (same ProjectileOpacitySettings slider
    /// Projectile/ProjectileTrailVFX/ProcAreaDamage already use). For prefabs
    /// that use Unity's built-in TrailRenderer component instead of the
    /// particle-based ProjectileTrailVFX (e.g. izzy4proj), since TrailRenderer
    /// isn't covered by ProjectileOpacitySettings on its own.
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class TrailRendererOpacityVFX : MonoBehaviour
    {
        private TrailRenderer trailRenderer;
        private GradientColorKey[] baseColorKeys;
        private GradientAlphaKey[] baseAlphaKeys;

        private void Awake()
        {
            trailRenderer = GetComponent<TrailRenderer>();
            baseColorKeys = trailRenderer.colorGradient.colorKeys;
            baseAlphaKeys = trailRenderer.colorGradient.alphaKeys;
        }

        private void OnEnable()
        {
            ProjectileOpacitySettings.OnChanged += ApplyOpacity;
            ApplyOpacity(ProjectileOpacitySettings.Value);
        }

        private void OnDisable()
        {
            ProjectileOpacitySettings.OnChanged -= ApplyOpacity;
        }

        /// <summary>Rebuilds the trail's gradient with every alpha key scaled by opacity, so an already-flying trail updates live instead of only picking up the new value on next spawn.</summary>
        private void ApplyOpacity(float opacity)
        {
            GradientAlphaKey[] scaledAlphaKeys = new GradientAlphaKey[baseAlphaKeys.Length];
            for (int i = 0; i < baseAlphaKeys.Length; i++)
            {
                scaledAlphaKeys[i] = new GradientAlphaKey(baseAlphaKeys[i].alpha * opacity, baseAlphaKeys[i].time);
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(baseColorKeys, scaledAlphaKeys);
            trailRenderer.colorGradient = gradient;
        }
    }
}
