using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Analytics;

namespace RobotTD.Events
{
    /// <summary>
    /// Manages seasonal and limited-time events with special challenges, rewards, and leaderboards.
    /// Events can be holiday-themed (Halloween, Christmas) or special occasions.
    /// Integrates with analytics and leaderboard systems.
    /// </summary>
    public class SeasonalEventManager : MonoBehaviour
    {
        public static SeasonalEventManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableEvents = true;
        [SerializeField] private float eventCheckInterval = 300f; // 5 minutes
        [SerializeField] private bool verboseLogging = true;

        [Header("Event Definitions")]
        [SerializeField] private SeasonalEventData[] availableEvents;

        // State
        private SeasonalEventData currentEvent;
        private Dictionary<string, int> eventCurrencies = new Dictionary<string, int>();
        private Dictionary<string, EventProgress> eventProgress = new Dictionary<string, EventProgress>();
        private HashSet<string> claimedRewards = new HashSet<string>();
        private float eventCheckTimer = 0f;
        private bool isInitialized = false;

        // Events
        public event Action<SeasonalEventData> OnEventStarted;
        public event Action<SeasonalEventData> OnEventEnded;
        public event Action<string, int> OnEventCurrencyEarned; // currencyId, amount
        public event Action<EventReward> OnRewardClaimed;
        public event Action<EventChallenge, int> OnChallengeProgress; // challenge, progress
        public event Action<EventChallenge> OnChallengeCompleted;

        // ══════════════════════════════════════════════════════════════════════
        // ── Unity Lifecycle ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!enableEvents)
            {
                LogDebug("Seasonal events disabled");
                return;
            }

            StartCoroutine(InitializeEventSystem());
        }

        private void Update()
        {
            if (!isInitialized || !enableEvents) return;

            // Periodic event check
            eventCheckTimer += Time.unscaledDeltaTime;
            if (eventCheckTimer >= eventCheckInterval)
            {
                eventCheckTimer = 0f;
                CheckForActiveEvents();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeEventSystem()
        {
            LogDebug("Initializing seasonal event system...");

            yield return new WaitForSeconds(0.5f);

            // Load saved event progress
            LoadEventProgressFromLocal();
            LoadEventCurrenciesFromLocal();
            LoadClaimedRewardsFromLocal();

            // Check for active events
            CheckForActiveEvents();

            isInitialized = true;
            LogDebug("Seasonal event system initialized");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("seasonal_events_initialized", new Dictionary<string, object>
                {
                    { "has_active_event", currentEvent != null },
                    { "event_id", currentEvent?.eventId ?? "none" }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Event Management ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Checks if any events should be active based on current date/time.
        /// </summary>
        private void CheckForActiveEvents()
        {
            DateTime now = DateTime.UtcNow;

            // Check if current event has ended
            if (currentEvent != null && now > currentEvent.endDate)
            {
                EndEvent(currentEvent);
            }

            // Check for new events to start
            if (currentEvent == null && availableEvents != null)
            {
                foreach (var eventData in availableEvents)
                {
                    if (now >= eventData.startDate && now <= eventData.endDate)
                    {
                        StartEvent(eventData);
                        break; // Only one event active at a time
                    }
                }
            }
        }

        /// <summary>
        /// Starts a seasonal event.
        /// </summary>
        private void StartEvent(SeasonalEventData eventData)
        {
            if (currentEvent != null)
            {
                LogDebug($"Cannot start event {eventData.eventId}: another event is already active");
                return;
            }

            currentEvent = eventData;
            LogDebug($"Starting seasonal event: {eventData.eventName}");

            // Initialize event progress if first time
            if (!eventProgress.ContainsKey(eventData.eventId))
            {
                eventProgress[eventData.eventId] = new EventProgress
                {
                    eventId = eventData.eventId,
                    startedDate = DateTime.UtcNow,
                    challengeProgress = new Dictionary<string, int>()
                };
            }

            // Initialize currency if not exists
            if (!eventCurrencies.ContainsKey(eventData.currencyId))
            {
                eventCurrencies[eventData.currencyId] = 0;
            }

            OnEventStarted?.Invoke(eventData);

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("seasonal_event_started", new Dictionary<string, object>
                {
                    { "event_id", eventData.eventId },
                    { "event_name", eventData.eventName },
                    { "event_type", eventData.eventType.ToString() }
                });
            }
        }

        /// <summary>
        /// Ends a seasonal event.
        /// </summary>
        private void EndEvent(SeasonalEventData eventData)
        {
            if (currentEvent == null || currentEvent.eventId != eventData.eventId)
                return;

            LogDebug($"Ending seasonal event: {eventData.eventName}");

            OnEventEnded?.Invoke(eventData);
            currentEvent = null;

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                var progress = GetEventProgress(eventData.eventId);
                int completedChallenges = progress != null ? progress.challengeProgress.Count(kvp => kvp.Value >= GetChallengeRequirement(eventData.eventId, kvp.Key)) : 0;

                AnalyticsManager.Instance.TrackEvent("seasonal_event_ended", new Dictionary<string, object>
                {
                    { "event_id", eventData.eventId },
                    { "event_name", eventData.eventName },
                    { "completed_challenges", completedChallenges },
                    { "total_challenges", eventData.challenges.Length }
                });
            }
        }

        /// <summary>
        /// Manually activates an event (for testing).
        /// </summary>
        public void ActivateEvent(string eventId)
        {
            if (availableEvents == null) return;

            var eventData = availableEvents.FirstOrDefault(e => e.eventId == eventId);
            if (eventData != null)
            {
                StartEvent(eventData);
            }
        }

        /// <summary>
        /// Gets the currently active event.
        /// </summary>
        public SeasonalEventData GetCurrentEvent()
        {
            return currentEvent;
        }

        /// <summary>
        /// Checks if an event is currently active.
        /// </summary>
        public bool IsEventActive()
        {
            return currentEvent != null;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Challenge System ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Updates progress for an event challenge.
        /// </summary>
        public void UpdateChallengeProgress(string challengeId, int amount)
        {
            if (currentEvent == null) return;

            var progress = GetEventProgress(currentEvent.eventId);
            if (progress == null) return;

            // Update progress
            if (!progress.challengeProgress.ContainsKey(challengeId))
            {
                progress.challengeProgress[challengeId] = 0;
            }

            progress.challengeProgress[challengeId] += amount;

            // Get challenge definition
            var challenge = currentEvent.challenges.FirstOrDefault(c => c.challengeId == challengeId);
            if (challenge == null) return;

            int currentProgress = progress.challengeProgress[challengeId];
            OnChallengeProgress?.Invoke(challenge, currentProgress);

            // Check if completed
            if (currentProgress >= challenge.targetValue && !progress.completedChallenges.Contains(challengeId))
            {
                CompleteChallenge(challenge);
            }

            SaveEventProgressToLocal();

            LogDebug($"Updated challenge progress: {challengeId} = {currentProgress}/{challenge.targetValue}");
        }

        /// <summary>
        /// Marks a challenge as completed and awards rewards.
        /// </summary>
        private void CompleteChallenge(EventChallenge challenge)
        {
            if (currentEvent == null) return;

            var progress = GetEventProgress(currentEvent.eventId);
            if (progress == null) return;

            progress.completedChallenges.Add(challenge.challengeId);

            // Award event currency
            AwardEventCurrency(currentEvent.currencyId, challenge.currencyReward);

            OnChallengeCompleted?.Invoke(challenge);

            LogDebug($"Challenge completed: {challenge.challengeName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("event_challenge_completed", new Dictionary<string, object>
                {
                    { "event_id", currentEvent.eventId },
                    { "challenge_id", challenge.challengeId },
                    { "challenge_name", challenge.challengeName },
                    { "currency_reward", challenge.currencyReward }
                });
            }
        }

        /// <summary>
        /// Gets progress for a specific challenge.
        /// </summary>
        public int GetChallengeProgress(string challengeId)
        {
            if (currentEvent == null) return 0;

            var progress = GetEventProgress(currentEvent.eventId);
            if (progress == null) return 0;

            return progress.challengeProgress.ContainsKey(challengeId) ? progress.challengeProgress[challengeId] : 0;
        }

        /// <summary>
        /// Checks if a challenge is completed.
        /// </summary>
        public bool IsChallengeCompleted(string challengeId)
        {
            if (currentEvent == null) return false;

            var progress = GetEventProgress(currentEvent.eventId);
            if (progress == null) return false;

            return progress.completedChallenges.Contains(challengeId);
        }

        private int GetChallengeRequirement(string eventId, string challengeId)
        {
            var eventData = availableEvents?.FirstOrDefault(e => e.eventId == eventId);
            if (eventData == null) return 0;

            var challenge = eventData.challenges.FirstOrDefault(c => c.challengeId == challengeId);
            return challenge != null ? challenge.targetValue : 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Event Currency ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Awards event currency to the player.
        /// </summary>
        public void AwardEventCurrency(string currencyId, int amount)
        {
            if (!eventCurrencies.ContainsKey(currencyId))
            {
                eventCurrencies[currencyId] = 0;
            }

            eventCurrencies[currencyId] += amount;
            OnEventCurrencyEarned?.Invoke(currencyId, amount);

            SaveEventCurrenciesToLocal();

            LogDebug($"Awarded {amount} {currencyId}. Total: {eventCurrencies[currencyId]}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("event_currency_earned", new Dictionary<string, object>
                {
                    { "currency_id", currencyId },
                    { "amount", amount },
                    { "new_total", eventCurrencies[currencyId] }
                });
            }
        }

        /// <summary>
        /// Gets the amount of event currency the player has.
        /// </summary>
        public int GetEventCurrency(string currencyId)
        {
            return eventCurrencies.ContainsKey(currencyId) ? eventCurrencies[currencyId] : 0;
        }

        /// <summary>
        /// Spends event currency (returns true if successful).
        /// </summary>
        public bool SpendEventCurrency(string currencyId, int amount)
        {
            if (!eventCurrencies.ContainsKey(currencyId))
                return false;

            if (eventCurrencies[currencyId] < amount)
                return false;

            eventCurrencies[currencyId] -= amount;
            SaveEventCurrenciesToLocal();

            LogDebug($"Spent {amount} {currencyId}. Remaining: {eventCurrencies[currencyId]}");

            return true;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Reward System ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Claims an event reward using event currency.
        /// </summary>
        public bool ClaimReward(string rewardId)
        {
            if (currentEvent == null) return false;

            // Check if already claimed
            string claimKey = $"{currentEvent.eventId}_{rewardId}";
            if (claimedRewards.Contains(claimKey))
            {
                LogDebug($"Reward {rewardId} already claimed");
                return false;
            }

            // Find reward
            var reward = currentEvent.rewards.FirstOrDefault(r => r.rewardId == rewardId);
            if (reward == null)
            {
                LogDebug($"Reward {rewardId} not found");
                return false;
            }

            // Check currency requirement
            if (!SpendEventCurrency(currentEvent.currencyId, reward.currencyCost))
            {
                LogDebug($"Insufficient currency to claim reward {rewardId}");
                return false;
            }

            // Mark as claimed
            claimedRewards.Add(claimKey);
            SaveClaimedRewardsToLocal();

            // Award the reward (integration with game systems would go here)
            OnRewardClaimed?.Invoke(reward);

            LogDebug($"Claimed reward: {reward.rewardName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("event_reward_claimed", new Dictionary<string, object>
                {
                    { "event_id", currentEvent.eventId },
                    { "reward_id", rewardId },
                    { "reward_type", reward.rewardType.ToString() },
                    { "currency_cost", reward.currencyCost }
                });
            }

            return true;
        }

        /// <summary>
        /// Checks if a reward has been claimed.
        /// </summary>
        public bool IsRewardClaimed(string rewardId)
        {
            if (currentEvent == null) return false;
            string claimKey = $"{currentEvent.eventId}_{rewardId}";
            return claimedRewards.Contains(claimKey);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Event Progress ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private EventProgress GetEventProgress(string eventId)
        {
            return eventProgress.ContainsKey(eventId) ? eventProgress[eventId] : null;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════────════════════

        private void LoadEventProgressFromLocal()
        {
            string json = PlayerPrefs.GetString("EventProgress", "{}");
            try
            {
                var data = JsonUtility.FromJson<EventProgressCollection>(json);
                if (data != null && data.progressList != null)
                {
                    eventProgress = data.progressList.ToDictionary(p => p.eventId);
                    LogDebug($"Loaded progress for {eventProgress.Count} events");
                }
            }
            catch
            {
                LogDebug("No event progress found");
            }
        }

        private void SaveEventProgressToLocal()
        {
            var data = new EventProgressCollection
            {
                progressList = eventProgress.Values.ToArray()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("EventProgress", json);
            PlayerPrefs.Save();
        }

        private void LoadEventCurrenciesFromLocal()
        {
            string json = PlayerPrefs.GetString("EventCurrencies", "{}");
            try
            {
                var data = JsonUtility.FromJson<EventCurrencyCollection>(json);
                if (data != null && data.currencies != null)
                {
                    eventCurrencies = new Dictionary<string, int>();
                    foreach (var entry in data.currencies)
                    {
                        eventCurrencies[entry.currencyId] = entry.amount;
                    }
                    LogDebug($"Loaded {eventCurrencies.Count} event currencies");
                }
            }
            catch
            {
                LogDebug("No event currencies found");
            }
        }

        private void SaveEventCurrenciesToLocal()
        {
            var data = new EventCurrencyCollection
            {
                currencies = eventCurrencies.Select(kvp => new EventCurrencyEntry { currencyId = kvp.Key, amount = kvp.Value }).ToArray()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("EventCurrencies", json);
            PlayerPrefs.Save();
        }

        private void LoadClaimedRewardsFromLocal()
        {
            string json = PlayerPrefs.GetString("EventClaimedRewards", "{}");
            try
            {
                var data = JsonUtility.FromJson<ClaimedRewardsData>(json);
                if (data != null && data.claimedRewards != null)
                {
                    claimedRewards = new HashSet<string>(data.claimedRewards);
                    LogDebug($"Loaded {claimedRewards.Count} claimed rewards");
                }
            }
            catch
            {
                LogDebug("No claimed rewards found");
            }
        }

        private void SaveClaimedRewardsToLocal()
        {
            var data = new ClaimedRewardsData { claimedRewards = claimedRewards.ToArray() };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("EventClaimedRewards", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[SeasonalEventManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Defines a seasonal event.
    /// </summary>
    [Serializable]
    public class SeasonalEventData
    {
        public string eventId;
        public string eventName;
        [TextArea(2, 4)] public string description;
        public EventType eventType;
        public DateTime startDate;
        public DateTime endDate;
        public string currencyId; // e.g., "candies", "snowflakes"
        public EventChallenge[] challenges;
        public EventReward[] rewards;
        public Sprite eventIcon;
        public Color eventColor = Color.white;
    }

    /// <summary>
    /// Type of seasonal event.
    /// </summary>
    public enum EventType
    {
        Halloween,
        Christmas,
        NewYear,
        Easter,
        Summer,
        Anniversary,
        Special
    }

    /// <summary>
    /// An event challenge that players complete for rewards.
    /// </summary>
    [Serializable]
    public class EventChallenge
    {
        public string challengeId;
        public string challengeName;
        [TextArea(1, 3)] public string description;
        public ChallengeType challengeType;
        public int targetValue;
        public int currencyReward;
    }

    /// <summary>
    /// Type of event challenge.
    /// </summary>
    public enum ChallengeType
    {
        KillEnemies,
        CompleteWaves,
        PlaceTowers,
        EarnCredits,
        WinMaps,
        PlayGames,
        NoLivesLost,
        SpeedRun,
        UseSpecificTower,
        DefeatBosses
    }

    /// <summary>
    /// Reward that can be purchased with event currency.
    /// </summary>
    [Serializable]
    public class EventReward
    {
        public string rewardId;
        public string rewardName;
        [TextArea(1, 2)] public string description;
        public RewardType rewardType;
        public int currencyCost;
        public string rewardValue; // Tower skin ID, cosmetic ID, etc.
        public Sprite rewardIcon;
    }

    /// <summary>
    /// Type of event reward.
    /// </summary>
    public enum RewardType
    {
        TowerSkin,
        Cosmetic,
        Currency,
        PowerUp,
        Avatar,
        Banner,
        Title,
        Exclusive
    }

    /// <summary>
    /// Tracks player's progress in an event.
    /// </summary>
    [Serializable]
    public class EventProgress
    {
        public string eventId;
        public DateTime startedDate;
        public Dictionary<string, int> challengeProgress = new Dictionary<string, int>();
        public List<string> completedChallenges = new List<string>();
    }

    // Serialization helpers
    [Serializable]
    public class EventProgressCollection
    {
        public EventProgress[] progressList;
    }

    [Serializable]
    public class EventCurrencyCollection
    {
        public EventCurrencyEntry[] currencies;
    }

    [Serializable]
    public class EventCurrencyEntry
    {
        public string currencyId;
        public int amount;
    }

    [Serializable]
    public class ClaimedRewardsData
    {
        public string[] claimedRewards;
    }
}
