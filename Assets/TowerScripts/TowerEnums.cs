namespace TwinsDefense.Data
{
    /// <summary>
    /// The three base characters/towers of the game.
    /// </summary>
    public enum TowerCharacter
    {
        Izzy,
        Court,
        Ralph
    }

    /// <summary>
    /// All 12 tower variants (3 base + 9 talent-tree unlockable subclasses).
    /// </summary>
    public enum TowerVariant
    {
        // Izzy branch
        IzzyBase,
        IzzyRanger,
        IzzyFireWitch,
        IzzyPopStar,

        // Court branch
        CourtBase,
        CourtIceWitch,
        CourtMegabrain,
        CourtEvil,

        // Ralph branch
        RalphBase,
        RalphThePriest,
        RalphThePaladin,
        RalphTheCute
    }

    /// <summary>
    /// Categorizes the special behavior a tower's attack/aura applies.
    /// Determines which fields of TowerEffectStats are relevant for that tower.
    /// </summary>
    public enum TowerEffectType
    {
        None,               // Plain physical/magical damage, no special effect
        Piercing,           // Single-target piercing shot (Izzy Ranger)
        AreaBurn,           // AoE damage + burn DoT (Izzy Fire Witch)
        ChainStun,          // Chain hits with temporary stun (Izzy Pop Star)
        SlowShatter,        // Slow/freeze + bonus damage on frozen targets (Court Ice Witch)
        ChainLightning,     // Damage that jumps between enemies, scales with target HP (Court Megabrain)
        DrainVulnerability, // Life drain + amplifies damage the target receives (Court Evil)
        AuraSupport         // Passive aura buffing nearby towers/map (all Ralph variants)
    }
}
