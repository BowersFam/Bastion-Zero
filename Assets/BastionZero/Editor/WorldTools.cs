using System.IO;
using BastionZero.World;
using UnityEditor;
using UnityEngine;

namespace BastionZero.EditorTools
{
    /// <summary>
    /// SYS5 authoring helpers. Idempotent: re-running an item never overwrites values that have
    /// been tuned in the inspector.
    /// </summary>
    public static class WorldTools
    {
        private const string TuningFolder = "Assets/BastionZero/Tuning";
        private const string TuningPath = TuningFolder + "/WorldCameraTuning.asset";

        [MenuItem("Bastion Zero/Tuning/Create World Camera Tuning", priority = 40)]
        public static void CreateWorldCameraTuning()
        {
            var existing = AssetDatabase.LoadAssetAtPath<WorldCameraTuning>(TuningPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                Debug.Log("[WorldTools] " + TuningPath + " already exists — left untouched.");
                return;
            }

            if (!Directory.Exists(TuningFolder))
            {
                Directory.CreateDirectory(TuningFolder);
                AssetDatabase.Refresh();
            }

            var asset = ScriptableObject.CreateInstance<WorldCameraTuning>();
            AssetDatabase.CreateAsset(asset, TuningPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            Debug.Log("[WorldTools] created " + TuningPath);
        }

        [MenuItem("Bastion Zero/World/Add Room Bounds To Selection", priority = 41)]
        public static void AddRoomBoundsToSelection()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("[WorldTools] select the room root first.");
                return;
            }

            if (go.GetComponent<CameraRoomBounds>() == null) go.AddComponent<CameraRoomBounds>();
            Debug.Log("[WorldTools] CameraRoomBounds on " + go.name + " — set center/size to the room.");
        }
    }
}
