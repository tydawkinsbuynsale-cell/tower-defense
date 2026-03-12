using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace RobotTD.UI
{
    /// <summary>
    /// Main menu screen — shown on app launch.
    /// Scene name: "MainMenu"
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject mapSelectPanel;
        [SerializeField] private GameObject techTreePanel;
        [SerializeField] private GameObject achievementsPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject endlessModePanel;
        [SerializeField] private GameObject bossRushPanel;

        [Header("Main Panel")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button endlessModeButton;
        [SerializeField] private Button bossRushButton;
        [SerializeField] private Button techTreeButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI techPointsText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Loading")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Transition")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeTime = 0.4f;

        private void Start()
        {
            ShowPanel(mainPanel);
            RefreshPlayerInfo();

            playButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
                ShowPanel(mapSelectPanel);
            });
            if (endlessModeButton != null)
            {
                endlessModeButton.onClick.AddListener(() =>
                {
                    Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                    ShowPanel(endlessModePanel);
                });
            }
            if (bossRushButton != null)
            {
                bossRushButton.onClick.AddListener(() =>
                {
                    Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                    ShowPanel(bossRushPanel);
                });
            }
            techTreeButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                ShowPanel(techTreePanel);
            });
            achievementsButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                ShowPanel(achievementsPanel);
            });
            settingsButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                ShowPanel(settingsPanel);
            });
            creditsButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                ShowPanel(creditsPanel);
            });

            if (versionText != null)
                versionText.text = $"v{Application.version}";

            Audio.AudioManager.Instance?.PlayMusic(Audio.MusicTrack.MainMenu);

            // Fade in
            StartCoroutine(FadeIn());
        }

        private void RefreshPlayerInfo()
        {
            var data = Core.SaveManager.Instance?.Data;
            if (data == null) return;

            if (playerLevelText != null)
                playerLevelText.text = $"LVL {data.playerLevel}";

            if (techPointsText != null)
                techPointsText.text = $"{data.techPoints} TP";

            if (xpSlider != null)
                xpSlider.value = Core.SaveManager.Instance.GetXPProgressToNextLevel();
        }

        private void ShowPanel(GameObject target)
        {
            mainPanel.SetActive(target == mainPanel);
            mapSelectPanel.SetActive(target == mapSelectPanel);
            techTreePanel.SetActive(target == techTreePanel);
            achievementsPanel.SetActive(target == achievementsPanel);
            settingsPanel.SetActive(target == settingsPanel);
            creditsPanel.SetActive(target == creditsPanel);
            if (endlessModePanel != null)
                endlessModePanel.SetActive(target == endlessModePanel);
            if (bossRushPanel != null)
                bossRushPanel.SetActive(target == bossRushPanel);
        }

        public void BackToMain()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            ShowPanel(mainPanel);
            RefreshPlayerInfo();
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            if (loadingScreen != null) loadingScreen.SetActive(true);

            // Fade out
            yield return StartCoroutine(FadeOut());

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);

                if (loadingBar != null) loadingBar.value = progress;
                if (loadingText != null) loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

                if (op.progress >= 0.9f)
                    op.allowSceneActivation = true;

                yield return null;
            }
        }

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;
            fadeCanvasGroup.alpha = 1f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - elapsed / fadeTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }

        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;
            fadeCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = elapsed / fadeTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
    }

    // ── Map Select ───────────────────────────────────────────────────────────

    [System.Serializable]
    public class MapEntry
    {
        public string mapId;
        public string displayName;
        public string sceneName;
        [TextArea(1, 2)] public string description;
        public Sprite thumbnail;
        public int difficulty; // 1–5
        public int totalWaves;
        public string[] prerequisites; // MapIds that must be completed first
    }

    public class MapSelectUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MapEntry[] maps;
        [SerializeField] private Map.MapRegistry mapRegistry; // NEW: Use MapRegistry for dynamic map loading
        [SerializeField] private Transform mapButtonContainer;
        [SerializeField] private GameObject mapButtonPrefab;
        [SerializeField] private MainMenuUI mainMenu;

        [Header("Preview Panel")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private Image previewThumbnail;
        [SerializeField] private TextMeshProUGUI previewName;
        [SerializeField] private TextMeshProUGUI previewDescription;
        [SerializeField] private TextMeshProUGUI previewWavesText;
        [SerializeField] private GameObject[] difficultyStars; // 5 star objects
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject[] scoreStars; // 3 earned stars
        [SerializeField] private Button launchButton;
        [SerializeField] private TextMeshProUGUI lockedText;

        private MapEntry selectedMap;

        private void OnEnable()
        {
            BuildMapList();
            previewPanel.SetActive(false);
        }

        private void BuildMapList()
        {
            // Clear existing
            foreach (Transform child in mapButtonContainer)
                Destroy(child.gameObject);

            var save = Core.SaveManager.Instance?.Data;

            // Use MapRegistry if available, otherwise fall back to manual MapEntry array
            MapEntry[] mapsToDisplay = GetMapsToDisplay();

            foreach (var map in mapsToDisplay)
            {
                bool unlocked = save == null || save.unlockedMaps.Contains(map.mapId);
                bool prereqMet = ArePrerequisitesMet(map, save);

                GameObject btn = Instantiate(mapButtonPrefab, mapButtonContainer);
                var mapBtn = btn.GetComponent<MapButton>();

                if (mapBtn != null)
                {
                    Core.MapRecord record = null;
                    save?.mapRecords.TryGetValue(map.mapId, out record);
                    mapBtn.Setup(map, unlocked && prereqMet, record, () => SelectMap(map));
                }
            }
        }

        private MapEntry[] GetMapsToDisplay()
        {
            // Prefer MapRegistry if set (dynamic loading from ScriptableObjects)
            if (mapRegistry != null && mapRegistry.MapCount > 0)
            {
                return mapRegistry.ToMapEntries();
            }

            // Fallback to manually configured MapEntry array
            return maps;
        }

        private bool ArePrerequisitesMet(MapEntry map, Core.PlayerSaveData save)
        {
            if (map.prerequisites == null || map.prerequisites.Length == 0) return true;
            if (save == null) return false;

            foreach (var prereq in map.prerequisites)
            {
                if (!save.mapRecords.TryGetValue(prereq, out var r) || !r.completed)
                    return false;
            }
            return true;
        }

        private void SelectMap(MapEntry map)
        {
            selectedMap = map;
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);

            previewPanel.SetActive(true);
            previewName.text = map.displayName;
            previewDescription.text = map.description;
            previewWavesText.text = $"{map.totalWaves} Waves";

            if (previewThumbnail != null && map.thumbnail != null)
                previewThumbnail.sprite = map.thumbnail;

            // Difficulty stars
            for (int i = 0; i < difficultyStars.Length; i++)
                difficultyStars[i].SetActive(i < map.difficulty);

            // High score / earned stars
            var save = Core.SaveManager.Instance?.Data;
            Core.MapRecord record = null;
            save?.mapRecords.TryGetValue(map.mapId, out record);

            highScoreText.text = record != null ? $"Best: {record.highScore:N0}" : "Best: ---";

            if (scoreStars != null)
                for (int i = 0; i < scoreStars.Length; i++)
                    scoreStars[i].SetActive(record != null && record.starsEarned > i);

            bool isUnlocked = save == null || save.unlockedMaps.Contains(map.mapId);
            launchButton.gameObject.SetActive(isUnlocked);
            lockedText.gameObject.SetActive(!isUnlocked);

            launchButton.onClick.RemoveAllListeners();
            launchButton.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
                
                // Set the selected map in MapSelector before loading scene
                var mapSelector = Core.MapSelector.Instance;
                if (mapSelector != null && mapRegistry != null)
                {
                    var mapData = mapRegistry.GetMap(map.mapId);
                    if (mapData != null)
                    {
                        mapSelector.SelectMap(mapData);
                    }
                }
                
                mainMenu.LoadScene(map.sceneName);
            });
        }
    }

    // ── Small map button component ───────────────────────────────────────────

    public class MapButton : MonoBehaviour
    {
        [SerializeField] private Image thumbnail;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject completedBadge;
        [SerializeField] private GameObject[] stars; // 3 stars
        [SerializeField] private Button button;

        public void Setup(MapEntry map, bool unlocked, Core.MapRecord record, System.Action onClick)
        {
            if (thumbnail != null && map.thumbnail != null)
                thumbnail.sprite = map.thumbnail;

            if (nameText != null) nameText.text = map.displayName;
            if (lockIcon != null) lockIcon.SetActive(!unlocked);
            if (completedBadge != null) completedBadge.SetActive(record?.completed == true);

            if (stars != null)
                for (int i = 0; i < stars.Length; i++)
                    stars[i].SetActive(record != null && record.starsEarned > i);

            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
