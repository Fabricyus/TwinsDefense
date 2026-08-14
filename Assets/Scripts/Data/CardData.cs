using UnityEngine;

namespace TwinsDefense.Data
{
    public enum CardRarity
    {
        Common,
        Rare,
        Epic
    }

    public enum CardEffectType
    {
        Damage,
        AttackFireRate,
        ProjectileSpeed,
        CritChance,
        CritDamage,
        ExtraProjectile,
        Pierce,
        AttackRange,
        AreaOfEffect,
        MaxHP,
        Defense,
        HPRegen,
        IFrameDuration,
        MoveSpeed,
        PickupRadius,
        XPGain,
        CoinGain,
        InstantHeal,
        ExplodeOnKillChance // appended at the end — existing assets serialize this enum as an int, inserting earlier would remap them
    }

    /// <summary>
    /// Data-driven definition of a single level-up upgrade card.
    /// One asset = one card offered in the draft. Draft/roll code and effect
    /// application should only ever read from this asset, never hardcode
    /// card numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardData", menuName = "TwinsDefense/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public string cardId;
        public string displayName; // English, in-game text
        [TextArea] public string description;
        public Sprite icon;

        [Header("Effect")]
        public CardEffectType effectType;
        public float value; // placeholder, designer tunes in Inspector
        public bool isPercentage;

        [Header("Special Card (dual buff/debuff)")]
        [Tooltip("If true, secondEffectType/secondValue is also applied when this card is picked (a debuff paired with the effect above as the buff). Used for milestone special-card drafts — see CardDraftService.RollSpecialCards.")]
        public bool isSpecial;
        public CardEffectType secondEffectType;
        public float secondValue;
        public bool secondIsPercentage;

        [Header("Rarity & Rolling")]
        public CardRarity rarity = CardRarity.Common;
        [Tooltip("Relative weight for this specific card within its rarity pool")]
        public float rollWeight = 1f;

        [Header("Stacking")]
        [Tooltip("Max times this card can be picked in a single run. 0 = unlimited")]
        public int maxStacks = 0;

        [Header("Character Restriction (optional)")]
        [Tooltip("Leave empty to allow all characters")]
        public string[] restrictedToCharacterIds;
    }
}
