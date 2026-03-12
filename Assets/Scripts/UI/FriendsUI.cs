using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;
using System.Collections.Generic;

namespace RobotTD.UI
{
    /// <summary>
    /// UI panel for managing friends, friend requests, and social features.
    /// </summary>
    public class FriendsUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("Tab Buttons")]
        [SerializeField] private Button friendsTabButton;
        [SerializeField] private Button requestsTabButton;
        [SerializeField] private Button searchTabButton;
        [SerializeField] private Color activeTabColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("Friends List")]
        [SerializeField] private Transform friendsContainer;
        [SerializeField] private GameObject friendCardPrefab;
        [SerializeField] private TextMeshProUGUI friendsCountText;
        [SerializeField] private GameObject noFriendsText;

        [Header("Friend Requests")]
        [SerializeField] private Transform requestsContainer;
        [SerializeField] private GameObject requestCardPrefab;
        [SerializeField] private TextMeshProUGUI requestsCountText;
        [SerializeField] private GameObject noRequestsText;

        [Header("Player Search")]
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private Button searchButton;
        [SerializeField] private Transform searchResultsContainer;
        [SerializeField] private GameObject searchResultPrefab;
        [SerializeField] private TextMeshProUGUI searchStatusText;

        [Header("Tab Panels")]
        [SerializeField] private GameObject friendsPanel;
        [SerializeField] private GameObject requestsPanel;
        [SerializeField] private GameObject searchPanel;

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup;

        private CurrentTab currentTab = CurrentTab.Friends;
        private bool isOpen = false;

        private enum CurrentTab
        {
            Friends,
            Requests,
            Search
        }

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToSocialEvents();

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromSocialEvents();
        }

        // ── Panel Control ─────────────────────────────────────────────────────

        public void Open()
        {
            if (panel == null) return;

            panel.SetActive(true);
            isOpen = true;

            ShowFriendsTab();
            RefreshCurrentTab();

            // Fade in animation
            if (canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEaseOutQuad();
            }

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("friends_ui_opened");
            }
        }

        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;

            // Fade out animation
            if (canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 0f, 0.2f).setEaseInQuad().setOnComplete(() =>
                {
                    if (panel != null) panel.SetActive(false);
                });
            }
            else
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        // ── Tab Switching ─────────────────────────────────────────────────────

        private void ShowFriendsTab()
        {
            currentTab = CurrentTab.Friends;
            UpdateTabButtons();
            ShowPanel(friendsPanel);
            RefreshFriendsList();
        }

        private void ShowRequestsTab()
        {
            currentTab = CurrentTab.Requests;
            UpdateTabButtons();
            ShowPanel(requestsPanel);
            RefreshRequestsList();
        }

        private void ShowSearchTab()
        {
            currentTab = CurrentTab.Search;
            UpdateTabButtons();
            ShowPanel(searchPanel);
            ClearSearchResults();
        }

        private void ShowPanel(GameObject targetPanel)
        {
            if (friendsPanel != null) friendsPanel.SetActive(targetPanel == friendsPanel);
            if (requestsPanel != null) requestsPanel.SetActive(targetPanel == requestsPanel);
            if (searchPanel != null) searchPanel.SetActive(targetPanel == searchPanel);
        }

        private void UpdateTabButtons()
        {
            UpdateTabButton(friendsTabButton, currentTab == CurrentTab.Friends);
            UpdateTabButton(requestsTabButton, currentTab == CurrentTab.Requests);
            UpdateTabButton(searchTabButton, currentTab == CurrentTab.Search);
        }

        private void UpdateTabButton(Button button, bool isActive)
        {
            if (button == null) return;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = isActive ? activeTabColor : inactiveTabColor;
            }
        }

        private void RefreshCurrentTab()
        {
            switch (currentTab)
            {
                case CurrentTab.Friends:
                    RefreshFriendsList();
                    break;
                case CurrentTab.Requests:
                    RefreshRequestsList();
                    break;
                case CurrentTab.Search:
                    // Search panel doesn't need auto-refresh
                    break;
            }
        }

        // ── Friends List Display ──────────────────────────────────────────────

        private void RefreshFriendsList()
        {
            if (SocialManager.Instance == null) return;

            ClearContainer(friendsContainer);

            var friends = SocialManager.Instance.Friends;

            if (friends.Count == 0)
            {
                if (noFriendsText != null) noFriendsText.SetActive(true);
            }
            else
            {
                if (noFriendsText != null) noFriendsText.SetActive(false);

                foreach (var friend in friends)
                {
                    SpawnFriendCard(friend);
                }
            }

            // Update count
            if (friendsCountText != null)
            {
                friendsCountText.text = $"Friends: {friends.Count} / {100}"; // Max friends hardcoded for now
            }
        }

        private void SpawnFriendCard(FriendInfo friend)
        {
            if (friendCardPrefab == null || friendsContainer == null) return;

            GameObject cardObj = Instantiate(friendCardPrefab, friendsContainer);
            FriendCardUI card = cardObj.GetComponent<FriendCardUI>();

            if (card != null)
            {
                card.Setup(friend);
                card.OnViewProfile += HandleViewProfile;
                card.OnRemoveFriend += HandleRemoveFriend;
            }
        }

        // ── Friend Requests Display ───────────────────────────────────────────

        private void RefreshRequestsList()
        {
            if (SocialManager.Instance == null) return;

            ClearContainer(requestsContainer);

            var requests = SocialManager.Instance.PendingRequests;

            if (requests.Count == 0)
            {
                if (noRequestsText != null) noRequestsText.SetActive(true);
            }
            else
            {
                if (noRequestsText != null) noRequestsText.SetActive(false);

                foreach (var request in requests)
                {
                    SpawnRequestCard(request);
                }
            }

            // Update count
            if (requestsCountText != null)
            {
                requestsCountText.text = $"Pending: {requests.Count}";
            }
        }

        private void SpawnRequestCard(FriendRequest request)
        {
            if (requestCardPrefab == null || requestsContainer == null) return;

            GameObject cardObj = Instantiate(requestCardPrefab, requestsContainer);
            FriendRequestCardUI card = cardObj.GetComponent<FriendRequestCardUI>();

            if (card != null)
            {
                card.Setup(request);
                card.OnAccept += HandleAcceptRequest;
                card.OnDecline += HandleDeclineRequest;
            }
        }

        // ── Player Search ─────────────────────────────────────────────────────

        private void HandleSearchButton()
        {
            string searchQuery = searchInputField != null ? searchInputField.text : "";

            if (string.IsNullOrEmpty(searchQuery) || searchQuery.Length < 3)
            {
                if (searchStatusText != null)
                {
                    searchStatusText.text = "Enter at least 3 characters to search";
                }
                return;
            }

            if (searchStatusText != null)
            {
                searchStatusText.text = "Searching...";
            }

            SocialManager.Instance?.SearchPlayers(searchQuery, HandleSearchResults);
        }

        private void HandleSearchResults(List<PlayerSearchResult> results)
        {
            ClearSearchResults();

            if (results == null || results.Count == 0)
            {
                if (searchStatusText != null)
                {
                    searchStatusText.text = "No players found";
                }
                return;
            }

            if (searchStatusText != null)
            {
                searchStatusText.text = $"Found {results.Count} player(s)";
            }

            foreach (var result in results)
            {
                SpawnSearchResultCard(result);
            }
        }

        private void SpawnSearchResultCard(PlayerSearchResult result)
        {
            if (searchResultPrefab == null || searchResultsContainer == null) return;

            GameObject cardObj = Instantiate(searchResultPrefab, searchResultsContainer);
            PlayerSearchResultCardUI card = cardObj.GetComponent<PlayerSearchResultCardUI>();

            if (card != null)
            {
                card.Setup(result);
                card.OnSendFriendRequest += HandleSendFriendRequest;
            }
        }

        private void ClearSearchResults()
        {
            ClearContainer(searchResultsContainer);
            if (searchStatusText != null)
            {
                searchStatusText.text = "";
            }
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void HandleViewProfile(string playerId)
        {
            Debug.Log($"[FriendsUI] View profile: {playerId}");
            // TODO: Open player profile UI
        }

        private void HandleRemoveFriend(string playerId)
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.RemoveFriend(playerId);
            }
        }

        private void HandleAcceptRequest(string requestId)
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.AcceptFriendRequest(requestId);
            }
        }

        private void HandleDeclineRequest(string requestId)
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.DeclineFriendRequest(requestId);
            }
        }

        private void HandleSendFriendRequest(string playerId)
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.SendFriendRequest(playerId);
                
                if (ToastNotification.Instance != null)
                {
                    ToastNotification.Instance.Show("Friend request sent!", ToastNotification.ToastType.Success);
                }
            }
        }

        // ── Social Events ─────────────────────────────────────────────────────

        private void SubscribeToSocialEvents()
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.OnFriendsListUpdated += RefreshFriendsList;
                SocialManager.Instance.OnFriendRequestReceived += HandleFriendRequestReceived;
                SocialManager.Instance.OnFriendAdded += HandleFriendAdded;
                SocialManager.Instance.OnFriendRemoved += HandleFriendRemoved;
            }
        }

        private void UnsubscribeFromSocialEvents()
        {
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.OnFriendsListUpdated -= RefreshFriendsList;
                SocialManager.Instance.OnFriendRequestReceived -= HandleFriendRequestReceived;
                SocialManager.Instance.OnFriendAdded -= HandleFriendAdded;
                SocialManager.Instance.OnFriendRemoved -= HandleFriendRemoved;
            }
        }

        private void HandleFriendRequestReceived(FriendRequest request)
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show($"Friend request from {request.senderPlayerName}", ToastNotification.ToastType.Info);
            }

            if (currentTab == CurrentTab.Requests)
            {
                RefreshRequestsList();
            }
        }

        private void HandleFriendAdded(FriendInfo friend)
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show($"Now friends with {friend.playerName}!", ToastNotification.ToastType.Success);
            }

            RefreshFriendsList();
            RefreshRequestsList();
        }

        private void HandleFriendRemoved(string playerId)
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show("Friend removed", ToastNotification.ToastType.Info);
            }

            RefreshFriendsList();
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private void SetupButtonListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (friendsTabButton != null)
            {
                friendsTabButton.onClick.AddListener(ShowFriendsTab);
            }

            if (requestsTabButton != null)
            {
                requestsTabButton.onClick.AddListener(ShowRequestsTab);
            }

            if (searchTabButton != null)
            {
                searchTabButton.onClick.AddListener(ShowSearchTab);
            }

            if (searchButton != null)
            {
                searchButton.onClick.AddListener(HandleSearchButton);
            }

            if (searchInputField != null)
            {
                searchInputField.onSubmit.AddListener((text) => HandleSearchButton());
            }
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        // ── Static Access ─────────────────────────────────────────────────────

        private static FriendsUI instance;
        public static FriendsUI Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<FriendsUI>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }
    }
}
