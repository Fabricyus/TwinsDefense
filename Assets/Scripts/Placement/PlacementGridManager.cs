using System.Collections.Generic;
using UnityEngine;
using TwinsDefense.Data;

namespace TwinsDefense.Placement
{
    /// <summary>
    /// Tracks every PlacementNode in the scene and answers placement queries for
    /// the drag-and-drop tray (which node is under the pointer, whether a
    /// character already has an active tower on the field).
    /// </summary>
    public class PlacementGridManager : MonoBehaviour
    {
        public static PlacementGridManager Instance { get; private set; }

        private readonly List<PlacementNode> nodes = new List<PlacementNode>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            nodes.AddRange(FindObjectsByType<PlacementNode>(FindObjectsSortMode.None));
        }

        public bool HasActiveTowerOfCharacter(TowerCharacter character)
        {
            foreach (PlacementNode node in nodes)
            {
                if (node.IsOccupied && node.allowedCharacter == character)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Finds the PlacementNode (if any) under the given screen-space point.</summary>
        public PlacementNode FindNodeUnderScreenPoint(Vector2 screenPoint, Camera camera)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return null;
            }

            Vector2 worldPoint = camera.ScreenToWorldPoint(screenPoint);
            PlacementNode closest = null;
            float closestSqrDistance = 0.75f * 0.75f;

            foreach (PlacementNode node in nodes)
            {
                float sqrDistance = ((Vector2)node.transform.position - worldPoint).sqrMagnitude;

                if (sqrDistance <= closestSqrDistance)
                {
                    closest = node;
                    closestSqrDistance = sqrDistance;
                }
            }

            return closest;
        }
    }
}
