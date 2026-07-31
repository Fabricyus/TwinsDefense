using UnityEngine;
using TwinsDefense.Economy;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Dims and disables tray slots the player can no longer afford, driven by
    /// GemsManager.OnGemsChanged instead of polling every frame.
    /// </summary>
    public class TrayGemsWatcher : MonoBehaviour
    {
        [System.Serializable]
        private struct TraySlot
        {
            public CanvasGroup canvasGroup;
            public TowerDragHandler dragHandler;
        }

        [SerializeField] private TraySlot[] slots;

        private void OnEnable()
        {
            if (GemsManager.Instance != null)
            {
                GemsManager.Instance.OnGemsChanged += HandleGemsChanged;
                HandleGemsChanged(GemsManager.Instance.CurrentGems);
            }
        }

        private void OnDisable()
        {
            if (GemsManager.Instance != null)
            {
                GemsManager.Instance.OnGemsChanged -= HandleGemsChanged;
            }
        }

        private void HandleGemsChanged(int currentGems)
        {
            foreach (TraySlot slot in slots)
            {
                if (slot.canvasGroup == null || slot.dragHandler == null || slot.dragHandler.TowerData == null) continue;

                bool affordable = currentGems >= slot.dragHandler.TowerData.gemCost;
                slot.canvasGroup.alpha = affordable ? 1f : 0.4f;
                slot.canvasGroup.interactable = affordable;
                slot.canvasGroup.blocksRaycasts = affordable;
            }
        }
    }
}
