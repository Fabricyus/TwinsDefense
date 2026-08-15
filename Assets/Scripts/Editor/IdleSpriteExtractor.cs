using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.EditorTools
{
    /// <summary>
    /// One-shot tool that reads each CharacterMetaData's animatorController,
    /// grabs the first sprite keyframe of its first AnimationClip, and writes
    /// it into idleSprite. Run via Tools/TwinsDefense/Extract Idle Sprites
    /// whenever a character's walk animation changes. Safe to re-run.
    /// </summary>
    public static class IdleSpriteExtractor
    {
        private const string CharactersFolder = "Assets/Data/Characters";

        [MenuItem("Tools/TwinsDefense/Extract Idle Sprites")]
        public static void ExtractIdleSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterMetaData", new[] { CharactersFolder });
            int updated = 0;
            int skipped = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterMetaData meta = AssetDatabase.LoadAssetAtPath<CharacterMetaData>(path);
                if (meta == null) continue;

                Sprite firstFrame = GetFirstFrameSprite(meta.animatorController as AnimatorController);

                if (firstFrame == null)
                {
                    Debug.LogWarning($"IdleSpriteExtractor: could not find a sprite keyframe for '{meta.slotId}' ({path}) — animatorController missing, has no clips, or its first clip has no sprite curve.");
                    skipped++;
                    continue;
                }

                meta.idleSprite = firstFrame;
                EditorUtility.SetDirty(meta);
                updated++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"IdleSpriteExtractor: updated {updated} character(s), skipped {skipped}.");
        }

        private static Sprite GetFirstFrameSprite(AnimatorController controller)
        {
            if (controller == null || controller.animationClips == null || controller.animationClips.Length == 0) return null;

            AnimationClip clip = controller.animationClips[0];
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite") continue;

                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (keyframes != null && keyframes.Length > 0)
                {
                    return keyframes[0].value as Sprite;
                }
            }

            return null;
        }
    }
}
