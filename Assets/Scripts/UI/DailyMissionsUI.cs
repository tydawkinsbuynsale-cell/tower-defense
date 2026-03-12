using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Core;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// Main UI panel for displaying daily missions.
    /// Shows 3 current missions with progress and claim buttons.
    /// </summary>
    public class DailyMissionsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform missionCardContainer;
        [SerializeField] private GameObject missionCardPrefab;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        
        [Header("Animation")]
        [SerializeField] private float cardSpawnDelay = 0.1f;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private MissionCardUI[] currentCards = new MissionCardUI[3];
        private bool isOpen = false;
        
        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
            
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(ForceRefresh);
                refreshButton.gameObject.SetActive(false); // Hide in production
#if UNITY_EDITOR
                refreshButton.gameObject.SetActive(true); // Show in editor for testing
#endif
            }
            
            if (panel != null)
            {
                panel.SetActive(false);
            }
            
            SubscribeToMissionEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromMissionEvents();
        }
        
        private void Update()
        {
            if (isOpen && timerText != null)
            {
                UpdateTimer();
            }
        }
        
        // ── Panel Control ─────────────────────────────────────────────────────
        
        public void Open()
        {
            if (panel == null) return;
            
            panel.SetActive(true);
            isOpen = true;
            
            RefreshDisplay();
            
            // Fade in animation
            if (canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEaseOutQuad();
            }
            
            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(
                    Analytics.AnalyticsEvents.EventType.UIOpened,
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "screen_name", "daily_missions" }
                    }
                );
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
        
        // ── Display Refresh ───────────────────────────────────────────────────
        
        private void RefreshDisplay()
        {
            if (MissionManager.Instance == null)
            {
                Debug.LogWarning("[DailyMissionsUI] MissionManager not found");
                return;
            }
            
            ClearCards();
            
            MissionData[] missions = MissionManager.Instance.CurrentMissions;
            
            if (missions == null || missions.Length == 0)
            {
                Debug.LogWarning("[DailyMissionsUI] No missions available");
                return;
            }
            
            // Spawn cards with delay for staggered animation
            for (int i = 0; i < missions.Length && i < currentCards.Length; i++)
            {
                int index = i; // Capture for lambda
                LeanTween.delayedCall(cardSpawnDelay * i, () => SpawnMissionCard(missions[index], index));
            }
            
            UpdateCompletionText();
        }
        
        private void SpawnMissionCard(MissionData mission, int index)
        {
            if (missionCardPrefab == null || missionCardContainer == null)
            {
                Debug.LogWarning("[DailyMissionsUI] Missing card prefab or container");
                return;
            }
            
            GameObject cardObj = Instantiate(missionCardPrefab, missionCardContainer);
            MissionCardUI card = cardObj.GetComponent<MissionCardUI>();
            
            if (card != null)
            {
                MissionProgress progress = MissionManager.Instance.GetMissionProgress(mission.MissionId);
                card.Setup(mission, progress);
                card.OnRewardClaimed += HandleRewardClaimed;
                currentCards[index] = card;
                
                // Animated entrance
                CanvasGroup cardGroup = cardObj.GetComponent<CanvasGroup>();
                if (cardGroup == null)
                    cardGroup = cardObj.AddComponent<CanvasGroup>();
                
                cardGroup.alpha = 0f;
                LeanTween.alphaCanvas(cardGroup, 1f, 0.3f).setEaseOutQuad();
                
                Vector3 startPos = cardObj.transform.localPosition;
                cardObj.transform.localPosition = startPos + Vector3.left * 100f;
                LeanTween.moveLocal(cardObj, startPos, 0.3f).setEaseOutQuad();
            }
        }
        
        private void ClearCards()
        {
            foreach (var card in currentCards)
            {
                if (card != null)
                {
                    card.OnRewardClaimed -= HandleRewardClaimed;
                    Destroy(card.gameObject);
                }
            }
            
            currentCards = new MissionCardUI[3];
        }
        
        private void UpdateTimer()
        {
            if (MissionManager.Instance == null) return;
            
            TimeSpan timeUntilRotation = MissionManager.Instance.GetTimeUntilRotation();
            
            if (timerText != null)
            {
                timerText.text = $"Next Rotation: {timeUntilRotation.Hours:D2}:{timeUntilRotation.Minutes:D2}:{timeUntilRotation.Seconds:D2}";
            }
        }
        
        private void UpdateCompletionText()
        {
            if (MissionManager.Instance == null || completionText == null) return;
            
            int completed = MissionManager.Instance.GetCompletedMissionsCount();
            int total = MissionManager.Instance.CurrentMissions.Length;
            
            completionText.text = $"Completed: {completed}/{total}";
            
            // Color based on completion
            if (completed == total)
            {
                completionText.color = new Color(0.2f, 0.8f, 0.2f); // Green
            }
            else if (completed > 0)
            {
                completionText.color = new Color(0.8f, 0.6f, 0.2f); // Orange
            }
            else
            {
                completionText.color = Color.white;
            }
        }
        
        // ── Event Handlers ────────────────────────────────────────────────────
        
        private void SubscribeToMissionEvents()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionProgressUpdated += HandleMissionProgressUpdated;
                MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
                MissionManager.Instance.OnMissionsRotated += HandleMissionsRotated;
            }
        }
        
        private void UnsubscribeFromMissionEvents()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionProgressUpdated -= HandleMissionProgressUpdated;
                MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
                MissionManager.Instance.OnMissionsRotated -= HandleMissionsRotated;
            }
        }
        
        private void HandleMissionProgressUpdated(MissionData mission)
        {
            // Find and update corresponding card
            foreach (var card in currentCards)
            {
                if (card != null && card.MissionId == mission.MissionId)
                {
                    MissionProgress progress = MissionManager.Instance.GetMissionProgress(mission.MissionId);
                    card.UpdateProgress(progress);
                    break;
                }
            }
        }
        
        private void HandleMissionCompleted(MissionData mission)
        {
            UpdateCompletionText();
            
            // Show completion effect on card
            foreach (var card in currentCards)
            {
                if (card != null && card.MissionId == mission.MissionId)
                {
                    card.PlayCompletionAnimation();
                    break;
                }
            }
        }
        
        private void HandleMissionsRotated()
        {
            if (isOpen)
            {
                RefreshDisplay();
            }
        }
        
        private void HandleRewardClaimed(string missionId)
        {
            UpdateCompletionText();
            
            // Show toast notification
            if (ToastNotification.Instance != null)
            {
                MissionData mission = MissionManager.Instance.GetMissionData(missionId);
                if (mission != null)
                {
                    ToastNotification.Instance.Show(
                        $"Claimed: {mission.CreditReward} credits, {mission.TechPointReward} tech points",
                        ToastNotification.ToastType.Success
                    );
                }
            }
        }
        
        private void ForceRefresh()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.CheckAndRotateMissions();
                RefreshDisplay();
            }
        }
        
        // ── Static Access ─────────────────────────────────────────────────────
        
        private static DailyMissionsUI instance;
        public static DailyMissionsUI Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<DailyMissionsUI>();
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
