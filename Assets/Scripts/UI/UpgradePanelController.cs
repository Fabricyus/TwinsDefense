using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TwinsDefense.Data;
using TwinsDefense.Towers;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Shows the star-upgrade card for whichever placed tower was last clicked.
    /// </summary>
    public class UpgradePanelController : MonoBehaviour
    {
        public static UpgradePanelController Instance { get; private set; }

        /// <summary>Raised whenever the panel is shown, with the tower GameObject it's showing. Used by the tutorial.</summary>
        public static event System.Action<GameObject> OnPanelShown;

        /// <summary>Raised whenever the panel is hidden. Used by the tutorial.</summary>
        public static event System.Action OnPanelHidden;


        [Header("Layout")]
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private Vector2 screenOffset = new Vector2(220f, 0f);

        [Header("Header")]
        [SerializeField] private Image headerIcon;
        [SerializeField] private TextMeshProUGUI headerName;

        [Header("Stars")]
        [SerializeField] private TextMeshProUGUI[] starGlyphs;

        [Header("Upgrade Button")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonLabel;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;


        [Header("Subclasses")]
        [SerializeField] private TextMeshProUGUI[] subclassLabels;
        [SerializeField] private GameObject subclassHint;

        private Tower currentTower;
        private TowerStarUpgrade currentUpgrade;

        private void Awake()
        {
            Instance = this;

            if (canvasRect == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) canvasRect = canvas.transform as RectTransform;
            }

            upgradeButton.onClick.AddListener(HandleUpgradeClicked);
            closeButton.onClick.AddListener(Hide);

            Tower.OnTowerClicked += HandleTowerClicked;

            Hide();
        }

private void OnDestroy()
        {
            Tower.OnTowerClicked -= HandleTowerClicked;
        }

        private void HandleTowerClicked(Tower tower)
        {
            Show(tower.gameObject);
        }


        /// <summary>Shows the panel for the given placed tower instance (needs Tower + TowerStarUpgrade).</summary>
        public void Show(GameObject towerObject)
        {
            if (towerObject == null) return;

            Tower tower = towerObject.GetComponent<Tower>();
            TowerStarUpgrade upgrade = towerObject.GetComponent<TowerStarUpgrade>();

            if (tower == null || upgrade == null || tower.Data == null) return;

            if (currentUpgrade != null)
            {
                currentUpgrade.OnStarChanged -= HandleStarChanged;
            }

            currentTower = tower;
            currentUpgrade = upgrade;
            currentUpgrade.OnStarChanged += HandleStarChanged;

            headerName.text = tower.Data.towerDisplayName.ToUpperInvariant();
            headerIcon.color = HeaderColorFor(tower.Data.character);

            for (int i = 0; i < subclassLabels.Length; i++)
            {
                subclassLabels[i].text = tower.Data.subclassNames != null && i < tower.Data.subclassNames.Length
                    ? tower.Data.subclassNames[i]
                    : string.Empty;
            }

            subclassHint.SetActive(false);

            RefreshStarDisplay();
            PositionNearTower(tower.transform.position);

            panelRoot.gameObject.SetActive(true);
            OnPanelShown?.Invoke(towerObject);

        }

        public void Hide()
        {
            if (currentUpgrade != null)
            {
                currentUpgrade.OnStarChanged -= HandleStarChanged;
            }

            currentTower = null;
            currentUpgrade = null;
            panelRoot.gameObject.SetActive(false);
            OnPanelHidden?.Invoke();

        }

        /// <summary>Reveals the "unlock in the Talent Tree" hint. Called by locked subclass cards.</summary>
        public void ShowSubclassHint()
        {
            subclassHint.SetActive(true);
        }

        private void HandleUpgradeClicked()
        {
            currentUpgrade?.TryUpgrade();
        }

        private void HandleStarChanged(int newStar)
        {
            RefreshStarDisplay();
        }

        private void RefreshStarDisplay()
        {
            if (currentUpgrade == null) return;

            for (int i = 0; i < starGlyphs.Length; i++)
            {
                bool filled = (i + 1) <= currentUpgrade.CurrentStar;
                starGlyphs[i].text = filled ? "★" : "☆";
            }

            if (currentUpgrade.CanUpgrade)
            {
                int cost = currentUpgrade.NextStarCost();
                upgradeButtonLabel.text = $"UPGRADE ★{currentUpgrade.CurrentStar + 1} — {cost} Gems";
                upgradeButton.gameObject.SetActive(true);
                upgradeButton.interactable = true;
                upgradeButtonLabel.color = Color.white;
            }
            else
            {
                upgradeButtonLabel.text = "MAX LEVEL";
                upgradeButtonLabel.color = new Color(1f, 0.85f, 0.3f);
                upgradeButton.interactable = false;
            }
        }

        private void PositionNearTower(Vector3 worldPosition)
        {
            if (canvasRect == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 screenPoint = cam.WorldToScreenPoint(worldPosition);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
            panelRoot.anchoredPosition = localPoint + screenOffset;
        }

        private Color HeaderColorFor(TowerCharacter character)
        {
            switch (character)
            {
                case TowerCharacter.Izzy: return new Color(0.85f, 0.45f, 0.2f);
                case TowerCharacter.Court: return new Color(0.25f, 0.4f, 0.65f);
                default: return new Color(0.6f, 0.5f, 0.4f);
            }
        }
    }
}
