using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RobotTD.Core
{
    /// <summary>
    /// Runtime validation tool for testing gameplay systems during play mode.
    /// Attach to a GameObject in test scenes. Press F12 to toggle display.
    /// </summary>
    public class RuntimeValidator : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        [SerializeField] private GameObject validatorPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button runTestsButton;
        [SerializeField] private Button closeButton;

        [Header("Test Settings")]
        [SerializeField] private float testInterval = 2f;

        private bool isVisible;
        private StringBuilder report = new StringBuilder();
        private Coroutine testRoutine;

        private void Start()
        {
            if (validatorPanel != null)
                validatorPanel.SetActive(false);

            if (runTestsButton != null)
                runTestsButton.onClick.AddListener(RunTests);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (isVisible)
                    Hide();
                else
                    Show();
            }
        }

        private void Show()
        {
            isVisible = true;
            if (validatorPanel != null)
                validatorPanel.SetActive(true);
            RunTests();
        }

        private void Hide()
        {
            isVisible = false;
            if (validatorPanel != null)
                validatorPanel.SetActive(false);
        }

        public void RunTests()
        {
            if (testRoutine != null)
                StopCoroutine(testRoutine);
            testRoutine = StartCoroutine(RunTestsCoroutine());
        }

        private IEnumerator RunTestsCoroutine()
        {
            report.Clear();
            report.AppendLine("=== RUNTIME VALIDATION ===");
            report.AppendLine($"Time: {System.DateTime.Now:HH:mm:ss}");
            report.AppendLine();

            // Test Core Managers
            TestCoreManagers();
            yield return null;

            // Test Game State
            TestGameState();
            yield return null;

            // Test Systems Integration
            TestSystemsIntegration();
            yield return null;

            // Test Performance
            TestPerformance();
            yield return null;

            // Test Save/Load
            TestSaveLoad();
            yield return null;

            report.AppendLine();
            report.AppendLine("=== TESTS COMPLETE ===");

            if (statusText != null)
                statusText.text = report.ToString();
        }

        private void TestCoreManagers()
        {
            report.AppendLine("━━━ Core Managers ━━━");

            CheckManager(GameManager.Instance, "GameManager");
            CheckManager(WaveManager.Instance, "WaveManager");
            CheckManager(SaveManager.Instance, "SaveManager");
            CheckManager(Towers.TowerPlacementManager.Instance, "TowerPlacementManager");
            CheckManager(Audio.AudioManager.Instance, "AudioManager");
            CheckManager(PerformanceManager.Instance, "PerformanceManager");
            CheckManager(TutorialManager.Instance, "TutorialManager");
            CheckManager(Progression.AchievementManager.Instance, "AchievementManager");
            CheckManager(Progression.TechTree.Instance, "TechTree");
            CheckManager(EndlessMode.Instance, "EndlessMode");

            report.AppendLine();
        }

        private void TestGameState()
        {
            report.AppendLine("━━━ Game State ━━━");

            if (GameManager.Instance != null)
            {
                report.AppendLine($"  State: {GameManager.Instance.CurrentState}");
                report.AppendLine($"  Credits: {GameManager.Instance.Credits}");
                report.AppendLine($"  Lives: {GameManager.Instance.Lives}");
                report.AppendLine($"  Score: {GameManager.Instance.Score}");
                report.AppendLine($"  Paused: {GameManager.Instance.IsPaused}");
                report.AppendLine($"  Game Over: {GameManager.Instance.IsGameOver}");
            }
            else
            {
                report.AppendLine("  ✗ GameManager not available");
            }

            if (WaveManager.Instance != null)
            {
                report.AppendLine($"  Current Wave: {WaveManager.Instance.CurrentWave}/{WaveManager.Instance.TotalWaves}");
                report.AppendLine($"  Wave Active: {WaveManager.Instance.IsWaveActive}");
                report.AppendLine($"  Enemies Alive: {WaveManager.Instance.CurrentEnemies.Count}");
            }

            report.AppendLine();
        }

        private void TestSystemsIntegration()
        {
            report.AppendLine("━━━ Systems Integration ━━━");

            // Check tower placement
            if (Towers.TowerPlacementManager.Instance != null)
            {
                int towerCount = Towers.TowerPlacementManager.Instance.PlacedTowerCount;
                report.AppendLine($"  Towers Placed: {towerCount}");
                report.AppendLine($"  Has Placed: {Towers.TowerPlacementManager.HasPlacedAnyTower()}");
                report.AppendLine($"  Has Upgraded: {Towers.TowerPlacementManager.HasUpgradedAnyTower()}");
            }

            // Check achievement progress
            if (Progression.AchievementManager.Instance != null)
            {
                int unlocked = Progression.AchievementManager.Instance.UnlockedAchievementCount;
                int total = Progression.AchievementManager.Instance.TotalAchievementCount;
                report.AppendLine($"  Achievements: {unlocked}/{total}");
            }

            // Check tech tree
            if (Progression.TechTree.Instance != null && SaveManager.Instance != null)
            {
                int techPoints = SaveManager.Instance.Data.techPoints;
                report.AppendLine($"  Tech Points: {techPoints}");
            }

            // Check endless mode
            if (EndlessMode.Instance != null)
            {
                report.AppendLine($"  Endless Active: {EndlessMode.Instance.IsActive}");
                if (EndlessMode.Instance.IsActive)
                {
                    report.AppendLine($"  Endless Wave: {EndlessMode.Instance.EndlessWaveNumber}");
                    report.AppendLine($"  Endless Score: {EndlessMode.Instance.EndlessScore}");
                }
            }

            report.AppendLine();
        }

        private void TestPerformance()
        {
            report.AppendLine("━━━ Performance ━━━");

            if (PerformanceManager.Instance != null)
            {
                report.AppendLine($"  Quality: {PerformanceManager.Instance.CurrentPreset}");
                report.AppendLine($"  Current FPS: {PerformanceManager.Instance.CurrentFPS:F1}");
                report.AppendLine($"  Average FPS: {PerformanceManager.Instance.GetAverageFPS():F1}");
                report.AppendLine($"  Min FPS: {PerformanceManager.Instance.GetMinimumFPS():F1}");
                report.AppendLine($"  Max FPS: {PerformanceManager.Instance.GetMaximumFPS():F1}");
                report.AppendLine($"  Frame Time: {PerformanceManager.Instance.AverageFrameTimeMs:F2}ms");
                report.AppendLine($"  Battery Save: {PerformanceManager.Instance.IsInBatterySaveMode}");
            }

            report.AppendLine($"  Memory: {(System.GC.GetTotalMemory(false) / 1048576f):F1} MB");
            report.AppendLine($"  Time Scale: {Time.timeScale}");
            report.AppendLine($"  Target FPS: {Application.targetFrameRate}");

            report.AppendLine();
        }

        private void TestSaveLoad()
        {
            report.AppendLine("━━━ Save/Load ━━━");

            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                var data = SaveManager.Instance.Data;
                report.AppendLine($"  Waves Completed: {data.totalWavesCompleted}");
                report.AppendLine($"  Highest Wave: {data.highestWaveReached}");
                report.AppendLine($"  High Score: {data.highScore}");
                report.AppendLine($"  Tutorial Done: {data.tutorialCompleted}");
                report.AppendLine($"  Graphics Quality: {data.graphicsQuality}");
                report.AppendLine($"  Master Volume: {data.masterVolume:F2}");
                report.AppendLine($"  Tech Points: {data.techPoints}");
                report.AppendLine($"  Achievements Unlocked: {data.unlockedAchievements.Count}");
                report.AppendLine($"  Tech Upgrades: {data.techLevels.Count}");
            }
            else
            {
                report.AppendLine("  ✗ Save data not available");
            }

            report.AppendLine();
        }

        private void CheckManager<T>(T instance, string name) where T : class
        {
            if (instance != null)
                report.AppendLine($"  ✓ {name}");
            else
                report.AppendLine($"  ✗ {name} NOT FOUND");
        }

        // ── Public API for Debug Commands ────────────────────────────────────

        [ContextMenu("Add 1000 Credits")]
        public void AddCredits()
        {
            GameManager.Instance?.AddCredits(1000);
            RunTests();
        }

        [ContextMenu("Skip to Wave 10")]
        public void SkipToWave10()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.CurrentWave = 9;
                Debug.Log("Skipped to wave 10");
            }
            RunTests();
        }

        [ContextMenu("Complete Tutorial")]
        public void CompleteTutorial()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ForceCompleteTutorial();
            }
            RunTests();
        }

        [ContextMenu("Unlock All Achievements")]
        public void UnlockAll Achievements()
        {
            if (Progression.AchievementManager.Instance != null)
            {
                // Test method - would need to be exposed or use reflection
                Debug.Log("Achievement unlock would be triggered here");
            }
            RunTests();
        }

        [ContextMenu("Reset Save")]
        public void ResetSave()
        {
            SaveManager.Instance?.DeleteSave();
            SaveManager.Instance?.Load();
            Debug.Log("Save reset");
            RunTests();
        }

        [ContextMenu("Force GC")]
        public void ForceGarbageCollection()
        {
            PerformanceManager.Instance?.ForceGarbageCollection();
            Debug.Log("Forced garbage collection");
            RunTests();
        }

        [ContextMenu("Toggle Quality")]
        public void CycleQuality()
        {
            if (PerformanceManager.Instance != null)
            {
                var current = PerformanceManager.Instance.CurrentPreset;
                PerformanceManager.QualityPreset next;
                
                switch (current)
                {
                    case PerformanceManager.QualityPreset.Low:
                        next = PerformanceManager.QualityPreset.Medium;
                        break;
                    case PerformanceManager.QualityPreset.Medium:
                        next = PerformanceManager.QualityPreset.High;
                        break;
                    default:
                        next = PerformanceManager.QualityPreset.Low;
                        break;
                }
                
                PerformanceManager.Instance.ApplyQualityPreset(next);
                Debug.Log($"Quality changed to {next}");
            }
            RunTests();
        }
    }
}
