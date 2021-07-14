using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Game.Generic.PathFinding;
using Game.MonoBehaviours;

namespace Game.Editor
{
    public class PlanarGraphGenerator : EditorWindow
    {
        [MenuItem("Window/PlanarGraphGenerator")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(PlanarGraphGenerator), false, "PlanarGraphGenerator");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Generate planar graph"))
            {
                WorldNode[] nodes = FindObjectsOfType<WorldNode>();

                foreach (WorldNode node in nodes)
                {
                    // Recalculate all neighbours
                    node.RecalculateNeighbours();

                    // Make it dirty
                    EditorUtility.SetDirty(node);
                }
            }
        }
    }
}

