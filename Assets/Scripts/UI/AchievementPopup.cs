using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using RobotTD.Progression;

namespace RobotTD.UI
{
    /// <summary>
    /// Standalone achievement notification popup system.
    /// Shows achievement unlocks with animations and queuing.
    /// </summary>
    public class AchievementPopup : MonoBehaviour
    {
        public static AchievementPopup Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private GameObject shineEffect;

        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float displayDuration = 4f;
        [SerializeField] private float slideOutDuration = 0.4f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Position Settings")]
        [SerializeField] private Vector2 hiddenPosition = new Vector2(0, 200);
        [SerializeField] private Vector2 visiblePosition = new Vector2(0, -100);

        [Header("Colors")]
        [SerializeField] private Color defaultBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        [SerializeField] private Color rareBackgroundColor = new Color(0.3f, 0.2f, 0.5f, 0.95f);
        [SerializeField] private Color epicBackgroundColor = new Color(0.5f, 0.3f, 0.1f, 0.95f);

        [Header("Audio")]
        [SerializeField] private AudioClip achievementSound;

        private Queue<AchievementDef> achievementQueue = new Queue<AchievementDef>();
        private Coroutine displayCoroutine;
        private RectTransform popupRect;
        private bool isShowing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            popupRect = popupPanel.GetComponent<RectTransform>();
            Hide(instant: true);
        }

        /// <summary>
        /// Show an achievement notification
        /// </summary>
        public void ShowAchievement(AchievementDef achievement)
        {
            if (achievement == null) return;

            achievementQueue.Enqueue(achievement);

            if (!isShowing && displayCoroutine == null)
            {
                displayCoroutine = StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            while (achievementQueue.Count > 0)
            {
                AchievementDef achievement = achievementQueue.Dequeue();
                yield return StartCoroutine(ShowAchievementInternal(achievement));
            }

            displayCoroutine = null;
        }

        private IEnumerator ShowAchievementInternal(AchievementDef achievement)
        {
            isShowing = true;

            // Setup UI elements
            SetupAchievementUI(achievement);

            // Slide in
            yield return StartCoroutine(SlideIn());

            // Play sound
            if (achievementSound != null)
            {
                Audio.AudioManager.Instance?.PlaySFX(achievementSound);
            }
            else
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
            }

            // Activate shine effect
            if (shineEffect != null)
            {
                shineEffect.SetActive(true);
            }

            // Display
            yield return new WaitForSecondsRealtime(displayDuration);

            // Deactivate shine
            if (shineEffect != null)
            {
                shineEffect.SetActive(false);
            }

            // Slide out
            yield return StartCoroutine(SlideOut());

            isShowing = false;
        }

        private void SetupAchievementUI(AchievementDef achievement)
        {
            // Set icon
            if (iconImage != null)
            {
                if (achievement.icon != null)
                {
                    iconImage.sprite = achievement.icon;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.color = Color.clear;
                }
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = achievement.title;
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = achievement.description;
            }

            // Set reward text
            if (rewardText != null)
            {
                string rewards = "";
                if (achievement.xpReward > 0)
                {
                    rewards += $"+{achievement.xpReward} XP";
                }
                if (achievement.techPointReward > 0)
                {
                    if (rewards.Length > 0) rewards += " | ";
                    rewards += $"+{achievement.techPointReward} Tech Points";
                }
                rewardText.text = rewards;
            }

            // Set background color based on rarity/type
            if (backgroundImage != null)
            {
                if (achievement.techPointReward > 0)
                {
                    backgroundImage.color = epicBackgroundColor;
                }
                else if (achievement.xpReward >= 100)
                {
                    backgroundImage.color = rareBackgroundColor;
                }
                else
                {
                    backgroundImage.color = defaultBackgroundColor;
                }
            }
        }

        private IEnumerator SlideIn()
        {
            popupPanel.SetActive(true);
            
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = slideInCurve.Evaluate(elapsed / slideInDuration);

                // Slide from top
                popupRect.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, t);

                // Fade in
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                }

                yield return null;
            }

            popupRect.anchoredPosition = visiblePosition;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private IEnumerator SlideOut()
        {
            float elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = slideOutCurve.Evaluate(elapsed / slideOutDuration);

                // Slide back up
                popupRect.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, t);

                // Fade out
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                yield return null;
            }

            popupRect.anchoredPosition = hiddenPosition;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            popupPanel.SetActive(false);
        }

        private void Hide(bool instant = false)
        {
            if (instant)
            {
                popupRect.anchoredPosition = hiddenPosition;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }
                popupPanel.SetActive(false);
            }
            else
            {
                if (displayCoroutine != null)
                {
                    StopCoroutine(displayCoroutine);
                }
                StartCoroutine(SlideOut());
            }
        }

        /// <summary>
        /// Clear the queue (e.g., when changing scenes)
        /// </summary>
        public void ClearQueue()
        {
            achievementQueue.Clear();
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }
            Hide(instant: true);
            isShowing = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
