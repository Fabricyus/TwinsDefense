using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// One-shot radial particle burst for an explosion (BombEnemy's self-detonation,
    /// ReaperEnemy's hazard-circle detonation, ExplodeOnKill procs). Fully self-contained
    /// (no prefab/art asset needed) — reuses AttackCircleVFX's procedural circle sprite as
    /// the particle texture, tinted per-caller (orange/fire by default).
    /// </summary>
    public class ExplosionVFX : MonoBehaviour
    {
        private static Material cachedMaterial;

        private static readonly Color DefaultColor = new Color(1f, 0.55f, 0.1f, 1f);

        /// <param name="radius">Rough visual radius of the burst — scales particle speed/size so a bigger explosion looks bigger, not just busier.</param>
        /// <param name="color">Base tint of the burst — defaults to orange/fire when left null. A brighter variant of this color is mixed in for a bit of per-particle variety.</param>
        public static void Spawn(Vector2 position, float radius = 1f, Color? color = null, int particleCount = 24)
        {
            const float lifetime = 0.4f;
            const float burstDuration = 0.05f;

            Color baseColor = color ?? DefaultColor;
            Color brightColor = Color.Lerp(baseColor, Color.white, 0.35f);

            GameObject obj = new GameObject("ExplosionVFX");
            obj.transform.position = position;

            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.duration = burstDuration;
            main.startLifetime = lifetime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 2f, radius * 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(radius * 0.3f, radius * 0.6f);
            main.startColor = new ParticleSystem.MinMaxGradient(baseColor, brightColor);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = particleCount + 4;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.01f, radius * 0.2f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

            ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.sortingOrder = 15;

            ps.Play();

            Destroy(obj, burstDuration + lifetime);
        }

        private static Material GetParticleMaterial()
        {
            if (cachedMaterial != null) return cachedMaterial;

            cachedMaterial = new Material(Shader.Find("Sprites/Default"));
            cachedMaterial.mainTexture = AttackCircleVFX.GetCircleSprite().texture;
            return cachedMaterial;
        }
    }
}
