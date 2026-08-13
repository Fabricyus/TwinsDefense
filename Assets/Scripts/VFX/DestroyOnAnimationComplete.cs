using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Destroys this GameObject once its Animator's current state finishes
    /// playing, instead of on a fixed timer. Used by one-shot hit-proc VFX
    /// (e.g. thunderFx_0, holyFx_0, heartFx_0) so the effect always plays out
    /// fully regardless of clip length.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DestroyOnAnimationComplete : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (animator.IsInTransition(0)) return;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
