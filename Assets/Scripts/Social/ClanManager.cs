using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Social
{
    /// <summary>
    /// Clan/Guild system for team collaboration and competition.
    /// Features clan creation, member management, clan wars, shared progression, and clan chat.
    /// Enables players to team up, compete as clans, and earn collaborative rewards.
    /// </summary>
    public class ClanManager : MonoBehaviour
    {
        public static ClanManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableClanSystem = true;
        [SerializeField] private int maxClanMembers = 50;
        [SerializeField] private int minClanNameLength = 3;
        [SerializeField] private int maxClanNameLength = 20;
        [SerializeField] private int clanCreationCost = 1000; // credits
        [SerializeField] private bool verboseLogging = true;

        [Header("Clan Wars")]
        [SerializeField] private int clanWarDuration = 7; // days
        [SerializeField] private int minClanWarMembers = 5;
        [SerializeField] private bool enableClanWars = true;

        [Header("Chat Settings")]
        [SerializeField] private int maxChatHistory = 100;
        [SerializeField] private int maxMessageLength = 200;

        // State
        private bool isInitialized = false;
        private ClanData currentClan;
        private string currentPlayerId;
        private List<ClanData> availableClans = new List<ClanData>();
        private List<ClanMessage> clanChatHistory = new List<ClanMessage>();
        private ClanWar activeClanWar;
        private ClanProgression clanProgression;

        // Events
        public event Action<ClanData> OnClanCreated;
        public event Action<ClanData> OnClanJoined;
        public event Action OnClanLeft;
        public event Action<ClanMember> OnMemberJoined;
        public event Action<string> OnMemberLeft; // member ID
        public event Action<ClanMessage> OnChatMessageReceived;
        public event Action<ClanWar> OnClanWarStarted;
        public event Action<ClanWar> OnClanWarEnded;
        public event Action<ClanAchievement> OnClanAchievementUnlocked;

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
            if (!enableClanSystem)
            {
                LogDebug("Clan system disabled");
                return;
            }

            StartCoroutine(InitializeClanSystem());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeClanSystem()
        {
            LogDebug("Initializing Clan System...");

            yield return new WaitForSeconds(0.3f);

            // Get current player ID (would come from authentication in real implementation)
            currentPlayerId = PlayerPrefs.GetString("PlayerId", System.Guid.NewGuid().ToString());

            // Load player's clan membership
            LoadCurrentClan();

            // Load clan chat history
            if (currentClan != null)
            {
                LoadChatHistory();
                LoadClanProgression();
                LoadActiveClanWar();
            }

            // Load available clans for browsing
            LoadAvailableClans();

            isInitialized = true;
            LogDebug($"Clan System initialized (In Clan: {currentClan != null})");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_system_initialized", new Dictionary<string, object>
                {
                    { "in_clan", currentClan != null },
                    { "available_clans", availableClans.Count }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Clan Creation & Management ────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a new clan with the player as leader.
        /// </summary>
        public bool CreateClan(string clanName, string clanTag, string description)
        {
            if (!isInitialized || currentClan != null)
            {
                LogDebug("Cannot create clan: already in a clan or not initialized");
                return false;
            }

            // Validate clan name
            if (string.IsNullOrWhiteSpace(clanName) || 
                clanName.Length < minClanNameLength || 
                clanName.Length > maxClanNameLength)
            {
                LogDebug($"Invalid clan name length: {clanName}");
                return false;
            }

            // Check if player has enough credits (in real implementation)
            // For now, we'll assume success

            currentClan = new ClanData
            {
                clanId = System.Guid.NewGuid().ToString(),
                clanName = clanName,
                clanTag = clanTag,
                description = description,
                leaderId = currentPlayerId,
                creationDate = DateTime.Now,
                memberCount = 1,
                totalTrophies = 0,
                clanLevel = 1,
                members = new List<ClanMember>
                {
                    new ClanMember
                    {
                        playerId = currentPlayerId,
                        playerName = PlayerPrefs.GetString("PlayerName", "Player"),
                        joinDate = DateTime.Now,
                        role = ClanRole.Leader,
                        contributionPoints = 0,
                        lastActive = DateTime.Now
                    }
                }
            };

            // Initialize clan progression
            clanProgression = new ClanProgression
            {
                clanId = currentClan.clanId,
                clanLevel = 1,
                clanExperience = 0,
                totalContributions = 0,
                unlockedPerks = new List<string>()
            };

            SaveCurrentClan();
            SaveClanProgression();

            OnClanCreated?.Invoke(currentClan);
            LogDebug($"Clan created: {clanName} [{clanTag}]");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_created", new Dictionary<string, object>
                {
                    { "clan_name", clanName },
                    { "clan_tag", clanTag }
                });
            }

            return true;
        }

        /// <summary>
        /// Joins an existing clan.
        /// </summary>
        public bool JoinClan(string clanId)
        {
            if (!isInitialized || currentClan != null)
            {
                LogDebug("Cannot join clan: already in a clan or not initialized");
                return false;
            }

            // Find clan in available clans
            ClanData targetClan = availableClans.FirstOrDefault(c => c.clanId == clanId);
            if (targetClan == null)
            {
                LogDebug($"Clan not found: {clanId}");
                return false;
            }

            // Check if clan is full
            if (targetClan.memberCount >= maxClanMembers)
            {
                LogDebug("Clan is full");
                return false;
            }

            // Add player as member
            ClanMember newMember = new ClanMember
            {
                playerId = currentPlayerId,
                playerName = PlayerPrefs.GetString("PlayerName", "Player"),
                joinDate = DateTime.Now,
                role = ClanRole.Member,
                contributionPoints = 0,
                lastActive = DateTime.Now
            };

            targetClan.members.Add(newMember);
            targetClan.memberCount++;

            currentClan = targetClan;
            SaveCurrentClan();

            OnClanJoined?.Invoke(currentClan);
            OnMemberJoined?.Invoke(newMember);
            LogDebug($"Joined clan: {currentClan.clanName}");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_joined", new Dictionary<string, object>
                {
                    { "clan_id", clanId },
                    { "clan_name", currentClan.clanName },
                    { "member_count", currentClan.memberCount }
                });
            }

            return true;
        }

        /// <summary>
        /// Leaves the current clan.
        /// </summary>
        public bool LeaveClan()
        {
            if (!isInitialized || currentClan == null)
            {
                LogDebug("Cannot leave clan: not in a clan");
                return false;
            }

            string clanName = currentClan.clanName;
            bool wasLeader = GetPlayerRole() == ClanRole.Leader;

            // Remove player from members
            currentClan.members.RemoveAll(m => m.playerId == currentPlayerId);
            currentClan.memberCount--;

            // If leader is leaving, promote another member
            if (wasLeader && currentClan.members.Count > 0)
            {
                currentClan.members[0].role = ClanRole.Leader;
                currentClan.leaderId = currentClan.members[0].playerId;
                LogDebug($"New leader: {currentClan.members[0].playerName}");
            }

            currentClan = null;
            clanProgression = null;
            clanChatHistory.Clear();

            SaveCurrentClan();
            DeleteChatHistory();
            DeleteClanProgression();

            OnClanLeft?.Invoke();
            LogDebug($"Left clan: {clanName}");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_left", new Dictionary<string, object>
                {
                    { "was_leader", wasLeader },
                    { "clan_name", clanName }
                });
            }

            return true;
        }

        /// <summary>
        /// Kicks a member from the clan (leader/officers only).
        /// </summary>
        public bool KickMember(string memberId)
        {
            if (!isInitialized || currentClan == null)
                return false;

            ClanRole playerRole = GetPlayerRole();
            if (playerRole != ClanRole.Leader && playerRole != ClanRole.Officer)
            {
                LogDebug("Insufficient permissions to kick members");
                return false;
            }

            ClanMember targetMember = currentClan.members.FirstOrDefault(m => m.playerId == memberId);
            if (targetMember == null)
            {
                LogDebug($"Member not found: {memberId}");
                return false;
            }

            // Cannot kick leader or officers (unless you're the leader)
            if (targetMember.role == ClanRole.Leader || 
                (targetMember.role == ClanRole.Officer && playerRole != ClanRole.Leader))
            {
                LogDebug("Cannot kick this member");
                return false;
            }

            currentClan.members.Remove(targetMember);
            currentClan.memberCount--;
            SaveCurrentClan();

            OnMemberLeft?.Invoke(memberId);
            LogDebug($"Kicked member: {targetMember.playerName}");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_member_kicked", new Dictionary<string, object>
                {
                    { "member_id", memberId },
                    { "clan_id", currentClan.clanId }
                });
            }

            return true;
        }

        /// <summary>
        /// Promotes a member to officer (leader only).
        /// </summary>
        public bool PromoteMember(string memberId)
        {
            if (!isInitialized || currentClan == null || GetPlayerRole() != ClanRole.Leader)
                return false;

            ClanMember targetMember = currentClan.members.FirstOrDefault(m => m.playerId == memberId);
            if (targetMember == null || targetMember.role != ClanRole.Member)
                return false;

            targetMember.role = ClanRole.Officer;
            SaveCurrentClan();

            LogDebug($"Promoted member: {targetMember.playerName}");
            return true;
        }

        /// <summary>
        /// Demotes an officer to member (leader only).
        /// </summary>
        public bool DemoteMember(string memberId)
        {
            if (!isInitialized || currentClan == null || GetPlayerRole() != ClanRole.Leader)
                return false;

            ClanMember targetMember = currentClan.members.FirstOrDefault(m => m.playerId == memberId);
            if (targetMember == null || targetMember.role != ClanRole.Officer)
                return false;

            targetMember.role = ClanRole.Member;
            SaveCurrentClan();

            LogDebug($"Demoted member: {targetMember.playerName}");
            return true;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Clan Chat ─────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sends a chat message to the clan.
        /// </summary>
        public bool SendChatMessage(string message)
        {
            if (!isInitialized || currentClan == null)
                return false;

            if (string.IsNullOrWhiteSpace(message) || message.Length > maxMessageLength)
            {
                LogDebug("Invalid message");
                return false;
            }

            ClanMessage chatMessage = new ClanMessage
            {
                messageId = System.Guid.NewGuid().ToString(),
                senderId = currentPlayerId,
                senderName = PlayerPrefs.GetString("PlayerName", "Player"),
                message = message,
                timestamp = DateTime.Now,
                messageType = ClanMessageType.Chat
            };

            clanChatHistory.Add(chatMessage);

            // Limit chat history
            if (clanChatHistory.Count > maxChatHistory)
            {
                clanChatHistory.RemoveAt(0);
            }

            SaveChatHistory();

            OnChatMessageReceived?.Invoke(chatMessage);
            LogDebug($"Chat message sent: {message}");

            return true;
        }

        /// <summary>
        /// Gets clan chat history.
        /// </summary>
        public List<ClanMessage> GetChatHistory(int count = 50)
        {
            return clanChatHistory.OrderByDescending(m => m.timestamp).Take(count).Reverse().ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Clan Wars ─────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Starts a clan war against another clan.
        /// </summary>
        public bool StartClanWar(string opponentClanId)
        {
            if (!isInitialized || !enableClanWars || currentClan == null)
                return false;

            if (GetPlayerRole() != ClanRole.Leader && GetPlayerRole() != ClanRole.Officer)
            {
                LogDebug("Insufficient permissions to start clan war");
                return false;
            }

            if (currentClan.memberCount < minClanWarMembers)
            {
                LogDebug($"Not enough members for clan war (min {minClanWarMembers})");
                return false;
            }

            if (activeClanWar != null && activeClanWar.isActive)
            {
                LogDebug("Already in an active clan war");
                return false;
            }

            activeClanWar = new ClanWar
            {
                warId = System.Guid.NewGuid().ToString(),
                clanId = currentClan.clanId,
                opponentClanId = opponentClanId,
                startDate = DateTime.Now,
                endDate = DateTime.Now.AddDays(clanWarDuration),
                isActive = true,
                clanScore = 0,
                opponentScore = 0,
                participants = new List<string>()
            };

            SaveActiveClanWar();

            OnClanWarStarted?.Invoke(activeClanWar);
            LogDebug($"Clan war started against: {opponentClanId}");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_war_started", new Dictionary<string, object>
                {
                    { "clan_id", currentClan.clanId },
                    { "opponent_id", opponentClanId }
                });
            }

            return true;
        }

        /// <summary>
        /// Records a clan war battle result.
        /// </summary>
        public void RecordClanWarBattle(bool isVictory, int score)
        {
            if (activeClanWar == null || !activeClanWar.isActive)
                return;

            if (isVictory)
            {
                activeClanWar.clanScore += score;
            }

            // Add participant if not already in
            if (!activeClanWar.participants.Contains(currentPlayerId))
            {
                activeClanWar.participants.Add(currentPlayerId);
            }

            SaveActiveClanWar();
            LogDebug($"Clan war battle recorded: Victory={isVictory}, Score={score}");
        }

        /// <summary>
        /// Ends the active clan war.
        /// </summary>
        public void EndClanWar()
        {
            if (activeClanWar == null || !activeClanWar.isActive)
                return;

            activeClanWar.isActive = false;
            activeClanWar.endDate = DateTime.Now;

            bool clanWon = activeClanWar.clanScore > activeClanWar.opponentScore;

            SaveActiveClanWar();

            OnClanWarEnded?.Invoke(activeClanWar);
            LogDebug($"Clan war ended: {(clanWon ? "Victory" : "Defeat")} ({activeClanWar.clanScore} vs {activeClanWar.opponentScore})");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_war_ended", new Dictionary<string, object>
                {
                    { "clan_id", currentClan.clanId },
                    { "victory", clanWon },
                    { "clan_score", activeClanWar.clanScore },
                    { "opponent_score", activeClanWar.opponentScore }
                });
            }
        }

        /// <summary>
        /// Gets active clan war.
        /// </summary>
        public ClanWar GetActiveClanWar()
        {
            return activeClanWar;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Clan Progression ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Contributes to clan progression.
        /// </summary>
        public void ContributeToClan(int amount, ContributionType type)
        {
            if (currentClan == null || clanProgression == null)
                return;

            // Update player's contribution
            ClanMember player = currentClan.members.FirstOrDefault(m => m.playerId == currentPlayerId);
            if (player != null)
            {
                player.contributionPoints += amount;
            }

            // Update clan progression
            clanProgression.totalContributions += amount;
            clanProgression.clanExperience += amount;

            // Check for level up
            int requiredXP = clanProgression.clanLevel * 1000;
            while (clanProgression.clanExperience >= requiredXP)
            {
                clanProgression.clanLevel++;
                clanProgression.clanExperience -= requiredXP;
                requiredXP = clanProgression.clanLevel * 1000;

                LogDebug($"Clan leveled up to level {clanProgression.clanLevel}!");
                UnlockClanPerk($"level_{clanProgression.clanLevel}");
            }

            currentClan.clanLevel = clanProgression.clanLevel;

            SaveCurrentClan();
            SaveClanProgression();

            LogDebug($"Contributed {amount} {type} to clan");

            // Track analytics
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent("clan_contribution", new Dictionary<string, object>
                {
                    { "amount", amount },
                    { "type", type.ToString() },
                    { "clan_level", clanProgression.clanLevel }
                });
            }
        }

        private void UnlockClanPerk(string perkId)
        {
            if (clanProgression.unlockedPerks.Contains(perkId))
                return;

            clanProgression.unlockedPerks.Add(perkId);
            LogDebug($"Unlocked clan perk: {perkId}");
        }

        /// <summary>
        /// Gets clan progression data.
        /// </summary>
        public ClanProgression GetClanProgression()
        {
            return clanProgression;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Data Retrieval ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets current clan data.
        /// </summary>
        public ClanData GetCurrentClan()
        {
            return currentClan;
        }

        /// <summary>
        /// Gets player's role in current clan.
        /// </summary>
        public ClanRole GetPlayerRole()
        {
            if (currentClan == null)
                return ClanRole.None;

            ClanMember player = currentClan.members.FirstOrDefault(m => m.playerId == currentPlayerId);
            return player?.role ?? ClanRole.None;
        }

        /// <summary>
        /// Gets list of available clans to join.
        /// </summary>
        public List<ClanData> GetAvailableClans()
        {
            return new List<ClanData>(availableClans);
        }

        /// <summary>
        /// Checks if player is in a clan.
        /// </summary>
        public bool IsInClan()
        {
            return currentClan != null;
        }

        /// <summary>
        /// Gets clan leaderboard (top clans by trophies).
        /// </summary>
        public List<ClanData> GetClanLeaderboard(int count = 100)
        {
            return availableClans.OrderByDescending(c => c.totalTrophies).Take(count).ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadCurrentClan()
        {
            string json = PlayerPrefs.GetString("CurrentClan", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentClan = JsonUtility.FromJson<ClanData>(json);
                    LogDebug($"Loaded clan: {currentClan.clanName}");
                }
                catch { LogDebug("Failed to load current clan"); }
            }
        }

        private void SaveCurrentClan()
        {
            if (currentClan == null)
            {
                PlayerPrefs.DeleteKey("CurrentClan");
            }
            else
            {
                string json = JsonUtility.ToJson(currentClan);
                PlayerPrefs.SetString("CurrentClan", json);
            }
            PlayerPrefs.Save();
        }

        private void LoadAvailableClans()
        {
            string json = PlayerPrefs.GetString("AvailableClans", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    ClanListData data = JsonUtility.FromJson<ClanListData>(json);
                    availableClans = data.clans ?? new List<ClanData>();
                    LogDebug($"Loaded {availableClans.Count} available clans");
                }
                catch { LogDebug("Failed to load available clans"); }
            }
        }

        private void LoadChatHistory()
        {
            string json = PlayerPrefs.GetString("ClanChatHistory", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    ClanChatData data = JsonUtility.FromJson<ClanChatData>(json);
                    clanChatHistory = data.messages ?? new List<ClanMessage>();
                    LogDebug($"Loaded {clanChatHistory.Count} chat messages");
                }
                catch { LogDebug("Failed to load chat history"); }
            }
        }

        private void SaveChatHistory()
        {
            ClanChatData data = new ClanChatData { messages = clanChatHistory };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("ClanChatHistory", json);
            PlayerPrefs.Save();
        }

        private void DeleteChatHistory()
        {
            PlayerPrefs.DeleteKey("ClanChatHistory");
            PlayerPrefs.Save();
        }

        private void LoadActiveClanWar()
        {
            string json = PlayerPrefs.GetString("ActiveClanWar", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    activeClanWar = JsonUtility.FromJson<ClanWar>(json);
                    LogDebug("Loaded active clan war");
                }
                catch { LogDebug("Failed to load active clan war"); }
            }
        }

        private void SaveActiveClanWar()
        {
            if (activeClanWar == null)
            {
                PlayerPrefs.DeleteKey("ActiveClanWar");
            }
            else
            {
                string json = JsonUtility.ToJson(activeClanWar);
                PlayerPrefs.SetString("ActiveClanWar", json);
            }
            PlayerPrefs.Save();
        }

        private void LoadClanProgression()
        {
            string json = PlayerPrefs.GetString("ClanProgression", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    clanProgression = JsonUtility.FromJson<ClanProgression>(json);
                    LogDebug($"Loaded clan progression: Level {clanProgression.clanLevel}");
                }
                catch { LogDebug("Failed to load clan progression"); }
            }
        }

        private void SaveClanProgression()
        {
            if (clanProgression == null)
            {
                PlayerPrefs.DeleteKey("ClanProgression");
            }
            else
            {
                string json = JsonUtility.ToJson(clanProgression);
                PlayerPrefs.SetString("ClanProgression", json);
            }
            PlayerPrefs.Save();
        }

        private void DeleteClanProgression()
        {
            PlayerPrefs.DeleteKey("ClanProgression");
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[ClanManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clan data structure.
    /// </summary>
    [Serializable]
    public class ClanData
    {
        public string clanId;
        public string clanName;
        public string clanTag;
        public string description;
        public string leaderId;
        public DateTime creationDate;
        public int memberCount;
        public int totalTrophies;
        public int clanLevel;
        public List<ClanMember> members;
    }

    /// <summary>
    /// Clan member data.
    /// </summary>
    [Serializable]
    public class ClanMember
    {
        public string playerId;
        public string playerName;
        public DateTime joinDate;
        public ClanRole role;
        public int contributionPoints;
        public DateTime lastActive;
    }

    /// <summary>
    /// Clan member roles.
    /// </summary>
    public enum ClanRole
    {
        None,
        Member,
        Officer,
        Leader
    }

    /// <summary>
    /// Clan chat message.
    /// </summary>
    [Serializable]
    public class ClanMessage
    {
        public string messageId;
        public string senderId;
        public string senderName;
        public string message;
        public DateTime timestamp;
        public ClanMessageType messageType;
    }

    /// <summary>
    /// Clan message types.
    /// </summary>
    public enum ClanMessageType
    {
        Chat,
        System,
        Announcement
    }

    /// <summary>
    /// Clan war data.
    /// </summary>
    [Serializable]
    public class ClanWar
    {
        public string warId;
        public string clanId;
        public string opponentClanId;
        public DateTime startDate;
        public DateTime endDate;
        public bool isActive;
        public int clanScore;
        public int opponentScore;
        public List<string> participants;
    }

    /// <summary>
    /// Clan progression data.
    /// </summary>
    [Serializable]
    public class ClanProgression
    {
        public string clanId;
        public int clanLevel;
        public int clanExperience;
        public int totalContributions;
        public List<string> unlockedPerks;
    }

    /// <summary>
    /// Contribution types.
    /// </summary>
    public enum ContributionType
    {
        Credits,
        Trophies,
        Experience
    }

    /// <summary>
    /// Clan achievement.
    /// </summary>
    [Serializable]
    public class ClanAchievement
    {
        public string achievementId;
        public string achievementName;
        public string description;
        public DateTime unlockedDate;
    }

    // Serialization helpers
    [Serializable]
    public class ClanListData
    {
        public List<ClanData> clans;
    }

    [Serializable]
    public class ClanChatData
    {
        public List<ClanMessage> messages;
    }
}
