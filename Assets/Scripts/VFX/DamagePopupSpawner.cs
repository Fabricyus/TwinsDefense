using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Scene-wide pool for <see cref="DamagePopup"/> instances. Place one instance
    /// of this component in each gameplay scene and assign a font asset (e.g.
    /// Assets/Fonts/fibberish SDF.asset) in the Inspector. Call
    /// <see cref="Spawn"/> from anywhere damage is applied.
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        public static DamagePopupSpawner Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Material fontMaterial;
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 500;

        [Header("Pool")]
        [SerializeField] private int prewarmCount = 16;

        private readonly Queue<DamagePopup> pool = new Queue<DamagePopup>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            for (int i = 0; i < prewarmCount; i++)
            {
                pool.Enqueue(CreatePopup());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Spawns (or recycles) a floating damage number at the given world position. popupColor overrides the popup's normal/crit color when set.</summary>
        public static void Spawn(Vector3 worldPosition, float damage, bool isCrit = false, Color? popupColor = null)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.SpawnInternal(worldPosition, damage, isCrit, popupColor);
        }

        private void SpawnInternal(Vector3 worldPosition, float damage, bool isCrit, Color? popupColor)
        {
            DamagePopup popup = pool.Count > 0 ? pool.Dequeue() : CreatePopup();
            popup.transform.position = worldPosition;
            popup.gameObject.SetActive(true);
            popup.Play(damage, isCrit, popupColor, ReturnToPool);
        }

        private void ReturnToPool(DamagePopup popup)
        {
            popup.gameObject.SetActive(false);
            pool.Enqueue(popup);
        }

        private DamagePopup CreatePopup()
        {
            GameObject go = new GameObject("DamagePopup");
            go.transform.SetParent(transform, false);

            TextMeshPro label = go.AddComponent<TextMeshPro>();
            label.font = font;
            if (fontMaterial != null)
            {
                label.fontSharedMaterial = fontMaterial;
            }

            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 4f;
            label.enableWordWrapping = false;
            label.extraPadding = true;

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;

            DamagePopup popup = go.AddComponent<DamagePopup>();
            go.SetActive(false);
            return popup;
        }
    }
}
