using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.UI
{
    /// <summary>
    /// In-game settings panel + main-menu settings screen.
    /// Also contains AchievementToast for notification pop-ups.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle vibrationToggle;

        [Header("Graphics")]
        [SerializeField] private Button lowQualityBtn;
        [SerializeField] private Button medQualityBtn;
        [SerializeField] private Button highQualityBtn;
        [SerializeField] private Color selectedQualityColor = new Color(0.2f, 0.7f, 1f);
        [SerializeField] private Color defaultQualityColor = Color.white;

        [Header("Misc")]
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private GameObject confirmResetPanel;
        [SerializeField] private Button confirmResetYes;
        [SerializeField] private Button confirmResetNo;

        [Header("Back")]
        [SerializeField] private Button backButton;
        [SerializeField] private MainMenuUI mainMenu;   // null if in-game settings

        private void OnEnable()
        {
            LoadCurrentSettings();
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void LoadCurrentSettings()
        {
            var data = Core.SaveManager.Instance?.Data;
            if (data == null) return;

            if (masterSlider != null) masterSlider.value = data.masterVolume;
            if (sfxSlider != null)    sfxSlider.value    = data.sfxVolume;
            if (musicSlider != null)  musicSlider.value  = data.musicVolume;
            if (vibrationToggle != null) vibrationToggle.isOn = data.vibrationEnabled;

            RefreshQualityButtons(data.graphicsQuality);
        }

        private void BindListeners()
        {
            masterSlider?.onValueChanged.AddListener(OnMasterVolumeChanged);
            sfxSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            musicSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            vibrationToggle?.onValueChanged.AddListener(OnVibrationChanged);

            lowQualityBtn?.onClick.AddListener(() => SetQuality(0));
            medQualityBtn?.onClick.AddListener(() => SetQuality(1));
            highQualityBtn?.onClick.AddListener(() => SetQuality(2));

            resetProgressButton?.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
                confirmResetPanel?.SetActive(true);
            });
            confirmResetYes?.onClick.AddListener(OnConfirmReset);
            confirmResetNo?.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
                confirmResetPanel?.SetActive(false);
            });

            backButton?.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
                Core.SaveManager.Instance?.Save();
                mainMenu?.BackToMain();
            });
        }

        private void UnbindListeners()
        {
            masterSlider?.onValueChanged.RemoveAllListeners();
            sfxSlider?.onValueChanged.RemoveAllListeners();
            musicSlider?.onValueChanged.RemoveAllListeners();
            vibrationToggle?.onValueChanged.RemoveAllListeners();
        }

        // ── Volume callbacks ─────────────────────────────────────────────────

        private void OnMasterVolumeChanged(float value)
        {
            Audio.AudioManager.Instance.MasterVolume = value;
            if (Core.SaveManager.Instance != null)
                Core.SaveManager.Instance.Data.masterVolume = value;
        }

        private void OnSFXVolumeChanged(float value)
        {
            Audio.AudioManager.Instance.SFXVolume = value;
            if (Core.SaveManager.Instance != null)
                Core.SaveManager.Instance.Data.sfxVolume = value;
            // Play a quick SFX to preview
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
        }

        private void OnMusicVolumeChanged(float value)
        {
            Audio.AudioManager.Instance.MusicVolume = value;
            if (Core.SaveManager.Instance != null)
                Core.SaveManager.Instance.Data.musicVolume = value;
        }

        private void OnVibrationChanged(bool enabled)
        {
            if (Core.SaveManager.Instance != null)
                Core.SaveManager.Instance.Data.vibrationEnabled = enabled;
        }

        // ── Quality ──────────────────────────────────────────────────────────

        private void SetQuality(int level)
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
            QualitySettings.SetQualityLevel(level, true);

            if (Core.SaveManager.Instance != null)
                Core.SaveManager.Instance.Data.graphicsQuality = level;

            RefreshQualityButtons(level);
        }

        private void RefreshQualityButtons(int active)
        {
            SetButtonColor(lowQualityBtn,  active == 0);
            SetButtonColor(medQualityBtn,  active == 1);
            SetButtonColor(highQualityBtn, active == 2);
        }

        private void SetButtonColor(Button btn, bool isSelected)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isSelected ? selectedQualityColor : defaultQualityColor;
        }

        // ── Reset ────────────────────────────────────────────────────────────

        private void OnConfirmReset()
        {
            Core.SaveManager.Instance?.DeleteSave();
            confirmResetPanel?.SetActive(false);
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
            mainMenu?.BackToMain();
        }
    }

    // ── TechTree UI ──────────────────────────────────────────────────────────

    public class TechTreeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI techPointsText;
        [SerializeField] private TechNodeButton[] nodeButtons;
        [SerializeField] private Button backButton;
        [SerializeField] private MainMenuUI mainMenu;

        private void OnEnable()
        {
            Refresh();
            backButton?.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
                mainMenu?.BackToMain();
            });
        }

        public void Refresh()
        {
            int tp = Core.SaveManager.Instance?.Data.techPoints ?? 0;
            if (techPointsText != null)
                techPointsText.text = $"{tp} Tech Points";

            foreach (var nb in nodeButtons)
                nb?.Refresh();
        }
    }

    [System.Serializable]
    public class TechNodeButton
    {
        public Progression.TechUpgrade upgrade;
        public Button button;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Image progressFill;
        public GameObject maxedOverlay;

        public void Refresh()
        {
            if (Progression.TechTree.Instance == null) return;

            int level = Progression.TechTree.Instance.GetLevel(upgrade);
            int maxLevel = Progression.TechTree.Instance.GetNode(upgrade)?.maxLevel ?? 5;
            int cost = Progression.TechTree.Instance.GetUpgradeCost(upgrade);

            if (levelText != null)   levelText.text = $"Lv {level}/{maxLevel}";
            if (costText != null)    costText.text = Progression.TechTree.Instance.CanUpgrade(upgrade)
                                                        ? $"{cost} TP" : "---";
            if (progressFill != null) progressFill.fillAmount = (float)level / maxLevel;
            if (maxedOverlay != null) maxedOverlay.SetActive(level >= maxLevel);

            if (button != null)
            {
                button.interactable = Progression.TechTree.Instance.CanUpgrade(upgrade);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (Progression.TechTree.Instance.TryUpgrade(upgrade))
                    {
                        Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.TowerUpgrade);
                        Progression.AchievementManager.Instance?.OnTechUpgrade();
                        Refresh();
                    }
                });
            }
        }
    }

    // ── Achievement Toast ─────────────────────────────────────────────────────

    public class AchievementToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine showCoroutine;
        private Queue<Progression.AchievementDef> queue = new Queue<Progression.AchievementDef>();

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public void Show(Progression.AchievementDef def)
        {
            queue.Enqueue(def);
            if (showCoroutine == null)
                showCoroutine = StartCoroutine(ShowNext());
        }

        private IEnumerator ShowNext()
        {
            while (queue.Count > 0)
            {
                var def = queue.Dequeue();

                if (iconImage != null && def.icon != null) iconImage.sprite = def.icon;
                if (titleText != null) titleText.text = $"Achievement: {def.title}";
                if (descriptionText != null) descriptionText.text = def.description;

                // Fade in
                yield return StartCoroutine(Fade(0f, 1f));

                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);

                yield return new WaitForSecondsRealtime(displayDuration);

                // Fade out
                yield return StartCoroutine(Fade(1f, 0f));
            }
            showCoroutine = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (canvasGroup == null) yield break;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }

    // ── Achievements Panel ────────────────────────────────────────────────────

    public class AchievementsUI : MonoBehaviour
    {
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject achievementRowPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private MainMenuUI mainMenu;
        [SerializeField] private TextMeshProUGUI progressText;

        private void OnEnable()
        {
            Build();
            backButton?.onClick.AddListener(() =>
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
                mainMenu?.BackToMain();
            });
        }

        private void Build()
        {
            foreach (Transform child in listContainer) Destroy(child.gameObject);

            var manager = Progression.AchievementManager.Instance;
            if (manager == null) return;

            var all = manager.GetAll();
            int unlocked = 0;

            foreach (var def in all)
            {
                bool earned = manager.IsUnlocked(def.id);
                if (earned) unlocked++;

                GameObject row = Instantiate(achievementRowPrefab, listContainer);
                var icon = row.transform.Find("Icon")?.GetComponent<Image>();
                var title = row.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                var desc = row.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                var check = row.transform.Find("Checkmark")?.gameObject;

                if (icon != null && def.icon != null) icon.sprite = def.icon;
                if (title != null) title.text = def.title;
                if (desc != null)  desc.text  = earned ? def.description : "???";
                if (check != null) check.SetActive(earned);
            }

            if (progressText != null)
                progressText.text = $"{unlocked}/{all.Count} Unlocked";
        }
    }
}
