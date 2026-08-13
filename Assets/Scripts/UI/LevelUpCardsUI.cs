using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;
using TwinsDefense.Player;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Owns the level-up card draft. Intended to sit on the Cards panel
    /// itself: rolls 3 cards into Card1/2/3 whenever this GameObject is
    /// enabled (LevelManager activates it on level-up), applies the chosen
    /// card's effect on click, then hides itself and resumes the game.
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
            var drafted = draftService.RollCards(cardSlots.Length, cardPool, runState, activeCharacterId: string.Empty);

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

            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
