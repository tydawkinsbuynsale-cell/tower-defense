using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace RobotTD.UI
{
    /// <summary>
    /// Main UI panel for browsing and selecting challenges.
    /// </summary>
    public class ChallengeSelectorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject challengeCardPrefab;
        [SerializeField] private Transform dailyContainer;
        [SerializeField] private Transform weeklyContainer;
        [SerializeField] private Transform permanentContainer;
        
        [Header("Panels")]
        [SerializeField] private GameObject dailyPanel;
        [SerializeField] private GameObject weeklyPanel;
        [SerializeField] private GameObject permanentPanel;
        
        [Header("Tab Buttons")]
        [SerializeField] private Button dailyTabButton;
        [SerializeField] private Button weeklyTabButton;
        [SerializeField] private Button permanentTabButton;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        
        [Header("Timers")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private TextMeshProUGUI dailyTimerText;
        [SerializeField] private TextMeshProUGUI weeklyTimerText;
        
        private List<ChallengeCardUI> instantiatedCards = new List<ChallengeCardUI>();
        private TabType currentTab = TabType.Daily;
        
        private enum TabType { Daily, Weekly, Permanent }
        
        // ── Unity ────────────────────────────────────────────────────────────
        
        private void OnEnable()
        {
            Core.ChallengeManager.OnDailyChallengeRotated += RefreshDailyChallenges;
            Core.ChallengeManager.OnWeeklyChallengeRotated += RefreshWeeklyChallenges;
            
            RefreshAllChallenges();
            ShowTab(TabType.Daily);
        }
        
        private void OnDisable()
        {
            Core.ChallengeManager.OnDailyChallengeRotated -= RefreshDailyChallenges;
            Core.ChallengeManager.OnWeeklyChallengeRotated -= RefreshWeeklyChallenges;
        }
        
        private void Start()
        {
            // Setup button listeners
            if (dailyTabButton != null)
                dailyTabButton.onClick.AddListener(() => ShowTab(TabType.Daily));
            
            if (weeklyTabButton != null)
                weeklyTabButton.onClick.AddListener(() => ShowTab(TabType.Weekly));
            
            if (permanentTabButton != null)
                permanentTabButton.onClick.AddListener(() => ShowTab(TabType.Permanent));
            
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshAllChallenges);
        }
        
        private void Update()
        {
            UpdateTimers();
        }
        
        // ── Display ──────────────────────────────────────────────────────────
        
        public void Open()
        {
            gameObject.SetActive(true);
            RefreshAllChallenges();
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
        
        private void ShowTab(TabType tab)
        {
            currentTab = tab;
            
            // Hide all panels
            if (dailyPanel != null) dailyPanel.SetActive(false);
            if (weeklyPanel != null) weeklyPanel.SetActive(false);
            if (permanentPanel != null) permanentPanel.SetActive(false);
            
            // Show selected panel
            switch (tab)
            {
                case TabType.Daily:
                    if (dailyPanel != null) dailyPanel.SetActive(true);
                    if (titleText != null) titleText.text = "Daily Challenges";
                    break;
                
                case TabType.Weekly:
                    if (weeklyPanel != null) weeklyPanel.SetActive(true);
                    if (titleText != null) titleText.text = "Weekly Challenges";
                    break;
                
                case TabType.Permanent:
                    if (permanentPanel != null) permanentPanel.SetActive(true);
                    if (titleText != null) titleText.text = "All Challenges";
                    break;
            }
            
            // Update tab button visuals
            UpdateTabButtons();
            
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
        
        private void UpdateTabButtons()
        {
            // Highlight active tab (you can customize colors)
            if (dailyTabButton != null)
            {
                var colors = dailyTabButton.colors;
                colors.normalColor = currentTab == TabType.Daily ? Color.cyan : Color.white;
                dailyTabButton.colors = colors;
            }
            
            if (weeklyTabButton != null)
            {
                var colors = weeklyTabButton.colors;
                colors.normalColor = currentTab == TabType.Weekly ? Color.cyan : Color.white;
                weeklyTabButton.colors = colors;
            }
            
            if (permanentTabButton != null)
            {
                var colors = permanentTabButton.colors;
                colors.normalColor = currentTab == TabType.Permanent ? Color.cyan : Color.white;
                permanentTabButton.colors = colors;
            }
        }
        
        // ── Challenge Population ─────────────────────────────────────────────
        
        private void RefreshAllChallenges()
        {
            RefreshDailyChallenges();
            RefreshWeeklyChallenges();
            RefreshPermanentChallenges();
        }
        
        private void RefreshDailyChallenges()
        {
            ClearContainer(dailyContainer);
            
            if (Core.ChallengeManager.Instance == null) return;
            
            var challenges = Core.ChallengeManager.Instance.GetActiveDailyChallenges();
            foreach (var challenge in challenges)
            {
                CreateChallengeCard(challenge, dailyContainer);
            }
        }
        
        private void RefreshWeeklyChallenges()
        {
            ClearContainer(weeklyContainer);
            
            if (Core.ChallengeManager.Instance == null) return;
            
            var challenges = Core.ChallengeManager.Instance.GetActiveWeeklyChallenges();
            foreach (var challenge in challenges)
            {
                CreateChallengeCard(challenge, weeklyContainer);
            }
        }
        
        private void RefreshPermanentChallenges()
        {
            ClearContainer(permanentContainer);
            
            if (Core.ChallengeManager.Instance == null) return;
            
            var challenges = Core.ChallengeManager.Instance.GetPermanentChallenges();
            foreach (var challenge in challenges)
            {
                CreateChallengeCard(challenge, permanentContainer);
            }
        }
        
        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            
            instantiatedCards.Clear();
        }
        
        private void CreateChallengeCard(Core.ChallengeData challenge, Transform parent)
        {
            if (challengeCardPrefab == null || parent == null) return;
            
            GameObject cardObj = Instantiate(challengeCardPrefab, parent);
            ChallengeCardUI card = cardObj.GetComponent<ChallengeCardUI>();
            
            if (card != null)
            {
                card.Setup(challenge);
                instantiatedCards.Add(card);
            }
        }
        
        // ── Timers ───────────────────────────────────────────────────────────
        
        private void UpdateTimers()
        {
            if (timerPanel != null && !timerPanel.activeSelf) return;
            
            // Daily timer
            if (dailyTimerText != null)
            {
                System.TimeSpan timeUntilDaily = GetTimeUntilNextDaily();
                dailyTimerText.text = $"Next Daily: {timeUntilDaily.Hours:D2}:{timeUntilDaily.Minutes:D2}:{timeUntilDaily.Seconds:D2}";
            }
            
            // Weekly timer
            if (weeklyTimerText != null)
            {
                System.TimeSpan timeUntilWeekly = GetTimeUntilNextWeekly();
                int days = timeUntilWeekly.Days;
                int hours = timeUntilWeekly.Hours;
                weeklyTimerText.text = $"Next Weekly: {days}d {hours:D2}h";
            }
        }
        
        private System.TimeSpan GetTimeUntilNextDaily()
        {
            // Get time until next 24-hour rotation
            System.DateTime now = System.DateTime.Now;
            System.DateTime nextRotation = now.Date.AddDays(1);
            return nextRotation - now;
        }
        
        private System.TimeSpan GetTimeUntilNextWeekly()
        {
            // Get time until next 7-day rotation
            System.DateTime now = System.DateTime.Now;
            System.DateTime nextRotation = now.Date.AddDays(7 - (int)now.DayOfWeek);
            return nextRotation - now;
        }
    }
}
