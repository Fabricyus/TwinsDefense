using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Systems;

namespace TwinsDefense.CharacterSelection
{
    /// <summary>
    /// Assembles all 12 character slots for the selection screen. Unlock state
    /// comes from CharacterProgressTracker.IsUnlocked (evaluated against each
    /// tier's real CharacterMetaData.unlockCondition), and attack stars/cost
    /// come from the real CharacterStarUpgrades purchase system — only defense
    /// stars are still a hardcoded placeholder, pending a real system for them.
    /// Slot order matches the scene's existing BgSlots hierarchy: tier-1 for
    /// Izzy/Court/Ralph first, then each character's tiers 2-4 grouped together.
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
                MakeSlot("izzy_1", 1),
                MakeSlot("court_1", 1),
                MakeSlot("ralph_1", 1),

                MakeSlot("izzy_2", 0),
                MakeSlot("izzy_3", 0),
                MakeSlot("izzy_4", 0),

                MakeSlot("court_2", 0),
                MakeSlot("court_3", 0),
                MakeSlot("court_4", 0),

                MakeSlot("ralph_2", 0),
                MakeSlot("ralph_3", 0),
                MakeSlot("ralph_4", 0),
            };
        }

        public void RequestUpgrade(string slotId)
        {
            CharacterStarUpgrades.Instance.TryPurchaseStar(slotId);
        }

        private CharacterSlotData MakeSlot(string slotId, int defenseStars)
        {
            CharacterMetaData meta = metaDataRegistry != null ? metaDataRegistry.GetBySlotId(slotId) : null;

            if (meta == null)
            {
                Debug.LogWarning($"StubCharacterProgressionProvider: no CharacterMetaData found for slotId '{slotId}' — icon will be blank.");
            }

            // Real unlock check against CharacterProgressTracker's persisted progress (level/card-pick/boss-kill
            // reports) — a slot with no meta can't have its unlockCondition evaluated, so it stays locked.
            bool isUnlocked = meta != null && CharacterProgressTracker.Instance.IsUnlocked(meta);

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
                upgradeCost = CharacterStarUpgrades.Instance.GetNextStarCost(slotId),
                attackPipCount = meta != null ? Mathf.FloorToInt(meta.baseStats.damage / 5f) : 0,
                defensePipCount = meta != null ? Mathf.FloorToInt(meta.baseStats.defense / 5f) : 0
            };
        }
    }
}
