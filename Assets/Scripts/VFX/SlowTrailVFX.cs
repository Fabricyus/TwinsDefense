using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Icy trailing particle effect attached to an enemy for as long as it's
    /// slowed (Court's Frost/Ralph's Cute SlowOnHit passives — see
    /// ArenaEnemy.ApplySlow, the only caller). Follows the enemy while active,
    /// but simulates in world space so emitted particles stay behind as a
    /// fading trail instead of moving with it. Fully self-contained (no
    /// prefab/art asset needed) — reuses AttackCircleVFX's procedural circle
    /// sprite as the particle texture.
    /// </summary>
    public class SlowTrailVFX : MonoBehaviour
    {
        private static Material cachedMaterial;

        private ParticleSystem trailParticles;

        public static SlowTrailVFX Attach(Transform target)
        {
            GameObject obj = new GameObject("SlowTrailVFX");
            obj.transform.SetParent(target, worldPositionStays: false);
            obj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 0.3f;
            main.startSize = 0.25f;
            main.startColor = new Color(0.55f, 0.85f, 1f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 100;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 20f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.sortingOrder = 5;

            ps.Play();

            SlowTrailVFX vfx = obj.AddComponent<SlowTrailVFX>();
            vfx.trailParticles = ps;
            return vfx;
        }

        /// <summary>Stops emitting new particles and destroys itself once the already-emitted ones finish fading, instead of cutting the trail off abruptly.</summary>
        public void StopAndFade()
        {
            if (trailParticles == null)
            {
                Destroy(gameObject);
                return;
            }

            trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(gameObject, trailParticles.main.startLifetime.constantMax);
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
