using System.Collections.Generic;
using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Single shared list of the 12 CharacterMetaData assets, so Character
    /// Selection (icons) and Arena Run (player link + animator swap) both
    /// resolve a character tier's data from one place instead of each scene
    /// wiring its own copy of the 12 references.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterMetaDataRegistry", menuName = "TwinsDefense/Character Meta Data Registry")]
    public class CharacterMetaDataRegistry : ScriptableObject
    {
        public List<CharacterMetaData> characters = new List<CharacterMetaData>();

        public CharacterMetaData GetBySlotId(string slotId)
        {
            return characters.Find(c => c != null && c.slotId == slotId);
        }

        public CharacterMetaData GetByCharacterAndTier(CharacterId characterId, int tier)
        {
            return characters.Find(c => c != null && c.characterId == characterId && c.tier == tier);
        }
    }
}
