using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Colored particle trail lived on a projectile prefab itself (unlike
    /// SlowTrailVFX, which is runtime-attached/detached for a status effect,
    /// this just plays for as long as the projectile GameObject exists).
    /// Emits in World Space so already-spawned particles stay behind as the
    /// projectile flies instead of following it, fading out over their
    /// lifetime. Fully self-contained — reuses AttackCircleVFX's procedural
    /// circle sprite as the particle texture, no art asset needed.
    ///
    /// Baked-in prefabs configure trailColor in the Inspector and just work
    /// via Awake. For a runtime-only cosmetic (e.g. AutoAttack's Star Upgrade
    /// cast trail), call Configure(color) after AddComponent — it re-applies
    /// every module with the override color, safe to call even after Awake
    /// already ran once with the Inspector default.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ProjectileTrailVFX : MonoBehaviour
    {
        [SerializeField] private Color trailColor = new Color(0.55f, 0.85f, 1f, 0.85f);
        [SerializeField] private float startLifetime = 0.4f;
        [SerializeField] private float startSize = 0.2f;
        [SerializeField] private float emissionRate = 30f;

        private static Material cachedMaterial;

        private void Awake()
        {
            ApplySettings();
        }

        /// <summary>Overrides the trail color and re-applies every particle module — used to dye a runtime-attached trail instead of relying on the Inspector default.</summary>
        public void Configure(Color color)
        {
            trailColor = color;
            ApplySettings();
        }

        private void ApplySettings()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = startLifetime;
            main.startSpeed = 0.3f;
            main.startSize = startSize;
            main.startColor = trailColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 100;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = emissionRate;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(trailColor.a, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            ParticleSystemRenderer particleRenderer = GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = GetParticleMaterial();
            particleRenderer.sortingOrder = 4;

            ps.Play();
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
