using System.Collections.Generic;
using UnityEngine;

namespace TwinsDefense.Data
{
    public enum CharacterId { Izzy, Court, Ralph }

    /// <summary>
    /// One character tier/form (e.g. "Izzy Blaze" is Izzy, tier 2). One asset
    /// per tier — 12 total across the 3 characters. slotId matches
    /// CharacterSlotData.slotId so a future provider swap can key off it.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterMetaData", menuName = "TwinsDefense/Character Meta Data")]
    public class CharacterMetaData : ScriptableObject
    {
        [Header("Identity")]
        public CharacterId characterId;
        public int tier; // 1-4
        public string slotId; // e.g. "izzy_2" — matches CharacterSlotData.slotId for later provider swap
        public string displayName;
        [TextArea] public string description; // English, in-game text — final copy, not placeholder
        public Sprite icon;
        public Sprite lockedSilhouette;

        [Header("Base Stats")]
        public CharacterBaseStats baseStats = new CharacterBaseStats();

        [Header("Animation")]
        public RuntimeAnimatorController animatorController;

        [Header("Combat")]
        public GameObject projectilePrefab;
        public bool isRotatingProjectile;
        [Tooltip("Spawned at the enemy on a ThunderStrikeOnHit proc (see passiveEffects). Left unassigned for characters without that passive.")]
        public GameObject procFxPrefab;

        [Header("Passive")]
        public List<CharacterPassiveEffect> passiveEffects = new List<CharacterPassiveEffect>();

        [Header("Unlock")]
        public CharacterUnlockCondition unlockCondition;

        [Header("Star Tracks (placeholder max — designer tunes)")]
        public int attackStarsMax = 5;
        public int defenseStarsMax = 3;
    }
}
