using UnityEngine;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Smoothly follows a target (the player) on the X/Y plane, keeping the camera's
    /// own Z so 2D rendering/sorting is unaffected. Runs in LateUpdate so it always
    /// reacts after the target's FixedUpdate/Update movement for that frame.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 velocity = Vector3.zero;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeMagnitude;

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
        }
    }
}
