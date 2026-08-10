using System;
using TMPro;
using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Single floating damage number. Pop-scales in, rises, and fades out using
    /// iTween, mimicking the classic Ragnarok Online hit-number feel. Instances
    /// are recycled by <see cref="DamagePopupSpawner"/> instead of being
    /// destroyed, so all animation state must be fully reset in <see cref="Play"/>.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamagePopup : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private float riseHeight = 1f;
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float popScale = 1.35f;
        [SerializeField] private float horizontalJitter = 0.25f;

        [Header("Critical Hit")]
        [Tooltip("Multiplies riseHeight, lifetime and popScale for a bigger, longer-lingering crit popup.")]
        [SerializeField] private float critMotionMultiplier = 1.5f;
        [SerializeField] private float normalFontSize = 4f;
        [SerializeField] private float critFontSize = 7.5f;
        [Tooltip("Magnitude of the elastic scale punch that plays once a crit popup reaches its rest size.")]
        [SerializeField] private Vector3 critPunchAmount = new Vector3(0.5f, 0.5f, 0f);
        [SerializeField] private float critPunchTime = 0.4f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color critColor = new Color(1f, 0.82f, 0.1f);

        private TextMeshPro label;
        private Action<DamagePopup> onFinished;
        private float restScale = 1f;

        private void Awake()
        {
            label = GetComponent<TextMeshPro>();
        }

        /// <summary>Resets and plays the popup at its current position, calling back once fully faded.</summary>
        public void Play(float damage, bool isCrit, Action<DamagePopup> onFinishedCallback)
        {
            onFinished = onFinishedCallback;

            iTween.Stop(gameObject);

            float motionMultiplier = isCrit ? critMotionMultiplier : 1f;
            float playLifetime = lifetime * motionMultiplier;
            float playRiseHeight = riseHeight * motionMultiplier;
            float playPopScale = isCrit ? popScale * critMotionMultiplier : popScale;
            restScale = isCrit ? critMotionMultiplier : 1f;

            label.text = Mathf.RoundToInt(damage).ToString();
            label.color = isCrit ? critColor : normalColor;
            label.fontSize = isCrit ? critFontSize : normalFontSize;
            label.alpha = 1f;

            transform.localScale = Vector3.zero;
            transform.position += new Vector3(UnityEngine.Random.Range(-horizontalJitter, horizontalJitter), 0f, 0f);

            if (isCrit)
            {
                // Pop straight to rest size, then let PunchScale supply the elastic wave.
                iTween.ScaleTo(gameObject, iTween.Hash(
                    "scale", Vector3.one * restScale,
                    "time", 0.1f,
                    "easetype", iTween.EaseType.easeOutQuad,
                    "oncomplete", "CritPunch"
                ));
            }
            else
            {
                // Overshoot pop-in, then settle to normal scale.
                iTween.ScaleTo(gameObject, iTween.Hash(
                    "scale", Vector3.one * playPopScale,
                    "time", 0.12f,
                    "easetype", iTween.EaseType.easeOutBack,
                    "oncomplete", "SettleScale"
                ));
            }

            Vector3 targetPosition = transform.position + new Vector3(0f, playRiseHeight, 0f);
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", targetPosition,
                "time", playLifetime,
                "easetype", iTween.EaseType.easeOutCubic
            ));

            iTween.ValueTo(gameObject, iTween.Hash(
                "from", 1f,
                "to", 0f,
                "time", playLifetime * 0.45f,
                "delay", playLifetime * 0.5f,
                "easetype", iTween.EaseType.easeInQuad,
                "onupdate", "UpdateAlpha",
                "oncomplete", "Finish"
            ));
        }

        private void SettleScale()
        {
            iTween.ScaleTo(gameObject, iTween.Hash(
                "scale", Vector3.one * restScale,
                "time", 0.08f,
                "easetype", iTween.EaseType.easeInOutQuad
            ));
        }

        /// <summary>Wobbles a crit popup's scale back to rest through a decaying elastic wave.</summary>
        private void CritPunch()
        {
            iTween.PunchScale(gameObject, iTween.Hash(
                "amount", critPunchAmount,
                "time", critPunchTime
            ));
        }

        private void UpdateAlpha(float value)
        {
            label.alpha = value;
        }

        private void Finish()
        {
            onFinished?.Invoke(this);
        }

        private void OnDisable()
        {
            iTween.Stop(gameObject);
        }
    }
}
