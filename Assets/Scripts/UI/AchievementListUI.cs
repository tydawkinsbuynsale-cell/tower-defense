using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RobotTD.Progression;

namespace RobotTD.UI
{
    /// <summary>
    /// Display panel showing all achievements with progress indicators.
    /// Shows locked, unlocked, and progress towards unlocked achievements.
    /// </summary>
    public class AchievementListUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Filter Buttons")]
        [SerializeField] private Button showAllButton;
        [SerializeField] private Button showUnlockedButton;
        [SerializeField] private Button showLockedButton;

        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI summaryText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Categories")]
        [SerializeField] private TMP_Dropdown categoryDropdown;

        private List<GameObject> achievementItems = new List<GameObject>();
        private AchievementCategory currentCategory = AchievementCategory.General;
        private FilterMode currentFilter = FilterMode.All;

        private enum FilterMode
        {
            All,
            Unlocked,
            Locked
        }

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (showAllButton != null)
                showAllButton.onClick.AddListener(() => SetFilter(FilterMode.All));

            if (showUnlockedButton != null)
                showUnlockedButton.onClick.AddListener(() => SetFilter(FilterMode.Unlocked));

            if (showLockedButton != null)
                showLockedButton.onClick.AddListener(() => SetFilter(FilterMode.Locked));

            if (categoryDropdown != null)
            {
                categoryDropdown.ClearOptions();
                List<string> categories = new List<string>();
                foreach (AchievementCategory cat in System.Enum.GetValues(typeof(AchievementCategory)))
                {
                    categories.Add(cat.ToString());
                }
                categoryDropdown.AddOptions(categories);
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            }

            Hide();
        }

        public void Show()
        {
            panel.SetActive(true);
            RefreshDisplay();
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void SetFilter(FilterMode mode)
        {
            currentFilter = mode;
            RefreshDisplay();
        }

        private void OnCategoryChanged(int index)
        {
            currentCategory = (AchievementCategory)index;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (AchievementManager.Instance == null) return;

            // Clear existing items
            foreach (var item in achievementItems)
            {
                Destroy(item);
            }
            achievementItems.Clear();

            // Get achievements based on filter
            List<AchievementDef> achievements = GetFilteredAchievements();

            // Create UI items for each achievement
            foreach (var achievement in achievements)
            {
                CreateAchievementItem(achievement);
            }

            // Update summary
            UpdateSummary();
        }

        private List<AchievementDef> GetFilteredAchievements()
        {
            List<AchievementDef> all = AchievementManager.Instance.GetAll();
            List<AchievementDef> filtered = new List<AchievementDef>();

            foreach (var achievement in all)
            {
                // Category filter
                if (currentCategory != AchievementCategory.General && 
                    achievement.category != currentCategory)
                {
                    continue;
                }

                // Lock state filter
                bool isUnlocked = AchievementManager.Instance.IsUnlocked(achievement.id);
                
                switch (currentFilter)
                {
                    case FilterMode.Unlocked:
                        if (!isUnlocked) continue;
                        break;
                    case FilterMode.Locked:
                        if (isUnlocked) continue;
                        break;
                }

                // Hide hidden achievements if not unlocked
                if (achievement.isHidden && !isUnlocked)
                    continue;

                filtered.Add(achievement);
            }

            return filtered;
        }

        private void CreateAchievementItem(AchievementDef achievement)
        {
            if (achievementItemPrefab == null) return;

            GameObject item = Instantiate(achievementItemPrefab, contentParent);
            achievementItems.Add(item);

            bool isUnlocked = AchievementManager.Instance.IsUnlocked(achievement.id);

            // Setup icon
            Image iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                if (achievement.icon != null)
                {
                    iconImage.sprite = achievement.icon;
                    iconImage.color = isUnlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f);
                }
                else
                {
                    iconImage.color = Color.clear;
                }
            }

            // Setup title
            TextMeshProUGUI titleText = item.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = achievement.title;
                titleText.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            // Setup description
            TextMeshProUGUI descText = item.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = achievement.description;
                descText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Setup reward
            TextMeshProUGUI rewardText = item.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
            if (rewardText != null)
            {
                string rewards = "";
                if (achievement.xpReward > 0)
                    rewards += $"+{achievement.xpReward} XP";
                if (achievement.techPointReward > 0)
                {
                    if (rewards.Length > 0) rewards += " | ";
                    rewards += $"+{achievement.techPointReward} Tech";
                }
                rewardText.text = rewards;
            }

            // Setup unlocked indicator
            GameObject unlockedIndicator = item.transform.Find("UnlockedIndicator")?.gameObject;
            if (unlockedIndicator != null)
            {
                unlockedIndicator.SetActive(isUnlocked);
            }

            GameObject lockedIndicator = item.transform.Find("LockedIndicator")?.gameObject;
            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(!isUnlocked);
            }
        }

        private void UpdateSummary()
        {
            if (AchievementManager.Instance == null) return;

            int unlocked = AchievementManager.Instance.GetUnlockedCount();
            int total = AchievementManager.Instance.GetTotalCount();
            float percent = AchievementManager.Instance.GetCompletionPercent();

            if (summaryText != null)
            {
                summaryText.text = $"Achievements: {unlocked} / {total}";
            }

            if (progressBar != null)
            {
                progressBar.value = percent / 100f;
            }

            if (progressText != null)
            {
                progressText.text = $"{percent:F1}%";
            }
        }

        /// <summary>
        /// Scroll to a specific achievement (e.g., when it's just unlocked)
        /// </summary>
        public void ScrollToAchievement(AchievementId id)
        {
            // Find the achievement in the list and scroll to it
            // Implementation depends on ScrollRect setup
        }
    }
}
