using UnityEngine;
using TwinsDefense.Data;
using TwinsDefense.Progression;

namespace TwinsDefense.Player
{
    /// <summary>
    /// Applies the selected character's per-level passives (Gold/XP gain
    /// growth, flat HP heal, flat Defense growth) each time LevelManager
    /// reports a level-up.
    /// </summary>
    [RequireComponent(typeof(PlayerCharacterData))]
    public class CharacterLevelPassives : MonoBehaviour
    {
        private PlayerCharacterData characterData;
        private PlayerStats stats;
        private PlayerHealth health;

        private void Awake()
        {
            characterData = GetComponent<PlayerCharacterData>();
            stats = GetComponent<PlayerStats>();
            health = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
            }
        }

        // LevelManager also fires OnLevelChanged once at Start with the initial
        // level (0) just to sync UI, before any real level-up happened — ignore that call.
        private void HandleLevelChanged(int newLevel)
        {
            if (newLevel <= 0 || characterData.Current == null) return;

            foreach (CharacterPassiveEffect effect in characterData.Current.passiveEffects)
            {
                switch (effect.effectType)
                {
                    case CharacterPassiveEffectType.GoldPerLevelMultiplier:
                        stats.coinGainMultiplier += effect.value / 100f;
                        break;
                    case CharacterPassiveEffectType.XPPerLevelMultiplier:
                        stats.xpGainMultiplier += effect.value / 100f;
                        break;
                    case CharacterPassiveEffectType.HPPerLevel:
                        health?.Heal(effect.value);
                        break;
                    case CharacterPassiveEffectType.DefensePerLevel:
                        stats.defense += effect.value;
                        break;
                }
            }
        }
    }
}
