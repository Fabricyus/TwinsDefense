using UnityEngine;
using UnityEngine.Tilemaps;
using TwinsDefense.Progression;

namespace TwinsDefense.VFX
{
    /// <summary>
    /// Tints the arena's background Tilemap (Grid/bg) based on the player's
    /// current level — a cheap way to signal escalating danger as bosses
    /// approach. Starts white, shifts to a pale blue at level 10+ and a pale
    /// purple at level 20+. Levels never decrease in a run, so this is a
    /// one-way progression, not a live threshold check.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class ArenaBackgroundColorController : MonoBehaviour
    {
        [SerializeField] private int level10Threshold = 10;
        [SerializeField] private int level20Threshold = 20;

        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color level10Color = new Color(0.788f, 0.914f, 1f); // C9E9FF
        [SerializeField] private Color level20Color = new Color(0.847f, 0.788f, 1f); // D8C9FF

        private Tilemap tilemap;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
                HandleLevelChanged(LevelManager.Instance.CurrentLevel);
            }
            else
            {
                tilemap.color = baseColor;
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(int level)
        {
            tilemap.color = level >= level20Threshold ? level20Color
                : level >= level10Threshold ? level10Color
                : baseColor;
        }
    }
}
