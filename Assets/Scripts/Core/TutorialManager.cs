using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Tutorial
{
    /// <summary>
    /// Step-based first-time player tutorial.
    /// Displays arrow & highlight mask over target UI elements and waits for
    /// the player to complete each required action.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────

        [Header("UI References")]
        [SerializeField] private Canvas tutorialCanvas;
        [SerializeField] private Image dimOverlay;          // full-screen dark overlay
        [SerializeField] private Image spotlightCutout;     // mask that reveals target area
        [SerializeField] private RectTransform arrowPointer;
        [SerializeField] private Text promptText;
        [SerializeField] private Button skipButton;

        [Header("Settings")]
        [SerializeField] private float arrowBobAmplitude = 15f;
        [SerializeField] private float arrowBobSpeed = 2f;
        [SerializeField] private float overlayAlpha = 0.65f;

        // ── State ────────────────────────────────────────────────────────────

        private List<TutorialStep> steps = new List<TutorialStep>();
        private int currentStep = -1;
        private bool waitingForCondition = false;
        private Vector3 arrowBasePos;

        // Tutorial completion flag stored in save data via AchievementManager
        private bool IsAlreadyCompleted =>
            Core.SaveManager.Instance != null &&
            Core.SaveManager.Instance.Data.totalWavesCompleted > 0;

        // ── Unity ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (tutorialCanvas != null)
                tutorialCanvas.gameObject.SetActive(false);

            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);
        }

        private void Update()
        {
            if (currentStep >= 0 && currentStep < steps.Count && waitingForCondition)
            {
                var step = steps[currentStep];
                if (step.completion != null && step.completion())
                    AdvanceStep();
            }

            // Bob the arrow pointer
            if (arrowPointer != null && arrowPointer.gameObject.activeSelf)
            {
                arrowPointer.anchoredPosition = arrowBasePos +
                    Vector3.up * (Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmplitude);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void StartTutorial()
        {
            if (IsAlreadyCompleted) return;

            BuildSteps();

            if (tutorialCanvas != null)
                tutorialCanvas.gameObject.SetActive(true);

            SetOverlayAlpha(overlayAlpha);
            currentStep = -1;
            AdvanceStep();
        }

        public void SkipTutorial()
        {
            StopAllCoroutines();
            HideTutorialUI();
            Debug.Log("[Tutorial] Skipped.");
        }

        // ── Internal ─────────────────────────────────────────────────────────

        private void BuildSteps()
        {
            steps.Clear();

            // Step 1 – explain the HUD
            steps.Add(new TutorialStep(
                "Welcome, Commander!\nThis is your CREDITS counter. Credits let you build towers.",
                null,     // no specific RectTransform target
                () => true // auto-advance after delay
            ) { autoAdvanceDelay = 3.5f });

            // Step 2 – place first tower
            steps.Add(new TutorialStep(
                "Select a tower from the bottom panel, then tap a BLUE tile to place it!",
                null,
                () => Core.GameManager.Instance != null &&
                      Towers.TowerPlacementManager.HasPlacedAnyTower()
            ));

            // Step 3 – start first wave
            steps.Add(new TutorialStep(
                "Great build! Now tap the PLAY button to send in the first wave of robots.",
                null,
                () => Core.WaveManager.Instance != null &&
                      Core.WaveManager.Instance.IsWaveActive
            ));

            // Step 4 – survive the wave
            steps.Add(new TutorialStep(
                "Your towers will target enemies automatically. Watch your LIVES in the top-left!",
                null,
                () => Core.WaveManager.Instance != null &&
                      !Core.WaveManager.Instance.IsWaveActive &&
                      Core.WaveManager.Instance.CurrentWave >= 1
            ));

            // Step 5 – upgrade a tower
            steps.Add(new TutorialStep(
                "Tap an existing tower to upgrade it — stronger towers survive longer assaults.",
                null,
                () => Towers.TowerPlacementManager.HasUpgradedAnyTower()
            ));

            // Step 6 – done
            steps.Add(new TutorialStep(
                "You're ready, Commander! Defend the core through all waves to win. Good luck!",
                null,
                () => true
            ) { autoAdvanceDelay = 4f, isLastStep = true });
        }

        private void AdvanceStep()
        {
            currentStep++;
            waitingForCondition = false;

            if (currentStep >= steps.Count)
            {
                CompleteTutorial();
                return;
            }

            var step = steps[currentStep];
            ShowStep(step);

            if (step.autoAdvanceDelay > 0)
                StartCoroutine(AutoAdvance(step.autoAdvanceDelay));
            else
                waitingForCondition = true;
        }

        private void ShowStep(TutorialStep step)
        {
            if (promptText != null)
                promptText.text = step.prompt;

            if (step.targetTransform != null)
            {
                PositionArrowAt(step.targetTransform);
                MoveSpotlightTo(step.targetTransform);
                arrowPointer?.gameObject.SetActive(true);
            }
            else
            {
                arrowPointer?.gameObject.SetActive(false);
                ClearSpotlight();
            }
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceStep();
        }

        private void CompleteTutorial()
        {
            HideTutorialUI();
            Debug.Log("[Tutorial] Completed.");
        }

        private void HideTutorialUI()
        {
            currentStep = -1;
            if (tutorialCanvas != null)
                tutorialCanvas.gameObject.SetActive(false);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (dimOverlay != null)
            {
                var c = dimOverlay.color;
                c.a = alpha;
                dimOverlay.color = c;
            }
        }

        private void PositionArrowAt(RectTransform target)
        {
            if (arrowPointer == null || target == null) return;
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                arrowPointer.parent as RectTransform, screenPt, null, out Vector2 local);
            arrowBasePos = local + new Vector2(0, target.rect.height * 0.5f + 40f);
            arrowPointer.anchoredPosition = arrowBasePos;
        }

        private void MoveSpotlightTo(RectTransform target)
        {
            if (spotlightCutout == null || target == null) return;
            spotlightCutout.rectTransform.position = target.position;
            spotlightCutout.rectTransform.sizeDelta = target.sizeDelta + new Vector2(20, 20);
        }

        private void ClearSpotlight()
        {
            if (spotlightCutout != null)
                spotlightCutout.rectTransform.sizeDelta = Vector2.zero;
        }
    }

    // ── Step data ────────────────────────────────────────────────────────────

    public class TutorialStep
    {
        public string prompt;
        public RectTransform targetTransform;
        public System.Func<bool> completion;
        public float autoAdvanceDelay = 0f;
        public bool isLastStep = false;

        public TutorialStep(string prompt, RectTransform target, System.Func<bool> completion)
        {
            this.prompt = prompt;
            this.targetTransform = target;
            this.completion = completion;
        }
    }
}
