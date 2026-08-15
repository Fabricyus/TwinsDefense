using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// One-shot generator for the placeholder level-up card assets. Run via
    /// Tools/TwinsDefense/Generate Card Data. Safe to re-run: existing assets
    /// at the target paths are overwritten in place rather than duplicated.
    /// </summary>
    public static class CardDataGenerator
    {
        private const string CardsFolder = "Assets/Data/Cards";
        private const string SpecialCardsFolder = "Assets/Data/Cards/Special";
        private const string ExclusiveCardsFolder = "Assets/Data/Cards/Exclusive";

        private struct CardDef
        {
            public string cardId;
            public string displayName;
            public CardEffectType effectType;
            public float value;
            public bool isPercentage;
            public CardRarity rarity;
            public int maxStacks;

            public CardDef(string cardId, string displayName, CardEffectType effectType, float value, bool isPercentage, CardRarity rarity, int maxStacks)
            {
                this.cardId = cardId;
                this.displayName = displayName;
                this.effectType = effectType;
                this.value = value;
                this.isPercentage = isPercentage;
                this.rarity = rarity;
                this.maxStacks = maxStacks;
            }
        }

        private static readonly CardDef[] Cards =
        {
            new CardDef("sharper_edge", "Sharper Edge", CardEffectType.Damage, 10f, true, CardRarity.Common, 0),
            new CardDef("rapid_cast", "Rapid Cast", CardEffectType.AttackFireRate, 8f, true, CardRarity.Common, 0),
            new CardDef("swift_projectile", "Swift Projectile", CardEffectType.ProjectileSpeed, 15f, true, CardRarity.Common, 0),
            new CardDef("lucky_strike", "Lucky Strike", CardEffectType.CritChance, 5f, true, CardRarity.Rare, 5),
            new CardDef("fatal_blow", "Fatal Blow", CardEffectType.CritDamage, 25f, true, CardRarity.Rare, 0),
            new CardDef("extra_round", "Extra Round", CardEffectType.ExtraProjectile, 1f, false, CardRarity.Epic, 3),
            new CardDef("piercing_shot", "Piercing Shot", CardEffectType.Pierce, 1f, false, CardRarity.Epic, 4),
            new CardDef("wider_reach", "Wider Reach", CardEffectType.AttackRange, 10f, true, CardRarity.Common, 0),
            new CardDef("bigger_impact", "Bigger Impact", CardEffectType.AreaOfEffect, 10f, true, CardRarity.Rare, 0),
            new CardDef("vital_boost", "Vital Boost", CardEffectType.MaxHP, 20f, false, CardRarity.Common, 0),
            new CardDef("iron_skin", "Iron Skin", CardEffectType.Defense, 15f, false, CardRarity.Common, 0),
            new CardDef("second_wind", "Second Wind", CardEffectType.InstantHeal, 20f, false, CardRarity.Rare, 0),
            new CardDef("guardian_ward", "Guardian Ward", CardEffectType.IFrameDuration, 0.2f, false, CardRarity.Epic, 3),
            new CardDef("quick_feet", "Quick Feet", CardEffectType.MoveSpeed, 8f, true, CardRarity.Common, 0),
            new CardDef("magnet_pull", "Magnet Pull", CardEffectType.PickupRadius, 15f, true, CardRarity.Common, 0),
            new CardDef("fast_learner", "Fast Learner", CardEffectType.XPGain, 10f, true, CardRarity.Rare, 0),
        };

        /// <summary>A milestone card offering both a buff and a debuff — see CardDraftService.RollSpecialCards.</summary>
        private struct SpecialCardDef
        {
            public string cardId;
            public string displayName;
            public CardEffectType buffEffectType;
            public float buffValue;
            public bool buffIsPercentage;
            public CardEffectType debuffEffectType;
            public float debuffValue;
            public bool debuffIsPercentage;

            public SpecialCardDef(string cardId, string displayName, CardEffectType buffEffectType, float buffValue, bool buffIsPercentage, CardEffectType debuffEffectType, float debuffValue, bool debuffIsPercentage)
            {
                this.cardId = cardId;
                this.displayName = displayName;
                this.buffEffectType = buffEffectType;
                this.buffValue = buffValue;
                this.buffIsPercentage = buffIsPercentage;
                this.debuffEffectType = debuffEffectType;
                this.debuffValue = debuffValue;
                this.debuffIsPercentage = debuffIsPercentage;
            }
        }

        private static readonly SpecialCardDef[] SpecialCards =
        {
            new SpecialCardDef("reckless_frenzy", "Reckless Frenzy", CardEffectType.AttackFireRate, 150f, true, CardEffectType.Damage, -50f, true),
            new SpecialCardDef("glass_cannon", "Glass Cannon", CardEffectType.Damage, 100f, true, CardEffectType.MaxHP, -50f, true),
            new SpecialCardDef("stone_twin", "Stone Twin", CardEffectType.MaxHP, 100f, true, CardEffectType.MoveSpeed, -30f, true),
            new SpecialCardDef("sugar_rush", "Sugar Rush", CardEffectType.MoveSpeed, 50f, true, CardEffectType.Defense, -30f, true),
            new SpecialCardDef("guardians_bargain", "Guardian's Bargain", CardEffectType.Defense, 40f, false, CardEffectType.Damage, -30f, true),
            new SpecialCardDef("gamblers_coin", "Gambler's Coin", CardEffectType.CritChance, 100f, true, CardEffectType.CritDamage, -50f, true),
            new SpecialCardDef("hoarders_curse", "Hoarder's Curse", CardEffectType.PickupRadius, 250f, true, CardEffectType.XPGain, -25f, true),
            new SpecialCardDef("big_bang", "Big Bang", CardEffectType.AreaOfEffect, 60f, true, CardEffectType.AttackFireRate, -30f, true),
            new SpecialCardDef("swarm_caller", "Swarm Caller", CardEffectType.ExtraProjectile, 1f, false, CardEffectType.Damage, -25f, true),
            new SpecialCardDef("focused_strikes", "Focused Strikes", CardEffectType.CritDamage, 100f, true, CardEffectType.CritChance, -50f, true),
            new SpecialCardDef("chain_reaction", "Chain Reaction", CardEffectType.ExplodeOnKillChance, 10f, false, CardEffectType.AttackFireRate, -20f, true),
        };

        /// <summary>A Star Upgrade reward: only drafted once the active character/tier has purchased minStarsRequired stars (see CardDraftService.GetEligibleCards).</summary>
        private struct ExclusiveCardDef
        {
            public string cardId;
            public string displayName;
            public string slotId;
            public int minStarsRequired;
            public CardEffectType effectType;
            public float value;
            public bool isPercentage;

            public ExclusiveCardDef(string cardId, string displayName, string slotId, int minStarsRequired, CardEffectType effectType, float value, bool isPercentage)
            {
                this.cardId = cardId;
                this.displayName = displayName;
                this.slotId = slotId;
                this.minStarsRequired = minStarsRequired;
                this.effectType = effectType;
                this.value = value;
                this.isPercentage = isPercentage;
            }
        }

        private static readonly ExclusiveCardDef[] ExclusiveCards =
        {
            new ExclusiveCardDef("twin_flame", "Twin Flame", "izzy_1", 3, CardEffectType.CritChance, 20f, true),
            new ExclusiveCardDef("tacticians_focus", "Tactician's Focus", "court_1", 3, CardEffectType.AttackFireRate, 20f, true),
            new ExclusiveCardDef("loyal_heart", "Loyal Heart", "ralph_1", 3, CardEffectType.MaxHP, 30f, false),
        };

        [MenuItem("Tools/TwinsDefense/Generate Card Data")]
        public static void GenerateCardData()
        {
            if (!AssetDatabase.IsValidFolder(CardsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Cards");
            }

            if (!AssetDatabase.IsValidFolder(SpecialCardsFolder))
            {
                AssetDatabase.CreateFolder(CardsFolder, "Special");
            }

            if (!AssetDatabase.IsValidFolder(ExclusiveCardsFolder))
            {
                AssetDatabase.CreateFolder(CardsFolder, "Exclusive");
            }

            List<CardData> createdCards = new List<CardData>(Cards.Length + SpecialCards.Length + ExclusiveCards.Length);

            foreach (CardDef def in Cards)
            {
                string path = $"{CardsFolder}/{def.cardId}.asset";
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(card, path);
                }

                card.cardId = def.cardId;
                card.displayName = def.displayName;
                card.effectType = def.effectType;
                card.value = def.value;
                card.isPercentage = def.isPercentage;
                card.isSpecial = false;
                card.rarity = def.rarity;
                card.maxStacks = def.maxStacks;
                card.rollWeight = 1f;

                EditorUtility.SetDirty(card);
                createdCards.Add(card);
            }

            foreach (SpecialCardDef def in SpecialCards)
            {
                string path = $"{SpecialCardsFolder}/{def.cardId}.asset";
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(card, path);
                }

                card.cardId = def.cardId;
                card.displayName = def.displayName;
                card.effectType = def.buffEffectType;
                card.value = def.buffValue;
                card.isPercentage = def.buffIsPercentage;
                card.isSpecial = true;
                card.secondEffectType = def.debuffEffectType;
                card.secondValue = def.debuffValue;
                card.secondIsPercentage = def.debuffIsPercentage;
                card.rarity = CardRarity.Epic; // unused by RollSpecialCards (no rarity weighting), kept for inspector clarity
                card.maxStacks = 0;
                card.rollWeight = 1f;

                EditorUtility.SetDirty(card);
                createdCards.Add(card);
            }

            foreach (ExclusiveCardDef def in ExclusiveCards)
            {
                string path = $"{ExclusiveCardsFolder}/{def.cardId}.asset";
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(card, path);
                }

                card.cardId = def.cardId;
                card.displayName = def.displayName;
                card.effectType = def.effectType;
                card.value = def.value;
                card.isPercentage = def.isPercentage;
                card.isSpecial = false;
                card.rarity = CardRarity.Epic;
                card.maxStacks = 1;
                card.rollWeight = 1f;
                card.restrictedToCharacterIds = new[] { def.slotId };
                card.minStarsRequired = def.minStarsRequired;

                EditorUtility.SetDirty(card);
                createdCards.Add(card);
            }

            string poolPath = $"{CardsFolder}/CardPoolConfig.asset";
            CardPoolConfig pool = AssetDatabase.LoadAssetAtPath<CardPoolConfig>(poolPath);

            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<CardPoolConfig>();
                AssetDatabase.CreateAsset(pool, poolPath);
            }

            pool.allCards = createdCards.ToArray();
            EditorUtility.SetDirty(pool);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CardDataGenerator: generated {createdCards.Count} card asset(s) ({SpecialCards.Length} special, {ExclusiveCards.Length} Star-exclusive) + CardPoolConfig at '{CardsFolder}'.");
        }
    }
}
