using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_NETCODE
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#endif

namespace RobotTowerDefense.Multiplayer
{
    /// <summary>
    /// Manages multiplayer lobby, matchmaking, and room discovery.
    /// Handles creating/joining rooms, quick match, and lobby UI coordination.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        #region Singleton

        private static LobbyManager instance;
        public static LobbyManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LobbyManager>();
                }
                return instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Matchmaking Settings")]
        [SerializeField] private bool enableQuickMatch = true;
        [SerializeField] private float matchmakingTimeout = 30f;
        [SerializeField] private int preferredPlayerCount = 2;

        [Header("Room Settings")]
        [SerializeField] private string defaultRoomName = "Tower Defense Room";
        [SerializeField] private bool roomsArePublic = true;
        [SerializeField] private int maxRoomsPerPlayer = 1;

        [Header("Connection")]
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private ushort serverPort = 7777;
        [SerializeField] private bool useRelay = false; // Unity Relay for cloud matchmaking

        #endregion

        #region State

        private List<RoomInfo> availableRooms = new List<RoomInfo>();
        private RoomInfo currentRoom = null;
        private bool isSearchingForMatch = false;
        private float searchStartTime = 0f;

        public List<RoomInfo> AvailableRooms => new List<RoomInfo>(availableRooms);
        public RoomInfo CurrentRoom => currentRoom;
        public bool IsSearchingForMatch => isSearchingForMatch;

        #endregion

        #region Events

        public event Action<RoomInfo> OnRoomCreatedSuccess;
        public event Action<string> OnRoomCreatedFailed;
        public event Action<RoomInfo> OnRoomJoinedSuccess;
        public event Action<string> OnRoomJoinedFailed;
        public event Action OnRoomLeft;
        public event Action<List<RoomInfo>> OnRoomListUpdated;
        public event Action OnQuickMatchFound;
        public event Action OnQuickMatchFailed;

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
        }

        private void Start()
        {
#if UNITY_NETCODE
            ConfigureNetworkTransport();
#else
            Debug.LogWarning("[LobbyManager] Unity Netcode not installed - Multiplayer disabled");
#endif
        }

        private void Update()
        {
            // Check matchmaking timeout
            if (isSearchingForMatch)
            {
                if (Time.time - searchStartTime > matchmakingTimeout)
                {
                    CancelQuickMatch();
                    OnQuickMatchFailed?.Invoke();
                    Debug.LogWarning("[LobbyManager] Quick match timeout");
                }
            }
        }

        #endregion

        #region Network Configuration

        private void ConfigureNetworkTransport()
        {
#if UNITY_NETCODE
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[LobbyManager] NetworkManager not found!");
                return;
            }

            // Configure Unity Transport
            var transport = networkManager.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(serverAddress, serverPort);
                Debug.Log($"✅ Network transport configured - {serverAddress}:{serverPort}");
            }
#endif
        }

        #endregion

        #region Room Creation

        /// <summary>
        /// Create a new multiplayer room.
        /// </summary>
        public void CreateRoom(string roomName, string hostName, int maxPlayers, bool isPublic)
        {
            if (currentRoom != null)
            {
                Debug.LogWarning("[LobbyManager] Already in a room");
                OnRoomCreatedFailed?.Invoke("Already in a room");
                return;
            }

            // Create room info
            RoomInfo newRoom = new RoomInfo
            {
                roomId = Guid.NewGuid().ToString(),
                roomName = roomName,
                roomCode = GenerateRoomCode(),
                hostName = hostName,
                maxPlayers = maxPlayers,
                currentPlayers = 1,
                isPublic = isPublic,
                mapName = "Default",
                difficulty = "Normal"
            };

            currentRoom = newRoom;

            // Start host via MultiplayerManager
            MultiplayerManager.Instance?.CreateRoom(hostName);

            // Subscribe to multiplayer events
            SubscribeToMultiplayerEvents();

            // Add to available rooms if public
            if (isPublic)
            {
                availableRooms.Add(newRoom);
                OnRoomListUpdated?.Invoke(availableRooms);
            }

            OnRoomCreatedSuccess?.Invoke(newRoom);

            Debug.Log($"✅ Room created: {newRoom.roomName} ({newRoom.roomCode})");

            Analytics.AnalyticsManager.Instance?.TrackEvent("lobby_room_created", new Dictionary<string, object>
            {
                { "room_name", roomName },
                { "room_code", newRoom.roomCode },
                { "max_players", maxPlayers },
                { "is_public", isPublic }
            });
        }

        #endregion

        #region Room Joining

        /// <summary>
        /// Join a room by room code.
        /// </summary>
        public void JoinRoomByCode(string roomCode, string playerName)
        {
            if (currentRoom != null)
            {
                Debug.LogWarning("[LobbyManager] Already in a room");
                OnRoomJoinedFailed?.Invoke("Already in a room");
                return;
            }

            // Find room by code
            RoomInfo room = availableRooms.Find(r => r.roomCode == roomCode);

            if (room == null)
            {
                // Try to join directly (room might be private)
                JoinRoomDirect(roomCode, playerName);
                return;
            }

            JoinRoom(room, playerName);
        }

        /// <summary>
        /// Join a specific room.
        /// </summary>
        public void JoinRoom(RoomInfo room, string playerName)
        {
            if (currentRoom != null)
            {
                Debug.LogWarning("[LobbyManager] Already in a room");
                OnRoomJoinedFailed?.Invoke("Already in a room");
                return;
            }

            if (room.currentPlayers >= room.maxPlayers)
            {
                Debug.LogWarning("[LobbyManager] Room is full");
                OnRoomJoinedFailed?.Invoke("Room is full");
                return;
            }

            currentRoom = room;
            room.currentPlayers++;

            // Join via MultiplayerManager
            MultiplayerManager.Instance?.JoinRoom(room.roomCode, playerName);

            // Subscribe to multiplayer events
            SubscribeToMultiplayerEvents();

            OnRoomJoinedSuccess?.Invoke(room);

            Debug.Log($"✅ Joined room: {room.roomName} ({room.roomCode})");

            Analytics.AnalyticsManager.Instance?.TrackEvent("lobby_room_joined", new Dictionary<string, object>
            {
                { "room_code", room.roomCode },
                { "player_name", playerName }
            });
        }

        private void JoinRoomDirect(string roomCode, string playerName)
        {
            // Direct join attempt (for private rooms)
            MultiplayerManager.Instance?.JoinRoom(roomCode, playerName);

            // Create placeholder room info
            currentRoom = new RoomInfo
            {
                roomCode = roomCode,
                roomName = "Private Room",
                currentPlayers = 1,
                maxPlayers = 4,
                isPublic = false
            };

            SubscribeToMultiplayerEvents();

            Debug.Log($"✅ Attempting direct join: {roomCode}");
        }

        #endregion

        #region Room Management

        /// <summary>
        /// Leave the current room.
        /// </summary>
        public void LeaveRoom()
        {
            if (currentRoom == null)
            {
                Debug.LogWarning("[LobbyManager] Not in a room");
                return;
            }

            // Remove from available rooms
            if (currentRoom.isPublic)
            {
                availableRooms.Remove(currentRoom);
                OnRoomListUpdated?.Invoke(availableRooms);
            }

            // Leave via MultiplayerManager
            MultiplayerManager.Instance?.LeaveRoom();

            // Unsubscribe from multiplayer events
            UnsubscribeFromMultiplayerEvents();

            currentRoom = null;
            OnRoomLeft?.Invoke();

            Debug.Log("✅ Left room");

            Analytics.AnalyticsManager.Instance?.TrackEvent("lobby_room_left", new Dictionary<string, object>());
        }

        /// <summary>
        /// Refresh list of available rooms.
        /// </summary>
        public void RefreshRoomList()
        {
            // In production, query server/relay for available rooms
            // For now, filter local list
            availableRooms.RemoveAll(r => r.currentPlayers >= r.maxPlayers);
            OnRoomListUpdated?.Invoke(availableRooms);

            Debug.Log($"✅ Room list refreshed - {availableRooms.Count} rooms available");
        }

        #endregion

        #region Quick Match

        /// <summary>
        /// Find and join a random available room. Create new room if none available.
        /// </summary>
        public void StartQuickMatch(string playerName)
        {
            if (isSearchingForMatch)
            {
                Debug.LogWarning("[LobbyManager] Already searching for match");
                return;
            }

            isSearchingForMatch = true;
            searchStartTime = Time.time;

            Debug.Log("[LobbyManager] Starting quick match...");

            Analytics.AnalyticsManager.Instance?.TrackEvent("lobby_quickmatch_started", new Dictionary<string, object>
            {
                { "player_name", playerName }
            });

            // Find available room with space
            RoomInfo availableRoom = FindBestAvailableRoom();

            if (availableRoom != null)
            {
                // Join existing room
                JoinRoom(availableRoom, playerName);
                isSearchingForMatch = false;
                OnQuickMatchFound?.Invoke();
                Debug.Log($"✅ Quick match found - Joining: {availableRoom.roomName}");
            }
            else
            {
                // Create new room and wait for others
                CreateRoom($"{playerName}'s Room", playerName, 4, true);
                isSearchingForMatch = false;
                OnQuickMatchFound?.Invoke();
                Debug.Log("✅ Quick match - Created new room, waiting for players");
            }
        }

        /// <summary>
        /// Cancel quick match search.
        /// </summary>
        public void CancelQuickMatch()
        {
            isSearchingForMatch = false;
            Debug.Log("✅ Quick match cancelled");
        }

        private RoomInfo FindBestAvailableRoom()
        {
            // Find room closest to preferred player count
            RoomInfo bestRoom = null;
            int bestScore = int.MaxValue;

            foreach (var room in availableRooms)
            {
                if (room.currentPlayers >= room.maxPlayers)
                    continue;

                // Score based on how close to preferred player count
                int score = Mathf.Abs(room.currentPlayers - preferredPlayerCount);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestRoom = room;
                }
            }

            return bestRoom;
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToMultiplayerEvents()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnPlayerJoined += OnPlayerJoinedRoom;
                MultiplayerManager.Instance.OnPlayerLeft += OnPlayerLeftRoom;
                MultiplayerManager.Instance.OnConnectionFailed += OnConnectionFailedHandler;
            }
        }

        private void UnsubscribeFromMultiplayerEvents()
        {
            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnPlayerJoined -= OnPlayerJoinedRoom;
                MultiplayerManager.Instance.OnPlayerLeft -= OnPlayerLeftRoom;
                MultiplayerManager.Instance.OnConnectionFailed -= OnConnectionFailedHandler;
            }
        }

        private void OnPlayerJoinedRoom(PlayerInfo player)
        {
            if (currentRoom != null)
            {
                currentRoom.currentPlayers++;
                OnRoomListUpdated?.Invoke(availableRooms);
                Debug.Log($"Player joined room: {player.playerName}");
            }
        }

        private void OnPlayerLeftRoom(PlayerInfo player)
        {
            if (currentRoom != null)
            {
                currentRoom.currentPlayers--;
                OnRoomListUpdated?.Invoke(availableRooms);
                Debug.Log($"Player left room: {player.playerName}");
            }
        }

        private void OnConnectionFailedHandler(string error)
        {
            Debug.LogError($"[LobbyManager] Connection failed: {error}");
            OnRoomJoinedFailed?.Invoke(error);
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

        /// <summary>
        /// Get room by code.
        /// </summary>
        public RoomInfo GetRoomByCode(string roomCode)
        {
            return availableRooms.Find(r => r.roomCode == roomCode);
        }

        /// <summary>
        /// Check if player can create room.
        /// </summary>
        public bool CanCreateRoom()
        {
            return currentRoom == null;
        }

        #endregion

        #region Context Menu

#if UNITY_EDITOR
        [ContextMenu("Create Test Room")]
        private void CreateTestRoom()
        {
            CreateRoom("Test Room", "TestHost", 4, true);
        }

        [ContextMenu("Simulate Quick Match")]
        private void SimulateQuickMatch()
        {
            StartQuickMatch("TestPlayer");
        }

        [ContextMenu("Print Room List")]
        private void PrintRoomList()
        {
            Debug.Log($"Available Rooms: {availableRooms.Count}");
            foreach (var room in availableRooms)
            {
                Debug.Log($"- {room.roomName} ({room.roomCode}) - {room.currentPlayers}/{room.maxPlayers}");
            }
        }
#endif

        #endregion
    }

    #region Data Classes

    [Serializable]
    public class RoomInfo
    {
        public string roomId;
        public string roomName;
        public string roomCode;
        public string hostName;
        public int maxPlayers;
        public int currentPlayers;
        public bool isPublic;
        public string mapName;
        public string difficulty;
        public bool hasStarted;

        public bool IsFull => currentPlayers >= maxPlayers;
        public bool HasSpace => currentPlayers < maxPlayers;
    }

    #endregion
}
