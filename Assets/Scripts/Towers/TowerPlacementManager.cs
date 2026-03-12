using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RobotTD.Core;
using RobotTD.Analytics;

namespace RobotTD.Towers
{
    /// <summary>
    /// Handles tower placement on the map.
    /// Supports both mouse and touch input for mobile.
    /// </summary>
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementLayerMask;
        [SerializeField] private LayerMask pathLayerMask;
        [SerializeField] private float minDistanceFromPath = 1.5f;
        [SerializeField] private float minDistanceBetweenTowers = 2f;

        [Header("Grid Snapping")]
        [SerializeField] private bool useGridSnapping = true;
        [SerializeField] private float gridSize = 1f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject placementIndicator;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

        // State
        private TowerData selectedTowerData;
        private GameObject previewTower;
        private bool isPlacing = false;
        private Camera mainCamera;
        private List<Tower> placedTowers = new List<Tower>();

        // Events
        public System.Action<TowerData> OnTowerSelected;
        public System.Action<Tower> OnTowerPlaced;
        public System.Action OnPlacementCanceled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (isPlacing)
            {
                UpdatePlacementPreview();
                HandlePlacementInput();
            }
            else
            {
                HandleTowerSelection();
            }
        }

        #region Tower Selection

        /// <summary>
        /// Start placing a tower of the specified type
        /// </summary>
        public void StartPlacement(TowerData towerData)
        {
            // Calculate cost with challenge modifier
            int cost = GetModifiedTowerCost(towerData.cost);
            
            // Check if player can afford it
            if (!GameManager.Instance.CanAfford(cost))
            {
                Debug.Log("Cannot afford this tower!");
                // TODO: Show UI feedback
                return;
            }

            selectedTowerData = towerData;
            isPlacing = true;

            // Create preview tower
            CreatePreviewTower();

            OnTowerSelected?.Invoke(towerData);
        }

        /// <summary>
        /// Cancel current placement
        /// </summary>
        public void CancelPlacement()
        {
            isPlacing = false;
            selectedTowerData = null;

            if (previewTower != null)
            {
                Destroy(previewTower);
                previewTower = null;
            }

            OnPlacementCanceled?.Invoke();
        }

        private void CreatePreviewTower()
        {
            // Load tower prefab
            string prefabPath = $"Prefabs/Towers/{selectedTowerData.towerType}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                previewTower = Instantiate(prefab);
                
                // Disable tower functionality during preview
                var tower = previewTower.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.enabled = false;
                }

                // Remove colliders temporarily
                foreach (var col in previewTower.GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }

                // Make semi-transparent
                SetPreviewMaterial(true);
            }
            else if (placementIndicator != null)
            {
                previewTower = Instantiate(placementIndicator);
            }
        }

        private void SetPreviewMaterial(bool isValid)
        {
            if (previewTower == null) return;

            // Change material/color based on validity
            var renderers = previewTower.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    mat.color = isValid ? validColor : invalidColor;
                }
            }
        }

        #endregion

        #region Placement

        private void UpdatePlacementPreview()
        {
            Vector3? worldPos = GetWorldPosition();

            if (worldPos.HasValue && previewTower != null)
            {
                Vector3 position = worldPos.Value;

                // Snap to grid
                if (useGridSnapping)
                {
                    position = SnapToGrid(position);
                }

                previewTower.transform.position = position;

                // Check if position is valid
                bool isValid = IsValidPlacement(position);
                SetPreviewMaterial(isValid);
            }
        }

        private void HandlePlacementInput()
        {
            // Cancel with escape or right click
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
                return;
            }

            // Place with left click or touch
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                // Don't place if clicking on UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                TryPlaceTower();
            }
        }

        private void TryPlaceTower()
        {
            Vector3? worldPos = GetWorldPosition();

            if (!worldPos.HasValue) return;

            Vector3 position = worldPos.Value;

            // Snap to grid
            if (useGridSnapping)
            {
                position = SnapToGrid(position);
            }

            // Validate position
            if (!IsValidPlacement(position))
            {
                Debug.Log("Invalid placement position!");
                // TODO: Show UI feedback / play error sound
                return;
            }

            // Check challenge tower limit
            if (Core.ChallengeManager.Instance != null && 
                !Core.ChallengeManager.Instance.CanPlaceTower())
            {
                Debug.Log("Challenge tower limit reached!");
                return;
            }

            // Calculate cost with challenge modifier
            int cost = GetModifiedTowerCost(selectedTowerData.cost);
            
            // Check if player can still afford it
            if (!GameManager.Instance.SpendCredits(cost))
            {
                Debug.Log("Cannot afford tower!");
                return;
            }

            // Place the tower
            PlaceTower(position);
        }

        private void PlaceTower(Vector3 position)
        {
            // Destroy preview
            if (previewTower != null)
            {
                Destroy(previewTower);
                previewTower = null;
            }

            // Instantiate actual tower
            string prefabPath = $"Prefabs/Towers/{selectedTowerData.towerType}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                GameObject towerObj = Instantiate(prefab, position, Quaternion.identity);
                Tower tower = towerObj.GetComponent<Tower>();

                if (tower != null)
                {
                    placedTowers.Add(tower);
                    OnTowerPlaced?.Invoke(tower);
                    
                    // Track tower placement in save data
                    Core.SaveManager.Instance?.RecordTowerPlaced();

                    // Track tower placement analytics
                    AnalyticsManager.Instance?.TrackTowerPlaced(
                        selectedTowerData.towerType.ToString(),
                        1,
                        selectedTowerData.cost,
                        new Vector2(position.x, position.z)
                    );

                    // Notify achievement manager
                    Progression.AchievementManager.Instance?.OnTowerPlaced(selectedTowerData.towerType);
                    
                    // Track mission progress
                    if (MissionManager.Instance != null)
                    {
                        MissionManager.Instance.UpdateMissionProgress(MissionType.PlaceTowers, 1);
                        MissionManager.Instance.UpdateMissionProgress(MissionType.UseTowerType, 1, selectedTowerData.towerType.ToString());
                    }
                }

                // Play placement sound
                if (selectedTowerData.placeSound != null)
                {
                    AudioSource.PlayClipAtPoint(selectedTowerData.placeSound, position);
                }
            }

            // End placement mode
            isPlacing = false;
            selectedTowerData = null;
        }

        #endregion

        #region Selection (existing towers)

        private Tower selectedTower;

        private void HandleTowerSelection()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Don't select if clicking on UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    Tower tower = hit.collider.GetComponent<Tower>();
                    if (tower != null)
                    {
                        SelectTower(tower);
                    }
                    else
                    {
                        DeselectTower();
                    }
                }
                else
                {
                    DeselectTower();
                }
            }
        }

        public void SelectTower(Tower tower)
        {
            // Deselect previous
            if (selectedTower != null)
            {
                selectedTower.ShowRange(false);
            }

            selectedTower = tower;
            selectedTower.ShowRange(true);

            // Show tower info UI
            UI.TowerInfoUI.Instance?.Show(tower);
        }

        public void DeselectTower()
        {
            if (selectedTower != null)
            {
                selectedTower.ShowRange(false);
                selectedTower = null;
            }

            UI.TowerInfoUI.Instance?.Hide();
        }

        #endregion

        #region Validation

        public bool IsValidPlacement(Vector3 position)
        {
            // Check distance from path
            if (IsTooCloseToPath(position))
            {
                return false;
            }

            // Check distance from other towers
            foreach (var tower in placedTowers)
            {
                if (tower == null) continue;
                if (Vector3.Distance(position, tower.transform.position) < minDistanceBetweenTowers)
                {
                    return false;
                }
            }

            // Check if on valid ground
            Ray groundRay = new Ray(position + Vector3.up * 5f, Vector3.down);
            if (!Physics.Raycast(groundRay, out RaycastHit hit, 10f, placementLayerMask))
            {
                return false;
            }

            return true;
        }

        private bool IsTooCloseToPath(Vector3 position)
        {
            // Check using overlap sphere or path distance
            Collider[] pathColliders = Physics.OverlapSphere(position, minDistanceFromPath, pathLayerMask);
            return pathColliders.Length > 0;
        }

        #endregion

        #region Helpers

        private Vector3? GetWorldPosition()
        {
            Vector3 inputPos;
            
            if (Input.touchCount > 0)
            {
                inputPos = Input.GetTouch(0).position;
            }
            else
            {
                inputPos = Input.mousePosition;
            }

            Ray ray = mainCamera.ScreenPointToRay(inputPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayerMask))
            {
                return hit.point;
            }

            return null;
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float z = Mathf.Round(position.z / gridSize) * gridSize;
            return new Vector3(x, position.y, z);
        }

        public List<Tower> GetAllTowers() => placedTowers;

        public int PlacedTowerCount => placedTowers.Count;

        public void RemoveTower(Tower tower)
        {
            placedTowers.Remove(tower);
        }

        /// <summary>Tutorial helper — true once the player has placed at least one tower.</summary>
        public static bool HasPlacedAnyTower()
        {
            return Instance != null && Instance.placedTowers.Count > 0;
        }

        /// <summary>Tutorial helper — true once any placed tower has been upgraded.</summary>
        public static bool HasUpgradedAnyTower()
        {
            if (Instance == null) return false;
            foreach (var t in Instance.placedTowers)
                if (t != null && t.Level > 1) return true;
            return false;
        }
        
        // ── Challenge Mode Integration ───────────────────────────────────────
        
        /// <summary>
        /// Get tower cost with challenge modifiers applied.
        /// </summary>
        public int GetModifiedTowerCost(int baseCost)
        {
            if (Core.ChallengeManager.Instance != null)
            {
                float mult = Core.ChallengeManager.Instance.GetTowerCostMultiplier();
                return Mathf.RoundToInt(baseCost * mult);
            }
            return baseCost;
        }

        #endregion
    }
}
