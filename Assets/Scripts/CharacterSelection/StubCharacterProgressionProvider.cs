using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Hardcoded placeholder data for all 12 character slots (unlock state,
    /// defense stars) — stands in for the real meta-progression source
    /// (achievement unlocks) until that system is built. Attack stars/cost
    /// come from the real CharacterStarUpgrades purchase system, not a
    /// placeholder. Slot order matches the scene's existing BgSlots
    /// hierarchy: tier-1 for Izzy/Court/Ralph first, then each character's
    /// tiers 2-4 grouped together.
    /// </summary>
    public class StubCharacterProgressionProvider : MonoBehaviour, ICharacterProgressionProvider
    {
        private const int DefenseStarsMax = 3;

        [Tooltip("Shared source for each tier's icon/lockedSilhouette/displayName/description.")]
        [SerializeField] private CharacterMetaDataRegistry metaDataRegistry;

        public List<CharacterSlotData> GetAllSlots()
        {
            return new List<CharacterSlotData>
            {
                MakeSlot("izzy_1", true, 1),
                MakeSlot("court_1", true, 1),
                MakeSlot("ralph_1", true, 1),

                MakeSlot("izzy_2", false, 0),
                MakeSlot("izzy_3", false, 0),
                MakeSlot("izzy_4", false, 0),

                MakeSlot("court_2", false, 0),
                MakeSlot("court_3", false, 0),
                MakeSlot("court_4", false, 0),

                MakeSlot("ralph_2", false, 0),
                MakeSlot("ralph_3", false, 0),
                MakeSlot("ralph_4", false, 0),
            };
        }

        public void RequestUpgrade(string slotId)
        {
            CharacterStarUpgrades.Instance.TryPurchaseStar(slotId);
        }

        private CharacterSlotData MakeSlot(string slotId, bool isUnlocked, int defenseStars)
        {
            CharacterMetaData meta = metaDataRegistry != null ? metaDataRegistry.GetBySlotId(slotId) : null;

            if (meta == null)
            {
                Debug.LogWarning($"StubCharacterProgressionProvider: no CharacterMetaData found for slotId '{slotId}' — icon will be blank.");
            }

            return new CharacterSlotData
            {
                slotId = slotId,
                characterId = meta != null ? meta.characterId : default,
                tier = meta != null ? meta.tier : 0,
                displayName = meta != null ? meta.displayName : slotId,
                description = meta != null ? meta.description : string.Empty,
                icon = meta != null ? meta.icon : null,
                lockedSilhouette = meta != null ? meta.lockedSilhouette : null,
                isUnlocked = isUnlocked,
                attackStars = CharacterStarUpgrades.Instance.GetStars(slotId),
                attackStarsMax = meta != null ? meta.attackStarsMax : CharacterStarUpgrades.MaxStars,
                defenseStars = defenseStars,
                defenseStarsMax = DefenseStarsMax,
                upgradeCost = CharacterStarUpgrades.Instance.GetNextStarCost(slotId)
            };
        }
    }
}
