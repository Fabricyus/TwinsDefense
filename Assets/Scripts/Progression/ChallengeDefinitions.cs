using TwinsDefense.Data;

namespace TwinsDefense.Progression
{
    public enum ChallengeRuleType
    {
        ForbiddenCards,
        Flawless,
        AlwaysMiddleOption
    }

    /// <summary>One of the 12 "Flawless Form" challenge achievements — killing the Magpie (final boss) as this exact character tier while never breaking ruleType's restriction anywhere in the run.</summary>
    public readonly struct ChallengeDefinition
    {
        public readonly CharacterId character;
        public readonly int tier;
        public readonly string achievementName;
        public readonly ChallengeRuleType ruleType;
        public readonly string[] forbiddenCardIds;

        public ChallengeDefinition(CharacterId character, int tier, string achievementName, ChallengeRuleType ruleType, string[] forbiddenCardIds = null)
        {
            this.character = character;
            this.tier = tier;
            this.achievementName = achievementName;
            this.ruleType = ruleType;
            this.forbiddenCardIds = forbiddenCardIds ?? System.Array.Empty<string>();
        }
    }

    /// <summary>
    /// The 12 "Flawless Form" achievements, one per character tier. Forbidden
    /// card ids are always exact cardId lists, never effectType comparisons —
    /// several Special (buff+debuff) cards use a stat as their DEBUFF side
    /// (e.g. Reckless Frenzy's secondEffectType is Damage at -50%, Chain
    /// Reaction's is AreaOfEffect at -30%) and must not accidentally trip a
    /// restriction meant for the BUFF side of that same stat. Every id below
    /// was cross-checked against its CardData asset's effectType/isSpecial
    /// before being added here.
    /// </summary>
    public static class ChallengeDefinitions
    {
        public static readonly ChallengeDefinition[] All =
        {
            // Izzy (×2 gold/level) — always pick the middle-rolled card option (2nd of 3) on every level-up.
            new ChallengeDefinition(CharacterId.Izzy, 1, "First Instinct", ChallengeRuleType.AlwaysMiddleOption),
            // Izzy Blaze (+1 AoE base) — never Bigger Impact (normal) or Big Bang (special), both +Area of Effect.
            new ChallengeDefinition(CharacterId.Izzy, 2, "Small Blaze", ChallengeRuleType.ForbiddenCards, new[] { "bigger_impact", "big_bang" }),
            // Izzy Archer (+1 Pierce base) — never Swarm Caller (special), +Projectiles.
            new ChallengeDefinition(CharacterId.Izzy, 3, "The Real Archer", ChallengeRuleType.ForbiddenCards, new[] { "swarm_caller" }),
            // Izzy PopStar (+2 projectiles base) — flawless, never take damage.
            new ChallengeDefinition(CharacterId.Izzy, 4, "Flawless Diva", ChallengeRuleType.Flawless),

            // Court (×2 EXP/level) — never Sharper Edge (normal) or Glass Cannon (special), both +Damage.
            new ChallengeDefinition(CharacterId.Court, 1, "Tactician, Not Brawler", ChallengeRuleType.ForbiddenCards, new[] { "sharper_edge", "glass_cannon" }),
            // Frost Court (15% slow chance) — never Quick Feet (normal) or Sugar Rush (special), both +Move Speed.
            new ChallengeDefinition(CharacterId.Court, 2, "Never Melt", ChallengeRuleType.ForbiddenCards, new[] { "quick_feet", "sugar_rush" }),
            // Court Reader (10% 300%-damage beam) — never any Crit Chance/Crit Damage card, normal or special.
            new ChallengeDefinition(CharacterId.Court, 3, "Storm Reader", ChallengeRuleType.ForbiddenCards, new[] { "lucky_strike", "fatal_blow", "gamblers_coin", "focused_strikes" }),
            // Dark Court (100% chain, +1 pierce base) — flawless, never take damage.
            new ChallengeDefinition(CharacterId.Court, 4, "One True Chain", ChallengeRuleType.Flawless),

            // Ralph (+2 Defense/level) — never Iron Skin (normal) or Guardian's Bargain (special), both +Defense.
            new ChallengeDefinition(CharacterId.Ralph, 1, "Iron Wall", ChallengeRuleType.ForbiddenCards, new[] { "iron_skin", "guardians_bargain" }),
            // Priest Ralph (+10 HP/level) — never Vital Boost (normal) or Stone Twin (special), both +Max HP.
            new ChallengeDefinition(CharacterId.Ralph, 2, "Humble Priest", ChallengeRuleType.ForbiddenCards, new[] { "vital_boost", "stone_twin" }),
            // Paladin Ralph (10% 300%-damage holy beam) — same restriction as Court Reader, the other beam character.
            new ChallengeDefinition(CharacterId.Ralph, 3, "Holy Solo", ChallengeRuleType.ForbiddenCards, new[] { "lucky_strike", "fatal_blow", "gamblers_coin", "focused_strikes" }),
            // Cute Ralph (100% slow chance) — flawless, never take damage.
            new ChallengeDefinition(CharacterId.Ralph, 4, "Too Cute to Hit", ChallengeRuleType.Flawless),
        };

        public static bool TryFind(CharacterId character, int tier, out ChallengeDefinition definition)
        {
            foreach (ChallengeDefinition candidate in All)
            {
                if (candidate.character == character && candidate.tier == tier)
                {
                    definition = candidate;
                    return true;
                }
            }

            definition = default;
            return false;
        }
    }
}
