using UnityEngine;

namespace TwinsDefense.Data
{
    /// <summary>
    /// Full pool of draftable level-up cards plus the relative weight of each
    /// rarity tier. One instance lives in the project; CardDraftService reads
    /// from it, never hardcodes the card list.
    /// </summary>
    [CreateAssetMenu(fileName = "CardPoolConfig", menuName = "TwinsDefense/Card Pool Config")]
    public class CardPoolConfig : ScriptableObject
    {
        public CardData[] allCards;

        [Header("Rarity Weights (placeholder — designer tunes)")]
        public float commonWeight = 60f;
        public float rareWeight = 30f;
        public float epicWeight = 10f;
    }
}
