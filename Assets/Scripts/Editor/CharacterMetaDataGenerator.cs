using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// One-shot generator for the 12 character tier meta-data assets. Run via
    /// Tools/TwinsDefense/Generate Character Meta Data. Safe to re-run:
    /// existing assets at the target paths are overwritten in place rather
    /// than duplicated.
    /// </summary>
    public static class CharacterMetaDataGenerator
    {
        private const string CharactersFolder = "Assets/Data/Characters";

        private struct CharacterDef
        {
            public string slotId;
            public CharacterId characterId;
            public int tier;
            public string displayName;
            public string description;
            public List<CharacterPassiveEffect> passiveEffects;
            public CharacterUnlockCondition unlockCondition;

            public CharacterDef(string slotId, CharacterId characterId, int tier, string displayName, string description, List<CharacterPassiveEffect> passiveEffects, CharacterUnlockCondition unlockCondition)
            {
                this.slotId = slotId;
                this.characterId = characterId;
                this.tier = tier;
                this.displayName = displayName;
                this.description = description;
                this.passiveEffects = passiveEffects;
                this.unlockCondition = unlockCondition;
            }
        }

        // ---- Passive effect helpers ----

        private static CharacterPassiveEffect GoldPerLevelMultiplier(float value) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.GoldPerLevelMultiplier,
            value = value
        };

        private static CharacterPassiveEffect XPPerLevelMultiplier(float value) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.XPPerLevelMultiplier,
            value = value
        };

        private static CharacterPassiveEffect HPPerLevel(float value) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.HPPerLevel,
            value = value
        };

        private static CharacterPassiveEffect DefensePerLevel(float value) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.DefensePerLevel,
            value = value
        };

        private static CharacterPassiveEffect RunStartBonus(RunStartStatType stat, float amount) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.RunStartBonusStat,
            runStartStat = stat,
            runStartStatValue = amount
        };

        private static CharacterPassiveEffect StunOnHit(float procChancePercent, float durationSeconds) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.StunOnHit,
            procChancePercent = procChancePercent,
            procDurationSeconds = durationSeconds
        };

        private static CharacterPassiveEffect SlowOnHit(float procChancePercent, float magnitudePercent, float durationSeconds) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.SlowOnHit,
            procChancePercent = procChancePercent,
            procMagnitudePercent = magnitudePercent,
            procDurationSeconds = durationSeconds
        };

        private static CharacterPassiveEffect ThunderStrikeOnHit(float procChancePercent, float damageMultiplier) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.ThunderStrikeOnHit,
            procChancePercent = procChancePercent,
            damageMultiplier = damageMultiplier
        };

        private static CharacterPassiveEffect ChainOnHit(float procChancePercent) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.ChainOnHit,
            procChancePercent = procChancePercent
        };

        private static CharacterPassiveEffect ExplodeOnKill(float procChancePercent, float damageMultiplier) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.ExplodeOnKill,
            procChancePercent = procChancePercent,
            damageMultiplier = damageMultiplier
        };

        private static CharacterPassiveEffect ExplodeOnKill(float procChancePercent, float damageMultiplier, Color explosionColor) => new CharacterPassiveEffect
        {
            effectType = CharacterPassiveEffectType.ExplodeOnKill,
            procChancePercent = procChancePercent,
            damageMultiplier = damageMultiplier,
            explosionColor = explosionColor
        };

        // ---- Unlock condition helpers ----

        private static CharacterUnlockCondition UnlockNone() => new CharacterUnlockCondition
        {
            type = UnlockConditionType.None
        };

        private static CharacterUnlockCondition UnlockAtLevel(int level) => new CharacterUnlockCondition
        {
            type = UnlockConditionType.ReachLevelFirstTime,
            requiredLevel = level
        };

        private static CharacterUnlockCondition UnlockByCardPicks(string cardId, int count) => new CharacterUnlockCondition
        {
            type = UnlockConditionType.AccumulateCardPicks,
            requiredCardId = cardId,
            requiredCount = count
        };

        private static CharacterUnlockCondition UnlockBySpecialCardPicks(int count) => new CharacterUnlockCondition
        {
            type = UnlockConditionType.AccumulateSpecialCardPicks,
            requiredCount = count
        };

        private static CharacterUnlockCondition UnlockByBossKill(int bossLevel, int characterTier) => new CharacterUnlockCondition
        {
            type = UnlockConditionType.KillBossAtTier,
            requiredBossLevel = bossLevel,
            requiredCharacterTier = characterTier
        };

        private static readonly CharacterDef[] Characters =
        {
            new CharacterDef("izzy_1", CharacterId.Izzy, 1, "Izzy", "Izzy gains ×2 more gold per level.",
                new List<CharacterPassiveEffect> { GoldPerLevelMultiplier(2f) },
                UnlockNone()),

            new CharacterDef("izzy_2", CharacterId.Izzy, 2, "Izzy Blaze", "Starts each run with +1 base Area of Effect. 10% chance to explode a killed enemy, dealing half your current damage to nearby enemies.",
                new List<CharacterPassiveEffect> { RunStartBonus(RunStartStatType.AreaOfEffect, 1f), ExplodeOnKill(10f, 0.5f) },
                UnlockAtLevel(10)),

            new CharacterDef("izzy_3", CharacterId.Izzy, 3, "Izzy Archer", "Starts each run with +1 base Pierce.",
                new List<CharacterPassiveEffect> { RunStartBonus(RunStartStatType.Pierce, 1f) },
                UnlockBySpecialCardPicks(10)),

            new CharacterDef("izzy_4", CharacterId.Izzy, 4, "Izzy PopStar", "Attacks have a 5% chance to stun the target for 1 second. Starts each run with +2 Projectiles.",
                new List<CharacterPassiveEffect> { StunOnHit(5f, 1f), RunStartBonus(RunStartStatType.Projectiles, 2f) },
                UnlockByBossKill(30, 3)),

            new CharacterDef("court_1", CharacterId.Court, 1, "Court", "Court gains ×2 more EXP per level.",
                new List<CharacterPassiveEffect> { XPPerLevelMultiplier(2f) },
                UnlockNone()),

            new CharacterDef("court_2", CharacterId.Court, 2, "Frost Court", "15% chance on hit to slow the target by 20% for a few seconds. 10% chance to explode a killed enemy in a burst of frost, dealing half your current damage to nearby enemies.",
                new List<CharacterPassiveEffect> { SlowOnHit(15f, 20f, 2f), ExplodeOnKill(10f, 0.5f, new Color(0.4f, 0.75f, 1f, 1f)) },
                UnlockAtLevel(10)),

            new CharacterDef("court_3", CharacterId.Court, 3, "Court Reader", "10% chance on hit to strike the enemy with a thunder bolt (300% damage).",
                new List<CharacterPassiveEffect> { ThunderStrikeOnHit(10f, 3f) },
                UnlockBySpecialCardPicks(10)),

            new CharacterDef("court_4", CharacterId.Court, 4, "Dark Court", "100% chance on hit to chain to a nearby enemy. Starts each run with +1 Pierce.",
                new List<CharacterPassiveEffect> { ChainOnHit(100f), RunStartBonus(RunStartStatType.Pierce, 1f) },
                UnlockByBossKill(30, 3)),

            new CharacterDef("ralph_1", CharacterId.Ralph, 1, "Ralph", "Ralph gains +2 Defense per level.",
                new List<CharacterPassiveEffect> { DefensePerLevel(2f) },
                UnlockNone()),

            new CharacterDef("ralph_2", CharacterId.Ralph, 2, "Priest Ralph", "Gains 10 HP per level.",
                new List<CharacterPassiveEffect> { HPPerLevel(10f) },
                UnlockAtLevel(10)),

            new CharacterDef("ralph_3", CharacterId.Ralph, 3, "Paladin Ralph", "10% chance on hit to strike the enemy with a holy thunder bolt (300% damage).",
                new List<CharacterPassiveEffect> { ThunderStrikeOnHit(10f, 3f) },
                UnlockBySpecialCardPicks(10)),

            new CharacterDef("ralph_4", CharacterId.Ralph, 4, "Cute Ralph", "100% chance on hit to slow the enemy by 5%. 10% chance on hit to strike the enemy with a fat heart (300% damage).",
                new List<CharacterPassiveEffect> { SlowOnHit(100f, 5f, 2f), ThunderStrikeOnHit(10f, 3f) },
                UnlockByBossKill(30, 3)),
        };

        [MenuItem("Tools/TwinsDefense/Generate Character Meta Data")]
        public static void GenerateCharacterMetaData()
        {
            if (!AssetDatabase.IsValidFolder(CharactersFolder))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Characters");
            }

            foreach (CharacterDef def in Characters)
            {
                string path = $"{CharactersFolder}/{def.slotId}.asset";
                CharacterMetaData meta = AssetDatabase.LoadAssetAtPath<CharacterMetaData>(path);

                if (meta == null)
                {
                    meta = ScriptableObject.CreateInstance<CharacterMetaData>();
                    AssetDatabase.CreateAsset(meta, path);
                }

                meta.characterId = def.characterId;
                meta.tier = def.tier;
                meta.slotId = def.slotId;
                meta.displayName = def.displayName;
                meta.description = def.description;
                meta.passiveEffects = def.passiveEffects;
                meta.unlockCondition = def.unlockCondition;
                meta.attackStarsMax = 5;
                meta.defenseStarsMax = 3;

                EditorUtility.SetDirty(meta);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CharacterMetaDataGenerator: generated {Characters.Length} character meta-data asset(s) at '{CharactersFolder}'.");
        }
    }
}
