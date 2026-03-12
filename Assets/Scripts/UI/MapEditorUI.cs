using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RobotTD.Core;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// Main UI panel for the custom map editor.
    /// Provides tool palette, properties, and map controls.
    /// </summary>
    public class MapEditorUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject editorPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button testPlayButton;

        [Header("Tool Palette")]
        [SerializeField] private Button tileModeButton;
        [SerializeField] private Button pathModeButton;
        [SerializeField] private Button spawnModeButton;
        [SerializeField] private Button baseModeButton;
        [SerializeField] private Button obstacleModeButton;
        [SerializeField] private Button decorationModeButton;
        [SerializeField] private Color selectedToolColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color normalToolColor = Color.white;

        [Header("Tile Type Buttons")]
        [SerializeField] private Button buildableTileButton;
        [SerializeField] private Button pathTileButton;
        [SerializeField] private Button obstacleTileButton;
        [SerializeField] private Button waterTileButton;

        [Header("Edit Tools")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private Button clearMapButton;
        [SerializeField] private Button fillToolButton;
        [SerializeField] private Button brushToolButton;
        [SerializeField] private Slider brushSizeSlider;
        [SerializeField] private TextMeshProUGUI brushSizeText;

        [Header("Map Properties")]
        [SerializeField] private TMP_InputField mapNameInput;
        [SerializeField] private TMP_InputField descriptionInput;
        [SerializeField] private TMP_InputField startingCreditsInput;
        [SerializeField] private TMP_InputField startingLivesInput;
        [SerializeField] private Slider difficultySlider;
        [SerializeField] private TextMeshProUGUI difficultyText;

        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI coordText;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private GameObject unsavedChangesIndicator;

        [Header("Validation Panel")]
        [SerializeField] private GameObject validationPanel;
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private Button validateButton;
        [SerializeField] private Button closeValidationButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        // State
        private EditorMode currentMode = EditorMode.Tile;
        private TileType currentTileType = TileType.Buildable;
        private int currentBrushSize = 1;
        private bool isToolActive = false;
        private Vector2Int hoveredTile = Vector2Int.zero;
        private Action pendingConfirmAction;

        // ══════════════════════════════════════════════════════════════════════
        // ── Unity Lifecycle ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void Start()
        {
            SetupButtonListeners();
            HidePanel();
            
            if (validationPanel != null)
                validationPanel.SetActive(false);
            
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void OnEnable()
        {
            if (MapEditorManager.Instance != null)
            {
                MapEditorManager.Instance.OnMapLoaded += OnMapLoaded;
                MapEditorManager.Instance.OnMapSaved += OnMapSaved;
                MapEditorManager.Instance.OnModeChanged += OnModeChanged;
                MapEditorManager.Instance.OnUndoRedoChanged += OnUndoRedoChanged;
                MapEditorManager.Instance.OnTileChanged += OnTileChanged;
            }
        }

        private void OnDisable()
        {
            if (MapEditorManager.Instance != null)
            {
                MapEditorManager.Instance.OnMapLoaded -= OnMapLoaded;
                MapEditorManager.Instance.OnMapSaved -= OnMapSaved;
                MapEditorManager.Instance.OnModeChanged -= OnModeChanged;
                MapEditorManager.Instance.OnUndoRedoChanged -= OnUndoRedoChanged;
                MapEditorManager.Instance.OnTileChanged -= OnTileChanged;
            }
        }

        private void Update()
        {
            if (!editorPanel.activeSelf) return;

            UpdateStatusDisplay();
            UpdateUnsavedIndicator();
            
            // Handle mouse input for editing
            HandleMouseInput();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Setup ─────────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SetupButtonListeners()
        {
            // Main controls
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);
            
            if (testPlayButton != null)
                testPlayButton.onClick.AddListener(OnTestPlayClicked);

            // Tool palette
            if (tileModeButton != null)
                tileModeButton.onClick.AddListener(() => SetMode(EditorMode.Tile));
            
            if (pathModeButton != null)
                pathModeButton.onClick.AddListener(() => SetMode(EditorMode.Path));
            
            if (spawnModeButton != null)
                spawnModeButton.onClick.AddListener(() => SetMode(EditorMode.Spawn));
            
            if (baseModeButton != null)
                baseModeButton.onClick.AddListener(() => SetMode(EditorMode.Base));
            
            if (obstacleModeButton != null)
                obstacleModeButton.onClick.AddListener(() => SetMode(EditorMode.Obstacle));
            
            if (decorationModeButton != null)
                decorationModeButton.onClick.AddListener(() => SetMode(EditorMode.Decoration));

            // Tile types
            if (buildableTileButton != null)
                buildableTileButton.onClick.AddListener(() => SetTileType(TileType.Buildable));
            
            if (pathTileButton != null)
                pathTileButton.onClick.AddListener(() => SetTileType(TileType.Path));
            
            if (obstacleTileButton != null)
                obstacleTileButton.onClick.AddListener(() => SetTileType(TileType.Obstacle));
            
            if (waterTileButton != null)
                waterTileButton.onClick.AddListener(() => SetTileType(TileType.Water));

            // Edit tools
            if (undoButton != null)
                undoButton.onClick.AddListener(OnUndoClicked);
            
            if (redoButton != null)
                redoButton.onClick.AddListener(OnRedoClicked);
            
            if (clearMapButton != null)
                clearMapButton.onClick.AddListener(OnClearMapClicked);

            // Brush size
            if (brushSizeSlider != null)
            {
                brushSizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);
                brushSizeSlider.value = 1;
            }

            // Map properties
            if (mapNameInput != null)
                mapNameInput.onEndEdit.AddListener(OnMapNameChanged);
            
            if (startingCreditsInput != null)
                startingCreditsInput.onEndEdit.AddListener(OnStartingCreditsChanged);
            
            if (startingLivesInput != null)
                startingLivesInput.onEndEdit.AddListener(OnStartingLivesChanged);
            
            if (difficultySlider != null)
                difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);

            // Validation
            if (validateButton != null)
                validateButton.onClick.AddListener(OnValidateClicked);
            
            if (closeValidationButton != null)
                closeValidationButton.onClick.AddListener(() => validationPanel.SetActive(false));

            // Confirmation dialog
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Public API ────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Show the editor UI for a new map.
        /// </summary>
        public void ShowForNewMap(string mapName = "New Map", int width = 20, int height = 15)
        {
            if (MapEditorManager.Instance == null) return;

            MapEditorManager.Instance.CreateNewMap(mapName, width, height);
            ShowPanel();
        }

        /// <summary>
        /// Show the editor UI for an existing map.
        /// </summary>
        public void ShowForExistingMap(CustomMapData mapData)
        {
            if (MapEditorManager.Instance == null || mapData == null) return;

            MapEditorManager.Instance.LoadMap(mapData);
            ShowPanel();
        }

        /// <summary>
        /// Hide the editor UI.
        /// </summary>
        public void Hide()
        {
            HidePanel();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Panel Management ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void ShowPanel()
        {
            if (editorPanel != null)
                editorPanel.SetActive(true);
            
            UpdateAllUI();
        }

        private void HidePanel()
        {
            if (editorPanel != null)
                editorPanel.SetActive(false);
        }

        private void UpdateAllUI()
        {
            UpdateToolPalette();
            UpdateMapProperties();
            UpdateUndoRedoButtons();
            UpdateStatusDisplay();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Mode & Tool Selection ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SetMode(EditorMode mode)
        {
            currentMode = mode;
            
            if (MapEditorManager.Instance != null)
                MapEditorManager.Instance.SetEditorMode(mode);
            
            UpdateToolPalette();
        }

        private void SetTileType(TileType type)
        {
            currentTileType = type;
            
            if (MapEditorManager.Instance != null)
                MapEditorManager.Instance.SetSelectedTileType(type);
        }

        private void UpdateToolPalette()
        {
            // Update mode button colors
            UpdateButtonColor(tileModeButton, currentMode == EditorMode.Tile);
            UpdateButtonColor(pathModeButton, currentMode == EditorMode.Path);
            UpdateButtonColor(spawnModeButton, currentMode == EditorMode.Spawn);
            UpdateButtonColor(baseModeButton, currentMode == EditorMode.Base);
            UpdateButtonColor(obstacleModeButton, currentMode == EditorMode.Obstacle);
            UpdateButtonColor(decorationModeButton, currentMode == EditorMode.Decoration);

            // Update mode text
            if (modeText != null)
                modeText.text = $"Mode: {currentMode}";
        }

        private void UpdateButtonColor(Button button, bool isSelected)
        {
            if (button == null) return;

            var colors = button.colors;
            colors.normalColor = isSelected ? selectedToolColor : normalToolColor;
            button.colors = colors;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Mouse Input Handling ──────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void HandleMouseInput()
        {
            // TODO: Implement raycasting to get grid coordinates from mouse position
            // For now, this is a placeholder for the input handling logic

            if (MapEditorManager.Instance == null || !MapEditorManager.Instance.IsEditing())
                return;

            // Detect mouse over grid tile
            // hoveredTile = GetTileFromMousePosition();

            // Update coordinate display
            if (coordText != null)
                coordText.text = $"Coord: ({hoveredTile.x}, {hoveredTile.y})";

            // Handle mouse clicks based on current mode
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                HandleLeftClick();
            }
            else if (Input.GetMouseButton(0)) // Left drag
            {
                HandleLeftDrag();
            }
            else if (Input.GetMouseButtonUp(0)) // Left release
            {
                HandleLeftRelease();
            }

            if (Input.GetMouseButtonDown(1)) // Right click
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            switch (currentMode)
            {
                case EditorMode.Tile:
                    PlaceTile(hoveredTile.x, hoveredTile.y);
                    break;
                
                case EditorMode.Path:
                    MapEditorManager.Instance.StartDrawingPath(hoveredTile.x, hoveredTile.y);
                    isToolActive = true;
                    break;
                
                case EditorMode.Spawn:
                    MapEditorManager.Instance.PlaceSpawnPoint(hoveredTile.x, hoveredTile.y);
                    break;
                
                case EditorMode.Base:
                    MapEditorManager.Instance.PlaceBase(hoveredTile.x, hoveredTile.y);
                    break;
            }
        }

        private void HandleLeftDrag()
        {
            switch (currentMode)
            {
                case EditorMode.Tile:
                    if (currentBrushSize == 1)
                    {
                        PlaceTile(hoveredTile.x, hoveredTile.y);
                    }
                    break;
                
                case EditorMode.Path:
                    if (isToolActive)
                    {
                        MapEditorManager.Instance.ContinueDrawingPath(hoveredTile.x, hoveredTile.y);
                    }
                    break;
            }
        }

        private void HandleLeftRelease()
        {
            if (currentMode == EditorMode.Path && isToolActive)
            {
                MapEditorManager.Instance.FinishDrawingPath();
                isToolActive = false;
            }
        }

        private void HandleRightClick()
        {
            // Right click to cancel or remove
            switch (currentMode)
            {
                case EditorMode.Path:
                    if (isToolActive)
                    {
                        MapEditorManager.Instance.CancelDrawingPath();
                        isToolActive = false;
                    }
                    break;
                
                case EditorMode.Spawn:
                    MapEditorManager.Instance.RemoveSpawnPoint(hoveredTile.x, hoveredTile.y);
                    break;
            }
        }

        private void PlaceTile(int x, int y)
        {
            if (MapEditorManager.Instance == null) return;

            if (currentBrushSize == 1)
            {
                MapEditorManager.Instance.SetTile(x, y, currentTileType);
            }
            else
            {
                // Brush tool - paint multiple tiles
                List<Vector2Int> positions = GetBrushPositions(x, y, currentBrushSize);
                MapEditorManager.Instance.PaintTiles(positions, currentTileType);
            }
        }

        private List<Vector2Int> GetBrushPositions(int centerX, int centerY, int size)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            int radius = size / 2;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }

            return positions;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Button Handlers ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void OnCloseClicked()
        {
            if (MapEditorManager.Instance != null && MapEditorManager.Instance.HasUnsavedChanges())
            {
                ShowConfirmDialog("You have unsaved changes. Close without saving?", () => {
                    MapEditorManager.Instance.CloseCurrentMap(false);
                    HidePanel();
                });
            }
            else
            {
                if (MapEditorManager.Instance != null)
                    MapEditorManager.Instance.CloseCurrentMap();
                
                HidePanel();
            }
        }

        private void OnSaveClicked()
        {
            if (MapEditorManager.Instance != null)
            {
                bool success = MapEditorManager.Instance.SaveCurrentMap();
                
                if (success)
                    ShowStatus("Map saved successfully!", 2f);
                else
                    ShowStatus("Failed to save map", 2f);
            }
        }

        private void OnTestPlayClicked()
        {
            // TODO: Implement test play functionality
            // Validate map, then load it into the game scene for testing
            ShowStatus("Test play not yet implemented", 2f);
        }

        private void OnUndoClicked()
        {
            if (MapEditorManager.Instance != null)
                MapEditorManager.Instance.Undo();
        }

        private void OnRedoClicked()
        {
            if (MapEditorManager.Instance != null)
                MapEditorManager.Instance.Redo();
        }

        private void OnClearMapClicked()
        {
            ShowConfirmDialog("Clear entire map? This cannot be undone.", () => {
                // TODO: Implement clear map functionality
                ShowStatus("Map cleared", 2f);
            });
        }

        private void OnValidateClicked()
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map == null) return;

            var result = MapEditorManager.Instance.ValidateMap(map);

            if (validationPanel != null && validationText != null)
            {
                validationPanel.SetActive(true);

                string resultText = result.isValid ? "<color=green>✓ Map is valid!</color>\n\n" : "<color=red>✗ Map has errors:</color>\n\n";

                if (result.HasErrors)
                {
                    resultText += "<b>Errors:</b>\n";
                    foreach (var error in result.errors)
                        resultText += $"  • {error}\n";
                    resultText += "\n";
                }

                if (result.HasWarnings)
                {
                    resultText += "<b>Warnings:</b>\n";
                    foreach (var warning in result.warnings)
                        resultText += $"  • {warning}\n";
                    resultText += "\n";
                }

                if (result.HasSuggestions)
                {
                    resultText += "<b>Suggestions:</b>\n";
                    foreach (var suggestion in result.suggestions)
                        resultText += $"  • {suggestion}\n";
                }

                validationText.text = resultText;
            }
        }

        private void OnBrushSizeChanged(float value)
        {
            currentBrushSize = Mathf.RoundToInt(value);
            
            if (brushSizeText != null)
                brushSizeText.text = $"Brush: {currentBrushSize}x{currentBrushSize}";
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Map Properties ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void UpdateMapProperties()
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map == null) return;

            if (mapNameInput != null)
                mapNameInput.text = map.mapName;
            
            if (descriptionInput != null)
                descriptionInput.text = map.description;
            
            if (startingCreditsInput != null)
                startingCreditsInput.text = map.startingCredits.ToString();
            
            if (startingLivesInput != null)
                startingLivesInput.text = map.startingLives.ToString();
            
            if (difficultySlider != null)
            {
                difficultySlider.value = map.recommendedDifficulty;
                UpdateDifficultyText(map.recommendedDifficulty);
            }
        }

        private void OnMapNameChanged(string newName)
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map != null)
            {
                map.mapName = newName;
            }
        }

        private void OnStartingCreditsChanged(string value)
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map != null && int.TryParse(value, out int credits))
            {
                map.startingCredits = Mathf.Max(0, credits);
            }
        }

        private void OnStartingLivesChanged(string value)
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map != null && int.TryParse(value, out int lives))
            {
                map.startingLives = Mathf.Max(1, lives);
            }
        }

        private void OnDifficultyChanged(float value)
        {
            if (MapEditorManager.Instance == null) return;

            var map = MapEditorManager.Instance.GetCurrentMap();
            if (map != null)
            {
                map.recommendedDifficulty = Mathf.RoundToInt(value);
                UpdateDifficultyText(map.recommendedDifficulty);
            }
        }

        private void UpdateDifficultyText(int difficulty)
        {
            if (difficultyText == null) return;

            string[] labels = { "Tutorial", "Easy", "Normal", "Hard", "Expert" };
            int index = Mathf.Clamp(difficulty - 1, 0, labels.Length - 1);
            difficultyText.text = $"Difficulty: {labels[index]}";
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Status & UI Updates ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void UpdateStatusDisplay()
        {
            if (MapEditorManager.Instance == null) return;

            // Update mode text is handled in UpdateToolPalette()
        }

        private void UpdateUnsavedIndicator()
        {
            if (unsavedChangesIndicator == null || MapEditorManager.Instance == null)
                return;

            unsavedChangesIndicator.SetActive(MapEditorManager.Instance.HasUnsavedChanges());
        }

        private void UpdateUndoRedoButtons()
        {
            if (MapEditorManager.Instance == null) return;

            if (undoButton != null)
                undoButton.interactable = MapEditorManager.Instance.CanUndo;
            
            if (redoButton != null)
                redoButton.interactable = MapEditorManager.Instance.CanRedo;
        }

        private void ShowStatus(string message, float duration = 3f)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.gameObject.SetActive(true);
                
                CancelInvoke(nameof(HideStatus));
                Invoke(nameof(HideStatus), duration);
            }
        }

        private void HideStatus()
        {
            if (statusText != null)
                statusText.gameObject.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Confirmation Dialog ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void ShowConfirmDialog(string message, Action onConfirm)
        {
            if (confirmDialog == null || confirmText == null)
                return;

            confirmText.text = message;
            confirmDialog.SetActive(true);
            pendingConfirmAction = onConfirm;
        }

        private void OnConfirmYes()
        {
            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
            
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void OnConfirmNo()
        {
            pendingConfirmAction = null;
            
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Event Handlers ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void OnMapLoaded(CustomMapData map)
        {
            UpdateAllUI();
            ShowStatus($"Loaded: {map.mapName}", 2f);
        }

        private void OnMapSaved()
        {
            UpdateUnsavedIndicator();
        }

        private void OnModeChanged(EditorMode mode)
        {
            currentMode = mode;
            UpdateToolPalette();
        }

        private void OnUndoRedoChanged()
        {
            UpdateUndoRedoButtons();
        }

        private void OnTileChanged()
        {
            UpdateUnsavedIndicator();
        }
    }
}
