using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Systems
{
    /// <summary>
    /// Persists which character and tier the player selected in Character
    /// Selection, so Arena Run systems (CharacterProgressTracker's report
    /// hooks) know who to report progress for. Set by
    /// CharacterSelectionController's PLAY button before Arena Run loads.
    /// If nothing set a selection (e.g. Arena Run opened directly for
    /// testing), lazily defaults to Izzy, tier 1 and warns once on first read.
    /// </summary>
    public class SelectedRunContext : MonoBehaviour
    {
        private static SelectedRunContext instance;

        public static SelectedRunContext Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(SelectedRunContext));
                    instance = go.AddComponent<SelectedRunContext>();
                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        private CharacterId selectedCharacter = CharacterId.Izzy;
        private int selectedTier = 1;
        private bool hasSelection;
        private bool hasWarned;

        public CharacterId SelectedCharacter
        {
            get { WarnIfNoSelection(); return selectedCharacter; }
        }

        public int SelectedTier
        {
            get { WarnIfNoSelection(); return selectedTier; }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetSelection(CharacterId character, int tier)
        {
            selectedCharacter = character;
            selectedTier = tier;
            hasSelection = true;
        }

        private void WarnIfNoSelection()
        {
            if (hasSelection || hasWarned) return;

            hasWarned = true;
            Debug.LogWarning("SelectedRunContext: no character selection was set before this scene loaded — defaulting to Izzy, tier 1.");
        }
    }
}
