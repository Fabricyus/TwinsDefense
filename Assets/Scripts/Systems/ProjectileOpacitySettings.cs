using System;
using UnityEngine;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Persistent 0..1 alpha applied to the player's own projectiles (AutoAttack's
    /// Projectile and the Star Upgrade's StarProjectile), for players who want to
    /// cut down on-screen clutter. Backed directly by PlayerPrefs — same
    /// placeholder-persistence rationale as KeyBindings (no project save system
    /// exists yet). OnChanged lets already-spawned projectiles live-update instead
    /// of only picking up the new value on their next spawn.
    /// </summary>
    public static class ProjectileOpacitySettings
    {
        private const string OpacityKey = "TwinsDefense.ProjectileOpacity";

        public static event Action<float> OnChanged;

        public static float Value => PlayerPrefs.GetFloat(OpacityKey, 1f);

        public static void SetValue(float opacity)
        {
            float clamped = Mathf.Clamp01(opacity);
            PlayerPrefs.SetFloat(OpacityKey, clamped);
            PlayerPrefs.Save();
            OnChanged?.Invoke(clamped);
        }
    }
}
