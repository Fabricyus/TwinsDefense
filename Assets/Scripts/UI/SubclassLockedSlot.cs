using UnityEngine;
using UnityEngine.EventSystems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// A locked talent-tree subclass placeholder card. No subclass is
    /// spawnable/unlockable yet, so this only ever reveals the unlock hint.
    /// </summary>
    public class SubclassLockedSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            UpgradePanelController.Instance?.ShowSubclassHint();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UpgradePanelController.Instance?.ShowSubclassHint();
        }
    }
}
