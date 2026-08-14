using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;
using TwinsDefense.Player;
using TwinsDefense.Progression;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Owns the level-up card draft. Intended to sit on the Cards panel
    /// itself: rolls 3 cards into Card1/2/3 whenever this GameObject is
    /// enabled (LevelManager activates it on level-up), applies the chosen
    /// card's effect on click, then hides itself and resumes the game.
    /// Every 5th level starting at 5 (5, 10, 15...) rolls special (buff+debuff)
    /// cards instead of the normal pool.
    /// </summary>
    public class LevelUpCardsUI : MonoBehaviour
    {
        [SerializeField] private CardPoolConfig cardPool;
        [SerializeField] private CardSlotUI[] cardSlots;
        [SerializeField] private PlayerStats playerStats;

        private readonly CardDraftService draftService = new CardDraftService();
        private readonly CardEffectApplier effectApplier = new CardEffectApplier();
        private readonly RunCardState runState = new RunCardState();

        private void Awake()
        {
            if (playerStats == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerStats = playerObject.GetComponent<PlayerStats>();
                }
            }
        }

        private void OnEnable()
        {
            RollAndShowCards();
        }

        private void RollAndShowCards()
        {
            int level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 0;
            bool isSpecialMilestone = level >= 5 && level % 5 == 0;

            var drafted = isSpecialMilestone
                ? draftService.RollSpecialCards(cardSlots.Length, cardPool, runState)
                : draftService.RollCards(cardSlots.Length, cardPool, runState, activeCharacterId: string.Empty);

            // No special cards configured/eligible (e.g. all maxed out) — fall back to a normal draft rather than showing nothing.
            if (drafted.Count == 0 && isSpecialMilestone)
            {
                drafted = draftService.RollCards(cardSlots.Length, cardPool, runState, activeCharacterId: string.Empty);
            }

            for (int i = 0; i < cardSlots.Length; i++)
            {
                bool hasCard = i < drafted.Count;
                cardSlots[i].gameObject.SetActive(hasCard);

                if (hasCard)
                {
                    cardSlots[i].Show(drafted[i], HandleCardPicked);
                }
            }
        }

        private void HandleCardPicked(CardData card)
        {
            if (playerStats != null)
            {
                effectApplier.ApplyCard(card, playerStats);
            }

            runState.ApplyPick(card.cardId);
            CharacterProgressTracker.Instance.ReportCardPicked(SelectedRunContext.Instance.SelectedCharacter, card.cardId);

            if (card.isSpecial)
            {
                CharacterProgressTracker.Instance.ReportSpecialCardPicked(SelectedRunContext.Instance.SelectedCharacter);
            }

            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
