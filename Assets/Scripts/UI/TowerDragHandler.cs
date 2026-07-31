using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TwinsDefense.Data;
using TwinsDefense.Economy;
using TwinsDefense.Placement;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Attached to a tray slot. Lets the player drag a tower from the tray and
    /// drop it onto a matching, unoccupied PlacementNode to summon it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TowerDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TowerData towerData;
        [SerializeField] private Canvas canvas;

        private GameObject dragGhost;
        private RectTransform dragGhostRect;

        public TowerData TowerData => towerData;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (towerData == null || !CanAffordAndPlace())
            {
                eventData.pointerDrag = null;
                return;
            }

            dragGhost = new GameObject($"DragGhost_{towerData.towerDisplayName}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dragGhost.transform.SetParent(canvas.transform, false);

            Image ghostImage = dragGhost.GetComponent<Image>();
            ghostImage.raycastTarget = false;
            ghostImage.color = new Color(1f, 1f, 1f, 0.5f);

            Image sourceImage = GetComponent<Image>();
            if (sourceImage != null)
            {
                ghostImage.sprite = sourceImage.sprite;
                ghostImage.color = new Color(sourceImage.color.r, sourceImage.color.g, sourceImage.color.b, 0.5f);
            }

            dragGhostRect = dragGhost.GetComponent<RectTransform>();
            dragGhostRect.sizeDelta = ((RectTransform)transform).sizeDelta;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhostRect == null) return;

            dragGhostRect.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }

            if (towerData == null || PlacementGridManager.Instance == null) return;

            PlacementNode node = PlacementGridManager.Instance.FindNodeUnderScreenPoint(eventData.position, eventData.pressEventCamera);

            if (node == null || node.IsOccupied || node.allowedCharacter != towerData.character)
            {
                return;
            }

            if (!CanAffordAndPlace()) return;

            GemsManager.Instance.Spend(towerData.gemCost);
            node.PlaceTower(towerData);
        }

        private bool CanAffordAndPlace()
        {
            bool affordable = GemsManager.Instance != null && GemsManager.Instance.HasEnough(towerData.gemCost);
            bool available = PlacementGridManager.Instance != null && !PlacementGridManager.Instance.HasActiveTowerOfCharacter(towerData.character);

            return affordable && available;
        }
    }
}
