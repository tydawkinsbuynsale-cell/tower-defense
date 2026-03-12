using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace RobotTD.Core
{
    /// <summary>
    /// Placed in every scene. Ensures all persistent singleton managers exist
    /// (creates them from Resources if missing) and are initialized in the
    /// correct order before any gameplay scene logic runs.
    ///
    /// Usage: Add the SceneBootstrapper prefab to every scene as the
    /// first object in the hierarchy (execution order -100).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("Manager Prefabs (Resources fallback)")]
        [Tooltip("Leave empty — managers auto-create from Resources/Managers/")]
        [SerializeField] private GameObject managerRootPrefab;

        private static bool coreInitialized = false;

        private void Awake()
        {
            if (!coreInitialized)
            {
                InitializeCore();
            }

            // Per-scene init (runs even if core already existed)
            StartCoroutine(PerSceneInit());
        }

        // ── Core (runs once, first scene only) ──────────────────────────────

        private void InitializeCore()
        {
            coreInitialized = true;

            // Try to load from prefab first, else create from code
            if (managerRootPrefab != null)
            {
                var root = Instantiate(managerRootPrefab);
                DontDestroyOnLoad(root);
                Debug.Log("[Bootstrapper] Manager root instantiated from prefab.");
                return;
            }

            // Fallback: auto-create each manager if missing
            EnsureManager<SaveManager>("SaveManager");
            EnsureManager<Audio.AudioManager>("AudioManager");
            EnsureManager<VFX.VFXManager>("VFXManager");
            EnsureManager<PerformanceManager>("PerformanceManager");
            EnsureManager<Progression.TechTree>("TechTree");
            EnsureManager<Progression.AchievementManager>("AchievementManager");
            EnsureManager<GameManager>("GameManager");
            EnsureManager<WaveManager>("WaveManager");
            EnsureManager<GameIntegrator>("GameIntegrator");

            Debug.Log("[Bootstrapper] Core managers initialized.");
        }

        private T EnsureManager<T>(string name) where T : MonoBehaviour
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;

            // Try Resources first
            GameObject prefab = Resources.Load<GameObject>($"Managers/{name}");
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                go.name = name;
                DontDestroyOnLoad(go);
                return go.GetComponent<T>();
            }

            // Create empty GameObject with the component
            var obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            return obj.AddComponent<T>();
        }

        // ── Per-scene (runs every time a scene loads) ────────────────────────

        private IEnumerator PerSceneInit()
        {
            // Wait one frame so all scene Awakes have completed
            yield return null;

            string sceneName = SceneManager.GetActiveScene().name;

            switch (sceneName)
            {
                case "MainMenu":
                    InitMainMenu();
                    break;

                case "TrainingGrounds":
                case "FactoryFloor":
                case "CircuitCity":
                case "NuclearCore":
                case "CommandCenter":
                    InitGameplayScene();
                    break;
            }
        }

        private void InitMainMenu()
        {
            Audio.AudioManager.Instance?.PlayMusic(Audio.MusicTrack.MainMenu);
            Debug.Log("[Bootstrapper] Main menu initialized.");
        }

        private void InitGameplayScene()
        {
            // Apply tech-tree bonuses to GameManager starting values
            ApplyTechTreeBonuses();

            // Play battle music
            Audio.AudioManager.Instance?.PlayMusic(Audio.MusicTrack.Battle_Low);

            // Check for tutorial flag
            bool needsTutorial = Core.SaveManager.Instance?.Data.totalWavesCompleted == 0;
            if (needsTutorial)
            {
                Tutorial.TutorialManager.Instance?.StartTutorial();
            }

            Debug.Log("[Bootstrapper] Gameplay scene initialized.");
        }

        private void ApplyTechTreeBonuses()
        {
            var gm = GameManager.Instance;
            var tt = Progression.TechTree.Instance;
            var sm = SaveManager.Instance;

            if (gm == null || tt == null || sm == null) return;

            // Resilience: +2 starting lives per level
            int bonusLives = tt.BonusStartingLives;
            if (bonusLives > 0)
            {
                for (int i = 0; i < bonusLives; i++)
                    gm.AddLives(1);
            }

            Debug.Log($"[Bootstrapper] TechTree bonuses applied: +{bonusLives} lives.");
        }
    }
}
