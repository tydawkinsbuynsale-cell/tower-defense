using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// UI panel for configuring custom waves in the map editor.
    /// Allows adding/removing waves and configuring enemy spawns.
    /// </summary>
    public class WaveConfigurationUI : MonoBehaviour
    {
        [Header("Wave Configuration Panel")]
        [SerializeField] private GameObject waveConfigPanel;
        [SerializeField] private Button openWaveConfigButton;
        [SerializeField] private Button closeWaveConfigButton;

        [Header("Wave List")]
        [SerializeField] private Transform waveListContainer;
        [SerializeField] private GameObject waveCardPrefab;
        [SerializeField] private Button addWaveButton;
        [SerializeField] private TextMeshProUGUI waveCountText;

        [Header("Wave Editor")]
        [SerializeField] private GameObject waveEditorPanel;
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TMP_InputField creditsRewardInput;
        [SerializeField] private TMP_InputField timeBetweenGroupsInput;
        [SerializeField] private Toggle bossWaveToggle;
        [SerializeField] private TMP_Dropdown bossTypeDropdown;

        [Header("Enemy Groups")]
        [SerializeField] private Transform enemyGroupContainer;
        [SerializeField] private GameObject enemyGroupPrefab;
        [SerializeField] private Button addEnemyGroupButton;

        [Header("Quick Setup")]
        [SerializeField] private Button generateDefaultWavesButton;
        [SerializeField] private TMP_InputField waveCountInput;
        [SerializeField] private TMP_Dropdown difficultyDropdown;

        [Header("Validation")]
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private GameObject validationPanel;

        // State
        private CustomMapData currentMap;
        private int selectedWaveIndex = -1;
        private List<GameObject> waveCardObjects = new List<GameObject>();
        private List<GameObject> enemyGroupObjects = new List<GameObject>();

        // Enemy type options
        private readonly string[] enemyTypes = new string[]
        {
            "Scout", "Soldier", "Tank", "Elite",
            "Flying", "Healer", "Splitter", "Teleporter",
            "HeavyAssault", "SwarmMother", "ShieldCommander", "Cloaker"
        };

        // ══════════════════════════════════════════════════════════════════════
        // ── Unity Lifecycle ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void Start()
        {
            SetupButtonListeners();
            HidePanel();
            
            if (validationPanel != null)
                validationPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (MapEditorManager.Instance != null)
            {
                MapEditorManager.Instance.OnMapLoaded += OnMapLoaded;
                MapEditorManager.Instance.OnMapSaved += OnMapSaved;
            }
        }

        private void OnDisable()
        {
            if (MapEditorManager.Instance != null)
            {
                MapEditorManager.Instance.OnMapLoaded -= OnMapLoaded;
                MapEditorManager.Instance.OnMapSaved -= OnMapSaved;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Setup ─────────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SetupButtonListeners()
        {
            if (openWaveConfigButton != null)
                openWaveConfigButton.onClick.AddListener(ShowPanel);

            if (closeWaveConfigButton != null)
                closeWaveConfigButton.onClick.AddListener(HidePanel);

            if (addWaveButton != null)
                addWaveButton.onClick.AddListener(OnAddWaveClicked);

            if (addEnemyGroupButton != null)
                addEnemyGroupButton.onClick.AddListener(OnAddEnemyGroupClicked);

            if (generateDefaultWavesButton != null)
                generateDefaultWavesButton.onClick.AddListener(OnGenerateDefaultWavesClicked);

            if (bossWaveToggle != null)
                bossWaveToggle.onValueChanged.AddListener(OnBossWaveToggled);

            // Setup dropdowns
            SetupBossTypeDropdown();
            SetupDifficultyDropdown();
        }

        private void SetupBossTypeDropdown()
        {
            if (bossTypeDropdown == null) return;

            bossTypeDropdown.ClearOptions();
            List<string> bossOptions = new List<string> { "None", "HeavyAssault", "SwarmMother", "ShieldCommander" };
            bossTypeDropdown.AddOptions(bossOptions);
        }

        private void SetupDifficultyDropdown()
        {
            if (difficultyDropdown == null) return;

            difficultyDropdown.ClearOptions();
            List<string> diffOptions = new List<string> { "Tutorial", "Easy", "Normal", "Hard", "Expert" };
            difficultyDropdown.AddOptions(diffOptions);
            difficultyDropdown.value = 2; // Normal
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Panel Management ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        public void ShowPanel()
        {
            if (MapEditorManager.Instance == null || MapEditorManager.Instance.GetCurrentMap() == null)
            {
                Debug.LogWarning("No map loaded for wave configuration!");
                return;
            }

            currentMap = MapEditorManager.Instance.GetCurrentMap();
            waveConfigPanel?.SetActive(true);
            RefreshWaveList();

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("wave_config_opened", new Dictionary<string, object>
                {
                    { "map_id", currentMap.mapId },
                    { "existing_waves", currentMap.waves.Count }
                });
            }
        }

        public void HidePanel()
        {
            waveConfigPanel?.SetActive(false);
            waveEditorPanel?.SetActive(false);
            selectedWaveIndex = -1;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Wave List Management ──────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void RefreshWaveList()
        {
            // Clear existing cards
            foreach (var card in waveCardObjects)
            {
                if (card != null) Destroy(card);
            }
            waveCardObjects.Clear();

            if (currentMap == null || waveListContainer == null) return;

            // Create cards for each wave
            for (int i = 0; i < currentMap.waves.Count; i++)
            {
                CreateWaveCard(i);
            }

            // Update count
            if (waveCountText != null)
                waveCountText.text = $"Waves: {currentMap.waves.Count}";
        }

        private void CreateWaveCard(int waveIndex)
        {
            if (waveCardPrefab == null) return;

            GameObject cardObj = Instantiate(waveCardPrefab, waveListContainer);
            waveCardObjects.Add(cardObj);

            var wave = currentMap.waves[waveIndex];

            // Setup card UI
            var waveNumText = cardObj.transform.Find("WaveNumberText")?.GetComponent<TextMeshProUGUI>();
            if (waveNumText != null)
                waveNumText.text = $"Wave {wave.waveNumber}";

            var enemyCountText = cardObj.transform.Find("EnemyCountText")?.GetComponent<TextMeshProUGUI>();
            if (enemyCountText != null)
            {
                int totalEnemies = 0;
                foreach (var group in wave.enemyGroups)
                    totalEnemies += group.count;
                enemyCountText.text = $"{totalEnemies} enemies";
            }

            var rewardText = cardObj.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();
            if (rewardText != null)
                rewardText.text = $"${wave.creditsReward}";

            // Edit button
            var editButton = cardObj.transform.Find("EditButton")?.GetComponent<Button>();
            if (editButton != null)
            {
                int index = waveIndex; // Capture for closure
                editButton.onClick.AddListener(() => OnEditWaveClicked(index));
            }

            // Delete button
            var deleteButton = cardObj.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (deleteButton != null)
            {
                int index = waveIndex;
                deleteButton.onClick.AddListener(() => OnDeleteWaveClicked(index));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Wave Editing ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void OnAddWaveClicked()
        {
            if (currentMap == null) return;

            // Create new wave
            var newWave = new CustomWaveData
            {
                waveNumber = currentMap.waves.Count + 1,
                creditsReward = 100 + (currentMap.waves.Count * 10),
                timeBetweenGroups = 2f
            };

            // Add default enemy group
            newWave.enemyGroups.Add(new EnemySpawnGroup
            {
                enemyType = "Soldier",
                count = 5,
                spawnInterval = 0.5f,
                spawnPointIndex = 0
            });

            currentMap.waves.Add(newWave);
            RefreshWaveList();

            // Edit the new wave
            OnEditWaveClicked(currentMap.waves.Count - 1);
        }

        private void OnEditWaveClicked(int waveIndex)
        {
            if (currentMap == null || waveIndex < 0 || waveIndex >= currentMap.waves.Count)
                return;

            selectedWaveIndex = waveIndex;
            var wave = currentMap.waves[waveIndex];

            // Show editor panel
            waveEditorPanel?.SetActive(true);

            // Populate fields
            if (waveNumberText != null)
                waveNumberText.text = $"Wave {wave.waveNumber} Configuration";

            if (creditsRewardInput != null)
                creditsRewardInput.text = wave.creditsReward.ToString();

            if (timeBetweenGroupsInput != null)
                timeBetweenGroupsInput.text = wave.timeBetweenGroups.ToString("F1");

            bool isBossWave = !string.IsNullOrEmpty(wave.bossType);
            if (bossWaveToggle != null)
                bossWaveToggle.isOn = isBossWave;

            if (bossTypeDropdown != null && isBossWave)
            {
                int bossIndex = bossTypeDropdown.options.FindIndex(opt => opt.text == wave.bossType);
                if (bossIndex >= 0)
                    bossTypeDropdown.value = bossIndex;
            }

            // Refresh enemy groups
            RefreshEnemyGroupList();
        }

        private void OnDeleteWaveClicked(int waveIndex)
        {
            if (currentMap == null || waveIndex < 0 || waveIndex >= currentMap.waves.Count)
                return;

            currentMap.waves.RemoveAt(waveIndex);

            // Renumber remaining waves
            for (int i = 0; i < currentMap.waves.Count; i++)
            {
                currentMap.waves[i].waveNumber = i + 1;
            }

            RefreshWaveList();

            // Close editor if this wave was being edited
            if (selectedWaveIndex == waveIndex)
            {
                waveEditorPanel?.SetActive(false);
                selectedWaveIndex = -1;
            }
        }

        private void OnBossWaveToggled(bool isEnabled)
        {
            if (bossTypeDropdown != null)
                bossTypeDropdown.gameObject.SetActive(isEnabled);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Enemy Group Editing ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void RefreshEnemyGroupList()
        {
            // Clear existing group UI
            foreach (var groupObj in enemyGroupObjects)
            {
                if (groupObj != null) Destroy(groupObj);
            }
            enemyGroupObjects.Clear();

            if (selectedWaveIndex < 0 || selectedWaveIndex >= currentMap.waves.Count)
                return;

            var wave = currentMap.waves[selectedWaveIndex];

            for (int i = 0; i < wave.enemyGroups.Count; i++)
            {
                CreateEnemyGroupUI(i);
            }
        }

        private void CreateEnemyGroupUI(int groupIndex)
        {
            if (enemyGroupPrefab == null || selectedWaveIndex < 0) return;

            GameObject groupObj = Instantiate(enemyGroupPrefab, enemyGroupContainer);
            enemyGroupObjects.Add(groupObj);

            var wave = currentMap.waves[selectedWaveIndex];
            var group = wave.enemyGroups[groupIndex];

            // Enemy type dropdown
            var typeDropdown = groupObj.transform.Find("EnemyTypeDropdown")?.GetComponent<TMP_Dropdown>();
            if (typeDropdown != null)
            {
                typeDropdown.ClearOptions();
                typeDropdown.AddOptions(new List<string>(enemyTypes));
                int typeIndex = System.Array.IndexOf(enemyTypes, group.enemyType);
                if (typeIndex >= 0)
                    typeDropdown.value = typeIndex;

                int index = groupIndex;
                typeDropdown.onValueChanged.AddListener(value =>
                {
                    wave.enemyGroups[index].enemyType = enemyTypes[value];
                });
            }

            // Count input
            var countInput = groupObj.transform.Find("CountInput")?.GetComponent<TMP_InputField>();
            if (countInput != null)
            {
                countInput.text = group.count.ToString();
                int index = groupIndex;
                countInput.onEndEdit.AddListener(value =>
                {
                    if (int.TryParse(value, out int count))
                        wave.enemyGroups[index].count = Mathf.Clamp(count, 1, 100);
                });
            }

            // Spawn interval input
            var intervalInput = groupObj.transform.Find("SpawnIntervalInput")?.GetComponent<TMP_InputField>();
            if (intervalInput != null)
            {
                intervalInput.text = group.spawnInterval.ToString("F1");
                int index = groupIndex;
                intervalInput.onEndEdit.AddListener(value =>
                {
                    if (float.TryParse(value, out float interval))
                        wave.enemyGroups[index].spawnInterval = Mathf.Clamp(interval, 0.1f, 10f);
                });
            }

            // Delete button
            var deleteButton = groupObj.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (deleteButton != null)
            {
                int index = groupIndex;
                deleteButton.onClick.AddListener(() => OnDeleteEnemyGroupClicked(index));
            }
        }

        private void OnAddEnemyGroupClicked()
        {
            if (selectedWaveIndex < 0 || selectedWaveIndex >= currentMap.waves.Count)
                return;

            var wave = currentMap.waves[selectedWaveIndex];
            wave.enemyGroups.Add(new EnemySpawnGroup
            {
                enemyType = "Soldier",
                count = 5,
                spawnInterval = 0.5f,
                spawnPointIndex = 0
            });

            RefreshEnemyGroupList();
        }

        private void OnDeleteEnemyGroupClicked(int groupIndex)
        {
            if (selectedWaveIndex < 0 || selectedWaveIndex >= currentMap.waves.Count)
                return;

            var wave = currentMap.waves[selectedWaveIndex];
            if (groupIndex >= 0 && groupIndex < wave.enemyGroups.Count)
            {
                wave.enemyGroups.RemoveAt(groupIndex);
                RefreshEnemyGroupList();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Quick Setup ───────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void OnGenerateDefaultWavesClicked()
        {
            if (currentMap == null) return;

            int waveCount = 10;
            if (waveCountInput != null && int.TryParse(waveCountInput.text, out int count))
                waveCount = Mathf.Clamp(count, 5, 30);

            int difficulty = difficultyDropdown != null ? difficultyDropdown.value + 1 : 3;

            // Clear existing waves
            currentMap.waves.Clear();

            // Generate waves
            for (int i = 1; i <= waveCount; i++)
            {
                var wave = GenerateWave(i, difficulty, waveCount);
                currentMap.waves.Add(wave);
            }

            RefreshWaveList();
            ShowValidation($"Generated {waveCount} waves for difficulty level {difficulty}", false);

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("wave_config_generated", new Dictionary<string, object>
                {
                    { "map_id", currentMap.mapId },
                    { "wave_count", waveCount },
                    { "difficulty", difficulty }
                });
            }
        }

        private CustomWaveData GenerateWave(int waveNumber, int difficulty, int totalWaves)
        {
            var wave = new CustomWaveData
            {
                waveNumber = waveNumber,
                creditsReward = 100 + (waveNumber * 10 * difficulty),
                timeBetweenGroups = 2f
            };

            // Calculate wave scaling
            float waveProgress = (float)waveNumber / totalWaves;
            int baseEnemyCount = 5 + (difficulty * 2);
            int enemyCount = Mathf.RoundToInt(baseEnemyCount * (1f + waveProgress * 1.5f));

            // Boss waves every 5 waves
            if (waveNumber % 5 == 0 && waveNumber > 0)
            {
                wave.bossType = GetBossTypeForWave(waveNumber, totalWaves);
                enemyCount = Mathf.RoundToInt(enemyCount * 0.6f); // Fewer regular enemies with boss
            }

            // Determine enemy types based on wave progress
            string primaryEnemyType = GetEnemyTypeForWave(waveNumber, waveProgress, difficulty);
            string secondaryEnemyType = GetSecondaryEnemyType(waveNumber, waveProgress);

            // Add primary enemy group
            wave.enemyGroups.Add(new EnemySpawnGroup
            {
                enemyType = primaryEnemyType,
                count = Mathf.RoundToInt(enemyCount * 0.7f),
                spawnInterval = 0.5f,
                spawnPointIndex = 0
            });

            // Add secondary enemy group for mid-late waves
            if (waveProgress > 0.3f)
            {
                wave.enemyGroups.Add(new EnemySpawnGroup
                {
                    enemyType = secondaryEnemyType,
                    count = Mathf.RoundToInt(enemyCount * 0.3f),
                    spawnInterval = 0.7f,
                    spawnPointIndex = 0
                });
            }

            return wave;
        }

        private string GetEnemyTypeForWave(int waveNumber, float progress, int difficulty)
        {
            if (progress < 0.2f) return "Scout";
            if (progress < 0.4f) return "Soldier";
            if (progress < 0.6f) return difficulty >= 3 ? "Tank" : "Soldier";
            if (progress < 0.8f) return "Elite";
            return "Tank";
        }

        private string GetSecondaryEnemyType(int waveNumber, float progress)
        {
            if (progress < 0.4f) return "Scout";
            if (progress < 0.6f) return "Flying";
            if (progress < 0.8f) return "Healer";
            return "Elite";
        }

        private string GetBossTypeForWave(int waveNumber, int totalWaves)
        {
            float progress = (float)waveNumber / totalWaves;
            if (progress < 0.4f) return "HeavyAssault";
            if (progress < 0.7f) return "SwarmMother";
            return "ShieldCommander";
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Validation ────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void ShowValidation(string message, bool isError)
        {
            if (validationPanel == null || validationText == null) return;

            validationPanel.SetActive(true);
            validationText.text = message;
            validationText.color = isError ? Color.red : Color.green;

            StartCoroutine(HideValidationAfterDelay(3f));
        }

        private System.Collections.IEnumerator HideValidationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (validationPanel != null)
                validationPanel.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Event Handlers ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void OnMapLoaded(CustomMapData map)
        {
            currentMap = map;
            if (waveConfigPanel != null && waveConfigPanel.activeSelf)
            {
                RefreshWaveList();
            }
        }

        private void OnMapSaved()
        {
            // Waves are automatically saved as part of CustomMapData
        }

        public void SaveCurrentWaveChanges()
        {
            if (selectedWaveIndex < 0 || selectedWaveIndex >= currentMap.waves.Count)
                return;

            var wave = currentMap.waves[selectedWaveIndex];

            // Update wave data from UI
            if (creditsRewardInput != null && int.TryParse(creditsRewardInput.text, out int credits))
                wave.creditsReward = credits;

            if (timeBetweenGroupsInput != null && float.TryParse(timeBetweenGroupsInput.text, out float time))
                wave.timeBetweenGroups = time;

            if (bossWaveToggle != null && bossWaveToggle.isOn && bossTypeDropdown != null)
            {
                wave.bossType = bossTypeDropdown.options[bossTypeDropdown.value].text;
                if (wave.bossType == "None")
                    wave.bossType = "";
            }
            else
            {
                wave.bossType = "";
            }

            RefreshWaveList();
        }
    }
}
