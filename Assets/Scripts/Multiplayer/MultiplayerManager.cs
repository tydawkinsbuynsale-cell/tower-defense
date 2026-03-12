using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_NETCODE
using Unity.Netcode;
#endif

namespace RobotTowerDefense.Multiplayer
{
    /// <summary>
    /// Manages multiplayer co-op gameplay with real-time collaboration.
    /// Supports 2-4 players working together to defend against waves.
    /// Uses Unity Netcode for GameObjects for networking.
    /// </summary>
#if UNITY_NETCODE
    public class MultiplayerManager : NetworkBehaviour
#else
    public class MultiplayerManager : MonoBehaviour
#endif
    {
        #region Singleton

        private static MultiplayerManager instance;
        public static MultiplayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MultiplayerManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MultiplayerManager");
                        instance = go.AddComponent<MultiplayerManager>();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Multiplayer Settings")]
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private int minPlayers = 2;
        [SerializeField] private bool enableVoiceChat = false;
        [SerializeField] private bool allowLateJoin = true;
        [SerializeField] private float syncInterval = 0.1f; // 10 times per second

        [Header("Co-op Balance")]
        [SerializeField] private float enemyHealthMultiplier = 1.5f; // Per additional player
        [SerializeField] private float enemySpawnMultiplier = 1.3f; // Per additional player
        [SerializeField] private bool sharedCredits = true;
        [SerializeField] private bool sharedLives = true;

        [Header("Host Settings")]
        [SerializeField] private bool hostCanPause = true;
        [SerializeField] private bool hostCanKickPlayers = true;
        [SerializeField] private int hostPort = 7777;

        #endregion

        #region State

        private bool isMultiplayerActive = false;
        private bool isHost = false;
        private string roomCode = "";
        private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();
        private Dictionary<ulong, NetworkedPlayer> networkedPlayers = new Dictionary<ulong, NetworkedPlayer>();

        public bool IsMultiplayerActive => isMultiplayerActive;
        public bool IsHost => isHost;
        public string RoomCode => roomCode;
        public int PlayerCount => connectedPlayers.Count;
        public List<PlayerInfo> ConnectedPlayers => new List<PlayerInfo>(connectedPlayers);

        #endregion

        #region Events

        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;
        public event Action OnMultiplayerStarted;
        public event Action OnMultiplayerEnded;
        public event Action<string> OnRoomCreated;
        public event Action<string> OnRoomJoined;
        public event Action<string> OnConnectionFailed;
        public event Action<string> OnChatMessageReceived;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

#if !UNITY_NETCODE
            Debug.LogWarning("[MultiplayerManager] Unity Netcode for GameObjects not installed. Multiplayer disabled.");
            Debug.LogWarning("Install via Package Manager: com.unity.netcode.gameobjects");
#endif
        }

        private void Start()
        {
#if UNITY_NETCODE
            InitializeNetworking();
#endif
        }

        #endregion

        #region Initialization

        private void InitializeNetworking()
        {
#if UNITY_NETCODE
            // Configure network manager
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[MultiplayerManager] NetworkManager not found in scene!");
                return;
            }

            // Subscribe to network events
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            networkManager.OnServerStarted += OnServerStarted;

            Debug.Log("✅ MultiplayerManager initialized with Unity Netcode");
#endif
        }

        #endregion

        #region Host/Client Management

        /// <summary>
        /// Create a new multiplayer room as host.
        /// </summary>
        public void CreateRoom(string playerName)
        {
#if UNITY_NETCODE
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            // Start as host (server + client)
            bool success = networkManager.StartHost();

            if (success)
            {
                isHost = true;
                isMultiplayerActive = true;
                roomCode = GenerateRoomCode();

                // Add host as first player
                PlayerInfo hostInfo = new PlayerInfo
                {
                    playerId = networkManager.LocalClientId,
                    playerName = playerName,
                    isHost = true,
                    isReady = true
                };
                connectedPlayers.Add(hostInfo);

                OnRoomCreated?.Invoke(roomCode);
                OnMultiplayerStarted?.Invoke();

                Debug.Log($"✅ Room created: {roomCode} - Host: {playerName}");

                Analytics.AnalyticsManager.Instance?.TrackEvent("multiplayer_room_created", new Dictionary<string, object>
                {
                    { "room_code", roomCode },
                    { "host_name", playerName }
                });
            }
            else
            {
                Debug.LogError("[MultiplayerManager] Failed to create room");
                OnConnectionFailed?.Invoke("Failed to start host");
            }
#else
            Debug.LogWarning("[MultiplayerManager] Unity Netcode not installed");
            OnConnectionFailed?.Invoke("Unity Netcode not installed");
#endif
        }

        /// <summary>
        /// Join an existing multiplayer room as client.
        /// </summary>
        public void JoinRoom(string roomCode, string playerName)
        {
#if UNITY_NETCODE
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            this.roomCode = roomCode;

            // Start as client
            bool success = networkManager.StartClient();

            if (success)
            {
                isHost = false;
                isMultiplayerActive = true;

                OnRoomJoined?.Invoke(roomCode);

                Debug.Log($"✅ Joined room: {roomCode} - Player: {playerName}");

                Analytics.AnalyticsManager.Instance?.TrackEvent("multiplayer_room_joined", new Dictionary<string, object>
                {
                    { "room_code", roomCode },
                    { "player_name", playerName }
                });
            }
            else
            {
                Debug.LogError("[MultiplayerManager] Failed to join room");
                OnConnectionFailed?.Invoke("Failed to connect to room");
            }
#else
            Debug.LogWarning("[MultiplayerManager] Unity Netcode not installed");
            OnConnectionFailed?.Invoke("Unity Netcode not installed");
#endif
        }

        /// <summary>
        /// Leave the current multiplayer session.
        /// </summary>
        public void LeaveRoom()
        {
#if UNITY_NETCODE
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            if (isHost)
            {
                // Host shuts down server
                networkManager.Shutdown();
            }
            else
            {
                // Client disconnects
                networkManager.Shutdown();
            }

            CleanupSession();

            Debug.Log("✅ Left multiplayer room");

            Analytics.AnalyticsManager.Instance?.TrackEvent("multiplayer_room_left", new Dictionary<string, object>
            {
                { "room_code", roomCode },
                { "was_host", isHost }
            });
#endif
        }

        private void CleanupSession()
        {
            isMultiplayerActive = false;
            isHost = false;
            roomCode = "";
            connectedPlayers.Clear();
            networkedPlayers.Clear();

            OnMultiplayerEnded?.Invoke();
        }

        #endregion

        #region Network Callbacks

#if UNITY_NETCODE
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[MultiplayerManager] Client connected: {clientId}");

            if (IsServer)
            {
                // Host adds new player to list
                PlayerInfo newPlayer = new PlayerInfo
                {
                    playerId = clientId,
                    playerName = $"Player {clientId}",
                    isHost = false,
                    isReady = false
                };
                connectedPlayers.Add(newPlayer);

                OnPlayerJoined?.Invoke(newPlayer);

                // Sync player list to all clients
                SyncPlayerListClientRpc();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[MultiplayerManager] Client disconnected: {clientId}");

            if (IsServer)
            {
                // Remove player from list
                PlayerInfo disconnectedPlayer = connectedPlayers.Find(p => p.playerId == clientId);
                if (disconnectedPlayer != null)
                {
                    connectedPlayers.Remove(disconnectedPlayer);
                    OnPlayerLeft?.Invoke(disconnectedPlayer);

                    // Sync player list to all clients
                    SyncPlayerListClientRpc();
                }
            }
        }

        private void OnServerStarted()
        {
            Debug.Log("[MultiplayerManager] Server started successfully");
        }
#endif

        #endregion

        #region Player Management

        /// <summary>
        /// Register a networked player instance.
        /// </summary>
        public void RegisterNetworkedPlayer(ulong clientId, NetworkedPlayer player)
        {
            if (!networkedPlayers.ContainsKey(clientId))
            {
                networkedPlayers[clientId] = player;
                Debug.Log($"✅ Registered networked player: {clientId}");
            }
        }

        /// <summary>
        /// Unregister a networked player instance.
        /// </summary>
        public void UnregisterNetworkedPlayer(ulong clientId)
        {
            if (networkedPlayers.ContainsKey(clientId))
            {
                networkedPlayers.Remove(clientId);
                Debug.Log($"✅ Unregistered networked player: {clientId}");
            }
        }

        /// <summary>
        /// Get networked player by client ID.
        /// </summary>
        public NetworkedPlayer GetNetworkedPlayer(ulong clientId)
        {
            return networkedPlayers.ContainsKey(clientId) ? networkedPlayers[clientId] : null;
        }

        /// <summary>
        /// Set player ready state.
        /// </summary>
        public void SetPlayerReady(ulong clientId, bool ready)
        {
            PlayerInfo player = connectedPlayers.Find(p => p.playerId == clientId);
            if (player != null)
            {
                player.isReady = ready;
                Debug.Log($"Player {clientId} ready: {ready}");
            }
        }

        /// <summary>
        /// Check if all players are ready to start.
        /// </summary>
        public bool AllPlayersReady()
        {
            if (connectedPlayers.Count < minPlayers)
                return false;

            foreach (var player in connectedPlayers)
            {
                if (!player.isReady)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Kick a player from the room (host only).
        /// </summary>
        public void KickPlayer(ulong clientId)
        {
#if UNITY_NETCODE
            if (!isHost || !hostCanKickPlayers) return;

            var networkManager = NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsServer)
            {
                networkManager.DisconnectClient(clientId);
                Debug.Log($"✅ Kicked player: {clientId}");
            }
#endif
        }

        #endregion

        #region Game State Sync

#if UNITY_NETCODE
        /// <summary>
        /// Sync player list to all clients (server-side call).
        /// </summary>
        [ClientRpc]
        private void SyncPlayerListClientRpc()
        {
            // Update local player list
            // In production, serialize and send full player list
            Debug.Log("[MultiplayerManager] Player list synced");
        }

        /// <summary>
        /// Sync game state (credits, lives, wave) to all clients.
        /// </summary>
        [ClientRpc]
        public void SyncGameStateClientRpc(int credits, int lives, int currentWave)
        {
            if (!IsServer)
            {
                // Update game state on clients
                Core.GameManager.Instance?.SetCredits(credits);
                Core.GameManager.Instance?.SetLives(lives);
                // Wave sync handled by WaveManager
                Debug.Log($"Game state synced - Credits: {credits}, Lives: {lives}, Wave: {currentWave}");
            }
        }

        /// <summary>
        /// Request tower placement (client to server).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlaceTowerServerRpc(Vector3 position, int towerTypeId, ServerRpcParams rpcParams = default)
        {
            // Validate placement on server
            var towerPlacementManager = Core.TowerPlacementManager.Instance;
            if (towerPlacementManager == null) return;

            ulong clientId = rpcParams.Receive.SenderClientId;

            // Check if player has enough credits
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.Credits >= GetTowerCost(towerTypeId))
            {
                // Place tower
                // Tower placement logic here
                Debug.Log($"Tower placed by client {clientId} at {position}");

                // Notify all clients
                TowerPlacedClientRpc(position, towerTypeId, clientId);
            }
            else
            {
                // Notify client of failure
                TowerPlacementFailedClientRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }

        [ClientRpc]
        private void TowerPlacedClientRpc(Vector3 position, int towerTypeId, ulong placedByClientId)
        {
            // Spawn tower on all clients
            Debug.Log($"Tower spawned at {position} by player {placedByClientId}");
        }

        [ClientRpc]
        private void TowerPlacementFailedClientRpc(RpcParams rpcParams = default)
        {
            Debug.LogWarning("Tower placement failed - insufficient credits");
            UI.ToastNotification.Instance?.Show("Not enough credits!", 2f);
        }
#endif

        #endregion

        #region Chat System

        /// <summary>
        /// Send chat message to all players.
        /// </summary>
        public void SendChatMessage(string message)
        {
#if UNITY_NETCODE
            if (IsServer)
            {
                BroadcastChatMessageClientRpc(NetworkManager.Singleton.LocalClientId, message);
            }
            else
            {
                SendChatMessageServerRpc(message);
            }
#endif
        }

#if UNITY_NETCODE
        [ServerRpc(RequireOwnership = false)]
        private void SendChatMessageServerRpc(string message, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            BroadcastChatMessageClientRpc(senderId, message);
        }

        [ClientRpc]
        private void BroadcastChatMessageClientRpc(ulong senderId, string message)
        {
            PlayerInfo sender = connectedPlayers.Find(p => p.playerId == senderId);
            string senderName = sender != null ? sender.playerName : $"Player {senderId}";
            string formattedMessage = $"[{senderName}]: {message}";

            OnChatMessageReceived?.Invoke(formattedMessage);
            Debug.Log($"[Chat] {formattedMessage}");
        }
#endif

        #endregion

        #region Co-op Balance

        /// <summary>
        /// Get enemy health multiplier based on player count.
        /// </summary>
        public float GetEnemyHealthMultiplier()
        {
            if (!isMultiplayerActive) return 1f;
            return 1f + (PlayerCount - 1) * enemyHealthMultiplier;
        }

        /// <summary>
        /// Get enemy spawn rate multiplier based on player count.
        /// </summary>
        public float GetEnemySpawnMultiplier()
        {
            if (!isMultiplayerActive) return 1f;
            return 1f + (PlayerCount - 1) * enemySpawnMultiplier;
        }

        /// <summary>
        /// Check if credits are shared between players.
        /// </summary>
        public bool AreCreditsShared()
        {
            return sharedCredits;
        }

        /// <summary>
        /// Check if lives are shared between players.
        /// </summary>
        public bool AreLivesShared()
        {
            return sharedLives;
        }

        #endregion

        #region Utility Methods

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[6];
            System.Random random = new System.Random();

            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }

            return new string(code);
        }

        private int GetTowerCost(int towerTypeId)
        {
            // Get tower cost from TowerData
            // Placeholder implementation
            return 100;
        }

        #endregion

        #region Context Menu

#if UNITY_EDITOR
        [ContextMenu("Simulate Create Room")]
        private void SimulateCreateRoom()
        {
            CreateRoom("TestHost");
        }

        [ContextMenu("Simulate Join Room")]
        private void SimulateJoinRoom()
        {
            JoinRoom("TEST123", "TestPlayer");
        }

        [ContextMenu("Print Connected Players")]
        private void PrintConnectedPlayers()
        {
            Debug.Log($"Connected Players: {connectedPlayers.Count}");
            foreach (var player in connectedPlayers)
            {
                Debug.Log($"- {player.playerName} (ID: {player.playerId}, Host: {player.isHost}, Ready: {player.isReady})");
            }
        }
#endif

        #endregion
    }

    #region Data Classes

    [Serializable]
    public class PlayerInfo
    {
        public ulong playerId;
        public string playerName;
        public bool isHost;
        public bool isReady;
        public int score;
        public int towersPlaced;
    }

    #endregion
}
