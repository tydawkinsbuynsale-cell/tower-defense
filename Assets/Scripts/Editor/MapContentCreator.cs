#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RobotTD.Map;
using RobotTD.Core;
using RobotTD.Enemies;

namespace RobotTD.Editor
{
    /// <summary>
    /// Editor tool for creating complete maps with wave configurations.
    /// Generates MapData and WaveSetData with predefined layouts.
    /// 
    /// Opens via: Tools > Robot TD > Create Map Content
    /// </summary>
    public class MapContentCreator : EditorWindow
    {
        private Vector2 scrollPosition;
        private string mapDirectory = "Assets/Data/Maps";
        private string waveDirectory = "Assets/Data/Waves";

        private GUIStyle headerStyle;

        [MenuItem("Tools/Robot TD/Create Map Content", priority = 30)]
        public static void ShowWindow()
        {
            var window = GetWindow<MapContentCreator>("Map Content Creator");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("Robot TD - Map Content Creator", headerStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(
                "Creates complete map content including:\n" +
                "• MapData with paths and settings\n" +
                "• WaveSetData with balanced enemy waves\n" +
                "• Progression chain (unlocks next map)",
                MessageType.Info);

            EditorGUILayout.Space(8);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Directories
            EditorGUILayout.LabelField("Output Directories:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Maps:", GUILayout.Width(80));
            mapDirectory = EditorGUILayout.TextField(mapDirectory);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Waves:", GUILayout.Width(80));
            waveDirectory = EditorGUILayout.TextField(waveDirectory);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);

            // Create buttons
            if (GUILayout.Button("Create All 6 Campaign Maps", GUILayout.Height(40)))
            {
                CreateAllMaps();
            }

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Or create individual maps:", EditorStyles.boldLabel);

            if (GUILayout.Button("Map 1: Training Grounds (Easy)", GUILayout.Height(30)))
                CreateMap1_TrainingGrounds();

            if (GUILayout.Button("Map 2: Industrial Complex (Easy-Medium)", GUILayout.Height(30)))
                CreateMap2_IndustrialComplex();

            if (GUILayout.Button("Map 3: Desert Outpost (Medium)", GUILayout.Height(30)))
                CreateMap3_DesertOutpost();

            if (GUILayout.Button("Map 4: Frozen Fortress (Medium-Hard)", GUILayout.Height(30)))
                CreateMap4_FrozenFortress();

            if (GUILayout.Button("Map 5: Final Assault (Hard)", GUILayout.Height(30)))
                CreateMap5_FinalAssault();

            if (GUILayout.Button("Map 6: Mega Factory (Very Hard)", GUILayout.Height(30)))
                CreateMap6_MegaFactory();

            EditorGUILayout.Space(12);

            EditorGUILayout.HelpBox(
                "Maps will be created in sequence with unlock progression.\n" +
                "Each map includes 30 waves with increasing difficulty.\n" +
                "Map 6 (Mega Factory) is a post-release endgame challenge map.",
                MessageType.None);

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
        }

        // ═══════════════════════════════════════════════════════════════════════
        // CREATE ALL MAPS
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateAllMaps()
        {
            EnsureDirectories();

            Debug.Log("[MapContentCreator] Creating all 6 campaign maps...");

            CreateMap1_TrainingGrounds();
            CreateMap2_IndustrialComplex();
            CreateMap3_DesertOutpost();
            CreateMap4_FrozenFortress();
            CreateMap5_FinalAssault();
            CreateMap6_MegaFactory();

            // Create MapRegistry
            CreateMapRegistry();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", 
                "Created 6 campaign maps with wave configurations!\n\n" +
                "Maps are unlocked in sequence:\n" +
                "1. Training Grounds\n" +
                "2. Industrial Complex\n" +
                "3. Desert Outpost\n" +
                "4. Frozen Fortress\n" +
                "5. Final Assault\n" +
                "6. Mega Factory (Endgame ++)\n\n" +
                "MapRegistry created at Assets/Data/MapRegistry.asset", 
                "OK");

            Debug.Log("[MapContentCreator] All 6 maps created successfully!");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 1: TRAINING GROUNDS (EASY)
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap1_TrainingGrounds()
        {
            EnsureDirectories();

            // Create MapData
            MapData map = CreateMapAsset("Map01_TrainingGrounds");
            map.mapName = "Training Grounds";
            map.description = "Your first mission. Learn the basics of tower defense against weak enemies.";
            map.difficulty = 1;
            map.isUnlocked = true;
            map.nextMapId = "Map02_IndustrialComplex";
            map.totalWaves = 30;
            map.difficultyMultiplier = 0.8f;
            map.startingCredits = 600;
            map.startingLives = 25;
            map.ambientColor = new Color(1f, 0.98f, 0.9f);
            map.fogColor = new Color(0.8f, 0.85f, 0.9f);
            
            // Simple S-curve path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-8f, 0f, -5f),  // Start left
                new Vector3(-5f, 0f, 0f),
                new Vector3(0f, 0f, 2f),
                new Vector3(5f, 0f, 0f),
                new Vector3(8f, 0f, -5f)    // End right
            };

            // Create WaveSet
            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_TrainingGrounds");
            waveSet.setName = "Training Grounds Waves";
            waveSet.description = "Beginner-friendly waves focusing on Scout and Soldier units.";
            waveSet.difficulty = 1;
            waveSet.globalHealthMultiplier = 0.8f;
            waveSet.waves = GenerateTrainingWaves();

            map.gameObject.name = "Map01_TrainingGrounds";
            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 1: Training Grounds with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateTrainingWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 8f;
                wave.timeBetweenSpawns = 0.8f;
                wave.healthMultiplier = 1f + (i * 0.15f);
                wave.speedMultiplier = 1f + (i * 0.02f);
                wave.rewardMultiplier = 1f + (i * 0.05f);
                wave.waveCompletionBonus = 50 + (i * 10);

                // Wave composition changes based on wave number
                if (i <= 5)
                {
                    // Early waves: Just scouts
                    wave.enemies.Add(new WaveEnemySpawn 
                    { 
                        enemyPrefabId = "Scout", 
                        count = 5 + i, 
                        pattern = SpawnPattern.Sequential 
                    });
                }
                else if (i <= 10)
                {
                    // Mix scouts and soldiers
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Scout", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 3 + (i - 5) });
                }
                else if (i == 15)
                {
                    // Mini-boss wave
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 10 });
                    wave.waveCompletionBonus = 200;
                    wave.techPointsReward = 1;
                }
                else if (i <= 25)
                {
                    // Mid-game: varied units
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Scout", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 1 + (i - 15) / 3 });
                    
                    if (i >= 20)
                        wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 3 });
                }
                else if (i == 30)
                {
                    // Final boss wave
                    wave.isBossWave = true;
                    wave.waveName = "Final Wave - Boss";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 1 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 15 });
                    wave.waveCompletionBonus = 500;
                    wave.techPointsReward = 3;
                }
                else
                {
                    // Late game variety
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 2 });
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 2: INDUSTRIAL COMPLEX (EASY-MEDIUM)
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap2_IndustrialComplex()
        {
            EnsureDirectories();

            MapData map = CreateMapAsset("Map02_IndustrialComplex");
            map.mapName = "Industrial Complex";
            map.description = "Fight through the abandoned factory. Expect more armored units and faster enemies.";
            map.difficulty = 2;
            map.isUnlocked = false;
            map.nextMapId = "Map03_DesertOutpost";
            map.totalWaves = 30;
            map.difficultyMultiplier = 1.0f;
            map.startingCredits = 550;
            map.startingLives = 20;
            map.ambientColor = new Color(0.7f, 0.75f, 0.8f);
            map.fogColor = new Color(0.6f, 0.65f, 0.7f);
            
            // Winding factory path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-10f, 0f, -6f),
                new Vector3(-6f, 0f, -4f),
                new Vector3(-4f, 0f, 0f),
                new Vector3(-6f, 0f, 4f),
                new Vector3(0f, 0f, 5f),
                new Vector3(6f, 0f, 4f),
                new Vector3(8f, 0f, 0f),
                new Vector3(10f, 0f, -6f)
            };

            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_IndustrialComplex");
            waveSet.setName = "Industrial Complex Waves";
            waveSet.description = "Increased difficulty with more armored and flying units.";
            waveSet.difficulty = 2;
            waveSet.globalHealthMultiplier = 1.0f;
            waveSet.waves = GenerateIndustrialWaves();

            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 2: Industrial Complex with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateIndustrialWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 7f;
                wave.timeBetweenSpawns = 0.7f;
                wave.healthMultiplier = 1f + (i * 0.18f);
                wave.speedMultiplier = 1f + (i * 0.03f);
                wave.rewardMultiplier = 1f + (i * 0.06f);
                wave.waveCompletionBonus = 60 + (i * 12);

                if (i <= 5)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Scout", count = 8 + i });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = i });
                }
                else if (i <= 10)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = i - 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 2 });
                }
                else if (i == 15)
                {
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 1 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 8 });
                    wave.waveCompletionBonus = 250;
                    wave.techPointsReward = 2;
                }
                else if (i <= 25)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 4 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 2 });
                    
                    if (i >= 20)
                        wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 1 });
                }
                else if (i == 30)
                {
                    wave.isBossWave = true;
                    wave.waveName = "Final Wave - Boss";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 10 });
                    wave.waveCompletionBonus = 600;
                    wave.techPointsReward = 4;
                }
                else
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 4 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 6 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 1 });
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 3: DESERT OUTPOST (MEDIUM)
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap3_DesertOutpost()
        {
            EnsureDirectories();

            MapData map = CreateMapAsset("Map03_DesertOutpost");
            map.mapName = "Desert Outpost";
            map.description = "Defend the remote desert base. Fast enemies and special units will test your strategy.";
            map.difficulty = 3;
            map.isUnlocked = false;
            map.nextMapId = "Map04_FrozenFortress";
            map.totalWaves = 30;
            map.difficultyMultiplier = 1.2f;
            map.startingCredits = 500;
            map.startingLives = 18;
            map.ambientColor = new Color(1f, 0.9f, 0.7f);
            map.fogColor = new Color(0.9f, 0.8f, 0.6f);
            
            // Zigzag desert path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-9f, 0f, 6f),
                new Vector3(-3f, 0f, 5f),
                new Vector3(0f, 0f, 0f),
                new Vector3(3f, 0f, -5f),
                new Vector3(9f, 0f, -6f)
            };

            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_DesertOutpost");
            waveSet.setName = "Desert Outpost Waves";
            waveSet.description = "Medium difficulty with Splitters, Teleporters, and Healers.";
            waveSet.difficulty = 3;
            waveSet.globalHealthMultiplier = 1.2f;
            waveSet.waves = GenerateDesertWaves();

            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 3: Desert Outpost with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateDesertWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 6f;
                wave.timeBetweenSpawns = 0.65f;
                wave.healthMultiplier = 1f + (i * 0.2f);
                wave.speedMultiplier = 1.1f + (i * 0.03f);
                wave.rewardMultiplier = 1f + (i * 0.07f);
                wave.waveCompletionBonus = 70 + (i * 15);

                if (i <= 5)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Scout", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 5 });
                }
                else if (i <= 10)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 4 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 6 });
                }
                else if (i == 15)
                {
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 1 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 10 });
                    wave.waveCompletionBonus = 300;
                    wave.techPointsReward = 2;
                }
                else if (i <= 25)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 6 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 3 });
                    
                    if (i >= 20)
                        wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 2 });
                }
                else if (i == 30)
                {
                    wave.isBossWave = true;
                    wave.waveName = "Final Wave - Boss Swarm";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 1 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 15 });
                    wave.waveCompletionBonus = 700;
                    wave.techPointsReward = 5;
                }
                else
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 4 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 2 });
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 4: FROZEN FORTRESS (MEDIUM-HARD)
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap4_FrozenFortress()
        {
            EnsureDirectories();

            MapData map = CreateMapAsset("Map04_FrozenFortress");
            map.mapName = "Frozen Fortress";
            map.description = "The frozen wasteland. Elite units and powerful bosses dominate the battlefield.";
            map.difficulty = 4;
            map.isUnlocked = false;
            map.nextMapId = "Map05_FinalAssault";
            map.totalWaves = 30;
            map.difficultyMultiplier = 1.4f;
            map.startingCredits = 450;
            map.startingLives = 15;
            map.ambientColor = new Color(0.8f, 0.9f, 1f);
            map.fogColor = new Color(0.7f, 0.8f, 0.95f);
            
            // Complex fortress path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-10f, 0f, 0f),
                new Vector3(-7f, 0f, 4f),
                new Vector3(-3f, 0f, 5f),
                new Vector3(0f, 0f, 3f),
                new Vector3(3f, 0f, 0f),
                new Vector3(5f, 0f, -4f),
                new Vector3(8f, 0f, -5f),
                new Vector3(10f, 0f, -2f)
            };

            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_FrozenFortress");
            waveSet.setName = "Frozen Fortress Waves";
            waveSet.description = "High difficulty with elite units and multiple bosses.";
            waveSet.difficulty = 4;
            waveSet.globalHealthMultiplier = 1.4f;
            waveSet.waves = GenerateFrozenWaves();

            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 4: Frozen Fortress with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateFrozenWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 5f;
                wave.timeBetweenSpawns = 0.6f;
                wave.healthMultiplier = 1.2f + (i * 0.22f);
                wave.speedMultiplier = 1.1f + (i * 0.035f);
                wave.rewardMultiplier = 1f + (i * 0.08f);
                wave.waveCompletionBonus = 80 + (i * 18);

                if (i <= 5)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 5 });
                }
                else if (i <= 10)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 2 });
                }
                else if (i == 15)
                {
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 12 });
                    wave.waveCompletionBonus = 350;
                    wave.techPointsReward = 3;
                }
                else if (i <= 25)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 3 });
                }
                else if (i == 30)
                {
                    wave.isBossWave = true;
                    wave.waveName = "Final Wave - Triple Threat";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 20 });
                    wave.waveCompletionBonus = 800;
                    wave.techPointsReward = 6;
                }
                else
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 18 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 4 });
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 5: FINAL ASSAULT (HARD)
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap5_FinalAssault()
        {
            EnsureDirectories();

            MapData map = CreateMapAsset("Map05_FinalAssault");
            map.mapName = "Final Assault";
            map.description = "The ultimate challenge. Survive overwhelming enemy forces and defeat all three boss types.";
            map.difficulty = 5;
            map.isUnlocked = false;
            map.nextMapId = "Map06_MegaFactory"; // Unlocks Mega Factory
            map.totalWaves = 30;
            map.difficultyMultiplier = 1.6f;
            map.startingCredits = 400;
            map.startingLives = 12;
            map.ambientColor = new Color(0.9f, 0.7f, 0.6f);
            map.fogColor = new Color(0.6f, 0.4f, 0.3f);
            
            // Long, winding final path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-12f, 0f, -6f),
                new Vector3(-8f, 0f, -3f),
                new Vector3(-5f, 0f, 2f),
                new Vector3(-2f, 0f, 5f),
                new Vector3(2f, 0f, 6f),
                new Vector3(5f, 0f, 3f),
                new Vector3(7f, 0f, -1f),
                new Vector3(9f, 0f, -5f),
                new Vector3(12f, 0f, -6f)
            };

            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_FinalAssault");
            waveSet.setName = "Final Assault Waves";
            waveSet.description = "Maximum difficulty. All enemy types, multiple bosses, extreme challenge.";
            waveSet.difficulty = 5;
            waveSet.globalHealthMultiplier = 1.6f;
            waveSet.waves = GenerateFinalWaves();

            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 5: Final Assault with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateFinalWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 5f;
                wave.timeBetweenSpawns = 0.55f;
                wave.healthMultiplier = 1.5f + (i * 0.25f);
                wave.speedMultiplier = 1.2f + (i * 0.04f);
                wave.rewardMultiplier = 1.2f + (i * 0.1f);
                wave.waveCompletionBonus = 100 + (i * 20);

                if (i <= 5)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 8 });
                }
                else if (i == 10)
                {
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 20 });
                    wave.waveCompletionBonus = 400;
                    wave.techPointsReward = 3;
                }
                else if (i <= 15)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 4 });
                }
                else if (i == 20)
                {
                    wave.isBossWave = true;
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 25 });
                    wave.waveCompletionBonus = 500;
                    wave.techPointsReward = 4;
                }
                else if (i <= 25)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 5 });
                }
                else if (i == 30)
                {
                    wave.isBossWave = true;
                    wave.waveName = "FINAL WAVE - ALL BOSSES";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 30 });
                    wave.waveCompletionBonus = 1000;
                    wave.techPointsReward = 10;
                }
                else
                {
                    // Ultra difficult mixed waves
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 30 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 18 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 6 });
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MAP 6: MEGA FACTORY (VERY HARD) - Version 1.2 Endgame Map
        // ═══════════════════════════════════════════════════════════════════════

        private void CreateMap6_MegaFactory()
        {
            EnsureDirectories();

            MapData map = CreateMapAsset("Map06_MegaFactory");
            map.mapName = "Mega Factory";
            map.description = "The ultimate industrial warfare challenge. Face relentless waves of advanced enemies in this massive automated complex. Endgame content for elite players.";
            map.difficulty = 5;
            map.isUnlocked = false;
            map.nextMapId = ""; // No next map - final challenge
            map.totalWaves = 30;
            map.difficultyMultiplier = 1.6f; // Significantly harder
            map.startingCredits = 700; // More credits to handle difficulty
            map.startingLives = 15; // Fewer lives - unforgiving
            map.ambientColor = new Color(0.5f, 0.55f, 0.6f); // Dark industrial
            map.fogColor = new Color(0.3f, 0.35f, 0.4f); // Heavy fog
            map.fogDensity = 0.04f; // Atmospheric
            
            // Complex multi-loop industrial path
            map.pathPoints = new Vector3[]
            {
                new Vector3(-12f, 0f, -8f),   // Entry from far left
                new Vector3(-8f, 0f, -6f),
                new Vector3(-4f, 0f, -2f),
                new Vector3(-6f, 0f, 2f),     // Loop back
                new Vector3(-2f, 0f, 4f),     // Forward
                new Vector3(2f, 0f, 2f),      // Cross center
                new Vector3(4f, 0f, -2f),
                new Vector3(8f, 0f, 0f),      // Right side loop
                new Vector3(6f, 0f, 4f),
                new Vector3(10f, 0f, 6f),
                new Vector3(12f, 0f, 2f),     // Exit far right
                new Vector3(14f, 0f, -4f)
            };

            WaveSetData waveSet = CreateWaveSetAsset("WaveSet_MegaFactory");
            waveSet.setName = "Mega Factory Assault Waves";
            waveSet.description = "Extreme endgame content. Massive enemy counts, multiple bosses, and unforgiving difficulty scaling. Requires mastery of all tower types and perfect strategy.";
            waveSet.difficulty = 5;
            waveSet.globalHealthMultiplier = 1.6f;
            waveSet.waves = GenerateMegaFactoryWaves();

            EditorUtility.SetDirty(map);
            EditorUtility.SetDirty(waveSet);

            Debug.Log($"[MapContentCreator] Created Map 6: Mega Factory (ENDGAME) with {waveSet.waves.Count} waves");
        }

        private List<WaveData> GenerateMegaFactoryWaves()
        {
            List<WaveData> waves = new List<WaveData>();

            for (int i = 1; i <= 30; i++)
            {
                WaveData wave = new WaveData();
                wave.waveNumber = i;
                wave.waveName = $"Wave {i}";
                wave.preparationTime = 6f; // Less prep time
                wave.timeBetweenSpawns = 0.5f; // Faster spawns
                wave.healthMultiplier = 1.2f + (i * 0.25f); // Aggressive scaling
                wave.speedMultiplier = 1.1f + (i * 0.04f); // Much faster enemies
                wave.rewardMultiplier = 1.2f + (i * 0.08f); // Higher rewards to compensate
                wave.waveCompletionBonus = 100 + (i * 20);

                if (i <= 3)
                {
                    // Start hard - no easy intro
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Soldier", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 8 });
                }
                else if (i <= 7)
                {
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 12 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 2 });
                }
                else if (i == 10)
                {
                    // Early boss wave
                    wave.isBossWave = true;
                    wave.waveName = "Wave 10 - Shield Commander";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 10 });
                    wave.waveCompletionBonus = 400;
                    wave.techPointsReward = 3;
                }
                else if (i <= 14)
                {
                    // Advanced enemy mix
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 8 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 3 });
                }
                else if (i == 15)
                {
                    // Mid boss wave
                    wave.isBossWave = true;
                    wave.waveName = "Wave 15 - Swarm Mother";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 20 });
                    wave.waveCompletionBonus = 500;
                    wave.techPointsReward = 4;
                }
                else if (i <= 19)
                {
                    // Heavy assault waves
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 18 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 10 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 4 });
                }
                else if (i == 20)
                {
                    // Major boss wave
                    wave.isBossWave = true;
                    wave.waveName = "Wave 20 - Titan Assault";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 20 });
                    wave.waveCompletionBonus = 700;
                    wave.techPointsReward = 5;
                }
                else if (i <= 24)
                {
                    // Maximum difficulty regular waves
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 30 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 18 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 15 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 6 });
                }
                else if (i == 25)
                {
                    // Pre-final boss wave
                    wave.isBossWave = true;
                    wave.waveName = "Wave 25 - Combined Forces";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 2 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 30 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 25 });
                    wave.waveCompletionBonus = 900;
                    wave.techPointsReward = 6;
                }
                else if (i == 30)
                {
                    // ULTIMATE FINAL BOSS WAVE
                    wave.isBossWave = true;
                    wave.waveName = "WAVE 30 - MEGA FACTORY APOCALYPSE";
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_Titan", count = 5 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_ShieldCommander", count = 4 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Boss_SwarmMother", count = 3 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 40 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 30 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 30 });
                    wave.waveCompletionBonus = 2000;
                    wave.techPointsReward = 15; // Massive reward
                }
                else
                {
                    // Waves 26-29: Ultra endgame
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Elite", count = 35 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Tank", count = 30 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Flying", count = 25 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Splitter", count = 20 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Teleporter", count = 18 });
                    wave.enemies.Add(new WaveEnemySpawn { enemyPrefabId = "Healer", count = 8 });
                    wave.waveCompletionBonus = 150 + (i * 20);
                }

                waves.Add(wave);
            }

            return waves;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════════════

        private void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(mapDirectory))
            {
                string parent = System.IO.Path.GetDirectoryName(mapDirectory).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(mapDirectory);
                AssetDatabase.CreateFolder(parent, folder);
            }

            if (!AssetDatabase.IsValidFolder(waveDirectory))
            {
                string parent = System.IO.Path.GetDirectoryName(waveDirectory).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(waveDirectory);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private MapData CreateMapAsset(string name)
        {
            string path = $"{mapDirectory}/{name}.asset";
            
            // Check if already exists
            MapData existing = AssetDatabase.LoadAssetAtPath<MapData>(path);
            if (existing != null)
            {
                Debug.Log($"[MapContentCreator] Map {name} already exists, overwriting...");
                return existing;
            }

            MapData map = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(map, path);
            return map;
        }

        private WaveSetData CreateWaveSetAsset(string name)
        {
            string path = $"{waveDirectory}/{name}.asset";
            
            WaveSetData existing = AssetDatabase.LoadAssetAtPath<WaveSetData>(path);
            if (existing != null)
            {
                Debug.Log($"[MapContentCreator] WaveSet {name} already exists, overwriting...");
                return existing;
            }

            WaveSetData waveSet = ScriptableObject.CreateInstance<WaveSetData>();
            AssetDatabase.CreateAsset(waveSet, path);
            return waveSet;
        }

        private void CreateMapRegistry()
        {
            string registryPath = "Assets/Data/MapRegistry.asset";

            // Check if exists
            Map.MapRegistry registry = AssetDatabase.LoadAssetAtPath<Map.MapRegistry>(registryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<Map.MapRegistry>();
                AssetDatabase.CreateAsset(registry, registryPath);
            }

            // Load all map assets
            registry.maps.Clear();
            string[] mapPaths = new string[]
            {
                $"{mapDirectory}/Map01_TrainingGrounds.asset",
                $"{mapDirectory}/Map02_IndustrialComplex.asset",
                $"{mapDirectory}/Map03_DesertOutpost.asset",
                $"{mapDirectory}/Map04_FrozenFortress.asset",
                $"{mapDirectory}/Map05_FinalAssault.asset",
                $"{mapDirectory}/Map06_MegaFactory.asset"
            };

            foreach (string path in mapPaths)
            {
                MapData map = AssetDatabase.LoadAssetAtPath<MapData>(path);
                if (map != null)
                {
                    registry.maps.Add(map);
                }
            }

            registry.gameplaySceneName = "GameplayScene";

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MapContentCreator] Created MapRegistry with {registry.maps.Count} maps (including Mega Factory endgame map)");
        }
    }
}
#endif
