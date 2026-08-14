using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Brief grey puff of smoke spawned wherever an enemy dies. Fully
    /// self-contained (no prefab/art asset needed) — reuses AttackCircleVFX's
    /// procedural circle sprite as the particle texture, drifting slowly
    /// upward and growing as it fades instead of bursting outward like
    /// ExplosionVFX.
    /// </summary>
    public class DeathSmokeVFX : MonoBehaviour
    {
        private static Material cachedMaterial;

        public static void Spawn(Vector2 position, float radius = 0.5f, int particleCount = 10)
        {
            const float lifetime = 0.7f;
            const float burstDuration = 0.05f;

            GameObject obj = new GameObject("DeathSmokeVFX");
            obj.transform.position = position;

            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.playOnAwake = true;
            main.duration = burstDuration;
            main.startLifetime = lifetime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 0.5f, radius * 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(radius * 0.8f, radius * 1.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.75f, 0.75f, 0.55f), new Color(0.95f, 0.95f, 0.95f, 0.45f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = particleCount + 4;
            main.gravityModifier = -0.15f; // slight upward drift, like rising smoke

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.01f, radius * 0.3f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.3f)); // puffs grow as they dissipate

            ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.sortingOrder = 8;

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
