using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Seam between the Character Selection screen and the real meta-progression
    /// source (unlock-by-achievement tracking, star cost formulas), which is
    /// still being designed. Implementations provide the 12 character slots and
    /// handle upgrade requests.
    /// </summary>
    public interface ICharacterProgressionProvider
    {
        /// <summary>Ordered to match the grid built by CharacterSelectionController (see its slotUIs field).</summary>
        List<CharacterSlotData> GetAllSlots();

        /// <summary>Fired by the UPGRADE button for the currently selected slot.</summary>
        void RequestUpgrade(string slotId);
    }

    [System.Serializable]
    public class CharacterSlotData
    {
        public string slotId;          // e.g. "izzy_2"
        public CharacterId characterId;
        public int tier;               // 1-4
        public string displayName;     // English, in-game text
        public string description;     // English, in-game text
        public Sprite icon;
        public Sprite lockedSilhouette;
        public bool isUnlocked;
        public int attackStars;        // current pips filled, sword track
        public int attackStarsMax;
        public int defenseStars;       // current pips filled, shield track
        public int defenseStarsMax;
        public int upgradeCost;        // placeholder value, ignore real formula for now
        public int attackPipCount;     // floor(baseStats.damage / 5) — raw power magnitude, independent of the Star Upgrade track above
        public int defensePipCount;    // floor(baseStats.defense / 5) — raw power magnitude, independent of the Star Upgrade track above
    }
}
