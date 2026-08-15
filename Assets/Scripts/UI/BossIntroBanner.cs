using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Boss-arrival banner: a static boss portrait revealed through an iris —
    /// only the mask's RectTransform is tweened (scale 0 to 1 with an
    /// overshoot pop, then a settling wiggle), the portrait Image underneath
    /// never gets its own tween, it's just dragged along by the mask as its
    /// child. Holds fully open for holdDuration seconds, then closes the same
    /// way in reverse. Lives on the always-active Canvas (not on the banner
    /// container itself, which starts hidden) so Show() can be called again
    /// after the container's been deactivated.
    /// </summary>
    public class BossIntroBanner : MonoBehaviour
    {
        [Header("Hierarchy")]
        [Tooltip("Outer container (Canvas/bossImg) toggled active/inactive around the whole banner.")]
        [SerializeField] private GameObject container;
        [Tooltip("The mask's own RectTransform (Canvas/bossImg/mask) — this is the only thing iTween touches.")]
        [SerializeField] private RectTransform maskTransform;
        [Tooltip("The static portrait Image inside the mask (Canvas/bossImg/mask/bossImg) — sprite is swapped per boss, never animated directly.")]
        [SerializeField] private Image bossImage;

        [Header("Timing")]
        [SerializeField] private float openDuration = 0.5f;
        [Tooltip("Seconds the portrait stays fully visible once the open animation finishes.")]
        [SerializeField] private float holdDuration = 5f;
        [SerializeField] private float closeDuration = 0.35f;
        [Tooltip("Degrees of the settling wiggle played once the mask finishes opening.")]
        [SerializeField] private float openWiggleAmount = 8f;

        private bool isShowing;

        private void Awake()
        {
            if (container != null)
            {
                container.SetActive(false);
            }

            if (maskTransform != null)
            {
                maskTransform.localScale = Vector3.zero;
            }
        }

        /// <summary>Plays the reveal/hold/hide sequence for the given boss portrait. Ignored if a banner is already showing.</summary>
        public void Show(Sprite bossSprite)
        {
            if (isShowing) return;

            StartCoroutine(ShowSequence(bossSprite));
        }

        private IEnumerator ShowSequence(Sprite bossSprite)
        {
            isShowing = true;

            if (bossImage != null && bossSprite != null)
            {
                bossImage.sprite = bossSprite;
            }

            if (container != null)
            {
                container.SetActive(true);
            }

            maskTransform.localScale = Vector3.zero;
            iTween.ScaleTo(maskTransform.gameObject, iTween.Hash(
                "scale", Vector3.one,
                "time", openDuration,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true,
                "oncomplete", "PlayOpenWiggle",
                "oncompletetarget", gameObject
            ));

            yield return new WaitForSecondsRealtime(openDuration + holdDuration);

            iTween.ScaleTo(maskTransform.gameObject, iTween.Hash(
                "scale", Vector3.zero,
                "time", closeDuration,
                "easetype", iTween.EaseType.easeInBack,
                "ignoretimescale", true
            ));

            yield return new WaitForSecondsRealtime(closeDuration);

            if (container != null)
            {
                container.SetActive(false);
            }

            isShowing = false;
        }

        /// <summary>Small rotational settle once the pop-open lands, echoing GameOverController's title wiggle.</summary>
        private void PlayOpenWiggle()
        {
            iTween.ShakeRotation(maskTransform.gameObject, iTween.Hash(
                "amount", new Vector3(0f, 0f, openWiggleAmount),
                "time", 0.35f,
                "ignoretimescale", true
            ));
        }
    }
}
