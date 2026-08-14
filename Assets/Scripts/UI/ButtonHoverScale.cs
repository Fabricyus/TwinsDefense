using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Generic hover feedback for menu buttons: scales up while the pointer is
    /// over it, eases back to normal on exit. Animates on unscaled time so it
    /// still works over a paused screen. Attach directly to any Button.
    /// </summary>
    public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animDuration = 0.12f;

        private Vector3 baseScale;
        private Coroutine scaleRoutine;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
                scaleRoutine = null;
            }

            transform.localScale = baseScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetScaleTarget(baseScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetScaleTarget(baseScale);
        }

        private void SetScaleTarget(Vector3 targetScale)
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(AnimateScale(targetScale));
        }

        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float t = 0f;

            while (t < animDuration)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, animDuration <= 0f ? 1f : t / animDuration);
                yield return null;
            }

            transform.localScale = targetScale;
            scaleRoutine = null;
        }
    }
}
