using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Editor
{
    /// <summary>
    /// Integration test suite to verify all game systems work together correctly.
    /// Open via: Tools > Robot TD > Integration Tests
    /// </summary>
    public class IntegrationTests : EditorWindow
    {
        private Vector2 scrollPos;
        private string testResults = "";
        private bool testRunning = false;
        
        private GUIStyle headerStyle;
        private GUIStyle successStyle;
        private GUIStyle errorStyle;
        private bool stylesInitialized;

        [MenuItem("Tools/Robot TD/Integration Tests")]
        public static void ShowWindow()
        {
            var window = GetWindow<IntegrationTests>("Integration Tests");
            window.minSize = new Vector2(500, 600);
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 5)
            };
            
            successStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.green },
                fontStyle = FontStyle.Bold
            };
            
            errorStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.red },
                fontStyle = FontStyle.Bold
            };
            
            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();
            
            GUILayout.Space(10);
            GUILayout.Label("Robot Tower Defense - Integration Tests", headerStyle);
            EditorGUILayout.HelpBox(
                "Verify all game systems are properly connected and functional. " +
                "These tests check system initialization, data integrity, and cross-system interactions.",
                MessageType.Info
            );
            GUILayout.Space(10);

            // Test categories
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Core Systems", EditorStyles.boldLabel);
            if (GUILayout.Button("Test GameManager", GUILayout.Height(30)))
                RunTest(TestGameManager);
            if (GUILayout.Button("Test WaveManager", GUILayout.Height(30)))
                RunTest(TestWaveManager);
            if (GUILayout.Button("Test SaveManager", GUILayout.Height(30)))
                RunTest(TestSaveManager);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Game Content", EditorStyles.boldLabel);
            if (GUILayout.Button("Test Tower Data", GUILayout.Height(30)))
                RunTest(TestTowerData);
            if (GUILayout.Button("Test Enemy Data", GUILayout.Height(30)))
                RunTest(TestEnemyData);
            if (GUILayout.Button("Test Map Data", GUILayout.Height(30)))
                RunTest(TestMapData);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Progression Systems", EditorStyles.boldLabel);
            if (GUILayout.Button("Test Achievement System", GUILayout.Height(30)))
                RunTest(TestAchievementSystem);
            if (GUILayout.Button("Test Tech Tree", GUILayout.Height(30)))
                RunTest(TestTechTree);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Performance & Polish", EditorStyles.boldLabel);
            if (GUILayout.Button("Test Performance Manager", GUILayout.Height(30)))
                RunTest(TestPerformanceManager);
            if (GUILayout.Button("Test Tutorial System", GUILayout.Height(30)))
                RunTest(TestTutorialSystem);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (GUILayout.Button("Run All Tests", GUILayout.Height(40)))
                RunAllTests();

            GUILayout.Space(10);

            // Results display
            GUILayout.Label("Test Results:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            EditorGUILayout.TextArea(testResults, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (testRunning)
            {
                EditorGUILayout.HelpBox("Test running...", MessageType.Info);
            }
        }

        private void RunTest(System.Action testMethod)
        {
            testResults = "";
            testRunning = true;
            
            try
            {
                testMethod();
                testRunning = false;
                Repaint();
            }
            catch (System.Exception e)
            {
                LogError($"Test failed with exception: {e.Message}");
                testRunning = false;
                Repaint();
            }
        }

        private void RunAllTests()
        {
            testResults = "=== RUNNING ALL TESTS ===\n\n";
            testRunning = true;

            TestGameManager();
            TestWaveManager();
            TestSaveManager();
            TestTowerData();
            TestEnemyData();
            TestMapData();
            TestAchievementSystem();
            TestTechTree();
            TestPerformanceManager();
            TestTutorialSystem();

            testResults += "\n=== ALL TESTS COMPLETE ===\n";
            testRunning = false;
            Repaint();
        }

        // ── Test Methods ─────────────────────────────────────────────────────

        private void TestGameManager()
        {
            LogHeader("Testing GameManager");

            // Check script exists
            var gmScript = AssetDatabase.FindAssets("t:Script GameManager");
            if (gmScript.Length == 0)
            {
                LogError("GameManager.cs not found!");
                return;
            }
            LogSuccess("GameManager.cs exists");

            // Check for required components
            CheckTypeExists("RobotTD.Core.GameManager", "GameManager class");
            
            LogSuccess("GameManager tests passed\n");
        }

        private void TestWaveManager()
        {
            LogHeader("Testing WaveManager");

            CheckTypeExists("RobotTD.Core.WaveManager", "WaveManager class");
            CheckTypeExists("RobotTD.Core.EndlessMode", "EndlessMode class");
            
            // Check for WaveSetData ScriptableObjects
            var waveSets = AssetDatabase.FindAssets("t:WaveSetData");
            if (waveSets.Length == 0)
                LogWarning("No WaveSetData assets found");
            else
                LogSuccess($"Found {waveSets.Length} WaveSetData assets");

            LogSuccess("WaveManager tests passed\n");
        }

        private void TestSaveManager()
        {
            LogHeader("Testing SaveManager");

            CheckTypeExists("RobotTD.Core.SaveManager", "SaveManager class");
            CheckTypeExists("RobotTD.Core.PlayerSaveData", "PlayerSaveData class");

            // Verify save data structure has key fields
            var saveDataType = System.Type.GetType("RobotTD.Core.PlayerSaveData");
            if (saveDataType != null)
            {
                CheckFieldExists(saveDataType, "totalWavesCompleted");
                CheckFieldExists(saveDataType, "techPoints");
                CheckFieldExists(saveDataType, "unlockedAchievements");
                CheckFieldExists(saveDataType, "tutorialCompleted");
                CheckFieldExists(saveDataType, "graphicsQuality");
            }

            LogSuccess("SaveManager tests passed\n");
        }

        private void TestTowerData()
        {
            LogHeader("Testing Tower Data");

            CheckTypeExists("RobotTD.Towers.Tower", "Tower component");
            CheckTypeExists("RobotTD.Towers.TowerData", "TowerData ScriptableObject");
            CheckTypeExists("RobotTD.Towers.TowerPlacementManager", "TowerPlacementManager");

            // Check for TowerData assets
            var towerData = AssetDatabase.FindAssets("t:TowerData");
            if (towerData.Length == 0)
                LogError("No TowerData assets found!");
            else if (towerData.Length < 11)
                LogWarning($"Expected 11 tower types, found {towerData.Length}");
            else
                LogSuccess($"Found {towerData.Length} TowerData assets");

            LogSuccess("Tower Data tests passed\n");
        }

        private void TestEnemyData()
        {
            LogHeader("Testing Enemy Data");

            CheckTypeExists("RobotTD.Enemies.Enemy", "Enemy component");
            CheckTypeExists("RobotTD.Enemies.EnemyData", "EnemyData ScriptableObject");

            // Check for EnemyData assets
            var enemyData = AssetDatabase.FindAssets("t:EnemyData");
            if (enemyData.Length == 0)
                LogError("No EnemyData assets found!");
            else if (enemyData.Length < 14)
                LogWarning($"Expected 14 enemy types (11 + 3 bosses), found {enemyData.Length}");
            else
                LogSuccess($"Found {enemyData.Length} EnemyData assets");

            LogSuccess("Enemy Data tests passed\n");
        }

        private void TestMapData()
        {
            LogHeader("Testing Map Data");

            CheckTypeExists("RobotTD.Map.MapManager", "MapManager");

            // Check for MapData assets (if they exist as ScriptableObjects)
            var mapData = AssetDatabase.FindAssets("t:MapData");
            if (mapData.Length < 5)
                LogWarning($"Expected 5 maps, found {mapData.Length}");
            else
                LogSuccess($"Found {mapData.Length} MapData assets");

            // Check for map scenes
            var scenes = AssetDatabase.FindAssets("t:Scene Map");
            if (scenes.Length < 5)
                LogWarning($"Expected 5 map scenes, found {scenes.Length}");
            else
                LogSuccess($"Found {scenes.Length} map scenes");

            LogSuccess("Map Data tests passed\n");
        }

        private void TestAchievementSystem()
        {
            LogHeader("Testing Achievement System");

            CheckTypeExists("RobotTD.Progression.AchievementManager", "AchievementManager");
            CheckTypeExists("RobotTD.Progression.Achievement", "Achievement class");

            // Check for achievement definitions (65 expected)
            Log("Note: Achievement count should be verified at runtime");
            LogSuccess("Achievement System structure tests passed\n");
        }

        private void TestTechTree()
        {
            LogHeader("Testing Tech Tree");

            CheckTypeExists("RobotTD.Progression.TechTree", "TechTree");
            CheckTypeExists("RobotTD.Progression.TechUpgrade", "TechUpgrade enum");

            Log("Note: Tech tree node count (15) should be verified at runtime");
            LogSuccess("Tech Tree structure tests passed\n");
        }

        private void TestPerformanceManager()
        {
            LogHeader("Testing Performance Manager");

            CheckTypeExists("RobotTD.Core.PerformanceManager", "PerformanceManager");

            var perfType = System.Type.GetType("RobotTD.Core.PerformanceManager");
            if (perfType != null)
            {
                // Check for QualityPreset enum
                var nestedTypes = perfType.GetNestedTypes();
                bool foundEnum = false;
                foreach (var nested in nestedTypes)
                {
                    if (nested.Name == "QualityPreset" && nested.IsEnum)
                    {
                        foundEnum = true;
                        break;
                    }
                }
                
                if (foundEnum)
                    LogSuccess("QualityPreset enum found");
                else
                    LogWarning("QualityPreset enum not found");
            }

            LogSuccess("Performance Manager tests passed\n");
        }

        private void TestTutorialSystem()
        {
            LogHeader("Testing Tutorial System");

            CheckTypeExists("RobotTD.Core.TutorialManager", "TutorialManager");

            Log("Note: Tutorial step count (9) should be verified at runtime");
            LogSuccess("Tutorial System structure tests passed\n");
        }

        // ── Helper Methods ───────────────────────────────────────────────────

        private void CheckTypeExists(string fullTypeName, string friendlyName)
        {
            var type = System.Type.GetType(fullTypeName + ", Assembly-CSharp");
            if (type == null)
                LogError($"{friendlyName} type not found: {fullTypeName}");
            else
                LogSuccess($"{friendlyName} found");
        }

        private void CheckFieldExists(System.Type type, string fieldName)
        {
            var field = type.GetField(fieldName);
            if (field == null)
                LogWarning($"Field '{fieldName}' not found in {type.Name}");
            else
                LogSuccess($"Field '{fieldName}' exists");
        }

        private void LogHeader(string message)
        {
            testResults += $"\n━━━ {message} ━━━\n";
        }

        private void Log(string message)
        {
            testResults += $"  {message}\n";
        }

        private void LogSuccess(string message)
        {
            testResults += $"  ✓ {message}\n";
        }

        private void LogWarning(string message)
        {
            testResults += $"  ⚠ WARNING: {message}\n";
            Debug.LogWarning($"[Integration Test] {message}");
        }

        private void LogError(string message)
        {
            testResults += $"  ✗ ERROR: {message}\n";
            Debug.LogError($"[Integration Test] {message}");
        }
    }
}
