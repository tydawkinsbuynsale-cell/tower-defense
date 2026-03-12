using UnityEngine;
using System.Collections.Generic;

namespace RobotTD.Map
{
    /// <summary>
    /// Registry of all available maps in the game.
    /// Populated via Tools > Robot TD > Create Map Content.
    /// Used by MainMenuUI to display available maps.
    /// </summary>
    [CreateAssetMenu(fileName = "MapRegistry", menuName = "RobotTD/Map Registry")]
    public class MapRegistry : ScriptableObject
    {
        [Header("All Game Maps")]
        [Tooltip("Maps in order of progression")]
        public List<MapData> maps = new List<MapData>();

        [Header("Scene Configuration")]
        [Tooltip("Name of the gameplay scene to load")]
        public string gameplaySceneName = "GameplayScene";

        /// <summary>
        /// Get MapData by ID (name)
        /// </summary>
        public MapData GetMap(string mapId)
        {
            return maps.Find(m => m.name == mapId);
        }

        /// <summary>
        /// Get the first unlocked map (for Quick Play)
        /// </summary>
        public MapData GetFirstMap()
        {
            return maps.Count > 0 ? maps[0] : null;
        }

        /// <summary>
        /// Get map count
        /// </summary>
        public int MapCount => maps.Count;

        /// <summary>
        /// Convert MapData to UI MapEntry for display
        /// </summary>
        public UI.MapEntry ToMapEntry(MapData mapData)
        {
            if (mapData == null) return null;

            UI.MapEntry entry = new UI.MapEntry();
            entry.mapId = mapData.name;
            entry.displayName = mapData.mapName;
            entry.sceneName = gameplaySceneName;
            entry.description = mapData.description;
            entry.thumbnail = mapData.thumbnail;
            entry.difficulty = mapData.difficulty;
            entry.totalWaves = mapData.totalWaves;

            // Prerequisite: previous map must be completed
            int index = maps.IndexOf(mapData);
            if (index > 0)
            {
                entry.prerequisites = new string[] { maps[index - 1].name };
            }
            else
            {
                entry.prerequisites = new string[0];
            }

            return entry;
        }

        /// <summary>
        /// Convert all maps to UI MapEntry array
        /// </summary>
        public UI.MapEntry[] ToMapEntries()
        {
            UI.MapEntry[] entries = new UI.MapEntry[maps.Count];
            for (int i = 0; i < maps.Count; i++)
            {
                entries[i] = ToMapEntry(maps[i]);
            }
            return entries;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Auto-populate from Assets/Data/Maps folder
        /// </summary>
        [ContextMenu("Auto-Populate from Data/Maps")]
        public void AutoPopulate()
        {
            maps.Clear();

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MapData", new[] { "Assets/Data/Maps" });
            List<MapData> foundMaps = new List<MapData>();

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                MapData mapData = UnityEditor.AssetDatabase.LoadAssetAtPath<MapData>(path);
                if (mapData != null)
                {
                    foundMaps.Add(mapData);
                }
            }

            // Sort by name (Map01, Map02, etc.)
            foundMaps.Sort((a, b) => string.Compare(a.name, b.name));

            maps = foundMaps;
            UnityEditor.EditorUtility.SetDirty(this);

            Debug.Log($"[MapRegistry] Auto-populated {maps.Count} maps");
        }
#endif
    }
}
