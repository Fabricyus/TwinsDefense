using UnityEngine;

namespace TwinsDefense.Enemies
{
    /// <summary>
    /// Diamond's ranged attack: reuses EnemyRangedAttacker's windup/telegraph
    /// (walk toward player, stop, punch-scale) but fires a 4-way cross instead
    /// of a single aimed shot — one projectile each up/right/down/left. Each
    /// shot after that rotates the whole cross further clockwise, so the
    /// pattern alternates between a "+" and an "x" shape over time.
    /// </summary>
    public class DiamondRangedAttacker : EnemyRangedAttacker
    {
        [Header("Cross Pattern")]
        [Tooltip("Degrees the 4-way cross rotates clockwise after each shot.")]
        [SerializeField] private float patternRotationStep = 45f;

        private float currentRotation;

        protected override void Fire()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 direction = RotateDegrees(Vector2.up, currentRotation + i * 90f);
                SpawnProjectile(direction);
            }

            // Negative because RotateDegrees is counter-clockwise for positive
            // angles, and the pattern is meant to turn clockwise each shot.
            currentRotation = (currentRotation - patternRotationStep) % 360f;
        }
    }
}
