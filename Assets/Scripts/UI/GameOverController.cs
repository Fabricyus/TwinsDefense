using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using TwinsDefense.Data;
using TwinsDefense.Player;
using TwinsDefense.Progression;
using TwinsDefense.Economy;
using TwinsDefense.Systems;

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
        [Tooltip("Forced inactive before a Mission Complete ending plays — a milestone kill/level-up can grant XP that also completes the level bar (e.g. LevelManager.CompleteCurrentLevelExp on a boss kill), which would otherwise pop the draft on top of the ending.")]
        [SerializeField] private GameObject levelUpCardsPanel;

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

        [Header("Return to Menu")]
        [Tooltip("Scene loaded when the player clicks anywhere after the total has finished revealing.")]
        [SerializeField] private string characterSelectionSceneName = "CharacterSelection";

        [Header("Achievement Popup")]
        [Tooltip("PopUpChallenge panel — shown once per newly-unlocked achievement this run, after the stat summary finishes revealing.")]
        [SerializeField] private GameObject achievementPopupPanel;
        [SerializeField] private Image achievementIconImage;
        [SerializeField] private TextMeshProUGUI achievementText;
        [SerializeField] private Color achievementStartColor = Color.white;
        [SerializeField] private Color achievementCompleteColor = new Color(0.45f, 0.85f, 0.4f, 1f);
        [SerializeField] private float achievementPopInTime = 0.5f;
        [SerializeField] private float achievementColorShiftTime = 0.4f;
        [Tooltip("How far below its resting position (in anchoredPosition units) the popup starts before tweening up into place.")]
        [SerializeField] private float achievementBelowScreenOffset = 700f;

        private CanvasGroup summaryCanvasGroup;
        private PlayerHealth playerHealth;
        private CameraFollow cameraFollow;
        private bool hasTriggered;
        private bool canReturnToCharacterSelection;
        private int finalTotal;

        private RectTransform achievementPopupRect;
        private Vector2 achievementRestPosition;
        private Sprite currentIdleSprite;
        private List<string> pendingAchievements;
        private int achievementIndex = -1;

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

            if (achievementPopupPanel != null)
            {
                achievementPopupRect = achievementPopupPanel.GetComponent<RectTransform>();
                achievementRestPosition = achievementPopupRect.anchoredPosition;
                achievementPopupPanel.SetActive(false);
            }
        }

private void Start()
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }

            cameraFollow = FindAnyObjectByType<CameraFollow>();

            PlayerCharacterData characterData = FindAnyObjectByType<PlayerCharacterData>();
            if (characterData != null && characterData.Current != null)
            {
                currentIdleSprite = characterData.Current.idleSprite;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }
        }

private void Update()
        {
            if (!canReturnToCharacterSelection || !AnyInputPressedThisFrame()) return;

            if (pendingAchievements != null && achievementIndex < pendingAchievements.Count - 1)
            {
                ShowNextAchievement();
                return;
            }

            ReturnToCharacterSelection();
        }

        private bool AnyInputPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) return true;

            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private void HandlePlayerDied()
        {
            RunGameOverSequence(null);
        }

        /// <summary>Plays the same reveal sequence as a player death, but for winning the run — swaps the headline to "Mission Complete" first. Called externally for every campaign milestone ending (first time reaching level 10, each level-20 boss kill until level 30 unlocks, and the final level-30 boss kill).</summary>
        public void TriggerMissionComplete()
        {
            if (levelUpCardsPanel != null)
            {
                levelUpCardsPanel.SetActive(false);
            }

            RunGameOverSequence("Mission Complete");
        }

        /// <summary>Shared entry point for both the death and mission-complete endings. headlineOverride null keeps whatever text gameOverTxt already has (the death case).</summary>
        private void RunGameOverSequence(string headlineOverride)
        {
            if (hasTriggered) return;
            hasTriggered = true;

            if (headlineOverride != null && gameOverTxt != null)
            {
                gameOverTxt.text = headlineOverride;
            }

            Time.timeScale = 0f;
            // Stops the hit-shake dead instead of leaving it stuck re-randomizing forever —
            // shakeTimer only counts down via Time.deltaTime, which freezes once we pause above.
            cameraFollow?.StopShake();
            PopulateStats();
            gameOverPanel.SetActive(true);
            PlaySequence();
        }

        /// <summary>Resets timeScale (frozen at 0 since death) before leaving, so Character Selection doesn't load paused.</summary>
        private void ReturnToCharacterSelection()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(characterSelectionSceneName);
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

            // Matches RevealTotal's own count-up tween time, so the achievement
            // popup (or the direct return, if there's nothing to show) only
            // starts once the player has actually seen the final number.
            yield return new WaitForSecondsRealtime(1f);
            BeginAchievementQueue();
        }

        /// <summary>Snapshots which achievements were newly unlocked this run (see AchievementUnlockTracker) and either shows the first one or, if there are none, opens the gate straight to ReturnToCharacterSelection.</summary>
        private void BeginAchievementQueue()
        {
            pendingAchievements = AchievementUnlockTracker.Instance != null
                ? AchievementUnlockTracker.Instance.GetNewlyUnlockedDescriptions()
                : new List<string>();

            achievementIndex = -1;
            canReturnToCharacterSelection = true;

            if (pendingAchievements.Count > 0)
            {
                ShowNextAchievement();
            }
        }

        private void ShowNextAchievement()
        {
            achievementIndex++;

            if (achievementPopupPanel != null) achievementPopupPanel.SetActive(true);
            if (achievementIconImage != null) achievementIconImage.sprite = currentIdleSprite;

            if (achievementText != null)
            {
                achievementText.text = pendingAchievements[achievementIndex];
                achievementText.color = achievementStartColor;
            }

            PlayAchievementPopAnimation();
        }

        /// <summary>Resets the popup to its hidden state (below its resting position, scaled to zero) then tweens both position and scale in together — the "pop up from below the screen" reveal.</summary>
        private void PlayAchievementPopAnimation()
        {
            if (achievementPopupRect == null) return;

            achievementPopupRect.anchoredPosition = new Vector2(achievementRestPosition.x, achievementRestPosition.y - achievementBelowScreenOffset);
            achievementPopupRect.localScale = Vector3.zero;

            iTween.MoveTo(achievementPopupPanel, iTween.Hash(
                "position", new Vector3(achievementRestPosition.x, achievementRestPosition.y, 0f),
                "islocal", true,
                "time", achievementPopInTime,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true
            ));

            iTween.ScaleTo(achievementPopupPanel, iTween.Hash(
                "scale", Vector3.one,
                "time", achievementPopInTime,
                "easetype", iTween.EaseType.easeOutBack,
                "ignoretimescale", true,
                "oncomplete", "PlayAchievementColorShift",
                "oncompletetarget", gameObject
            ));
        }

        /// <summary>Fades the achievement text from achievementStartColor to achievementCompleteColor once the pop-in lands.</summary>
        private void PlayAchievementColorShift()
        {
            if (achievementText == null) return;

            iTween.ValueTo(achievementPopupPanel, iTween.Hash(
                "from", 0f,
                "to", 1f,
                "time", achievementColorShiftTime,
                "ignoretimescale", true,
                "onupdate", "UpdateAchievementTextColor",
                "onupdatetarget", gameObject
            ));
        }

        private void UpdateAchievementTextColor(float t)
        {
            if (achievementText == null) return;
            achievementText.color = Color.Lerp(achievementStartColor, achievementCompleteColor, t);
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
