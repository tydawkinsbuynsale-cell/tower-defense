#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Towers;
using RobotTD.Enemies;
using RobotTD.Map;
using RobotTD.Progression;

namespace RobotTD.Editor
{
    /// <summary>
    /// Validation tool to check for missing references, configuration errors,
    /// and potential issues before building or testing.
    /// 
    /// Opens via: Tools > Robot TD > Validate Project
    /// </summary>
    public class ProjectValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationResult> results = new List<ValidationResult>();
        private bool hasRun = false;
        private GUIStyle headerStyle;
        private GUIStyle errorStyle;
        private GUIStyle warningStyle;
        private GUIStyle successStyle;

        private enum ValidationLevel
        {
            Error,
            Warning,
            Info,
            Success
        }

        private class ValidationResult
        {
            public string category;
            public string message;
            public ValidationLevel level;
            public Object target;

            public ValidationResult(string cat, string msg, ValidationLevel lvl, Object obj = null)
            {
                category = cat;
                message = msg;
                level = lvl;
                target = obj;
            }
        }

        [MenuItem("Tools/Robot TD/Validate Project", priority = 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectValidator>("Project Validator");
            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("Robot Tower Defense - Project Validator", headerStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(
                "Checks for missing references, configuration errors, and balance issues.\n" +
                "Run this before building or pushing to production.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Run All Validations", GUILayout.Height(35)))
            {
                RunValidation();
            }

            EditorGUILayout.Space(8);

            if (hasRun)
            {
                DisplayResults();
            }
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

            if (errorStyle == null)
            {
                errorStyle = new GUIStyle(EditorStyles.label);
                errorStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                errorStyle.wordWrap = true;
            }

            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = new Color(1f, 0.8f, 0f);
                warningStyle.wordWrap = true;
            }

            if (successStyle == null)
            {
                successStyle = new GUIStyle(EditorStyles.label);
                successStyle.normal.textColor = new Color(0.2f, 1f, 0.2f);
                successStyle.wordWrap = true;
            }
        }

        private void RunValidation()
        {
            results.Clear();
            hasRun = true;

            // Run all validation checks
            ValidateScriptableObjects();
            ValidatePrefabs();
            ValidateScenes();
            ValidateManagers();
            ValidateBalance();
            ValidateBuildSettings();
            ValidateResources();

            // Summary
            int errors = results.Count(r => r.level == ValidationLevel.Error);
            int warnings = results.Count(r => r.level == ValidationLevel.Warning);
            int infos = results.Count(r => r.level == ValidationLevel.Info);

            Debug.Log($"[ProjectValidator] Validation complete: {errors} errors, {warnings} warnings, {infos} info");

            if (errors == 0 && warnings == 0)
            {
                results.Insert(0, new ValidationResult("Summary", "✓ All checks passed! Project is ready.", ValidationLevel.Success));
            }
        }

        private void DisplayResults()
        {
            int errors = results.Count(r => r.level == ValidationLevel.Error);
            int warnings = results.Count(r => r.level == ValidationLevel.Warning);

            // Summary header
            GUILayout.Label("Validation Results:", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            string summary = $"Errors: {errors}  |  Warnings: {warnings}  |  Total Checks: {results.Count}";
            EditorGUILayout.HelpBox(summary, errors > 0 ? MessageType.Error : warnings > 0 ? MessageType.Warning : MessageType.Info);

            EditorGUILayout.Space(8);

            // Results list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string lastCategory = "";
            foreach (var result in results)
            {
                // Category header
                if (result.category != lastCategory)
                {
                    EditorGUILayout.Space(8);
                    GUILayout.Label($"━━━ {result.category} ━━━", EditorStyles.boldLabel);
                    lastCategory = result.category;
                }

                // Result line
                EditorGUILayout.BeginHorizontal();

                GUIStyle style = result.level == ValidationLevel.Error ? errorStyle :
                                result.level == ValidationLevel.Warning ? warningStyle :
                                result.level == ValidationLevel.Success ? successStyle :
                                EditorStyles.label;

                string icon = result.level == ValidationLevel.Error ? "✖" :
                             result.level == ValidationLevel.Warning ? "⚠" :
                             result.level == ValidationLevel.Success ? "✓" : "ℹ";

                GUILayout.Label(icon, style, GUILayout.Width(20));
                GUILayout.Label(result.message, style);

                if (result.target != null)
                {
                    if (GUILayout.Button("→", GUILayout.Width(30)))
                    {
                        Selection.activeObject = result.target;
                        EditorGUIUtility.PingObject(result.target);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // VALIDATION CHECKS
        // ═══════════════════════════════════════════════════════════════════════

        private void ValidateScriptableObjects()
        {
            // Tower Data
            string[] towerGuids = AssetDatabase.FindAssets("t:TowerData");
            if (towerGuids.Length == 0)
            {
                results.Add(new ValidationResult("ScriptableObjects", "No TowerData assets found. Run Tools > Create Tower Data.", ValidationLevel.Error));
            }
            else
            {
                results.Add(new ValidationResult("ScriptableObjects", $"Found {towerGuids.Length} TowerData assets.", ValidationLevel.Info));

                foreach (string guid in towerGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(path);
                    
                    if (data.cost <= 0)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Cost must be > 0", ValidationLevel.Error, data));
                    
                    if (data.baseDamage <= 0 && data.towerType != TowerType.BuffStation)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Damage should be > 0", ValidationLevel.Warning, data));
                    
                    if (data.baseRange <= 0)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Range must be > 0", ValidationLevel.Error, data));
                }
            }

            // Enemy Data
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData");
            if (enemyGuids.Length == 0)
            {
                results.Add(new ValidationResult("ScriptableObjects", "No EnemyData assets found. Run Tools > Create Enemy Data.", ValidationLevel.Error));
            }
            else
            {
                results.Add(new ValidationResult("ScriptableObjects", $"Found {enemyGuids.Length} EnemyData assets.", ValidationLevel.Info));

                foreach (string guid in enemyGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                    
                    if (data.baseHealth <= 0)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Health must be > 0", ValidationLevel.Error, data));
                    
                    if (data.baseMoveSpeed <= 0)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Move speed must be > 0", ValidationLevel.Error, data));
                    
                    if (data.baseReward < 0)
                        results.Add(new ValidationResult("ScriptableObjects", $"{data.name}: Reward should be >= 0", ValidationLevel.Warning, data));
                }
            }

            // Map Data
            string[] mapGuids = AssetDatabase.FindAssets("t:MapData");
            if (mapGuids.Length == 0)
            {
                results.Add(new ValidationResult("ScriptableObjects", "No MapData assets found. Create at least one map.", ValidationLevel.Warning));
            }
            else
            {
                results.Add(new ValidationResult("ScriptableObjects", $"Found {mapGuids.Length} MapData assets.", ValidationLevel.Info));
            }
        }

        private void ValidatePrefabs()
        {
            // Tower prefabs
            string[] towerPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Towers" });
            if (towerPrefabs.Length < 5)
            {
                results.Add(new ValidationResult("Prefabs", $"Only {towerPrefabs.Length} tower prefabs found. Expected at least 5.", ValidationLevel.Warning));
            }

            // Enemy prefabs
            string[] enemyPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" });
            if (enemyPrefabs.Length < 5)
            {
                results.Add(new ValidationResult("Prefabs", $"Only {enemyPrefabs.Length} enemy prefabs found. Expected at least 5.", ValidationLevel.Warning));
            }

            // Projectile prefabs
            string[] projectilePrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Projectiles" });
            if (projectilePrefabs.Length == 0)
            {
                results.Add(new ValidationResult("Prefabs", "No projectile prefabs found.", ValidationLevel.Warning));
            }
        }

        private void ValidateScenes()
        {
            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0)
            {
                results.Add(new ValidationResult("Scenes", "No scenes in build settings!", ValidationLevel.Error));
                return;
            }

            bool hasMainMenu = false;
            bool hasGameplay = false;

            foreach (var scene in buildScenes)
            {
                if (!scene.enabled) continue;
                
                string name = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                if (name.Contains("MainMenu") || name.Contains("Menu"))
                    hasMainMenu = true;
                if (name.Contains("Game") || name.Contains("Level") || name.Contains("Map"))
                    hasGameplay = true;
            }

            if (!hasMainMenu)
                results.Add(new ValidationResult("Scenes", "No MainMenu scene found in build settings.", ValidationLevel.Error));
            
            if (!hasGameplay)
                results.Add(new ValidationResult("Scenes", "No gameplay scene found in build settings.", ValidationLevel.Error));

            if (hasMainMenu && hasGameplay)
                results.Add(new ValidationResult("Scenes", $"{buildScenes.Length} scenes configured in build settings.", ValidationLevel.Info));
        }

        private void ValidateManagers()
        {
            // Check current scene
            var scene = SceneManager.GetActiveScene();
            
            var gameManager = FindFirstObjectByType<GameManager>();
            var waveManager = FindFirstObjectByType<WaveManager>();
            var saveManager = FindFirstObjectByType<SaveManager>();

            if (scene.name.Contains("Game") || scene.name.Contains("Level"))
            {
                if (gameManager == null)
                    results.Add(new ValidationResult("Managers", "GameManager not found in gameplay scene.", ValidationLevel.Error));
                
                if (waveManager == null)
                    results.Add(new ValidationResult("Managers", "WaveManager not found in gameplay scene.", ValidationLevel.Error));
                
                if (saveManager == null)
                    results.Add(new ValidationResult("Managers", "SaveManager not found. Should be persistent.", ValidationLevel.Warning));
                
                if (gameManager != null && waveManager != null)
                    results.Add(new ValidationResult("Managers", "Core managers present in scene.", ValidationLevel.Info));
            }
        }

        private void ValidateBalance()
        {
            // Load all tower data and check balance
            string[] towerGuids = AssetDatabase.FindAssets("t:TowerData");
            List<TowerData> towers = new List<TowerData>();
            
            foreach (string guid in towerGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(path);
                towers.Add(data);
            }

            if (towers.Count > 0)
            {
                // Check for reasonable DPS values
                foreach (var tower in towers)
                {
                    float dps = tower.baseDamage * tower.baseFireRate;
                    float dpsPerCost = dps / tower.cost;

                    if (dpsPerCost < 0.05f && tower.towerType != TowerType.BuffStation)
                        results.Add(new ValidationResult("Balance", $"{tower.name}: Very low DPS/Cost ratio ({dpsPerCost:F3}). May be underpowered.", ValidationLevel.Warning, tower));
                    
                    if (dpsPerCost > 1.0f)
                        results.Add(new ValidationResult("Balance", $"{tower.name}: Very high DPS/Cost ratio ({dpsPerCost:F3}). May be overpowered.", ValidationLevel.Warning, tower));
                }
            }

            // Load all enemy data
            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData");
            List<EnemyData> enemies = new List<EnemyData>();
            
            foreach (string guid in enemyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                enemies.Add(data);
            }

            if (enemies.Count > 0)
            {
                // Check reward ratios
                foreach (var enemy in enemies)
                {
                    float rewardPerHP = enemy.baseReward / enemy.baseHealth;
                    
                    if (rewardPerHP < 0.05f)
                        results.Add(new ValidationResult("Balance", $"{enemy.name}: Low reward/HP ratio ({rewardPerHP:F3}). Players may avoid.", ValidationLevel.Info, enemy));
                }
            }
        }

        private void ValidateBuildSettings()
        {
            // Company name
            if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
                results.Add(new ValidationResult("Build Settings", "Company name not set (still 'DefaultCompany').", ValidationLevel.Warning));

            // Product name
            if (string.IsNullOrEmpty(PlayerSettings.productName))
                results.Add(new ValidationResult("Build Settings", "Product name not set.", ValidationLevel.Error));

            // Bundle ID for mobile
            if (PlayerSettings.applicationIdentifier.Contains("com.Company.ProductName"))
                results.Add(new ValidationResult("Build Settings", "Bundle identifier not customized for mobile.", ValidationLevel.Warning));

            // Version
            if (PlayerSettings.bundleVersion == "0.1" || PlayerSettings.bundleVersion == "1.0")
                results.Add(new ValidationResult("Build Settings", $"Version is {PlayerSettings.bundleVersion}. Update before release.", ValidationLevel.Info));

            results.Add(new ValidationResult("Build Settings", "Build settings configuration checked.", ValidationLevel.Info));
        }

        private void ValidateResources()
        {
            // Check for Resources folder
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                results.Add(new ValidationResult("Resources", "Assets/Resources folder not found. Create it for runtime asset loading.", ValidationLevel.Warning));
            }
            else
            {
                // Check for prefabs in Resources
                string[] resourcePrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });
                if (resourcePrefabs.Length == 0)
                {
                    results.Add(new ValidationResult("Resources", "No prefabs in Resources folder. Game may fail at runtime.", ValidationLevel.Warning));
                }
                else
                {
                    results.Add(new ValidationResult("Resources", $"{resourcePrefabs.Length} prefabs found in Resources folder.", ValidationLevel.Info));
                }
            }
        }

        private T FindFirstObjectByType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}
#endif
