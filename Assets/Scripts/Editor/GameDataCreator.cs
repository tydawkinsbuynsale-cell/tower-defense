#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using RobotTD.Towers;
using RobotTD.Enemies;

namespace RobotTD.Editor
{
    /// <summary>
    /// One-click creator for all TowerData and EnemyData ScriptableObject assets.
    /// Run via: Tools > Robot TD > Create All Game Data
    /// </summary>
    public static class GameDataCreator
    {
        private const string TowerPath = "Assets/Data/Towers";
        private const string EnemyPath = "Assets/Data/Enemies";

        [MenuItem("Tools/Robot TD/Create All Game Data")]
        public static void CreateAll()
        {
            CreateTowerData();
            CreateEnemyData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done",
                "All Tower and Enemy ScriptableObject assets created in:\n" +
                $"• {TowerPath}\n• {EnemyPath}",
                "OK");
        }

        // ── Towers ────────────────────────────────────────────────────────────

        [MenuItem("Tools/Robot TD/Create Tower Data")]
        public static void CreateTowerData()
        {
            EnsureDir(TowerPath);

            CreateTower("LaserTurret",      TowerType.LaserTurret,
                cost: 100,  damage: 25,  fireRate: 2f,  range: 6f, description: "Instant hit laser. High accuracy.");

            CreateTower("PlasmaCannon",     TowerType.PlasmaCannon,
                cost: 150,  damage: 40,  fireRate: 1.5f, range: 5f, description: "Plasma projectile. Energy damage.");

            CreateTower("RocketLauncher",   TowerType.RocketLauncher,
                cost: 200,  damage: 60,  fireRate: 0.8f, range: 7f, description: "Explosive splash (radius 2). Great vs groups.");

            CreateTower("FreezeTurret",     TowerType.FreezeTurret,
                cost: 150,  damage: 5,   fireRate: 1f,  range: 5f, description: "Slows enemies by 40% for 2 seconds.");

            CreateTower("ShockTower",       TowerType.ShockTower,
                cost: 200,  damage: 30,  fireRate: 1.2f, range: 6f, description: "Chain lightning hits 3 enemies.");

            CreateTower("SniperBot",        TowerType.SniperBot,
                cost: 250,  damage: 100, fireRate: 0.5f, range: 12f, description: "20% crit chance. Ideal vs Bosses.");

            CreateTower("Flamethrower",     TowerType.Flamethrower,
                cost: 175,  damage: 35,  fireRate: 10f, range: 4f, description: "Cone AoE burn DOT (3× stack). Fire damage.");

            CreateTower("TeslaCoil",        TowerType.TeslaCoil,
                cost: 300,  damage: 45,  fireRate: 1.5f, range: 5f, description: "Multi-target electric burst.");

            CreateTower("BuffStation",      TowerType.BuffStation,
                cost: 250,  damage: 0,   fireRate: 0f,  range: 4f, description: "+25% damage to nearby towers.");

            CreateTower("EMPTower",         TowerType.EMPTower,
                cost: 200,  damage: 20,  fireRate: 0.3f, range: 6f, description: "Disables enemy abilities for 2s.");

            CreateTower("MissileBattery",   TowerType.MissileBattery,
                cost: 350,  damage: 80,  fireRate: 1f,  range: 8f, description: "Homing missiles. Hard to dodge.");

            Debug.Log($"[GameDataCreator] Tower data created at {TowerPath}");
        }

        private static void CreateTower(string assetName, TowerType type,
            int cost, float damage, float fireRate, float range, string description)
        {
            string fullPath = $"{TowerPath}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TowerData>(fullPath) != null) return; // skip if exists

            var data = ScriptableObject.CreateInstance<TowerData>();

            // Use SerializedObject to safely set fields
            var so = new SerializedObject(data);
            so.FindProperty("towerName").stringValue     = assetName;
            so.FindProperty("towerType").enumValueIndex  = (int)type;
            so.FindProperty("description").stringValue   = description;
            so.FindProperty("cost").intValue             = cost;
            so.FindProperty("damage").floatValue         = damage;
            so.FindProperty("fireRate").floatValue       = fireRate;
            so.FindProperty("range").floatValue          = range;

            // Upgrade costs (50%, 75%, 100% of base)
            var upgrade1 = so.FindProperty("upgrade1Cost");
            var upgrade2 = so.FindProperty("upgrade2Cost");
            var upgrade3 = so.FindProperty("upgrade3Cost");
            if (upgrade1 != null) upgrade1.intValue = Mathf.RoundToInt(cost * 0.5f);
            if (upgrade2 != null) upgrade2.intValue = Mathf.RoundToInt(cost * 0.75f);
            if (upgrade3 != null) upgrade3.intValue = cost;

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, fullPath);
        }

        // ── Enemies ───────────────────────────────────────────────────────────

        [MenuItem("Tools/Robot TD/Create Enemy Data")]
        public static void CreateEnemyData()
        {
            EnsureDir(EnemyPath);

            CreateEnemy("ScoutDrone",    speed: 3.0f, maxHealth: 50,   creditReward: 10, armor: 0f,
                description: "Fast and evasive. 15% chance to dodge attacks.");

            CreateEnemy("SoldierBot",    speed: 2.0f, maxHealth: 100,  creditReward: 20, armor: 0.1f,
                description: "Balanced unit. Minor armor plating.");

            CreateEnemy("TankMech",      speed: 1.0f, maxHealth: 300,  creditReward: 50, armor: 0.3f,
                description: "Heavily armored. Slow but absorbs massive damage.");

            CreateEnemy("EliteUnit",     speed: 2.5f, maxHealth: 200,  creditReward: 40, armor: 0.15f,
                description: "Fast with regenerating shield.");

            CreateEnemy("FlyingDrone",   speed: 2.5f, maxHealth: 75,   creditReward: 25, armor: 0f,
                description: "Aerial unit. Unaffected by ground obstacles.");

            CreateEnemy("HealerBot",     speed: 2.0f, maxHealth: 80,   creditReward: 35, armor: 0f,
                description: "Restores 15HP/s to nearby allies. High priority target.");

            CreateEnemy("SplitterUnit",  speed: 2.0f, maxHealth: 150,  creditReward: 30, armor: 0.05f,
                description: "Splits into 2 smaller units on death.");

            CreateEnemy("Teleporter",    speed: 2.0f, maxHealth: 100,  creditReward: 45, armor: 0f,
                description: "Teleports forward along path periodically.");

            CreateEnemy("BossUnit",      speed: 0.8f, maxHealth: 2000, creditReward: 200, armor: 0.2f,
                description: "Massive boss. Regenerates HP and enrages below 30%.");

            Debug.Log($"[GameDataCreator] Enemy data created at {EnemyPath}");
        }

        private static void CreateEnemy(string assetName, float speed, float maxHealth,
            int creditReward, float armor, string description)
        {
            string fullPath = $"{EnemyPath}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(fullPath) != null) return;

            var data = ScriptableObject.CreateInstance<EnemyData>();
            var so   = new SerializedObject(data);

            so.FindProperty("enemyName").stringValue     = assetName;
            so.FindProperty("description").stringValue   = description;
            so.FindProperty("maxHealth").floatValue      = maxHealth;
            so.FindProperty("moveSpeed").floatValue      = speed;
            so.FindProperty("creditReward").intValue     = creditReward;
            so.FindProperty("scoreValue").intValue       = creditReward * 2;

            // Armor as physical damage reduction
            var resistances = so.FindProperty("damageResistances");
            if (resistances != null && resistances.isArray && resistances.arraySize > 0)
                resistances.GetArrayElementAtIndex(0).floatValue = armor;

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, fullPath);
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        private static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
