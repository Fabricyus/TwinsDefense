using System;
using TwinsDefense.Data;

namespace TwinsDefense.Systems
{
    public struct AchievementDef
    {
        public string description;
        public int required;
        public Func<int> getCurrent;
        public bool isSecret;

        public AchievementDef(string description, int required, Func<int> getCurrent, bool isSecret = false)
        {
            this.description = description;
            this.required = required;
            this.getCurrent = getCurrent;
            this.isSecret = isSecret;
        }
    }

    /// <summary>
    /// Canonical list of every achievement in the game — shared by
    /// AchievementsPanelController (the Menu screen's list, which masks
    /// isSecret entries as "???" while incomplete — see Refresh) and
    /// AchievementUnlockTracker (the Arena Run "newly unlocked" popup queue,
    /// which always shows the real description, since by definition the
    /// player just earned it) so the two can never drift out of sync. Order
    /// here is display order.
    /// </summary>
    public static class AchievementRegistry
    {
        public static readonly AchievementDef[] All =
        {
            new AchievementDef("Unlock Level 11+ by killing the Reaper", 1,
                () => CampaignProgress.Level20Unlocked ? 1 : 0),
            new AchievementDef("Unlock Level 21+ by killing the Skull", CampaignProgress.SkullKillsRequiredForLevel30,
                () => CampaignProgress.SkullKillCount),
            new AchievementDef("Complete the game by defeating the Magpie", 1,
                () => CampaignProgress.GameCompleted ? 1 : 0),

            new AchievementDef("Unlock Izzy Blaze by reaching Level 10 as Izzy", 10,
                () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Izzy)),
            new AchievementDef("Unlock Izzy Archer by picking 10 special cards as Izzy", 10,
                () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Izzy)),
            new AchievementDef("Unlock Izzy PopStar by defeating the Magpie as Izzy Archer", 1,
                () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Izzy, 30, 3) ? 1 : 0),

            new AchievementDef("Unlock Frost Court by reaching Level 10 as Court", 10,
                () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Court)),
            new AchievementDef("Unlock Court Reader by picking 10 special cards as Court", 10,
                () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Court)),
            new AchievementDef("Unlock Dark Court by defeating the Magpie as Court Reader", 1,
                () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Court, 30, 3) ? 1 : 0),

            new AchievementDef("Unlock Priest Ralph by reaching Level 10 as Ralph", 10,
                () => CharacterProgressTracker.Instance.GetHighestLevel(CharacterId.Ralph)),
            new AchievementDef("Unlock Paladin Ralph by picking 10 special cards as Ralph", 10,
                () => CharacterProgressTracker.Instance.GetSpecialCardPickCount(CharacterId.Ralph)),
            new AchievementDef("Unlock Cute Ralph by defeating the Magpie as Paladin Ralph", 1,
                () => CharacterProgressTracker.Instance.HasKilledBossAtTier(CharacterId.Ralph, 30, 3) ? 1 : 0),

            // Rainbow Aura — secret, one per exact tier (matches the in-code per-tier
            // gating on PlayerRainbowAuraVFX/CharacterSlotUI, GetHighestLevelForTier).
            // Panel shows "???" for each until that specific tier reaches level 100.
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Izzy", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Izzy, 1), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Izzy Blaze", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Izzy, 2), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Izzy Archer", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Izzy, 3), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Izzy PopStar", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Izzy, 4), isSecret: true),

            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Court", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Court, 1), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Frost Court", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Court, 2), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Court Reader", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Court, 3), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Dark Court", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Court, 4), isSecret: true),

            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Ralph", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Ralph, 1), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Priest Ralph", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Ralph, 2), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Paladin Ralph", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Ralph, 3), isSecret: true),
            new AchievementDef("Unlock the Rainbow Aura by reaching Level 100 as Cute Ralph", 100,
                () => CharacterProgressTracker.Instance.GetHighestLevelForTier(CharacterId.Ralph, 4), isSecret: true),

            new AchievementDef("First Instinct: kill the Magpie as Izzy always picking the first card option", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Izzy, 1) ? 1 : 0),
            new AchievementDef("Small Blaze: kill the Magpie as Izzy Blaze without ever picking Bigger Impact or Big Bang", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Izzy, 2) ? 1 : 0),
            new AchievementDef("The Real Archer: kill the Magpie as Izzy Archer without ever picking Extra Round or Swarm Caller", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Izzy, 3) ? 1 : 0),
            new AchievementDef("Flawless Diva: kill the Magpie as Izzy PopStar without ever taking damage", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Izzy, 4) ? 1 : 0),

            new AchievementDef("Tactician, Not Brawler: kill the Magpie as Court without ever picking Sharper Edge or Glass Cannon", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Court, 1) ? 1 : 0),
            new AchievementDef("Never Melt: kill the Magpie as Frost Court without ever picking Quick Feet or Sugar Rush", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Court, 2) ? 1 : 0),
            new AchievementDef("Storm Reader: kill the Magpie as Court Reader without ever picking a Crit Chance or Crit Damage card", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Court, 3) ? 1 : 0),
            new AchievementDef("One True Chain: kill the Magpie as Dark Court without ever picking Piercing Shot", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Court, 4) ? 1 : 0),

            new AchievementDef("Iron Wall: kill the Magpie as Ralph without ever picking Iron Skin or Guardian's Bargain", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Ralph, 1) ? 1 : 0),
            new AchievementDef("Humble Priest: kill the Magpie as Priest Ralph without ever picking Vital Boost or Stone Twin", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Ralph, 2) ? 1 : 0),
            new AchievementDef("Holy Solo: kill the Magpie as Paladin Ralph without ever picking a Crit Chance or Crit Damage card", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Ralph, 3) ? 1 : 0),
            new AchievementDef("Too Cute to Hit: kill the Magpie as Cute Ralph without ever taking damage", 1,
                () => CharacterProgressTracker.Instance.HasCompletedChallenge(CharacterId.Ralph, 4) ? 1 : 0),
        };

        public static bool IsComplete(int index)
        {
            AchievementDef def = All[index];
            return def.getCurrent() >= def.required;
        }
    }
}
