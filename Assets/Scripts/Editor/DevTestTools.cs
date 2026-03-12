#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RobotTD.Core;
using RobotTD.Progression;
using RobotTD.Towers;
using RobotTD.Enemies;

namespace RobotTD.Editor
{
    /// <summary>
    /// Developer testing utilities for quick gameplay testing and debugging.
    /// Provides cheats, spawning tools, and state manipulation.
    /// 
    /// Opens via: Tools > Robot TD > Dev Test Tools
    /// Only available in Play Mode.
    /// </summary>
    public class DevTestTools : EditorWindow
    {
        private Vector2 scrollPosition;
        private int creditsToAdd = 1000;
        private int livesToAdd = 5;
        private int wavesToSkip = 1;
        private int xpToAdd = 500;
        private int techPointsToAdd = 5;

        private EnemyType spawnEnemyType = EnemyType.Scout;
        private int spawnCount = 1;

        private TowerType spawnTowerType = TowerType.LaserTurret;
        private Vector2 towerSpawnPos = Vector2.zero;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;

        [MenuItem("Tools/Robot TD/Dev Test Tools", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<DevTestTools>("Dev Test Tools");
            window.minSize = new Vector2(400, 600);
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("Robot TD - Developer Test Tools", headerStyle);
            EditorGUILayout.Space(4);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Test tools are only available in Play Mode.", MessageType.Info);
                if (GUILayout.Button("Enter Play Mode", GUILayout.Height(30)))
                {
                    EditorApplication.isPlaying = true;
                }
                return;
            }

            EditorGUILayout.HelpBox("⚠ These tools are for testing only. Use carefully!", MessageType.Warning);
            EditorGUILayout.Space(8);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawResourceSection();
            EditorGUILayout.Space(12);
            DrawWaveSection();
            EditorGUILayout.Space(12);
            DrawProgressionSection();
            EditorGUILayout.Space(12);
            DrawSpawnSection();
            EditorGUILayout.Space(12);
            DrawStateSection();
            EditorGUILayout.Space(12);
            DrawDebugSection();

            EditorGUILayout.EndScrollView();
        }

        private void InitStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(10, 10, 5, 5)
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    padding = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // RESOURCE MANIPULATION
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawResourceSection()
        {
            GUILayout.Label("━━━ Resources ━━━", sectionStyle);

            var gm = GameManager.Instance;
            if (gm == null)
            {
                EditorGUILayout.HelpBox("GameManager not found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Credits: {gm.Credits}", GUILayout.Width(150));
            creditsToAdd = EditorGUILayout.IntField(creditsToAdd, GUILayout.Width(80));
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                gm.AddCredits(creditsToAdd);
                Debug.Log($"[DevTest] Added {creditsToAdd} credits. Total: {gm.Credits}");
            }
            if (GUILayout.Button("Max (9999)", GUILayout.Width(100)))
            {
                gm.AddCredits(9999 - gm.Credits);
                Debug.Log($"[DevTest] Set credits to 9999");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Lives: {gm.Lives}", GUILayout.Width(150));
            livesToAdd = EditorGUILayout.IntField(livesToAdd, GUILayout.Width(80));
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                for (int i = 0; i < livesToAdd; i++)
                    gm.AddLife();
                Debug.Log($"[DevTest] Added {livesToAdd} lives. Total: {gm.Lives}");
            }
            if (GUILayout.Button("Set 99", GUILayout.Width(100)))
            {
                while (gm.Lives < 99)
                    gm.AddLife();
                Debug.Log($"[DevTest] Set lives to 99");
            }
            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // WAVE CONTROL
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawWaveSection()
        {
            GUILayout.Label("━━━ Wave Control ━━━", sectionStyle);

            var wm = WaveManager.Instance;
            if (wm == null)
            {
                EditorGUILayout.HelpBox("WaveManager not found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Current Wave: {wm.CurrentWave}", GUILayout.Width(150));
            wavesToSkip = EditorGUILayout.IntField(wavesToSkip, GUILayout.Width(80));
            if (GUILayout.Button("Skip Waves", GUILayout.Width(100)))
            {
                // This requires a skip method in WaveManager, or we can simulate
                Debug.Log($"[DevTest] Skipping {wavesToSkip} waves (killing all enemies)");
                KillAllEnemies();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Next Wave Now", GUILayout.Height(25)))
            {
                // Force start if not already running
                Debug.Log("[DevTest] Starting next wave immediately");
                wm.StartNextWave();
            }
            if (GUILayout.Button("Kill All Enemies", GUILayout.Height(25)))
            {
                KillAllEnemies();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Game Speed: {Time.timeScale:F1}x");
            if (GUILayout.Button("0.5x", GUILayout.Width(60)))
                Time.timeScale = 0.5f;
            if (GUILayout.Button("1x", GUILayout.Width(60)))
                Time.timeScale = 1f;
            if (GUILayout.Button("2x", GUILayout.Width(60)))
                Time.timeScale = 2f;
            if (GUILayout.Button("5x", GUILayout.Width(60)))
                Time.timeScale = 5f;
            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PROGRESSION
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawProgressionSection()
        {
            GUILayout.Label("━━━ Progression ━━━", sectionStyle);

            var save = SaveManager.Instance;
            if (save == null)
            {
                EditorGUILayout.HelpBox("SaveManager not found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Level: {save.PlayerLevel} | XP: {save.CurrentXP}/{save.XPForNextLevel}", GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            xpToAdd = EditorGUILayout.IntField("Add XP:", xpToAdd, GUILayout.Width(150));
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                save.AddXP(xpToAdd);
                Debug.Log($"[DevTest] Added {xpToAdd} XP");
            }
            if (GUILayout.Button("Level Up", GUILayout.Width(100)))
            {
                int needed = save.XPForNextLevel - save.CurrentXP;
                save.AddXP(needed);
                Debug.Log($"[DevTest] Added {needed} XP to level up");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Tech Points: {save.TechPoints}", GUILayout.Width(150));
            techPointsToAdd = EditorGUILayout.IntField(techPointsToAdd, GUILayout.Width(80));
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                save.AddTechPoints(techPointsToAdd);
                Debug.Log($"[DevTest] Added {techPointsToAdd} tech points");
            }
            if (GUILayout.Button("Max (50)", GUILayout.Width(100)))
            {
                save.AddTechPoints(50);
                Debug.Log($"[DevTest] Added 50 tech points");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All Achievements", GUILayout.Height(25)))
            {
                UnlockAllAchievements();
            }
            if (GUILayout.Button("Reset Achievements", GUILayout.Height(25)))
            {
                ResetAchievements();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All Tech Tree", GUILayout.Height(25)))
            {
                UnlockAllTech();
            }
            if (GUILayout.Button("Reset Tech Tree", GUILayout.Height(25)))
            {
                ResetTechTree();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SPAWNING
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawSpawnSection()
        {
            GUILayout.Label("━━━ Spawn Entities ━━━", sectionStyle);

            // Enemy spawning
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enemy Type:", GUILayout.Width(100));
            spawnEnemyType = (EnemyType)EditorGUILayout.EnumPopup(spawnEnemyType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Count:", GUILayout.Width(100));
            spawnCount = EditorGUILayout.IntField(spawnCount, GUILayout.Width(80));
            if (GUILayout.Button("Spawn Enemies", GUILayout.Height(25)))
            {
                SpawnTestEnemies(spawnEnemyType, spawnCount);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // Tower spawning
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tower Type:", GUILayout.Width(100));
            spawnTowerType = (TowerType)EditorGUILayout.EnumPopup(spawnTowerType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Position:", GUILayout.Width(100));
            towerSpawnPos = EditorGUILayout.Vector2Field("", towerSpawnPos);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Spawn Tower at Position", GUILayout.Height(25)))
            {
                SpawnTestTower(spawnTowerType, towerSpawnPos);
            }

            EditorGUILayout.HelpBox("Tip: Click in Game view to see world coordinates.", MessageType.Info);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // STATE CONTROL
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawStateSection()
        {
            GUILayout.Label("━━━ Game State ━━━", sectionStyle);

            var gm = GameManager.Instance;
            if (gm == null) return;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Win Current Level", GUILayout.Height(30)))
            {
                Debug.Log("[DevTest] Triggering victory");
                KillAllEnemies();
                // Victory will trigger automatically when wave completes
            }
            if (GUILayout.Button("Lose Current Level", GUILayout.Height(30)))
            {
                Debug.Log("[DevTest] Triggering defeat");
                while (gm.Lives > 0)
                    gm.LoseLife();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause Game", GUILayout.Height(25)))
            {
                gm.PauseGame();
            }
            if (GUILayout.Button("Resume Game", GUILayout.Height(25)))
            {
                gm.ResumeGame();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // DEBUG INFO
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawDebugSection()
        {
            GUILayout.Label("━━━ Debug Info ━━━", sectionStyle);

            var gm = GameManager.Instance;
            var wm = WaveManager.Instance;
            var save = SaveManager.Instance;

            if (gm != null)
            {
                GUILayout.Label($"Game State: {(gm.IsPaused ? "PAUSED" : "RUNNING")}");
                GUILayout.Label($"Credits: {gm.Credits} | Lives: {gm.Lives}");
            }

            if (wm != null)
            {
                GUILayout.Label($"Wave: {wm.CurrentWave} | Active Enemies: {FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length}");
            }

            if (save != null)
            {
                GUILayout.Label($"Player Level: {save.PlayerLevel} | Tech Points: {save.TechPoints}");
                GUILayout.Label($"Total Play Time: {save.TotalPlayTimeSeconds / 3600f:F1} hours");
                GUILayout.Label($"Total Games: {save.TotalGamesPlayed} | Victories: {save.TotalVictories}");
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Clear Console", GUILayout.Height(25)))
            {
                ClearConsole();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox($"FPS: {(1f / Time.deltaTime):F0} | Time Scale: {Time.timeScale}x", MessageType.None);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════════════

        private void KillAllEnemies()
        {
            var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                    enemy.TakeDamage(999999f);
            }
            Debug.Log($"[DevTest] Killed {enemies.Length} enemies");
        }

        private void SpawnTestEnemies(EnemyType type, int count)
        {
            var wm = WaveManager.Instance;
            if (wm == null)
            {
                Debug.LogWarning("[DevTest] WaveManager not found. Cannot spawn enemies.");
                return;
            }

            // This would require a public spawn method in WaveManager
            Debug.Log($"[DevTest] Spawning {count}x {type} (requires WaveManager.SpawnEnemy method)");
            // wm.SpawnEnemy(type, count);
        }

        private void SpawnTestTower(TowerType type, Vector2 position)
        {
            var placement = TowerPlacementManager.Instance;
            if (placement == null)
            {
                Debug.LogWarning("[DevTest] TowerPlacementManager not found.");
                return;
            }

            Debug.Log($"[DevTest] Spawning {type} at {position} (requires manual placement method)");
            // This would require a programmatic placement method
        }

        private void UnlockAllAchievements()
        {
            var achMgr = AchievementManager.Instance;
            if (achMgr == null)
            {
                Debug.LogWarning("[DevTest] AchievementManager not found.");
                return;
            }

            // Would need a method to unlock all achievements
            Debug.Log("[DevTest] Unlocking all achievements (requires AchievementManager.UnlockAll method)");
        }

        private void ResetAchievements()
        {
            var save = SaveManager.Instance;
            if (save != null)
            {
                save.ResetAchievements();
                Debug.Log("[DevTest] Reset all achievements");
            }
        }

        private void UnlockAllTech()
        {
            var tech = TechTree.Instance;
            if (tech == null)
            {
                Debug.LogWarning("[DevTest] TechTree not found.");
                return;
            }

            Debug.Log("[DevTest] Unlocking all tech upgrades");
            // Would require implementing unlock all method
        }

        private void ResetTechTree()
        {
            var save = SaveManager.Instance;
            if (save != null)
            {
                save.ResetTechTree();
                Debug.Log("[DevTest] Reset tech tree");
            }
        }

        private void ClearConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        private T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(sortMode);
#else
            return Object.FindObjectsOfType<T>();
#endif
        }
    }
}
#endif
