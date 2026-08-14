using System;
using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Pure logic for drafting level-up cards: filters out ineligible cards
    /// (stack cap reached, character-restricted), then rolls unique picks
    /// using a two-step weighted random (rarity tier first, then a card
    /// within that tier).
    /// </summary>
    public class CardDraftService
    {
        /// <summary>Rolls up to <paramref name="count"/> unique, eligible cards from the pool. Special (buff+debuff) cards are excluded — those only appear via RollSpecialCards.</summary>
        public List<CardData> RollCards(int count, CardPoolConfig pool, RunCardState state, string activeCharacterId)
        {
            List<CardData> eligible = GetEligibleCards(pool, state, activeCharacterId);
            List<CardData> drafted = new List<CardData>();

            if (eligible.Count < count)
            {
                Debug.LogWarning($"CardDraftService: only {eligible.Count} eligible card(s) available for a draft of {count}.");
            }

            int drawCount = Mathf.Min(count, eligible.Count);

            for (int i = 0; i < drawCount; i++)
            {
                CardData picked = RollOne(eligible, pool);
                drafted.Add(picked);
                eligible.Remove(picked); // no duplicates within the same draft
            }

            return drafted;
        }

        /// <summary>
        /// Rolls up to <paramref name="count"/> unique, eligible special (buff+debuff) cards, uniformly at
        /// random — specials don't use the rarity-weighted system since they're all equally "special".
        /// </summary>
        public List<CardData> RollSpecialCards(int count, CardPoolConfig pool, RunCardState state)
        {
            List<CardData> eligible = new List<CardData>();

            if (pool != null && pool.allCards != null)
            {
                foreach (CardData card in pool.allCards)
                {
                    if (card == null || !card.isSpecial) continue;
                    if (card.maxStacks > 0 && state.GetTimesPicked(card.cardId) >= card.maxStacks) continue;

                    eligible.Add(card);
                }
            }

            List<CardData> drafted = new List<CardData>();
            int drawCount = Mathf.Min(count, eligible.Count);

            for (int i = 0; i < drawCount; i++)
            {
                int index = UnityEngine.Random.Range(0, eligible.Count);
                drafted.Add(eligible[index]);
                eligible.RemoveAt(index); // no duplicates within the same draft
            }

            return drafted;
        }

        private List<CardData> GetEligibleCards(CardPoolConfig pool, RunCardState state, string activeCharacterId)
        {
            List<CardData> eligible = new List<CardData>();

            if (pool == null || pool.allCards == null) return eligible;

            foreach (CardData card in pool.allCards)
            {
                if (card == null || card.isSpecial) continue; // specials only ever appear via RollSpecialCards

                if (card.maxStacks > 0 && state.GetTimesPicked(card.cardId) >= card.maxStacks) continue;

                if (card.restrictedToCharacterIds != null && card.restrictedToCharacterIds.Length > 0
                    && Array.IndexOf(card.restrictedToCharacterIds, activeCharacterId) < 0) continue;

                eligible.Add(card);
            }

            return eligible;
        }

        private CardData RollOne(List<CardData> eligible, CardPoolConfig pool)
        {
            CardRarity rarity = RollRarity(eligible, pool);
            List<CardData> tierCards = eligible.FindAll(c => c.rarity == rarity);

            // Rarity tier ran dry (e.g. all its cards already drafted/maxed) — fall back to whatever remains.
            if (tierCards.Count == 0)
            {
                tierCards = eligible;
            }

            return RollWeighted(tierCards, c => c.rollWeight);
        }

        private CardRarity RollRarity(List<CardData> eligible, CardPoolConfig pool)
        {
            bool hasCommon = eligible.Exists(c => c.rarity == CardRarity.Common);
            bool hasRare = eligible.Exists(c => c.rarity == CardRarity.Rare);
            bool hasEpic = eligible.Exists(c => c.rarity == CardRarity.Epic);

            float total = (hasCommon ? pool.commonWeight : 0f)
                        + (hasRare ? pool.rareWeight : 0f)
                        + (hasEpic ? pool.epicWeight : 0f);

            if (total <= 0f)
            {
                return CardRarity.Common;
            }

            float roll = UnityEngine.Random.value * total;

            if (hasCommon)
            {
                if (roll < pool.commonWeight) return CardRarity.Common;
                roll -= pool.commonWeight;
            }

            if (hasRare)
            {
                if (roll < pool.rareWeight) return CardRarity.Rare;
                roll -= pool.rareWeight;
            }

            return CardRarity.Epic;
        }

        private CardData RollWeighted(List<CardData> candidates, Func<CardData, float> weightSelector)
        {
            float total = 0f;
            foreach (CardData card in candidates)
            {
                total += Mathf.Max(0f, weightSelector(card));
            }

            if (total <= 0f)
            {
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }

            float roll = UnityEngine.Random.value * total;

            foreach (CardData card in candidates)
            {
                float weight = Mathf.Max(0f, weightSelector(card));
                if (roll < weight)
                {
                    return card;
                }
                roll -= weight;
            }

            return candidates[candidates.Count - 1];
        }
    }
}
