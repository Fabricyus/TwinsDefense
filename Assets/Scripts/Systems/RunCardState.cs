using System.Collections.Generic;

namespace TwinsDefense.Systems
{
    /// <summary>Tracks how many times each card has been picked during the current run.</summary>
    public class RunCardState
    {
        private readonly Dictionary<string, int> timesPicked = new Dictionary<string, int>();

        public int GetTimesPicked(string cardId)
        {
            return timesPicked.TryGetValue(cardId, out int count) ? count : 0;
        }

        public void ApplyPick(string cardId)
        {
            timesPicked[cardId] = GetTimesPicked(cardId) + 1;
        }
    }
}
