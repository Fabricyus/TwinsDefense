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

            /// <summary>Optional second effect a normal (non-special) card can also apply — e.g. Vital Boost's heal alongside its Max HP bump. Leave at defaults (secondValue 0) for cards with only one effect; see CardEffectApplier.ApplyCard / CardSlotUI.BuildDescription, both gated on secondValue != 0, not isSpecial.</summary>
            public CardEffectType secondEffectType;
            public float secondValue;
            public bool secondIsPercentage;

            /// <summary>Any further effects beyond the primary/second pair — e.g. Star Round's star-exclusive damage/range/cooldown bundle. Leave null for cards with one or two effects.</summary>
            public CardEffect[] additionalEffects;

            public CardDef(string cardId, string displayName, CardEffectType effectType, float value, bool isPercentage, CardRarity rarity, int maxStacks,
                CardEffectType secondEffectType = CardEffectType.Damage, float secondValue = 0f, bool secondIsPercentage = false, CardEffect[] additionalEffects = null)
            {
                this.cardId = cardId;
                this.displayName = displayName;
                this.effectType = effectType;
                this.value = value;
                this.isPercentage = isPercentage;
                this.rarity = rarity;
                this.maxStacks = maxStacks;
                this.secondEffectType = secondEffectType;
                this.secondValue = secondValue;
                this.secondIsPercentage = secondIsPercentage;
                this.additionalEffects = additionalEffects;
            }
        }

        private static readonly CardDef[] Cards =
        {
            new CardDef("sharper_edge", "Sharper Edge", CardEffectType.Damage, 10f, true, CardRarity.Common, 0),
            new CardDef("rapid_cast", "Rapid Cast", CardEffectType.AttackFireRate, 8f, true, CardRarity.Common, 0),
            new CardDef("swift_projectile", "Swift Projectile", CardEffectType.ProjectileSpeed, 15f, true, CardRarity.Common, 0),
            new CardDef("lucky_strike", "Lucky Strike", CardEffectType.CritChance, 5f, true, CardRarity.Rare, 5),
            new CardDef("fatal_blow", "Fatal Blow", CardEffectType.CritDamage, 25f, true, CardRarity.Rare, 0),
            // Star Round: +1 Star Projectile, plus a bundle of star-exclusive bonuses (never touch
            // the player's main damage/attackRange/attackFireRate — see StarProjectileLauncher).
            new CardDef("extra_round", "Star Round", CardEffectType.StarProjectileCount, 1f, false, CardRarity.Epic, 5,
                additionalEffects: new[]
                {
                    new CardEffect { effectType = CardEffectType.StarDamageBonus, value = 10f, isPercentage = false },
                    new CardEffect { effectType = CardEffectType.StarRangeBonus, value = 10f, isPercentage = false },
                    new CardEffect { effectType = CardEffectType.StarCooldownReduction, value = -1f, isPercentage = false },
                }),
            new CardDef("piercing_shot", "Piercing Shot", CardEffectType.Pierce, 1f, false, CardRarity.Epic, 4),
            new CardDef("wider_reach", "Wider Reach", CardEffectType.AttackRange, 10f, true, CardRarity.Common, 0),
            new CardDef("bigger_impact", "Bigger Impact", CardEffectType.AreaOfEffect, 10f, true, CardRarity.Rare, 0),
            new CardDef("vital_boost", "Vital Boost", CardEffectType.MaxHP, 20f, false, CardRarity.Common, 0, CardEffectType.InstantHeal, 20f, false),
            new CardDef("iron_skin", "Iron Skin", CardEffectType.Defense, 15f, false, CardRarity.Common, 0),
            new CardDef("second_wind", "Second Wind", CardEffectType.InstantHeal, 100f, true, CardRarity.Rare, 0),
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
            new SpecialCardDef("big_bang", "Big Bang", CardEffectType.AreaOfEffect, 60f, true, CardEffectType.ProjectileSpeed, -30f, true),
            new SpecialCardDef("swarm_caller", "Swarm Caller", CardEffectType.ExtraProjectile, 1f, false, CardEffectType.Damage, -25f, true),
            new SpecialCardDef("focused_strikes", "Focused Strikes", CardEffectType.CritDamage, 100f, true, CardEffectType.CritChance, -50f, true),
            new SpecialCardDef("chain_reaction", "Chain Reaction", CardEffectType.ExplodeOnKillChance, 50f, false, CardEffectType.AreaOfEffect, -30f, true),
        };

        /// <summary>An Exclusive card: only drafted once the named character tier's Flawless Form challenge has been completed (see ChallengeDefinitions, CardDraftService.GetEligibleCards).</summary>
        private struct ExclusiveCardDef
        {
            public string cardId;
            public string displayName;
            public string slotId;
            public CharacterId requiredChallengeCharacter;
            public int requiredChallengeTier;
            public CardEffectType effectType;
            public float value;
            public bool isPercentage;

            public ExclusiveCardDef(string cardId, string displayName, string slotId, CharacterId requiredChallengeCharacter, int requiredChallengeTier, CardEffectType effectType, float value, bool isPercentage)
            {
                this.cardId = cardId;
                this.displayName = displayName;
                this.slotId = slotId;
                this.requiredChallengeCharacter = requiredChallengeCharacter;
                this.requiredChallengeTier = requiredChallengeTier;
                this.effectType = effectType;
                this.value = value;
                this.isPercentage = isPercentage;
            }
        }

        private static readonly ExclusiveCardDef[] ExclusiveCards =
        {
            // Twin Flame (izzy_1) — unlocked by Izzy Blaze's "Small Blaze" challenge (Izzy tier 2).
            new ExclusiveCardDef("twin_flame", "Twin Flame", "izzy_1", CharacterId.Izzy, 2, CardEffectType.CritChance, 20f, true),
            // Tactician's Focus (court_1) — unlocked by Court's "Tactician, Not Brawler" challenge (Court tier 1).
            new ExclusiveCardDef("tacticians_focus", "Tactician's Focus", "court_1", CharacterId.Court, 1, CardEffectType.AttackFireRate, 20f, true),
            // Loyal Heart (ralph_1) — unlocked by Ralph's "Iron Wall" challenge (Ralph tier 1).
            new ExclusiveCardDef("loyal_heart", "Loyal Heart", "ralph_1", CharacterId.Ralph, 1, CardEffectType.BlockChance, 10f, false),

            // Gut Feeling (izzy_1) — unlocked by Izzy's "First Instinct" challenge (Izzy tier 1).
            new ExclusiveCardDef("gut_feeling", "Gut Feeling", "izzy_1", CharacterId.Izzy, 1, CardEffectType.AreaOfEffect, 15f, true),
            // True Aim (izzy_3) — unlocked by Izzy Archer's "The Real Archer" challenge (Izzy tier 3).
            new ExclusiveCardDef("true_aim", "True Aim", "izzy_3", CharacterId.Izzy, 3, CardEffectType.Pierce, 2f, false),
            // Extra Round (izzy_4) — unlocked by Izzy PopStar's "Flawless Diva" challenge (Izzy tier 4).
            new ExclusiveCardDef("bonus_round", "Extra Round", "izzy_4", CharacterId.Izzy, 4, CardEffectType.ExtraProjectile, 1f, false),

            // Absolute Zero (court_2) — unlocked by Frost Court's "Never Melt" challenge (Court tier 2).
            new ExclusiveCardDef("absolute_zero", "Absolute Zero", "court_2", CharacterId.Court, 2, CardEffectType.AttackRange, 15f, true),
            // Static Strike (court_3) — unlocked by Court Reader's "Storm Reader" challenge (Court tier 3). Boosts the character's native ThunderStrikeOnHit proc chance.
            new ExclusiveCardDef("static_strike", "Static Strike", "court_3", CharacterId.Court, 3, CardEffectType.PassiveProcChanceBonus, 1f, false),
            // Dark Chain (court_4) — unlocked by Dark Court's "One True Chain" challenge (Court tier 4). Boosts the character's native ChainOnHit proc chance.
            new ExclusiveCardDef("dark_chain", "Dark Chain", "court_4", CharacterId.Court, 4, CardEffectType.PassiveProcChanceBonus, 25f, false),

            // Blessed Ward (ralph_2) — unlocked by Priest Ralph's "Humble Priest" challenge (Ralph tier 2).
            new ExclusiveCardDef("blessed_ward", "Blessed Ward", "ralph_2", CharacterId.Ralph, 2, CardEffectType.HPRegen, 1f, false),
            // Holy Strike (ralph_3) — unlocked by Paladin Ralph's "Holy Solo" challenge (Ralph tier 3). Boosts the character's native ThunderStrikeOnHit (holy bolt) proc chance.
            new ExclusiveCardDef("holy_strike", "Holy Strike", "ralph_3", CharacterId.Ralph, 3, CardEffectType.PassiveProcChanceBonus, 1f, false),
            // Cute Strike (ralph_4) — unlocked by Cute Ralph's "Too Cute to Hit" challenge (Ralph tier 4). Boosts the character's native ThunderStrikeOnHit (heart bolt) proc chance.
            new ExclusiveCardDef("cute_strike", "Cute Strike", "ralph_4", CharacterId.Ralph, 4, CardEffectType.PassiveProcChanceBonus, 5f, false),
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
                card.secondEffectType = def.secondEffectType;
                card.secondValue = def.secondValue;
                card.secondIsPercentage = def.secondIsPercentage;
                card.additionalEffects = def.additionalEffects;
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
                card.minStarsRequired = 0;
                card.requiredChallengeCharacter = def.requiredChallengeCharacter;
                card.requiredChallengeTier = def.requiredChallengeTier;

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
