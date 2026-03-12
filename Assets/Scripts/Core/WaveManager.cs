using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Manages wave spawning, enemy compositions, and wave progression.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Settings")]
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private float timeBetweenSpawns = 1f;
        [SerializeField] private bool autoStartWaves = false;
        [SerializeField] private int totalWaves = 30; // 0 = endless mode

        [Header("Difficulty Scaling")]
        [SerializeField] private float healthScalePerWave = 0.15f; // 15% more HP per wave
        [SerializeField] private float speedScalePerWave = 0.02f;  // 2% faster per wave
        [SerializeField] private int maxEnemiesPerWave = 50;

        [Header("References")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform[] waypoints;

        // Runtime state
        public int CurrentWave { get; private set; } = 0;
        public int TotalWaves => totalWaves;
        public bool WaveInProgress { get; private set; } = false;
        public int EnemiesRemaining { get; private set; } = 0;
        public int EnemiesToSpawn { get; private set; } = 0;

        // Unity inspector events
        public UnityEvent<int> OnWaveStarted;
        public UnityEvent<int> OnWaveCompleted;
        public UnityEvent OnAllWavesCompleted;
        public UnityEvent<int> OnEnemyCountChanged;

        // C# action events
        public System.Action<Enemies.Enemy> OnEnemyKilled;
        public System.Action OnBossSpawned;

        // Enemy prefabs - assign in inspector or load from Resources
        private Dictionary<EnemyType, GameObject> enemyPrefabs;
        
        // Wave composition
        private Queue<EnemyType> spawnQueue = new Queue<EnemyType>();
        private Coroutine spawnCoroutine;

        public enum EnemyType
        {
            Scout,      // Fast, weak
            Soldier,    // Balanced
            Tank,       // Slow, high HP
            Elite,      // Fast, high HP
            Boss        // Very slow, massive HP
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize events
            OnWaveStarted ??= new UnityEvent<int>();
            OnWaveCompleted ??= new UnityEvent<int>();
            OnAllWavesCompleted ??= new UnityEvent();
            OnEnemyCountChanged ??= new UnityEvent<int>();
        }

        private void Start()
        {
            LoadEnemyPrefabs();
        }

        private void LoadEnemyPrefabs()
        {
            enemyPrefabs = new Dictionary<EnemyType, GameObject>();
            
            // Load from Resources/Prefabs/Enemies/
            foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
            {
                string path = $"Prefabs/Enemies/{type}";
                GameObject prefab = Resources.Load<GameObject>(path);
                if (prefab != null)
                {
                    enemyPrefabs[type] = prefab;
                }
                else
                {
                    Debug.LogWarning($"Enemy prefab not found: {path}");
                }
            }
        }

        /// <summary>
        /// Start the next wave
        /// </summary>
        public void StartNextWave()
        {
            if (WaveInProgress) return;

            CurrentWave++;
            WaveInProgress = true;

            // Generate wave composition
            GenerateWaveComposition(CurrentWave);

            OnWaveStarted?.Invoke(CurrentWave);

            // Start spawning
            spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
        }

        /// <summary>
        /// Generate enemy composition for a wave
        /// </summary>
        private void GenerateWaveComposition(int wave)
        {
            spawnQueue.Clear();
            List<EnemyType> enemies = new List<EnemyType>();

            // Base enemy count increases with waves
            int baseCount = 5 + wave * 2;
            int totalEnemies = Mathf.Min(baseCount, maxEnemiesPerWave);

            // Wave composition changes based on wave number
            if (wave <= 3)
            {
                // Early waves: Mostly scouts
                for (int i = 0; i < totalEnemies; i++)
                    enemies.Add(EnemyType.Scout);
            }
            else if (wave <= 6)
            {
                // Mix of scouts and soldiers
                int scouts = totalEnemies / 2;
                int soldiers = totalEnemies - scouts;
                for (int i = 0; i < scouts; i++) enemies.Add(EnemyType.Scout);
                for (int i = 0; i < soldiers; i++) enemies.Add(EnemyType.Soldier);
            }
            else if (wave <= 10)
            {
                // Add tanks
                int scouts = totalEnemies / 3;
                int soldiers = totalEnemies / 3;
                int tanks = totalEnemies - scouts - soldiers;
                for (int i = 0; i < scouts; i++) enemies.Add(EnemyType.Scout);
                for (int i = 0; i < soldiers; i++) enemies.Add(EnemyType.Soldier);
                for (int i = 0; i < tanks; i++) enemies.Add(EnemyType.Tank);
            }
            else if (wave <= 20)
            {
                // Add elites
                int scouts = totalEnemies / 4;
                int soldiers = totalEnemies / 4;
                int tanks = totalEnemies / 4;
                int elites = totalEnemies - scouts - soldiers - tanks;
                for (int i = 0; i < scouts; i++) enemies.Add(EnemyType.Scout);
                for (int i = 0; i < soldiers; i++) enemies.Add(EnemyType.Soldier);
                for (int i = 0; i < tanks; i++) enemies.Add(EnemyType.Tank);
                for (int i = 0; i < elites; i++) enemies.Add(EnemyType.Elite);
            }
            else
            {
                // Late game: All types including bosses
                int scouts = totalEnemies / 5;
                int soldiers = totalEnemies / 5;
                int tanks = totalEnemies / 5;
                int elites = totalEnemies / 5;
                int bosses = Mathf.Max(1, wave / 10);
                for (int i = 0; i < scouts; i++) enemies.Add(EnemyType.Scout);
                for (int i = 0; i < soldiers; i++) enemies.Add(EnemyType.Soldier);
                for (int i = 0; i < tanks; i++) enemies.Add(EnemyType.Tank);
                for (int i = 0; i < elites; i++) enemies.Add(EnemyType.Elite);
                for (int i = 0; i < bosses; i++) enemies.Add(EnemyType.Boss);
            }

            // Boss wave every 5 waves
            if (wave % 5 == 0 && wave > 0)
            {
                enemies.Add(EnemyType.Boss);
            }

            // Shuffle for variety
            ShuffleList(enemies);

            // Queue them up
            foreach (var enemy in enemies)
            {
                spawnQueue.Enqueue(enemy);
            }

            EnemiesToSpawn = spawnQueue.Count;
            EnemiesRemaining = EnemiesToSpawn;
            OnEnemyCountChanged?.Invoke(EnemiesRemaining);
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            while (spawnQueue.Count > 0)
            {
                EnemyType type = spawnQueue.Dequeue();
                SpawnEnemy(type);
                EnemiesToSpawn = spawnQueue.Count;

                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        private void SpawnEnemy(EnemyType type)
        {
            // Get from object pool or instantiate
            GameObject enemy = ObjectPooler.Instance?.GetPooledObject($"Enemy_{type}");
            
            if (enemy == null && enemyPrefabs.TryGetValue(type, out GameObject prefab))
            {
                enemy = Instantiate(prefab);
            }

            if (enemy != null)
            {
                enemy.transform.position = spawnPoint.position;
                enemy.SetActive(true);

                // Configure enemy with wave scaling
                var enemyComponent = enemy.GetComponent<Enemies.Enemy>();
                if (enemyComponent != null)
                {
                    float healthMultiplier = 1f + (CurrentWave - 1) * healthScalePerWave;
                    float speedMultiplier = 1f + (CurrentWave - 1) * speedScalePerWave;
                    enemyComponent.Initialize(waypoints, healthMultiplier, speedMultiplier);
                }
            }
        }

        /// <summary>
        /// Called when an enemy dies or reaches the end
        /// </summary>
        public void OnEnemyRemoved(Enemies.Enemy enemy = null, bool wasKilled = false)
        {
            EnemiesRemaining--;
            OnEnemyCountChanged?.Invoke(EnemiesRemaining);

            if (wasKilled && enemy != null)
            {
                OnEnemyKilled?.Invoke(enemy);
                bool isBoss = enemy is Enemies.BossEnemy;
                if (isBoss) OnBossSpawned?.Invoke(); // reuse event; bosses announce themselves
            }

            if (EnemiesRemaining <= 0 && spawnQueue.Count == 0)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            WaveInProgress = false;
            OnWaveCompleted?.Invoke(CurrentWave);

            // Award bonus
            GameManager.Instance?.AwardWaveBonus();

            // Trigger achievement check
            if (GameManager.Instance != null)
            {
                bool livesIntact = GameManager.Instance.Lives >= GameManager.Instance.StartingLives;
                Progression.AchievementManager.Instance?.CheckWaveComplete(CurrentWave, livesIntact);
            }

            // Check for victory
            if (totalWaves > 0 && CurrentWave >= totalWaves)
            {
                OnAllWavesCompleted?.Invoke();
                Instance_OnAllWavesCompleted?.Invoke();   // EndlessMode hook
                if (!IsEndlessMode)
                    GameManager.Instance?.TriggerVictory();
                return;
            }

            // Auto-start next wave if enabled
            if (autoStartWaves)
            {
                StartCoroutine(AutoStartNextWave());
            }
        }

        private IEnumerator AutoStartNextWave()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            if (!WaveInProgress)
            {
                StartNextWave();
            }
        }

        /// <summary>
        /// Get current wave progress as percentage
        /// </summary>
        public float GetWaveProgress()
        {
            if (EnemiesToSpawn == 0) return 1f;
            return 1f - ((float)EnemiesRemaining / EnemiesToSpawn);
        }

        /// <summary>
        /// Get waypoints array for enemies
        /// </summary>
        public Transform[] GetWaypoints() => waypoints;

        /// <summary>
        /// Set spawn point and waypoints at runtime (called by MapManager)
        /// </summary>
        public void SetPath(Transform spawn, Transform[] points)
        {
            spawnPoint = spawn;
            waypoints  = points;
        }

        // ── Endless Mode Extensions ───────────────────────────────────────────

        /// <summary>True while enemies are actively spawning or alive on the map.</summary>
        public bool IsWaveActive => WaveInProgress || EnemiesRemaining > 0;

        /// <summary>
        /// Static event fired when all designed waves complete.
        /// EndlessMode subscribes to this to trigger the endless loop.
        /// </summary>
        public static System.Action Instance_OnAllWavesCompleted;

        /// <summary>True when EndlessMode has taken over wave generation.</summary>
        public bool IsEndlessMode { get; private set; }

        public void SetEndlessMode(bool enabled)
        {
            IsEndlessMode = enabled;
        }

        /// <summary>
        /// Spawn a single endless wave with the given scaling multipliers.
        /// Called by EndlessMode each loop iteration.
        /// </summary>
        public void SpawnEndlessWave(int effectiveWave, int count, float hpMult, float spdMult)
        {
            if (WaveInProgress) return;

            WaveInProgress = true;
            spawnQueue.Clear();

            // Build composition scaled to count
            var composition = GenerateEndlessComposition(effectiveWave, count);
            foreach (var type in composition)
                spawnQueue.Enqueue(type);

            EnemiesToSpawn = spawnQueue.Count;
            EnemiesRemaining = EnemiesToSpawn;

            // Override spawn multipliers temporarily via coroutine
            spawnCoroutine = StartCoroutine(SpawnEndlessWaveRoutine(hpMult, spdMult));
        }

        private List<EnemyType> GenerateEndlessComposition(int wave, int count)
        {
            var list = new List<EnemyType>();
            float bossChance = Mathf.Min(0.05f + wave * 0.005f, 0.25f);
            float eliteChance = Mathf.Min(0.10f + wave * 0.01f, 0.40f);
            float tankChance = 0.20f;

            for (int i = 0; i < count; i++)
            {
                float r = UnityEngine.Random.value;
                if (r < bossChance)         list.Add(EnemyType.Boss);
                else if (r < eliteChance)   list.Add(EnemyType.Elite);
                else if (r < tankChance)    list.Add(EnemyType.Tank);
                else                        list.Add(EnemyType.Soldier);
            }
            return list;
        }

        private System.Collections.IEnumerator SpawnEndlessWaveRoutine(float hpMult, float spdMult)
        {
            while (spawnQueue.Count > 0)
            {
                if (GameManager.Instance == null || GameManager.Instance.IsGameOver) yield break;
                if (spawnPoint == null) yield break;

                EnemyType type = spawnQueue.Dequeue();
                SpawnEnemy(type, hpMult, spdMult);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        private void SpawnEnemy(EnemyType type, float hpMult, float spdMult)
        {
            if (!enemyPrefabs.TryGetValue(type, out GameObject prefab)) return;

            GameObject go = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.GetFromPool(prefab, spawnPoint.position, Quaternion.identity)
                : Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            if (go == null) return;

            if (go.TryGetComponent<Enemies.Enemy>(out var enemy))
            {
                enemy.Initialize(waypoints,
                    healthMultiplier: hpMult,
                    speedMultiplier: spdMult);
            }
        }
    }
}
