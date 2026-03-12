using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RobotTD.Online;

namespace RobotTD.UI
{
    /// <summary>
    /// Leaderboard UI panel — displays top scores from LeaderboardManager.
    /// Supports multiple leaderboard tabs (endless, daily, weekly).
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        
        [Header("Tab Buttons")]
        [SerializeField] private Button endlessTabButton;
        [SerializeField] private Button dailyTabButton;
        [SerializeField] private Button weeklyTabButton;
        
        [Header("Player Info")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerRankText;
        [SerializeField] private Button changeNameButton;
        
        [Header("Scroll Settings")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private int maxDisplayedEntries = 50;
        
        [Header("Highlight Colors")]
        [SerializeField] private Color playerEntryColor = new Color(1f, 0.92f, 0.016f, 0.3f); // Gold tint
        [SerializeField] private Color topThreeColor = new Color(0.8f, 0.8f, 1f, 0.3f); // Blue tint
        
        private string currentLeaderboardId;
        private LeaderboardScope currentScope = LeaderboardScope.Global;
        private List<GameObject> activeEntries = new List<GameObject>();
        private bool isLoading;
        
        // ── Unity ────────────────────────────────────────────────────────────
        
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshCurrentLeaderboard);
            
            if (changeNameButton != null)
                changeNameButton.onClick.AddListener(OnChangeNameClicked);
            
            if (endlessTabButton != null)
                endlessTabButton.onClick.AddListener(() => ShowLeaderboard("endless_high_score"));
            
            if (dailyTabButton != null)
                dailyTabButton.onClick.AddListener(() => ShowLeaderboard("daily_challenge"));
            
            if (weeklyTabButton != null)
                weeklyTabButton.onClick.AddListener(() => ShowLeaderboard("weekly_challenge"));
            
            if (panel != null)
                panel.SetActive(false);
        }
        
        private void OnEnable()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnScoresLoaded += OnScoresLoaded;
                LeaderboardManager.Instance.OnError += OnLeaderboardError;
            }
        }
        
        private void OnDisable()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnScoresLoaded -= OnScoresLoaded;
                LeaderboardManager.Instance.OnError -= OnLeaderboardError;
            }
        }
        
        // ── Public API ───────────────────────────────────────────────────────
        
        /// <summary>
        /// Show the leaderboard UI with the specified board (endless/daily/weekly)
        /// </summary>
        public void ShowLeaderboard(string leaderboardId)
        {
            currentLeaderboardId = leaderboardId;
            
            if (panel != null)
                panel.SetActive(true);
            
            UpdateTitle();
            UpdatePlayerInfo();
            LoadLeaderboard();
        }
        
        /// <summary>
        /// Show endless mode leaderboard (default)
        /// </summary>
        public void ShowEndlessLeaderboard()
        {
            ShowLeaderboard("endless_high_score");
        }
        
        /// <summary>
        /// Hide the leaderboard panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
        }
        
        /// <summary>
        /// Refresh the currently displayed leaderboard
        /// </summary>
        public void RefreshCurrentLeaderboard()
        {
            if (string.IsNullOrEmpty(currentLeaderboardId))
                return;
            
            // Clear cache to force fresh fetch
            LeaderboardManager.Instance?.ClearCache();
            LoadLeaderboard();
        }
        
        // ── Private Methods ──────────────────────────────────────────────────
        
        private void UpdateTitle()
        {
            if (titleText == null)
                return;
            
            switch (currentLeaderboardId)
            {
                case "endless_high_score":
                    titleText.text = "Endless Mode Leaderboard";
                    break;
                case "daily_challenge":
                    titleText.text = "Daily Challenge";
                    break;
                case "weekly_challenge":
                    titleText.text = "Weekly Challenge";
                    break;
                default:
                    titleText.text = "Leaderboard";
                    break;
            }
        }
        
        private void UpdatePlayerInfo()
        {
            if (LeaderboardManager.Instance == null)
                return;
            
            if (playerNameText != null)
            {
                string playerName = LeaderboardManager.Instance.GetPlayerName();
                playerNameText.text = $"Player: {playerName}";
            }
            
            // Update player rank (async - will update when scores load)
            UpdatePlayerRank();
        }
        
        private void UpdatePlayerRank()
        {
            if (playerRankText == null || LeaderboardManager.Instance == null)
                return;
            
            // Try to get player's rank from local scores
            var localScores = LeaderboardManager.Instance.GetLocalScores(currentLeaderboardId);
            string playerId = LeaderboardManager.Instance.GetPlayerId();
            
            var playerEntry = localScores?.Find(e => e.playerId == playerId);
            if (playerEntry != null)
            {
                playerRankText.text = $"Your Rank: #{playerEntry.rank}";
            }
            else
            {
                playerRankText.text = "Your Rank: Not ranked yet";
            }
        }
        
        private void LoadLeaderboard()
        {
            if (LeaderboardManager.Instance == null)
            {
                ShowError("Leaderboard system not available");
                return;
            }
            
            if (isLoading)
                return;
            
            isLoading = true;
            ShowLoading(true);
            HideError();
            
            // Fetch leaderboard (will trigger OnScoresLoaded callback)
            LeaderboardManager.Instance.FetchLeaderboard(
                currentLeaderboardId,
                currentScope,
                maxDisplayedEntries
            );
        }
        
        private void OnScoresLoaded(string leaderboardId, List<LeaderboardEntry> entries)
        {
            // Only process if this is the current leaderboard
            if (leaderboardId != currentLeaderboardId)
                return;
            
            isLoading = false;
            ShowLoading(false);
            
            DisplayEntries(entries);
            UpdatePlayerRank();
        }
        
        private void DisplayEntries(List<LeaderboardEntry> entries)
        {
            // Clear existing entries
            ClearEntries();
            
            if (entries == null || entries.Count == 0)
            {
                ShowError("No scores available");
                return;
            }
            
            // Create UI entries
            string playerId = LeaderboardManager.Instance?.GetPlayerId();
            
            foreach (var entry in entries)
            {
                GameObject entryObj = Instantiate(entryPrefab, entryContainer);
                var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
                
                if (entryUI != null)
                {
                    bool isPlayer = entry.playerId == playerId;
                    bool isTopThree = entry.rank <= 3;
                    
                    Color bgColor = Color.clear;
                    if (isPlayer)
                        bgColor = playerEntryColor;
                    else if (isTopThree)
                        bgColor = topThreeColor;
                    
                    entryUI.SetData(entry, bgColor);
                }
                
                activeEntries.Add(entryObj);
            }
            
            // Scroll to top
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
        
        private void ClearEntries()
        {
            foreach (var entry in activeEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            activeEntries.Clear();
        }
        
        private void ShowLoading(bool show)
        {
            if (loadingText != null)
                loadingText.gameObject.SetActive(show);
        }
        
        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }
        
        private void HideError()
        {
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }
        
        private void OnLeaderboardError(string message)
        {
            isLoading = false;
            ShowLoading(false);
            ShowError(message);
        }
        
        private void OnChangeNameClicked()
        {
            // Show name input dialog
            PlayerNameDialog.Instance?.Show(
                isFirstLaunch: false,
                currentName: LeaderboardManager.Instance?.GetPlayerName() ?? "",
                onConfirm: (newName) => {
                    UpdatePlayerInfo();
                    Debug.Log($"Name changed to: {newName}");
                }
            );
        }
        
        // ── Debug ────────────────────────────────────────────────────────────
        
        [ContextMenu("Show Endless Leaderboard")]
        private void ShowEndlessLeaderboardDebug()
        {
            ShowEndlessLeaderboard();
        }
        
        [ContextMenu("Submit Test Score")]
        private void SubmitTestScore()
        {
            if (LeaderboardManager.Instance != null)
            {
                int wave = Random.Range(10, 50);
                long score = Random.Range(10000, 100000);
                LeaderboardManager.Instance.SubmitEndlessScore(wave, score);
                
                // Refresh after short delay
                Invoke(nameof(RefreshCurrentLeaderboard), 1f);
            }
        }
    }
}
