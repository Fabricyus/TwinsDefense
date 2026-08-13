using System.Collections;
using UnityEngine;
using TMPro;
using TwinsDefense.Player;
using TwinsDefense.Progression;
using TwinsDefense.Economy;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Drives the Game Over sequence once the player dies: pauses the run,
    /// reveals the GameOver panel, and plays a scripted iTween reveal — punch
    /// + wiggle on the title, then a staggered summary stat cascade.
    ///
    /// All staggering is done with our own coroutine on WaitForSecondsRealtime
    /// rather than iTween's own "delay" hash key — iTween's delay stage runs
    /// through a plain WaitForSeconds internally, which ignores the
    /// "ignoretimescale" flag and never elapses while Time.timeScale is 0.
    /// Each individual tween still gets ignoretimescale so it animates once
    /// started.
    ///
    /// Lives on its own always-active GameObject (NOT on the GameOver panel
    /// itself, since that starts disabled and would skip this script's Start).
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTxt;
        [SerializeField] private RectTransform summaryPanel;

        [Header("Summary Stats")]
        [SerializeField] private TextMeshProUGUI monsterTxt;
        [SerializeField] private TextMeshProUGUI levelTxt;
        [SerializeField] private TextMeshProUGUI coinsTxt;
        [SerializeField] private TextMeshProUGUI totalTxt;

        [Header("Timing")]
        [SerializeField] private float summaryDelay = 1f;
        [SerializeField] private float statStagger = 0.3f;
        [SerializeField] private float statAnimTime = 0.3f;
        [SerializeField] private float totalDelay = 3f;

        private CanvasGroup summaryCanvasGroup;
        private PlayerHealth playerHealth;
        private bool hasTriggered;
        private int finalTotal;

        private void Awake()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (summaryPanel != null)
            {
                summaryCanvasGroup = summaryPanel.GetComponent<CanvasGroup>();
                if (summaryCanvasGroup == null)
                {
                    summaryCanvasGroup = summaryPanel.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }
        }

        private void HandlePlayerDied()
        {
            if (hasTriggered) return;
            hasTriggered = true;

            Time.timeScale = 0f;
            PopulateStats();
            gameOverPanel.SetActive(true);
            PlaySequence();
        }

        private void PopulateStats()
        {
            int monsters = RunStats.Instance != null ? RunStats.Instance.MonstersKilled : 0;
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;
            int coins = RunStats.Instance != null ? RunStats.Instance.CoinsCollected : 0;
            finalTotal = monsters + (coins * 10) + level * 10;
            PlayerWallet.AddCoins(finalTotal);

            if (monsterTxt != null) monsterTxt.text = monsters.ToString();
            if (levelTxt != null) levelTxt.text = level.ToString();
            if (coinsTxt != null) coinsTxt.text = coins.ToString();
            if (totalTxt != null) totalTxt.text = "0"; // counts up to finalTotal when RevealTotal() plays
        }

        private void PlaySequence()
        {
            // gameOver_txt: punch in immediately, then a rotational wiggle once it lands.
            gameOverTxt.transform.localScale = Vector3.one;
            iTween.PunchScale(gameOverTxt.gameObject, iTween.Hash(
                "amount", Vector3.one * 0.4f,
                "time", 0.4f,
                "ignoretimescale", true,
                "oncomplete", "PlayGameOverWiggle",
                "oncompletetarget", gameObject
            ));

            // Reset every reveal element to its hidden starting state up front.
            summaryPanel.localScale = Vector3.zero;
            summaryCanvasGroup.alpha = 0f;
            ResetStat(monsterTxt);
            ResetStat(levelTxt);
            ResetStat(coinsTxt);
            ResetStat(totalTxt);

            StartCoroutine(RevealSequence());
        }

        private IEnumerator RevealSequence()
        {
            yield return new WaitForSecondsRealtime(summaryDelay);
            RevealSummaryPanel();
            RevealStat(monsterTxt);

            yield return new WaitForSecondsRealtime(statStagger);
            RevealStat(levelTxt);

            yield return new WaitForSecondsRealtime(statStagger);
            RevealStat(coinsTxt);

            float elapsed = summaryDelay + statStagger * 2f;
            float remaining = totalDelay - elapsed;
            if (remaining > 0f)
            {
                yield return new WaitForSecondsRealtime(remaining);
            }

            RevealTotal();
        }

        private void RevealSummaryPanel()
        {
            iTween.ScaleTo(summaryPanel.gameObject, iTween.Hash(
                "scale", Vector3.one,
                "time", 0.4f,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true
            ));
            iTween.ValueTo(summaryPanel.gameObject, iTween.Hash(
                "from", 0f,
                "to", 1f,
                "time", 0.4f,
                "ignoretimescale", true,
                "onupdate", "UpdateSummaryAlpha",
                "onupdatetarget", gameObject
            ));
        }

        private void PlayGameOverWiggle()
        {
            iTween.ShakeRotation(gameOverTxt.gameObject, iTween.Hash(
                "amount", new Vector3(0f, 0f, 12f),
                "time", 0.4f,
                "ignoretimescale", true
            ));
        }

        private void ResetStat(TextMeshProUGUI target)
        {
            if (target == null) return;
            target.transform.localScale = Vector3.zero;
        }

        private void RevealStat(TextMeshProUGUI target)
        {
            if (target == null) return;

            iTween.ScaleTo(target.gameObject, iTween.Hash(
                "scale", Vector3.one,
                "time", statAnimTime,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true
            ));
        }

        private void UpdateSummaryAlpha(float value)
        {
            summaryCanvasGroup.alpha = value;
        }

        /// <summary>Pops the total in like the other stats, and counts its number up from 0 to finalTotal over 1 second.</summary>
        private void RevealTotal()
        {
            if (totalTxt == null) return;

            iTween.ScaleTo(totalTxt.gameObject, iTween.Hash(
                "scale", Vector3.one,
                "time", statAnimTime,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true
            ));

            iTween.ValueTo(totalTxt.gameObject, iTween.Hash(
                "from", 0f,
                "to", (float)finalTotal,
                "time", 1f,
                "easetype", iTween.EaseType.easeOutQuad,
                "ignoretimescale", true,
                "onupdate", "UpdateTotalCount",
                "onupdatetarget", gameObject
            ));
        }

        private void UpdateTotalCount(float value)
        {
            totalTxt.text = Mathf.RoundToInt(value).ToString();
        }
    }
}
