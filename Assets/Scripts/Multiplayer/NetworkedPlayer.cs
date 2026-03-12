using UnityEngine;
using System.Collections.Generic;

#if UNITY_NETCODE
using Unity.Netcode;
#endif

namespace RobotTowerDefense.Multiplayer
{
    /// <summary>
    /// Represents a networked player in multiplayer co-op mode.
    /// Handles player actions, cursor position, and state synchronization.
    /// </summary>
#if UNITY_NETCODE
    public class NetworkedPlayer : NetworkBehaviour
#else
    public class NetworkedPlayer : MonoBehaviour
#endif
    {
        #region Configuration

        [Header("Player Info")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private Color playerColor = Color.blue;

        [Header("Cursor")]
        [SerializeField] private GameObject cursorPrefab;
        [SerializeField] private float cursorSyncRate = 0.1f; // 10 times per second

        [Header("Permissions")]
        [SerializeField] private bool canPlaceTowers = true;
        [SerializeField] private bool canUpgradeTowers = true;
        [SerializeField] private bool canSellTowers = true;
        [SerializeField] private bool canUsePowerUps = true;

        #endregion

        #region State

#if UNITY_NETCODE
        private NetworkVariable<Vector3> networkCursorPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<bool> networkIsPlacingTower = new NetworkVariable<bool>();
        private NetworkVariable<int> networkSelectedTowerType = new NetworkVariable<int>();
#else
        private Vector3 networkCursorPosition;
        private bool networkIsPlacingTower;
        private int networkSelectedTowerType;
#endif

        private GameObject cursorInstance;
        private float lastCursorSyncTime = 0f;
        private Vector3 lastCursorPosition;

        public string PlayerName => playerName;
        public Color PlayerColor => playerColor;
        public ulong ClientId
        {
            get
            {
#if UNITY_NETCODE
                return OwnerClientId;
#else
                return 0;
#endif
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
#if UNITY_NETCODE
            if (IsOwner)
            {
                // Local player setup
                SetupLocalPlayer();
            }
            else
            {
                // Remote player setup
                SetupRemotePlayer();
            }

            // Register with MultiplayerManager
            MultiplayerManager.Instance?.RegisterNetworkedPlayer(OwnerClientId, this);

            // Subscribe to network variable changes
            networkCursorPosition.OnValueChanged += OnCursorPositionChanged;
            networkIsPlacingTower.OnValueChanged += OnPlacingTowerChanged;
#else
            Debug.LogWarning("[NetworkedPlayer] Unity Netcode not installed");
#endif
        }

        private void Update()
        {
#if UNITY_NETCODE
            if (IsOwner)
            {
                UpdateLocalPlayer();
            }
            else
            {
                UpdateRemotePlayer();
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_NETCODE
            // Unregister from MultiplayerManager
            MultiplayerManager.Instance?.UnregisterNetworkedPlayer(OwnerClientId);

            // Clean up cursor
            if (cursorInstance != null)
            {
                Destroy(cursorInstance);
            }
#endif
        }

        #endregion

        #region Local Player Setup

        private void SetupLocalPlayer()
        {
            // Local player doesn't need visible cursor (uses actual mouse cursor)
            Debug.Log($"[NetworkedPlayer] Local player initialized: {playerName}");
        }

        private void UpdateLocalPlayer()
        {
            // Sync cursor position to network
            if (Time.time - lastCursorSyncTime >= cursorSyncRate)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                if (Vector3.Distance(mouseWorldPos, lastCursorPosition) > 0.1f)
                {
                    SyncCursorPosition(mouseWorldPos);
                    lastCursorPosition = mouseWorldPos;
                    lastCursorSyncTime = Time.time;
                }
            }

            // Handle input (tower placement, etc.)
            HandleLocalInput();
        }

        private void HandleLocalInput()
        {
            // Tower placement input
            if (canPlaceTowers && Input.GetMouseButtonDown(0))
            {
                TryPlaceTower();
            }

            // Power-up activation
            if (canUsePowerUps && Input.GetKeyDown(KeyCode.Alpha1))
            {
                TryUsePowerUp(Core.PowerUpType.DamageBoost);
            }
        }

        #endregion

        #region Remote Player Setup

        private void SetupRemotePlayer()
        {
            // Create visible cursor for remote player
            if (cursorPrefab != null)
            {
                cursorInstance = Instantiate(cursorPrefab);
                cursorInstance.name = $"{playerName}_Cursor";

                // Apply player color to cursor
                var renderer = cursorInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = playerColor;
                }
            }

            Debug.Log($"[NetworkedPlayer] Remote player initialized: {playerName}");
        }

        private void UpdateRemotePlayer()
        {
            // Update cursor position smoothly
            if (cursorInstance != null)
            {
#if UNITY_NETCODE
                cursorInstance.transform.position = Vector3.Lerp(
                    cursorInstance.transform.position,
                    networkCursorPosition.Value,
                    Time.deltaTime * 10f
                );

                // Show/hide cursor based on placement state
                cursorInstance.SetActive(networkIsPlacingTower.Value);
#endif
            }
        }

        #endregion

        #region Network Synchronization

        private void SyncCursorPosition(Vector3 position)
        {
#if UNITY_NETCODE
            if (IsOwner)
            {
                UpdateCursorPositionServerRpc(position);
            }
#endif
        }

#if UNITY_NETCODE
        [ServerRpc]
        private void UpdateCursorPositionServerRpc(Vector3 position)
        {
            networkCursorPosition.Value = position;
        }

        private void OnCursorPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            // Cursor position updated (handled in Update for smooth interpolation)
        }

        private void OnPlacingTowerChanged(bool previousValue, bool newValue)
        {
            // Tower placement state changed
            if (cursorInstance != null)
            {
                cursorInstance.SetActive(newValue);
            }
        }
#endif

        #endregion

        #region Tower Placement

        private void TryPlaceTower()
        {
#if UNITY_NETCODE
            if (!IsOwner || !canPlaceTowers) return;

            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Get selected tower type (from UI)
            int towerType = GetSelectedTowerType();
            if (towerType == -1) return;

            // Request placement from server
            MultiplayerManager.Instance?.PlaceTowerServerRpc(mouseWorldPos, towerType);

            Debug.Log($"[NetworkedPlayer] Requesting tower placement at {mouseWorldPos}");

            Analytics.AnalyticsManager.Instance?.TrackEvent("multiplayer_tower_placed", new Dictionary<string, object>
            {
                { "tower_type", towerType },
                { "position", mouseWorldPos.ToString() }
            });
#endif
        }

        private int GetSelectedTowerType()
        {
            // Get selected tower type from TowerPlacementManager or UI
            // Placeholder implementation
            return 0;
        }

        #endregion

        #region Power-Ups

        private void TryUsePowerUp(Core.PowerUpType powerUpType)
        {
#if UNITY_NETCODE
            if (!IsOwner || !canUsePowerUps) return;

            // Request power-up activation from server
            ActivatePowerUpServerRpc((int)powerUpType);

            Debug.Log($"[NetworkedPlayer] Requesting power-up: {powerUpType}");
#endif
        }

#if UNITY_NETCODE
        [ServerRpc]
        private void ActivatePowerUpServerRpc(int powerUpType)
        {
            // Validate and activate power-up on server
            var powerUpManager = Core.PowerUpManager.Instance;
            if (powerUpManager != null)
            {
                powerUpManager.ActivatePowerUp((Core.PowerUpType)powerUpType);

                // Notify all clients
                PowerUpActivatedClientRpc(powerUpType, OwnerClientId);
            }
        }

        [ClientRpc]
        private void PowerUpActivatedClientRpc(int powerUpType, ulong activatedByClientId)
        {
            Core.PowerUpType type = (Core.PowerUpType)powerUpType;
            Debug.Log($"Power-up {type} activated by player {activatedByClientId}");

            // Show notification
            UI.ToastNotification.Instance?.Show($"{playerName} used {type}!", 3f);
        }
#endif

        #endregion

        #region Player Actions

        /// <summary>
        /// Set player ready state in lobby.
        /// </summary>
        public void SetReady(bool ready)
        {
#if UNITY_NETCODE
            if (IsOwner)
            {
                SetReadyServerRpc(ready);
            }
#endif
        }

#if UNITY_NETCODE
        [ServerRpc]
        private void SetReadyServerRpc(bool ready)
        {
            MultiplayerManager.Instance?.SetPlayerReady(OwnerClientId, ready);
            PlayerReadyStateChangedClientRpc(ready);
        }

        [ClientRpc]
        private void PlayerReadyStateChangedClientRpc(bool ready)
        {
            Debug.Log($"[NetworkedPlayer] {playerName} ready: {ready}");
        }
#endif

        /// <summary>
        /// Send chat message to all players.
        /// </summary>
        public void SendChatMessage(string message)
        {
            MultiplayerManager.Instance?.SendChatMessage(message);
        }

        #endregion

        #region Utility Methods

        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Set player name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            playerName = name;

#if UNITY_NETCODE
            if (IsOwner)
            {
                UpdatePlayerNameServerRpc(name);
            }
#endif
        }

#if UNITY_NETCODE
        [ServerRpc]
        private void UpdatePlayerNameServerRpc(string name)
        {
            playerName = name;
            PlayerNameUpdatedClientRpc(name);
        }

        [ClientRpc]
        private void PlayerNameUpdatedClientRpc(string name)
        {
            playerName = name;
            Debug.Log($"[NetworkedPlayer] Name updated: {name}");
        }
#endif

        /// <summary>
        /// Set player color for identification.
        /// </summary>
        public void SetPlayerColor(Color color)
        {
            playerColor = color;

            if (cursorInstance != null)
            {
                var renderer = cursorInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        /// <summary>
        /// Check if this is the local player.
        /// </summary>
        public bool IsLocalPlayer()
        {
#if UNITY_NETCODE
            return IsOwner;
#else
            return true;
#endif
        }

        #endregion

        #region Context Menu

#if UNITY_EDITOR
        [ContextMenu("Simulate Place Tower")]
        private void SimulatePlaceTower()
        {
            TryPlaceTower();
        }

        [ContextMenu("Simulate Use Power-Up")]
        private void SimulateUsePowerUp()
        {
            TryUsePowerUp(Core.PowerUpType.DamageBoost);
        }

        [ContextMenu("Print Player Info")]
        private void PrintPlayerInfo()
        {
            Debug.Log($"Player Info:\n" +
                $"Name: {playerName}\n" +
                $"Client ID: {ClientId}\n" +
                $"Color: {playerColor}\n" +
                $"Can Place Towers: {canPlaceTowers}");
        }
#endif

        #endregion
    }
}
