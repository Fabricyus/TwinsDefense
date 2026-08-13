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
            new CardDef("lucky_strike", "Lucky Strike", CardEffectType.CritChance, 5f, false, CardRarity.Rare, 5),
            new CardDef("fatal_blow", "Fatal Blow", CardEffectType.CritDamage, 25f, true, CardRarity.Rare, 0),
            new CardDef("extra_round", "Extra Round", CardEffectType.ExtraProjectile, 1f, false, CardRarity.Epic, 0),
            new CardDef("piercing_shot", "Piercing Shot", CardEffectType.Pierce, 1f, false, CardRarity.Epic, 0),
            new CardDef("wider_reach", "Wider Reach", CardEffectType.AttackRange, 10f, true, CardRarity.Common, 0),
            new CardDef("bigger_impact", "Bigger Impact", CardEffectType.AreaOfEffect, 10f, true, CardRarity.Rare, 0),
            new CardDef("vital_boost", "Vital Boost", CardEffectType.MaxHP, 20f, false, CardRarity.Common, 0),
            new CardDef("iron_skin", "Iron Skin", CardEffectType.Defense, 5f, true, CardRarity.Common, 0),
            new CardDef("second_wind", "Second Wind", CardEffectType.HPRegen, 0.5f, false, CardRarity.Rare, 0),
            new CardDef("guardian_ward", "Guardian Ward", CardEffectType.IFrameDuration, 0.2f, false, CardRarity.Epic, 3),
            new CardDef("quick_feet", "Quick Feet", CardEffectType.MoveSpeed, 8f, true, CardRarity.Common, 0),
            new CardDef("magnet_pull", "Magnet Pull", CardEffectType.PickupRadius, 15f, true, CardRarity.Common, 0),
            new CardDef("fast_learner", "Fast Learner", CardEffectType.XPGain, 10f, true, CardRarity.Rare, 0),
            new CardDef("golden_touch", "Golden Touch", CardEffectType.CoinGain, 10f, true, CardRarity.Rare, 0),
        };

        [MenuItem("Tools/TwinsDefense/Generate Card Data")]
        public static void GenerateCardData()
        {
            if (!AssetDatabase.IsValidFolder(CardsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Cards");
            }

            List<CardData> createdCards = new List<CardData>(Cards.Length);

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
                card.rarity = def.rarity;
                card.maxStacks = def.maxStacks;
                card.rollWeight = 1f;

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

            Debug.Log($"CardDataGenerator: generated {createdCards.Count} card asset(s) + CardPoolConfig at '{CardsFolder}'.");
        }
    }
}
