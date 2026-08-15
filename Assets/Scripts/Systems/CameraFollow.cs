using UnityEngine;
using TwinsDefense.Environment;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Smoothly follows a target (the player) on the X/Y plane, keeping the camera's
    /// own Z so 2D rendering/sorting is unaffected. Runs in LateUpdate so it always
    /// reacts after the target's FixedUpdate/Update movement for that frame.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float smoothTime = 0.15f;

        private Camera cam;
        private Vector3 velocity = Vector3.zero;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeMagnitude;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>Kicks off a brief positional shake that decays linearly to zero over duration. Called by PlayerHealth on TakeDamage.</summary>
        public void Shake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }

        /// <summary>
        /// Cancels any in-progress shake immediately. Needed because shakeTimer only
        /// counts down via Time.deltaTime, which freezes at Time.timeScale = 0 — without
        /// this, a shake caught mid-decay by a pause (e.g. the Game Over screen) would
        /// stay stuck re-randomizing its offset forever instead of settling.
        /// </summary>
        public void StopShake()
        {
            shakeTimer = 0f;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude * (shakeTimer / shakeDuration);
                transform.position += (Vector3)shakeOffset;
            }

            if (ArenaBounds.Instance != null)
            {
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;
                Vector2 clamped = ArenaBounds.Instance.Clamp(transform.position, halfWidth, halfHeight);
                transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);
            }
        }
    }
}
