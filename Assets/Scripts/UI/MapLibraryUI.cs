using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Analytics;

/// <summary>
/// UI panel for browsing and managing the local custom map library.
/// Allows players to view, load, delete, and export their custom maps.
/// </summary>
public class MapLibraryUI : MonoBehaviour
{
    #region UI References
    [Header("Main Panel")]
    [SerializeField] private GameObject libraryPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;

    [Header("Search & Filter")]
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Button searchButton;
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private TMP_Dropdown difficultyFilterDropdown;

    [Header("Map List")]
    [SerializeField] private Transform mapListContent;
    [SerializeField] private GameObject mapCardPrefab;
    [SerializeField] private TextMeshProUGUI emptyListText;

    [Header("Storage Info")]
    [SerializeField] private TextMeshProUGUI storageInfoText;
    [SerializeField] private Slider storageUsageSlider;

    [Header("Selected Map Details")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI detailMapName;
    [SerializeField] private TextMeshProUGUI detailAuthor;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private TextMeshProUGUI detailGridSize;
    [SerializeField] private TextMeshProUGUI detailDifficulty;
    [SerializeField] private TextMeshProUGUI detailStats;
    [SerializeField] private RawImage detailThumbnail;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button duplicateButton;

    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    #endregion

    #region Private Fields
    private List<MapCardUI> mapCards = new List<MapCardUI>();
    private CustomMapMetadata selectedMap;
    private SortMode currentSortMode = SortMode.DateModified;
    private int currentDifficultyFilter = 0; // 0 = All
    private System.Action currentConfirmAction;
    private bool isVisible = false;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        SetupButtons();
        SetupSearchFilter();

        if (libraryPanel != null)
        {
            libraryPanel.SetActive(false);
        }

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }

        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
    }

    private void OnEnable()
    {
        CustomMapStorage.OnMapSaved += OnMapSaved;
        CustomMapStorage.OnMapDeleted += OnMapDeleted;
        CustomMapStorage.OnLibraryRefreshed += OnLibraryRefreshed;
    }

    private void OnDisable()
    {
        CustomMapStorage.OnMapSaved -= OnMapSaved;
        CustomMapStorage.OnMapDeleted -= OnMapDeleted;
        CustomMapStorage.OnLibraryRefreshed -= OnLibraryRefreshed;
    }
    #endregion

    #region Setup
    private void SetupButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLibrary);

        if (searchButton != null)
            searchButton.onClick.AddListener(PerformSearch);

        if (loadButton != null)
            loadButton.onClick.AddListener(LoadSelectedMap);

        if (editButton != null)
            editButton.onClick.AddListener(EditSelectedMap);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(RequestDeleteMap);

        if (exportButton != null)
            exportButton.onClick.AddListener(ExportSelectedMap);

        if (duplicateButton != null)
            duplicateButton.onClick.AddListener(DuplicateSelectedMap);

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmAction);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CancelConfirmation);
    }

    private void SetupSearchFilter()
    {
        if (sortDropdown != null)
        {
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string>
            {
                "Date Modified (Newest)",
                "Date Created (Newest)",
                "Name (A-Z)",
                "Name (Z-A)",
                "Play Count (Most)",
                "Rating (Highest)",
                "Difficulty (Easiest)",
                "Difficulty (Hardest)"
            });
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        if (difficultyFilterDropdown != null)
        {
            difficultyFilterDropdown.ClearOptions();
            difficultyFilterDropdown.AddOptions(new List<string>
            {
                "All Difficulties",
                "Tutorial",
                "Easy",
                "Normal",
                "Hard",
                "Expert"
            });
            difficultyFilterDropdown.onValueChanged.AddListener(OnDifficultyFilterChanged);
        }

        if (searchInput != null)
        {
            searchInput.onEndEdit.AddListener(_ => PerformSearch());
        }
    }
    #endregion

    #region Show/Hide
    public void Show()
    {
        if (isVisible) return;

        if (libraryPanel != null)
        {
            libraryPanel.SetActive(true);
        }

        RefreshLibrary();
        UpdateStorageInfo();

        if (panelCanvasGroup != null)
        {
            LeanTween.alphaCanvas(panelCanvasGroup, 1f, fadeInDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }

        isVisible = true;

        // Analytics
        AnalyticsManager.Instance?.LogEvent(AnalyticsEvents.MAP_LIBRARY_OPENED, new Dictionary<string, object>
        {
            { AnalyticsParameters.TOTAL_MAPS, CustomMapStorage.Instance?.GetMapCount() ?? 0 },
            { AnalyticsParameters.STORAGE_SIZE, CustomMapStorage.Instance?.GetTotalStorageSize() ?? 0 }
        });
    }

    public void Hide()
    {
        if (!isVisible) return;

        if (panelCanvasGroup != null)
        {
            LeanTween.alphaCanvas(panelCanvasGroup, 0f, fadeOutDuration)
                .setEase(LeanTweenType.easeInQuad)
                .setOnComplete(() =>
                {
                    if (libraryPanel != null)
                    {
                        libraryPanel.SetActive(false);
                    }
                });
        }
        else
        {
            if (libraryPanel != null)
            {
                libraryPanel.SetActive(false);
            }
        }

        isVisible = false;
        DeselectMap();
    }
    #endregion

    #region Library Management
    private void RefreshLibrary()
    {
        if (CustomMapStorage.Instance == null)
        {
            Debug.LogWarning("CustomMapStorage not available");
            return;
        }

        CustomMapStorage.Instance.RefreshMapLibrary();
        UpdateMapList();
    }

    private void UpdateMapList()
    {
        // Clear existing cards
        foreach (var card in mapCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        mapCards.Clear();

        // Get filtered and sorted maps
        List<CustomMapMetadata> maps = GetFilteredAndSortedMaps();

        // Show/hide empty message
        if (emptyListText != null)
        {
            emptyListText.gameObject.SetActive(maps.Count == 0);
        }

        // Create cards
        foreach (var mapMetadata in maps)
        {
            CreateMapCard(mapMetadata);
        }
    }

    private List<CustomMapMetadata> GetFilteredAndSortedMaps()
    {
        if (CustomMapStorage.Instance == null)
            return new List<CustomMapMetadata>();

        List<CustomMapMetadata> maps;

        // Apply search filter
        string searchTerm = searchInput != null ? searchInput.text : "";
        if (!string.IsNullOrEmpty(searchTerm))
        {
            maps = CustomMapStorage.Instance.SearchMaps(searchTerm);
        }
        else
        {
            maps = CustomMapStorage.Instance.GetAllMaps();
        }

        // Apply difficulty filter
        if (currentDifficultyFilter > 0)
        {
            maps = maps.Where(m => m.difficulty == currentDifficultyFilter).ToList();
        }

        // Apply sort
        maps = SortMaps(maps, currentSortMode);

        return maps;
    }

    private List<CustomMapMetadata> SortMaps(List<CustomMapMetadata> maps, SortMode sortMode)
    {
        switch (sortMode)
        {
            case SortMode.DateModified:
                return maps.OrderByDescending(m => m.lastModifiedDate).ToList();
            case SortMode.DateCreated:
                return maps.OrderByDescending(m => m.creationDate).ToList();
            case SortMode.NameAscending:
                return maps.OrderBy(m => m.mapName).ToList();
            case SortMode.NameDescending:
                return maps.OrderByDescending(m => m.mapName).ToList();
            case SortMode.PlayCount:
                return maps.OrderByDescending(m => m.playCount).ToList();
            case SortMode.Rating:
                return maps.OrderByDescending(m => m.rating).ToList();
            case SortMode.DifficultyEasy:
                return maps.OrderBy(m => m.difficulty).ToList();
            case SortMode.DifficultyHard:
                return maps.OrderByDescending(m => m.difficulty).ToList();
            default:
                return maps;
        }
    }

    private void CreateMapCard(CustomMapMetadata mapMetadata)
    {
        if (mapCardPrefab == null || mapListContent == null)
            return;

        GameObject cardObj = Instantiate(mapCardPrefab, mapListContent);
        MapCardUI card = cardObj.GetComponent<MapCardUI>();

        if (card != null)
        {
            card.Setup(mapMetadata, OnMapCardClicked);
            mapCards.Add(card);
        }
    }
    #endregion

    #region Map Selection
    private void OnMapCardClicked(CustomMapMetadata mapMetadata)
    {
        selectedMap = mapMetadata;
        ShowMapDetails(mapMetadata);
    }

    private void ShowMapDetails(CustomMapMetadata mapMetadata)
    {
        if (detailsPanel == null) return;

        detailsPanel.SetActive(true);

        if (detailMapName != null)
            detailMapName.text = mapMetadata.mapName;

        if (detailAuthor != null)
            detailAuthor.text = $"by {mapMetadata.authorName}";

        if (detailGridSize != null)
            detailGridSize.text = $"Grid: {mapMetadata.gridWidth}x{mapMetadata.gridHeight}";

        if (detailDifficulty != null)
            detailDifficulty.text = GetDifficultyName(mapMetadata.difficulty);

        if (detailStats != null)
        {
            detailStats.text = $"Played: {mapMetadata.playCount} times\n" +
                             $"Rating: {mapMetadata.rating:F1}/5.0\n" +
                             $"Created: {mapMetadata.creationDate}\n" +
                             $"Modified: {mapMetadata.lastModifiedDate}";
        }

        // Load thumbnail
        if (detailThumbnail != null && CustomMapStorage.Instance != null)
        {
            Texture2D thumbnail = CustomMapStorage.Instance.LoadThumbnail(mapMetadata.mapId);
            detailThumbnail.texture = thumbnail;
            detailThumbnail.gameObject.SetActive(thumbnail != null);
        }

        // Load description if available
        if (detailDescription != null)
        {
            CustomMapData fullData = CustomMapStorage.Instance?.LoadMap(mapMetadata.mapId);
            detailDescription.text = fullData?.description ?? "No description available.";
        }
    }

    private void DeselectMap()
    {
        selectedMap = null;
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(false);
        }
    }
    #endregion

    #region Map Actions
    private void LoadSelectedMap()
    {
        if (selectedMap == null) return;

        CustomMapData mapData = CustomMapStorage.Instance?.LoadMap(selectedMap.mapId);

        if (mapData != null)
        {
            // TODO: Load map into game or editor
            Debug.Log($"Loading map: {mapData.mapName}");
            ShowStatusMessage($"Map loaded: {mapData.mapName}");
        }
    }

    private void EditSelectedMap()
    {
        if (selectedMap == null) return;

        CustomMapData mapData = CustomMapStorage.Instance?.LoadMap(selectedMap.mapId);

        if (mapData != null)
        {
            // TODO: Open map in editor
            Debug.Log($"Opening map in editor: {mapData.mapName}");
            // MapEditorManager.Instance?.LoadMap(mapData);
            Hide();
        }
    }

    private void RequestDeleteMap()
    {
        if (selectedMap == null) return;

        ShowConfirmationDialog(
            $"Delete map \"{selectedMap.mapName}\"?\n\nThis action cannot be undone.",
            () => DeleteSelectedMap()
        );
    }

    private void DeleteSelectedMap()
    {
        if (selectedMap == null) return;

        bool success = CustomMapStorage.Instance?.DeleteMap(selectedMap.mapId, true) ?? false;

        if (success)
        {
            ShowStatusMessage($"Map deleted: {selectedMap.mapName}");

            // Analytics
            AnalyticsManager.Instance?.LogEvent(AnalyticsEvents.MAP_STORAGE_DELETED, new Dictionary<string, object>
            {
                { AnalyticsParameters.MAP_ID, selectedMap.mapId },
                { AnalyticsParameters.MAP_NAME, selectedMap.mapName }
            });

            DeselectMap();
            RefreshLibrary();
            UpdateStorageInfo();
        }
        else
        {
            ShowStatusMessage("Failed to delete map", true);
        }
    }

    private void ExportSelectedMap()
    {
        if (selectedMap == null) return;

        // TODO: Open file save dialog
        string exportPath = $"{Application.persistentDataPath}/{selectedMap.mapName}.rtdmap";
        bool success = CustomMapStorage.Instance?.ExportMap(selectedMap.mapId, exportPath) ?? false;

        if (success)
        {
            ShowStatusMessage($"Map exported to: {exportPath}");

            // Analytics
            AnalyticsManager.Instance?.LogEvent(AnalyticsEvents.MAP_STORAGE_EXPORTED, new Dictionary<string, object>
            {
                { AnalyticsParameters.MAP_ID, selectedMap.mapId },
                { AnalyticsParameters.MAP_NAME, selectedMap.mapName }
            });
        }
        else
        {
            ShowStatusMessage("Failed to export map", true);
        }
    }

    private void DuplicateSelectedMap()
    {
        if (selectedMap == null) return;

        CustomMapData originalMap = CustomMapStorage.Instance?.LoadMap(selectedMap.mapId);

        if (originalMap != null)
        {
            string newName = $"{originalMap.mapName} (Copy)";
            CustomMapData duplicatedMap = CustomMapStorage.Instance?.SaveMapAs(originalMap, newName);

            if (duplicatedMap != null)
            {
                ShowStatusMessage($"Map duplicated: {newName}");
                RefreshLibrary();
                UpdateStorageInfo();
            }
            else
            {
                ShowStatusMessage("Failed to duplicate map", true);
            }
        }
    }
    #endregion

    #region Search & Filter
    private void PerformSearch()
    {
        UpdateMapList();
    }

    private void OnSortChanged(int index)
    {
        currentSortMode = (SortMode)index;
        UpdateMapList();
    }

    private void OnDifficultyFilterChanged(int index)
    {
        currentDifficultyFilter = index;
        UpdateMapList();
    }
    #endregion

    #region Storage Info
    private void UpdateStorageInfo()
    {
        if (CustomMapStorage.Instance == null) return;

        int mapCount = CustomMapStorage.Instance.GetMapCount();
        long storageSize = CustomMapStorage.Instance.GetTotalStorageSize();

        if (storageInfoText != null)
        {
            string sizeStr = FormatBytes(storageSize);
            storageInfoText.text = $"{mapCount} Maps | {sizeStr}";
        }

        if (storageUsageSlider != null)
        {
            // Assuming 100 maps is the max (can be adjusted)
            storageUsageSlider.value = mapCount / 100f;
        }
    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F1} KB";
        else
            return $"{bytes / (1024f * 1024f):F1} MB";
    }
    #endregion

    #region Confirmation Dialog
    private void ShowConfirmationDialog(string message, System.Action onConfirm)
    {
        if (confirmationDialog == null) return;

        currentConfirmAction = onConfirm;
        confirmationDialog.SetActive(true);

        if (confirmationText != null)
        {
            confirmationText.text = message;
        }
    }

    private void ConfirmAction()
    {
        currentConfirmAction?.Invoke();
        CancelConfirmation();
    }

    private void CancelConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        currentConfirmAction = null;
    }
    #endregion

    #region UI Feedback
    private void ShowStatusMessage(string message, bool isError = false)
    {
        Debug.Log($"[MapLibrary] {message}");
        // TODO: Show toast notification
    }
    #endregion

    #region Helpers
    private string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return "Tutorial";
            case 2: return "Easy";
            case 3: return "Normal";
            case 4: return "Hard";
            case 5: return "Expert";
            default: return "Normal";
        }
    }
    #endregion

    #region Event Handlers
    private void OnMapSaved(CustomMapData mapData)
    {
        RefreshLibrary();
        UpdateStorageInfo();
    }

    private void OnMapDeleted(string mapId)
    {
        if (selectedMap != null && selectedMap.mapId == mapId)
        {
            DeselectMap();
        }
        RefreshLibrary();
        UpdateStorageInfo();
    }

    private void OnLibraryRefreshed()
    {
        UpdateMapList();
        UpdateStorageInfo();
    }
    #endregion

    #region Nested Types
    private enum SortMode
    {
        DateModified = 0,
        DateCreated = 1,
        NameAscending = 2,
        NameDescending = 3,
        PlayCount = 4,
        Rating = 5,
        DifficultyEasy = 6,
        DifficultyHard = 7
    }
    #endregion
}
