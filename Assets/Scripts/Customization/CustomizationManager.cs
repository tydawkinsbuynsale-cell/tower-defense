using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Analytics;

namespace RobotTD.Customization
{
    /// <summary>
    /// Advanced customization system for cosmetic personalization.
    /// Handles tower skins, map themes, UI themes, and player profile customization.
    /// Features unlocking, equipping, previewing, and collection tracking.
    /// </summary>
    public class CustomizationManager : MonoBehaviour
    {
        public static CustomizationManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableCustomization = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("Tower Skins")]
        [SerializeField] private TowerSkinData[] towerSkins;

        [Header("Map Themes")]
        [SerializeField] private MapThemeData[] mapThemes;

        [Header("UI Themes")]
        [SerializeField] private UIThemeData[] uiThemes;

        [Header("Profile Customization")]
        [SerializeField] private AvatarData[] avatars;
        [SerializeField] private BannerData[] banners;
        [SerializeField] private TitleData[] titles;

        // State
        private bool isInitialized = false;
        private HashSet<string> unlockedTowerSkins = new HashSet<string>();
        private HashSet<string> unlockedMapThemes = new HashSet<string>();
        private HashSet<string> unlockedUIThemes = new HashSet<string>();
        private HashSet<string> unlockedAvatars = new HashSet<string>();
        private HashSet<string> unlockedBanners = new HashSet<string>();
        private HashSet<string> unlockedTitles = new HashSet<string>();

        private Dictionary<string, string> equippedTowerSkins = new Dictionary<string, string>(); // towerId -> skinId
        private string equippedMapTheme = "default";
        private string equippedUITheme = "default";
        private string equippedAvatar = "default";
        private string equippedBanner = "default";
        private string equippedTitle = "none";

        private UIThemeData currentUITheme;

        // Events
        public event Action<TowerSkinData> OnTowerSkinUnlocked;
        public event Action<string, string> OnTowerSkinEquipped; // towerId, skinId
        public event Action<MapThemeData> OnMapThemeUnlocked;
        public event Action<string> OnMapThemeChanged;
        public event Action<UIThemeData> OnUIThemeUnlocked;
        public event Action<UIThemeData> OnUIThemeChanged;
        public event Action<AvatarData> OnAvatarUnlocked;
        public event Action<string> OnAvatarChanged;
        public event Action<BannerData> OnBannerUnlocked;
        public event Action<string> OnBannerChanged;
        public event Action<TitleData> OnTitleUnlocked;
        public event Action<string> OnTitleChanged;

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
            if (!enableCustomization)
            {
                LogDebug("Customization system disabled");
                return;
            }

            StartCoroutine(InitializeCustomizationSystem());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeCustomizationSystem()
        {
            LogDebug("Initializing customization system...");

            yield return new WaitForSeconds(0.3f);

            // Load unlocked items
            LoadUnlockedItems();

            // Load equipped items
            LoadEquippedItems();

            // Apply current UI theme
            ApplyUITheme(equippedUITheme);

            // Unlock default items if first launch
            if (unlockedTowerSkins.Count == 0)
            {
                UnlockDefaultItems();
            }

            isInitialized = true;
            LogDebug("Customization system initialized");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("customization_system_initialized", new Dictionary<string, object>
                {
                    { "unlocked_tower_skins", unlockedTowerSkins.Count },
                    { "unlocked_map_themes", unlockedMapThemes.Count },
                    { "unlocked_ui_themes", unlockedUIThemes.Count },
                    { "unlocked_avatars", unlockedAvatars.Count },
                    { "unlocked_banners", unlockedBanners.Count },
                    { "unlocked_titles", unlockedTitles.Count },
                    { "equipped_ui_theme", equippedUITheme },
                    { "equipped_avatar", equippedAvatar }
                });
            }
        }

        private void UnlockDefaultItems()
        {
            // Unlock default skins for all towers
            foreach (var skin in towerSkins)
            {
                if (skin.isDefault)
                {
                    UnlockTowerSkin(skin.skinId, false);
                }
            }

            // Unlock default themes
            foreach (var theme in mapThemes)
            {
                if (theme.isDefault)
                {
                    UnlockMapTheme(theme.themeId, false);
                }
            }

            foreach (var theme in uiThemes)
            {
                if (theme.isDefault)
                {
                    UnlockUITheme(theme.themeId, false);
                }
            }

            // Unlock default avatar and banner
            foreach (var avatar in avatars)
            {
                if (avatar.isDefault)
                {
                    UnlockAvatar(avatar.avatarId, false);
                }
            }

            foreach (var banner in banners)
            {
                if (banner.isDefault)
                {
                    UnlockBanner(banner.bannerId, false);
                }
            }

            SaveUnlockedItems();
            LogDebug("Default items unlocked");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Tower Skin System ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a tower skin.
        /// </summary>
        public bool UnlockTowerSkin(string skinId, bool saveImmediately = true)
        {
            if (unlockedTowerSkins.Contains(skinId))
            {
                LogDebug($"Tower skin already unlocked: {skinId}");
                return false;
            }

            unlockedTowerSkins.Add(skinId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var skinData = GetTowerSkinData(skinId);
            if (skinData != null)
            {
                OnTowerSkinUnlocked?.Invoke(skinData);
                LogDebug($"Unlocked tower skin: {skinData.skinName} ({skinId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("tower_skin_unlocked", new Dictionary<string, object>
                    {
                        { "skin_id", skinId },
                        { "skin_name", skinData.skinName },
                        { "rarity", skinData.rarity.ToString() },
                        { "tower_type", skinData.towerType }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips a tower skin for a specific tower type.
        /// </summary>
        public bool EquipTowerSkin(string towerType, string skinId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedTowerSkins.Contains(skinId) && skinId != "default")
            {
                LogDebug($"Tower skin not unlocked: {skinId}");
                return false;
            }

            var skinData = GetTowerSkinData(skinId);
            if (skinData != null && skinData.towerType != towerType)
            {
                LogDebug($"Skin {skinId} not compatible with tower type {towerType}");
                return false;
            }

            equippedTowerSkins[towerType] = skinId;
            SaveEquippedItems();

            OnTowerSkinEquipped?.Invoke(towerType, skinId);
            LogDebug($"Equipped tower skin: {towerType} -> {skinId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("tower_skin_equipped", new Dictionary<string, object>
                {
                    { "tower_type", towerType },
                    { "skin_id", skinId }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets the equipped skin for a tower type.
        /// </summary>
        public string GetEquippedTowerSkin(string towerType)
        {
            return equippedTowerSkins.ContainsKey(towerType) ? equippedTowerSkins[towerType] : "default";
        }

        /// <summary>
        /// Gets all unlocked tower skins.
        /// </summary>
        public List<TowerSkinData> GetUnlockedTowerSkins()
        {
            return towerSkins.Where(skin => unlockedTowerSkins.Contains(skin.skinId)).ToList();
        }

        /// <summary>
        /// Gets all tower skins for a specific tower type.
        /// </summary>
        public List<TowerSkinData> GetTowerSkinsForType(string towerType)
        {
            return towerSkins.Where(skin => skin.towerType == towerType).ToList();
        }

        /// <summary>
        /// Checks if a tower skin is unlocked.
        /// </summary>
        public bool IsTowerSkinUnlocked(string skinId)
        {
            return unlockedTowerSkins.Contains(skinId);
        }

        private TowerSkinData GetTowerSkinData(string skinId)
        {
            return towerSkins.FirstOrDefault(skin => skin.skinId == skinId);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Map Theme System ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a map theme.
        /// </summary>
        public bool UnlockMapTheme(string themeId, bool saveImmediately = true)
        {
            if (unlockedMapThemes.Contains(themeId))
            {
                LogDebug($"Map theme already unlocked: {themeId}");
                return false;
            }

            unlockedMapThemes.Add(themeId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var themeData = GetMapThemeData(themeId);
            if (themeData != null)
            {
                OnMapThemeUnlocked?.Invoke(themeData);
                LogDebug($"Unlocked map theme: {themeData.themeName} ({themeId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("map_theme_unlocked", new Dictionary<string, object>
                    {
                        { "theme_id", themeId },
                        { "theme_name", themeData.themeName }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips a map theme.
        /// </summary>
        public bool EquipMapTheme(string themeId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedMapThemes.Contains(themeId) && themeId != "default")
            {
                LogDebug($"Map theme not unlocked: {themeId}");
                return false;
            }

            equippedMapTheme = themeId;
            SaveEquippedItems();

            OnMapThemeChanged?.Invoke(themeId);
            LogDebug($"Equipped map theme: {themeId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("map_theme_changed", new Dictionary<string, object>
                {
                    { "theme_id", themeId }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets the currently equipped map theme.
        /// </summary>
        public string GetEquippedMapTheme()
        {
            return equippedMapTheme;
        }

        /// <summary>
        /// Gets all unlocked map themes.
        /// </summary>
        public List<MapThemeData> GetUnlockedMapThemes()
        {
            return mapThemes.Where(theme => unlockedMapThemes.Contains(theme.themeId)).ToList();
        }

        /// <summary>
        /// Checks if a map theme is unlocked.
        /// </summary>
        public bool IsMapThemeUnlocked(string themeId)
        {
            return unlockedMapThemes.Contains(themeId);
        }

        private MapThemeData GetMapThemeData(string themeId)
        {
            return mapThemes.FirstOrDefault(theme => theme.themeId == themeId);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── UI Theme System ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a UI theme.
        /// </summary>
        public bool UnlockUITheme(string themeId, bool saveImmediately = true)
        {
            if (unlockedUIThemes.Contains(themeId))
            {
                LogDebug($"UI theme already unlocked: {themeId}");
                return false;
            }

            unlockedUIThemes.Add(themeId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var themeData = GetUIThemeData(themeId);
            if (themeData != null)
            {
                OnUIThemeUnlocked?.Invoke(themeData);
                LogDebug($"Unlocked UI theme: {themeData.themeName} ({themeId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("ui_theme_unlocked", new Dictionary<string, object>
                    {
                        { "theme_id", themeId },
                        { "theme_name", themeData.themeName },
                        { "theme_type", themeData.themeType.ToString() }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips a UI theme and applies it immediately.
        /// </summary>
        public bool EquipUITheme(string themeId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedUIThemes.Contains(themeId) && themeId != "default")
            {
                LogDebug($"UI theme not unlocked: {themeId}");
                return false;
            }

            equippedUITheme = themeId;
            SaveEquippedItems();

            ApplyUITheme(themeId);

            LogDebug($"Equipped UI theme: {themeId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("ui_theme_changed", new Dictionary<string, object>
                {
                    { "theme_id", themeId }
                });
            }

            return true;
        }

        /// <summary>
        /// Applies a UI theme to the game.
        /// </summary>
        private void ApplyUITheme(string themeId)
        {
            var themeData = GetUIThemeData(themeId);
            if (themeData == null)
            {
                LogDebug($"UI theme not found: {themeId}");
                return;
            }

            currentUITheme = themeData;
            OnUIThemeChanged?.Invoke(themeData);

            LogDebug($"Applied UI theme: {themeData.themeName}");
        }

        /// <summary>
        /// Gets the currently equipped UI theme.
        /// </summary>
        public string GetEquippedUITheme()
        {
            return equippedUITheme;
        }

        /// <summary>
        /// Gets the current UI theme data.
        /// </summary>
        public UIThemeData GetCurrentUIThemeData()
        {
            return currentUITheme;
        }

        /// <summary>
        /// Gets all unlocked UI themes.
        /// </summary>
        public List<UIThemeData> GetUnlockedUIThemes()
        {
            return uiThemes.Where(theme => unlockedUIThemes.Contains(theme.themeId)).ToList();
        }

        /// <summary>
        /// Checks if a UI theme is unlocked.
        /// </summary>
        public bool IsUIThemeUnlocked(string themeId)
        {
            return unlockedUIThemes.Contains(themeId);
        }

        private UIThemeData GetUIThemeData(string themeId)
        {
            return uiThemes.FirstOrDefault(theme => theme.themeId == themeId);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Player Profile System ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks an avatar.
        /// </summary>
        public bool UnlockAvatar(string avatarId, bool saveImmediately = true)
        {
            if (unlockedAvatars.Contains(avatarId))
            {
                LogDebug($"Avatar already unlocked: {avatarId}");
                return false;
            }

            unlockedAvatars.Add(avatarId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var avatarData = GetAvatarData(avatarId);
            if (avatarData != null)
            {
                OnAvatarUnlocked?.Invoke(avatarData);
                LogDebug($"Unlocked avatar: {avatarData.avatarName} ({avatarId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("avatar_unlocked", new Dictionary<string, object>
                    {
                        { "avatar_id", avatarId },
                        { "avatar_name", avatarData.avatarName }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips an avatar.
        /// </summary>
        public bool EquipAvatar(string avatarId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedAvatars.Contains(avatarId) && avatarId != "default")
            {
                LogDebug($"Avatar not unlocked: {avatarId}");
                return false;
            }

            equippedAvatar = avatarId;
            SaveEquippedItems();

            OnAvatarChanged?.Invoke(avatarId);
            LogDebug($"Equipped avatar: {avatarId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("avatar_changed", new Dictionary<string, object>
                {
                    { "avatar_id", avatarId }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets the currently equipped avatar.
        /// </summary>
        public string GetEquippedAvatar()
        {
            return equippedAvatar;
        }

        /// <summary>
        /// Gets all unlocked avatars.
        /// </summary>
        public List<AvatarData> GetUnlockedAvatars()
        {
            return avatars.Where(avatar => unlockedAvatars.Contains(avatar.avatarId)).ToList();
        }

        /// <summary>
        /// Checks if an avatar is unlocked.
        /// </summary>
        public bool IsAvatarUnlocked(string avatarId)
        {
            return unlockedAvatars.Contains(avatarId);
        }

        private AvatarData GetAvatarData(string avatarId)
        {
            return avatars.FirstOrDefault(avatar => avatar.avatarId == avatarId);
        }

        /// <summary>
        /// Unlocks a banner.
        /// </summary>
        public bool UnlockBanner(string bannerId, bool saveImmediately = true)
        {
            if (unlockedBanners.Contains(bannerId))
            {
                LogDebug($"Banner already unlocked: {bannerId}");
                return false;
            }

            unlockedBanners.Add(bannerId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var bannerData = GetBannerData(bannerId);
            if (bannerData != null)
            {
                OnBannerUnlocked?.Invoke(bannerData);
                LogDebug($"Unlocked banner: {bannerData.bannerName} ({bannerId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("banner_unlocked", new Dictionary<string, object>
                    {
                        { "banner_id", bannerId },
                        { "banner_name", bannerData.bannerName }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips a banner.
        /// </summary>
        public bool EquipBanner(string bannerId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedBanners.Contains(bannerId) && bannerId != "default")
            {
                LogDebug($"Banner not unlocked: {bannerId}");
                return false;
            }

            equippedBanner = bannerId;
            SaveEquippedItems();

            OnBannerChanged?.Invoke(bannerId);
            LogDebug($"Equipped banner: {bannerId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("banner_changed", new Dictionary<string, object>
                {
                    { "banner_id", bannerId }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets the currently equipped banner.
        /// </summary>
        public string GetEquippedBanner()
        {
            return equippedBanner;
        }

        /// <summary>
        /// Gets all unlocked banners.
        /// </summary>
        public List<BannerData> GetUnlockedBanners()
        {
            return banners.Where(banner => unlockedBanners.Contains(banner.bannerId)).ToList();
        }

        /// <summary>
        /// Checks if a banner is unlocked.
        /// </summary>
        public bool IsBannerUnlocked(string bannerId)
        {
            return unlockedBanners.Contains(bannerId);
        }

        private BannerData GetBannerData(string bannerId)
        {
            return banners.FirstOrDefault(banner => banner.bannerId == bannerId);
        }

        /// <summary>
        /// Unlocks a title.
        /// </summary>
        public bool UnlockTitle(string titleId, bool saveImmediately = true)
        {
            if (unlockedTitles.Contains(titleId))
            {
                LogDebug($"Title already unlocked: {titleId}");
                return false;
            }

            unlockedTitles.Add(titleId);

            if (saveImmediately)
            {
                SaveUnlockedItems();
            }

            var titleData = GetTitleData(titleId);
            if (titleData != null)
            {
                OnTitleUnlocked?.Invoke(titleData);
                LogDebug($"Unlocked title: {titleData.titleText} ({titleId})");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("title_unlocked", new Dictionary<string, object>
                    {
                        { "title_id", titleId },
                        { "title_text", titleData.titleText }
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// Equips a title.
        /// </summary>
        public bool EquipTitle(string titleId)
        {
            if (!isInitialized)
            {
                LogDebug("Customization system not initialized");
                return false;
            }

            if (!unlockedTitles.Contains(titleId) && titleId != "none")
            {
                LogDebug($"Title not unlocked: {titleId}");
                return false;
            }

            equippedTitle = titleId;
            SaveEquippedItems();

            OnTitleChanged?.Invoke(titleId);
            LogDebug($"Equipped title: {titleId}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("title_changed", new Dictionary<string, object>
                {
                    { "title_id", titleId }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets the currently equipped title.
        /// </summary>
        public string GetEquippedTitle()
        {
            return equippedTitle;
        }

        /// <summary>
        /// Gets all unlocked titles.
        /// </summary>
        public List<TitleData> GetUnlockedTitles()
        {
            return titles.Where(title => unlockedTitles.Contains(title.titleId)).ToList();
        }

        /// <summary>
        /// Checks if a title is unlocked.
        /// </summary>
        public bool IsTitleUnlocked(string titleId)
        {
            return unlockedTitles.Contains(titleId);
        }

        private TitleData GetTitleData(string titleId)
        {
            return titles.FirstOrDefault(title => title.titleId == titleId);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Collection Statistics ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets collection completion percentage.
        /// </summary>
        public float GetCollectionCompletion()
        {
            int totalItems = towerSkins.Length + mapThemes.Length + uiThemes.Length + 
                             avatars.Length + banners.Length + titles.Length;
            int unlockedItems = unlockedTowerSkins.Count + unlockedMapThemes.Count + 
                                unlockedUIThemes.Count + unlockedAvatars.Count + 
                                unlockedBanners.Count + unlockedTitles.Count;

            return totalItems > 0 ? (float)unlockedItems / totalItems : 0f;
        }

        /// <summary>
        /// Gets collection statistics by category.
        /// </summary>
        public CollectionStats GetCollectionStats()
        {
            return new CollectionStats
            {
                towerSkinsUnlocked = unlockedTowerSkins.Count,
                towerSkinsTotal = towerSkins.Length,
                mapThemesUnlocked = unlockedMapThemes.Count,
                mapThemesTotal = mapThemes.Length,
                uiThemesUnlocked = unlockedUIThemes.Count,
                uiThemesTotal = uiThemes.Length,
                avatarsUnlocked = unlockedAvatars.Count,
                avatarsTotal = avatars.Length,
                bannersUnlocked = unlockedBanners.Count,
                bannersTotal = banners.Length,
                titlesUnlocked = unlockedTitles.Count,
                titlesTotal = titles.Length
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadUnlockedItems()
        {
            string json = PlayerPrefs.GetString("UnlockedCustomization", "{}");
            try
            {
                var data = JsonUtility.FromJson<UnlockedItemsData>(json);
                if (data != null)
                {
                    unlockedTowerSkins = new HashSet<string>(data.towerSkins ?? new string[0]);
                    unlockedMapThemes = new HashSet<string>(data.mapThemes ?? new string[0]);
                    unlockedUIThemes = new HashSet<string>(data.uiThemes ?? new string[0]);
                    unlockedAvatars = new HashSet<string>(data.avatars ?? new string[0]);
                    unlockedBanners = new HashSet<string>(data.banners ?? new string[0]);
                    unlockedTitles = new HashSet<string>(data.titles ?? new string[0]);

                    LogDebug($"Loaded {unlockedTowerSkins.Count} tower skins, {unlockedMapThemes.Count} map themes, " +
                             $"{unlockedUIThemes.Count} UI themes, {unlockedAvatars.Count} avatars, " +
                             $"{unlockedBanners.Count} banners, {unlockedTitles.Count} titles");
                }
            }
            catch
            {
                LogDebug("No unlocked customization data found");
            }
        }

        private void SaveUnlockedItems()
        {
            var data = new UnlockedItemsData
            {
                towerSkins = unlockedTowerSkins.ToArray(),
                mapThemes = unlockedMapThemes.ToArray(),
                uiThemes = unlockedUIThemes.ToArray(),
                avatars = unlockedAvatars.ToArray(),
                banners = unlockedBanners.ToArray(),
                titles = unlockedTitles.ToArray()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("UnlockedCustomization", json);
            PlayerPrefs.Save();
        }

        private void LoadEquippedItems()
        {
            string json = PlayerPrefs.GetString("EquippedCustomization", "{}");
            try
            {
                var data = JsonUtility.FromJson<EquippedItemsData>(json);
                if (data != null)
                {
                    equippedMapTheme = data.mapTheme ?? "default";
                    equippedUITheme = data.uiTheme ?? "default";
                    equippedAvatar = data.avatar ?? "default";
                    equippedBanner = data.banner ?? "default";
                    equippedTitle = data.title ?? "none";

                    if (data.towerSkinKeys != null && data.towerSkinValues != null)
                    {
                        equippedTowerSkins.Clear();
                        for (int i = 0; i < data.towerSkinKeys.Length && i < data.towerSkinValues.Length; i++)
                        {
                            equippedTowerSkins[data.towerSkinKeys[i]] = data.towerSkinValues[i];
                        }
                    }

                    LogDebug($"Loaded equipped items: Map={equippedMapTheme}, UI={equippedUITheme}, Avatar={equippedAvatar}");
                }
            }
            catch
            {
                LogDebug("No equipped customization data found");
            }
        }

        private void SaveEquippedItems()
        {
            var data = new EquippedItemsData
            {
                towerSkinKeys = equippedTowerSkins.Keys.ToArray(),
                towerSkinValues = equippedTowerSkins.Values.ToArray(),
                mapTheme = equippedMapTheme,
                uiTheme = equippedUITheme,
                avatar = equippedAvatar,
                banner = equippedBanner,
                title = equippedTitle
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("EquippedCustomization", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[CustomizationManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tower skin cosmetic data.
    /// </summary>
    [Serializable]
    public class TowerSkinData
    {
        public string skinId;
        public string skinName;
        public string towerType;
        public SkinRarity rarity = SkinRarity.Common;
        public Sprite previewImage;
        public GameObject skinPrefab;
        public bool isDefault = false;
        public string unlockRequirement; // e.g., "achievement_xxx", "level_10", "purchase"
    }

    /// <summary>
    /// Skin rarity levels.
    /// </summary>
    public enum SkinRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Map theme visual pack.
    /// </summary>
    [Serializable]
    public class MapThemeData
    {
        public string themeId;
        public string themeName;
        public string description;
        public Sprite previewImage;
        public Material groundMaterial;
        public Material pathMaterial;
        public Color ambientColor;
        public bool isDefault = false;
        public string unlockRequirement;
    }

    /// <summary>
    /// UI theme color scheme.
    /// </summary>
    [Serializable]
    public class UIThemeData
    {
        public string themeId;
        public string themeName;
        public UIThemeType themeType = UIThemeType.Standard;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
        public Color backgroundColor;
        public Color textColor;
        public Color buttonColor;
        public bool isDefault = false;
        public string unlockRequirement;
    }

    /// <summary>
    /// UI theme types.
    /// </summary>
    public enum UIThemeType
    {
        Standard,
        DarkMode,
        Protanopia,      // Red-blind
        Deuteranopia,    // Green-blind
        Tritanopia,      // Blue-blind
        HighContrast
    }

    /// <summary>
    /// Player avatar data.
    /// </summary>
    [Serializable]
    public class AvatarData
    {
        public string avatarId;
        public string avatarName;
        public Sprite avatarImage;
        public bool isDefault = false;
        public string unlockRequirement;
    }

    /// <summary>
    /// Player banner data.
    /// </summary>
    [Serializable]
    public class BannerData
    {
        public string bannerId;
        public string bannerName;
        public Sprite bannerImage;
        public bool isDefault = false;
        public string unlockRequirement;
    }

    /// <summary>
    /// Player title data.
    /// </summary>
    [Serializable]
    public class TitleData
    {
        public string titleId;
        public string titleText;
        public Color titleColor = Color.white;
        public bool isDefault = false;
        public string unlockRequirement;
    }

    /// <summary>
    /// Collection statistics.
    /// </summary>
    [Serializable]
    public class CollectionStats
    {
        public int towerSkinsUnlocked;
        public int towerSkinsTotal;
        public int mapThemesUnlocked;
        public int mapThemesTotal;
        public int uiThemesUnlocked;
        public int uiThemesTotal;
        public int avatarsUnlocked;
        public int avatarsTotal;
        public int bannersUnlocked;
        public int bannersTotal;
        public int titlesUnlocked;
        public int titlesTotal;
    }

    /// <summary>
    /// Storage structure for unlocked items.
    /// </summary>
    [Serializable]
    public class UnlockedItemsData
    {
        public string[] towerSkins;
        public string[] mapThemes;
        public string[] uiThemes;
        public string[] avatars;
        public string[] banners;
        public string[] titles;
    }

    /// <summary>
    /// Storage structure for equipped items.
    /// </summary>
    [Serializable]
    public class EquippedItemsData
    {
        public string[] towerSkinKeys;
        public string[] towerSkinValues;
        public string mapTheme;
        public string uiTheme;
        public string avatar;
        public string banner;
        public string title;
    }
}
