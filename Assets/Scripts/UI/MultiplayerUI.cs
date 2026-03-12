using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RobotTowerDefense.Multiplayer;

namespace RobotTowerDefense.UI
{
    /// <summary>
    /// UI system for multiplayer lobby, room creation, and player management.
    /// </summary>
    public class MultiplayerUI : MonoBehaviour
    {
        #region Singleton

        public static MultiplayerUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        #region UI Panels

        [Header("Main Panels")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject createRoomPanel;
        [SerializeField] private GameObject joinRoomPanel;
        [SerializeField] private GameObject roomPanel;

        #endregion

        #region Lobby UI

        [Header("Lobby")]
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Transform roomListContainer;
        [SerializeField] private GameObject roomListItemPrefab;
        [SerializeField] private Button refreshRoomListButton;

        #endregion

        #region Create Room UI

        [Header("Create Room")]
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private Toggle isPublicToggle;
        [SerializeField] private Button confirmCreateButton;
        [SerializeField] private Button cancelCreateButton;

        #endregion

        #region Join Room UI

        [Header("Join Room")]
        [SerializeField] private TMP_InputField roomCodeInput;
        [SerializeField] private Button confirmJoinButton;
        [SerializeField] private Button cancelJoinButton;

        #endregion

        #region Room UI

        [Header("Room")]
        [SerializeField] private TextMeshProUGUI roomCodeText;
        [SerializeField] private TextMeshProUGUI roomNameText;
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerListItemPrefab;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button leaveRoomButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Chat")]
        [SerializeField] private Transform chatContainer;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private Button sendChatButton;
        [SerializeField] private ScrollRect chatScrollRect;

        #endregion

        #region State

        private string currentPlayerName = "Player";
        private bool isReady = false;
        private List<GameObject> roomListItems = new List<GameObject>();
        private List<GameObject> playerListItems = new List<GameObject>();
        private List<GameObject> chatMessages = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            ShowLobbyPanel();

            // Load player name from save
            currentPlayerName = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(1000, 9999));
            if (playerNameInput != null)
            {
                playerNameInput.text = currentPlayerName;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Setup

        private void SetupUI()
        {
            // Lobby buttons
            if (createRoomButton != null)
                createRoomButton.onClick.AddListener(ShowCreateRoomPanel);

            if (joinRoomButton != null)
                joinRoomButton.onClick.AddListener(ShowJoinRoomPanel);

            if (quickMatchButton != null)
                quickMatchButton.onClick.AddListener(OnQuickMatchClicked);

            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

            if (refreshRoomListButton != null)
                refreshRoomListButton.onClick.AddListener(RefreshRoomList);

            // Create room buttons
            if (confirmCreateButton != null)
                confirmCreateButton.onClick.AddListener(OnCreateRoomConfirmed);

            if (cancelCreateButton != null)
                cancelCreateButton.onClick.AddListener(ShowLobbyPanel);

            // Join room buttons
            if (confirmJoinButton != null)
                confirmJoinButton.onClick.AddListener(OnJoinRoomConfirmed);

            if (cancelJoinButton != null)
                cancelJoinButton.onClick.AddListener(ShowLobbyPanel);

            // Room buttons
            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyClicked);

            if (leaveRoomButton != null)
                leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);

            if (sendChatButton != null)
                sendChatButton.onClick.AddListener(OnSendChatClicked);

            // Player name input
            if (playerNameInput != null)
            {
                playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged);
            }

            // Chat input
            if (chatInput != null)
            {
                chatInput.onSubmit.AddListener(_ => OnSendChatClicked());
            }
        }

        private void SubscribeToEvents()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnRoomCreatedSuccess += OnRoomCreated;
                LobbyManager.Instance.OnRoomJoinedSuccess += OnRoomJoined;
                LobbyManager.Instance.OnQuickMatchFound += OnQuickMatchFound;
                LobbyManager.Instance.OnRoomListUpdated += OnRoomListUpdated;
            }

            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnPlayerJoined += OnPlayerJoined;
                MultiplayerManager.Instance.OnPlayerLeft += OnPlayerLeft;
                MultiplayerManager.Instance.OnMultiplayerStarted += OnMultiplayerStarted;
                MultiplayerManager.Instance.OnChatMessageReceived += OnChatMessageReceived;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnRoomCreatedSuccess -= OnRoomCreated;
                LobbyManager.Instance.OnRoomJoinedSuccess -= OnRoomJoined;
                LobbyManager.Instance.OnQuickMatchFound -= OnQuickMatchFound;
                LobbyManager.Instance.OnRoomListUpdated -= OnRoomListUpdated;
            }

            if (MultiplayerManager.Instance != null)
            {
                MultiplayerManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                MultiplayerManager.Instance.OnPlayerLeft -= OnPlayerLeft;
                MultiplayerManager.Instance.OnMultiplayerStarted -= OnMultiplayerStarted;
                MultiplayerManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
            }
        }

        #endregion

        #region Panel Management

        private void ShowLobbyPanel()
        {
            HideAllPanels();
            if (lobbyPanel != null) lobbyPanel.SetActive(true);
            RefreshRoomList();
        }

        private void ShowCreateRoomPanel()
        {
            HideAllPanels();
            if (createRoomPanel != null) createRoomPanel.SetActive(true);

            // Set default values
            if (roomNameInput != null)
                roomNameInput.text = $"{currentPlayerName}'s Room";

            if (maxPlayersDropdown != null)
                maxPlayersDropdown.value = 3; // 4 players (index 3)

            if (isPublicToggle != null)
                isPublicToggle.isOn = true;
        }

        private void ShowJoinRoomPanel()
        {
            HideAllPanels();
            if (joinRoomPanel != null) joinRoomPanel.SetActive(true);

            if (roomCodeInput != null)
                roomCodeInput.text = "";
        }

        private void ShowRoomPanel()
        {
            HideAllPanels();
            if (roomPanel != null) roomPanel.SetActive(true);

            isReady = false;
            UpdateReadyButton();
            UpdateStatusText("Waiting for players...");
        }

        private void HideAllPanels()
        {
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
            if (createRoomPanel != null) createRoomPanel.SetActive(false);
            if (joinRoomPanel != null) joinRoomPanel.SetActive(false);
            if (roomPanel != null) roomPanel.SetActive(false);
        }

        #endregion

        #region Lobby Actions

        private void OnQuickMatchClicked()
        {
            UpdatePlayerName();

            LobbyManager.Instance?.StartQuickMatch(currentPlayerName);

            ToastNotification.Instance?.Show("Searching for match...", 2f);
        }

        private void OnBackToMenuClicked()
        {
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void RefreshRoomList()
        {
            LobbyManager.Instance?.RefreshRoomList();
        }

        private void OnPlayerNameChanged(string newName)
        {
            if (!string.IsNullOrEmpty(newName))
            {
                currentPlayerName = newName;
                PlayerPrefs.SetString("PlayerName", currentPlayerName);
                PlayerPrefs.Save();
            }
        }

        private void UpdatePlayerName()
        {
            if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            {
                currentPlayerName = playerNameInput.text;
                PlayerPrefs.SetString("PlayerName", currentPlayerName);
                PlayerPrefs.Save();
            }
        }

        #endregion

        #region Room List

        private void OnRoomListUpdated(List<LobbyManager.RoomInfo> rooms)
        {
            // Clear existing items
            foreach (var item in roomListItems)
            {
                Destroy(item);
            }
            roomListItems.Clear();

            // Create new items
            foreach (var room in rooms)
            {
                CreateRoomListItem(room);
            }
        }

        private void CreateRoomListItem(LobbyManager.RoomInfo room)
        {
            if (roomListItemPrefab == null || roomListContainer == null) return;

            GameObject itemObj = Instantiate(roomListItemPrefab, roomListContainer);
            roomListItems.Add(itemObj);

            // Set room info
            var nameText = itemObj.transform.Find("RoomName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = room.roomCode;

            var hostText = itemObj.transform.Find("HostName")?.GetComponent<TextMeshProUGUI>();
            if (hostText != null)
                hostText.text = $"Host: {room.hostName}";

            var playersText = itemObj.transform.Find("Players")?.GetComponent<TextMeshProUGUI>();
            if (playersText != null)
                playersText.text = $"{room.currentPlayers}/{room.maxPlayers}";

            // Join button
            var joinButton = itemObj.transform.Find("JoinButton")?.GetComponent<Button>();
            if (joinButton != null)
            {
                joinButton.onClick.AddListener(() => JoinRoom(room));
            }
        }

        private void JoinRoom(LobbyManager.RoomInfo room)
        {
            UpdatePlayerName();
            LobbyManager.Instance?.JoinRoom(room, currentPlayerName);
        }

        #endregion

        #region Create Room

        private void OnCreateRoomConfirmed()
        {
            UpdatePlayerName();

            string roomName = roomNameInput != null ? roomNameInput.text : currentPlayerName + "'s Room";
            int maxPlayers = maxPlayersDropdown != null ? maxPlayersDropdown.value + 1 : 4;
            bool isPublic = isPublicToggle != null ? isPublicToggle.isOn : true;

            LobbyManager.Instance?.CreateRoom(roomName, currentPlayerName, maxPlayers, isPublic);
        }

        private void OnRoomCreated(string roomCode)
        {
            ShowRoomPanel();

            if (roomCodeText != null)
                roomCodeText.text = $"Room Code: {roomCode}";

            if (roomNameText != null)
                roomNameText.text = roomNameInput != null ? roomNameInput.text : "Room";

            ToastNotification.Instance?.Show($"Room created! Code: {roomCode}", 5f);
        }

        #endregion

        #region Join Room

        private void OnJoinRoomConfirmed()
        {
            UpdatePlayerName();

            string roomCode = roomCodeInput != null ? roomCodeInput.text : "";

            if (string.IsNullOrEmpty(roomCode))
            {
                ToastNotification.Instance?.Show("Please enter a room code", 2f);
                return;
            }

            LobbyManager.Instance?.JoinRoomByCode(roomCode, currentPlayerName);
        }

        private void OnRoomJoined(string roomCode)
        {
            ShowRoomPanel();

            if (roomCodeText != null)
                roomCodeText.text = $"Room Code: {roomCode}";

            ToastNotification.Instance?.Show($"Joined room: {roomCode}", 3f);
        }

        private void OnQuickMatchFound(LobbyManager.RoomInfo room)
        {
            ShowRoomPanel();

            if (roomCodeText != null)
                roomCodeText.text = $"Room Code: {room.roomCode}";

            if (roomNameText != null)
                roomNameText.text = "Quick Match";

            ToastNotification.Instance?.Show("Match found!", 2f);
        }

        #endregion

        #region Room Management

        private void OnPlayerJoined(ulong clientId, string playerName)
        {
            UpdatePlayerList();
            AddChatMessage($"{playerName} joined the room", Color.gray);
        }

        private void OnPlayerLeft(ulong clientId, string playerName)
        {
            UpdatePlayerList();
            AddChatMessage($"{playerName} left the room", Color.gray);
        }

        private void UpdatePlayerList()
        {
            // Clear existing items
            foreach (var item in playerListItems)
            {
                Destroy(item);
            }
            playerListItems.Clear();

            // Get player list from MultiplayerManager
            if (MultiplayerManager.Instance == null) return;

            var players = MultiplayerManager.Instance.GetConnectedPlayers();
            foreach (var player in players)
            {
                CreatePlayerListItem(player.Key, player.Value);
            }
        }

        private void CreatePlayerListItem(ulong clientId, NetworkedPlayer player)
        {
            if (playerListItemPrefab == null || playerListContainer == null) return;

            GameObject itemObj = Instantiate(playerListItemPrefab, playerListContainer);
            playerListItems.Add(itemObj);

            // Set player info
            var nameText = itemObj.transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = player.PlayerName;
                nameText.color = player.PlayerColor;
            }

            var readyIndicator = itemObj.transform.Find("ReadyIndicator")?.gameObject;
            if (readyIndicator != null)
            {
                // Show ready indicator if player is ready
                bool playerReady = MultiplayerManager.Instance?.IsPlayerReady(clientId) ?? false;
                readyIndicator.SetActive(playerReady);
            }
        }

        private void OnReadyClicked()
        {
            isReady = !isReady;

            // Notify server of ready state
            // Find local player
            if (MultiplayerManager.Instance != null)
            {
                var players = MultiplayerManager.Instance.GetConnectedPlayers();
                foreach (var player in players.Values)
                {
                    if (player.IsLocalPlayer())
                    {
                        player.SetReady(isReady);
                        break;
                    }
                }
            }

            UpdateReadyButton();
            UpdatePlayerList();
        }

        private void UpdateReadyButton()
        {
            if (readyButton == null) return;

            var buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isReady ? "Not Ready" : "Ready";
            }

            // Change button color
            var colors = readyButton.colors;
            colors.normalColor = isReady ? Color.red : Color.green;
            readyButton.colors = colors;
        }

        private void OnLeaveRoomClicked()
        {
            LobbyManager.Instance?.LeaveRoom();
            ShowLobbyPanel();
            ClearChat();
        }

        private void UpdateStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private void OnMultiplayerStarted()
        {
            UpdateStatusText("Game starting...");

            // Hide UI and start game
            HideAllPanels();

            ToastNotification.Instance?.Show("Game started!", 2f);
        }

        #endregion

        #region Chat

        private void OnSendChatClicked()
        {
            if (chatInput == null || string.IsNullOrEmpty(chatInput.text)) return;

            string message = chatInput.text;

            // Find local player and send message
            if (MultiplayerManager.Instance != null)
            {
                var players = MultiplayerManager.Instance.GetConnectedPlayers();
                foreach (var player in players.Values)
                {
                    if (player.IsLocalPlayer())
                    {
                        player.SendChatMessage(message);
                        break;
                    }
                }
            }

            chatInput.text = "";
        }

        private void OnChatMessageReceived(ulong clientId, string playerName, string message)
        {
            // Get player color
            Color playerColor = Color.white;
            if (MultiplayerManager.Instance != null)
            {
                var players = MultiplayerManager.Instance.GetConnectedPlayers();
                if (players.ContainsKey(clientId))
                {
                    playerColor = players[clientId].PlayerColor;
                }
            }

            AddChatMessage($"{playerName}: {message}", playerColor);
        }

        private void AddChatMessage(string message, Color color)
        {
            if (chatMessagePrefab == null || chatContainer == null) return;

            GameObject messageObj = Instantiate(chatMessagePrefab, chatContainer);
            chatMessages.Add(messageObj);

            var messageText = messageObj.GetComponent<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = color;
            }

            // Scroll to bottom
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }

            // Limit chat history to 50 messages
            if (chatMessages.Count > 50)
            {
                Destroy(chatMessages[0]);
                chatMessages.RemoveAt(0);
            }
        }

        private void ClearChat()
        {
            foreach (var message in chatMessages)
            {
                Destroy(message);
            }
            chatMessages.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show multiplayer lobby UI.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ShowLobbyPanel();
        }

        /// <summary>
        /// Hide multiplayer UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Get current player name.
        /// </summary>
        public string GetPlayerName()
        {
            return currentPlayerName;
        }

        #endregion

        #region Context Menu

#if UNITY_EDITOR
        [ContextMenu("Simulate Room Created")]
        private void SimulateRoomCreated()
        {
            OnRoomCreated("TEST123");
        }

        [ContextMenu("Simulate Player Joined")]
        private void SimulatePlayerJoined()
        {
            OnPlayerJoined(12345, "TestPlayer");
        }

        [ContextMenu("Simulate Chat Message")]
        private void SimulateChatMessage()
        {
            AddChatMessage("TestPlayer: Hello world!", Color.cyan);
        }
#endif

        #endregion
    }
}
