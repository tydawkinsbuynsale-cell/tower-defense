using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Core
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
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button nextButton;

        [Header("Tutorial Target References")]
        [SerializeField] private RectTransform creditsDisplay;
        [SerializeField] private RectTransform livesDisplay;
        [SerializeField] private RectTransform towerButtonPanel;
        [SerializeField] private RectTransform playWaveButton;
        [SerializeField] private GameObject tutorialHandPointer;

        [Header("Settings")]
        [SerializeField] private float arrowBobAmplitude = 15f;
        [SerializeField] private float arrowBobSpeed = 2f;
        [SerializeField] private float overlayAlpha = 0.65f;
        [SerializeField] private bool autoStartOnFirstPlay = true;

        // ── State ────────────────────────────────────────────────────────────

        private List<TutorialStep> steps = new List<TutorialStep>();
        private int currentStep = -1;
        private bool waitingForCondition = false;
        private Vector3 arrowBasePos;
        private bool tutorialActive = false;

        // Tutorial completion flag stored in save data
        private bool IsAlreadyCompleted =>
            SaveManager.Instance != null &&
            SaveManager.Instance.Data.totalWavesCompleted > 0;

        public bool IsTutorialActive => tutorialActive;

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

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => AdvanceStep());
                nextButton.gameObject.SetActive(false);
            }

            if (tutorialHandPointer != null)
                tutorialHandPointer.SetActive(false);

            // Auto-start tutorial for first-time players
            if (autoStartOnFirstPlay && !IsAlreadyCompleted)
            {
                StartCoroutine(DelayedTutorialStart());
            }
        }

        private IEnumerator DelayedTutorialStart()
        {
            // Wait for game to initialize
            yield return new WaitForSeconds(1.5f);

            // Only start if player hasn't completed any waves
            if (!IsAlreadyCompleted)
            {
                StartTutorial();
            }
        }

        private void Update()
        {
            if (currentStep >= 0 && currentStep < steps.Count && waitingForCondition)
            {
                var step = steps[currentStep];
                if (step.completion != null && step.completion())
                {
                    waitingForCondition = false;
                    
                    // Show next button or auto-advance
                    if (step.requiresManualAdvance && nextButton != null)
                    {
                        nextButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        StartCoroutine(DelayedAdvance(1f));
                    }
                }
            }

            // Bob the arrow pointer up and down
            if (arrowPointer != null && arrowPointer.gameObject.activeSelf)
            {
                float offset = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmplitude;
                arrowPointer.anchoredPosition = new Vector2(arrowBasePos.x, arrowBasePos.y + offset);
            }

            // Animate hand pointer (scale pulse)
            if (tutorialHandPointer != null && tutorialHandPointer.activeSelf)
            {
                float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.1f;
                tutorialHandPointer.transform.localScale = Vector3.one * scale;
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void StartTutorial()
        {
            if (tutorialActive || IsAlreadyCompleted)
            {
                Debug.Log("Tutorial already active or completed.");
                return;
            }

            tutorialActive = true;
            currentStep = -1;

            // Set game to Tutorial state (keeps time running for animations)
            GameManager.Instance?.SetGameState(GameManager.GameState.Tutorial);

            BuildSteps();
            tutorialCanvas.SetActive(true);
            
            if (arrowPointer != null)
                arrowBasePos = arrowPointer.anchoredPosition;

            SetOverlayAlpha(overlayAlpha);
            AdvanceStep();

            Debug.Log("Tutorial Started!");
        }

        public void SkipTutorial()
        {
            StopAllCoroutines();
            tutorialCanvas.SetActive(false);
            tutorialActive = false;

            // Resume normal gameplay
            GameManager.Instance?.SetGameState(GameManager.GameState.Playing);

            // Save tutorial completion status
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager != null && saveManager.Data != null)
            {
                saveManager.Data.tutorialCompleted = true;
                saveManager.Save();
            }

            Debug.Log("Tutorial Skipped!");
        }

        /// <summary>
        /// For testing: Force complete the tutorial
        /// </summary>
        public void ForceCompleteTutorial()
        {
            CompleteTutorial();
        }

        // ── Tutorial Steps ───────────────────────────────────────────────────

        private void BuildSteps()
        {
            steps.Clear();

            // Step 1 – Welcome & explain HUD
            steps.Add(new TutorialStep(
                "Welcome, Commander!\n\n" +
                "Your mission: Defend the base against waves of enemy robots!\n\n" +
                "Let's learn the basics...",
                null,
                () => true
            )
            {
                autoAdvanceDelay = 4f,
                requiresManualAdvance = false
            });

            // Step 2 – Credits display
            steps.Add(new TutorialStep(
                "This is your CREDITS counter.\n\n" +
                "You'll need credits to build towers.\n\n" +
                "Earn more by defeating enemies!",
                creditsDisplay,
                () => true
            )
            {
                autoAdvanceDelay = 3.5f,
                requiresManualAdvance = false
            });

            // Step 3 – Lives display
            steps.Add(new TutorialStep(
                "These are your LIVES.\n\n" +
                "If an enemy reaches the end of the path, you lose a life.\n\n" +
                "Don't let them reach zero!",
                livesDisplay,
                () => true
            )
            {
                autoAdvanceDelay = 4f,
                requiresManualAdvance = false
            });

            // Step 4 – Place first tower
            steps.Add(new TutorialStep(
                "Now let's build your first tower!\n\n" +
                "Select a tower from the bottom panel, then tap a valid tile (shown in green) to place it.",
                towerButtonPanel,
                () => Towers.TowerPlacementManager.HasPlacedAnyTower()
            )
            {
                showHandPointer = true,
                requiresManualAdvance = true
            });

            // Step 5 – Start first wave
            steps.Add(new TutorialStep(
                "Excellent! Your tower is ready to defend.\n\n" +
                "Now tap the PLAY button to start the first wave of enemies!",
                playWaveButton,
                () => WaveManager.Instance != null && WaveManager.Instance.IsWaveActive
            )
            {
                showHandPointer = true,
                requiresManualAdvance = true
            });

            // Step 6 – Watch the wave
            steps.Add(new TutorialStep(
                "Great! Your towers will automatically target and shoot enemies.\n\n" +
                "Watch as they defend the path!",
                null,
                () => WaveManager.Instance != null &&
                      !WaveManager.Instance.IsWaveActive &&
                      WaveManager.Instance.CurrentWave >= 1
            )
            {
                autoAdvanceDelay = 2f,
                requiresManualAdvance = false
            });

            // Step 7 – Build more towers
            steps.Add(new TutorialStep(
                "Well done! Between waves, use your earned credits to build MORE towers.\n\n" +
                "More towers = Better defense!",
                towerButtonPanel,
                () => Towers.TowerPlacementManager.HasPlacedAnyTower() &&
                      Towers.TowerPlacementManager.Instance != null &&
                      Towers.TowerPlacementManager.Instance.PlacedTowerCount >= 2
            )
            {
                requiresManualAdvance = true
            });

            // Step 8 – Upgrade towers
            steps.Add(new TutorialStep(
                "Towers can be UPGRADED to deal more damage!\n\n" +
                "Tap any tower, then select UPGRADE to make it stronger.",
                null,
                () => Towers.TowerPlacementManager.HasUpgradedAnyTower()
            )
            {
                requiresManualAdvance = true
            });

            // Step 9 – Final tips
            steps.Add(new TutorialStep(
                "Perfect! You're ready for combat, Commander!\n\n" +
                "TIPS:\n" +
                "• Different towers excel against different enemies\n" +
                "• Speed up gameplay with the 2x/3x buttons\n" +
                "• Upgrade your tech tree for permanent bonuses\n\n" +
                "Good luck defending the core!",
                null,
                () => true
            )
            {
                autoAdvanceDelay = 6f,
                isLastStep = true,
                requiresManualAdvance = false
            });
        }

        // ── Step Flow ────────────────────────────────────────────────────────

        private void AdvanceStep()
        {
            currentStep++;
            waitingForCondition = false;

            if (nextButton != null)
                nextButton.gameObject.SetActive(false);

            if (tutorialHandPointer != null)
                tutorialHandPointer.SetActive(false);

            if (currentStep >= steps.Count)
            {
                CompleteTutorial();
                return;
            }

            var step = steps[currentStep];
            ShowStep(step);

            if (step.autoAdvanceDelay > 0)
            {
                StartCoroutine(AutoAdvance(step.autoAdvanceDelay));
            }
            else
            {
                waitingForCondition = true;
            }
        }

        private IEnumerator DelayedAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceStep();
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceStep();
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

            // Show hand pointer for interactive steps
            if (step.showHandPointer && tutorialHandPointer != null && step.targetTransform != null)
            {
                tutorialHandPointer.SetActive(true);
                tutorialHandPointer.transform.position = step.targetTransform.position;
            }

            // Play audio feedback
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
        }

        private void CompleteTutorial()
        {
            tutorialActive = false;
            tutorialCanvas.SetActive(false);

            if (tutorialHandPointer != null)
                tutorialHandPointer.SetActive(false);

            // Resume normal gameplay
            GameManager.Instance?.SetGameState(GameManager.GameState.Playing);

            // Mark as completed
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager != null && saveManager.Data != null)
            {
                saveManager.Data.tutorialCompleted = true;
                saveManager.Save();
            }

            // Play completion sound
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);

            Debug.Log("Tutorial Completed!");
        }

        // ── UI Helpers ───────────────────────────────────────────────────────

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
                tutorialCanvas.transform as RectTransform,
                screenPt,
                null,
                out Vector2 localPt
            );

            arrowPointer.anchoredPosition = localPt;
            arrowBasePos = localPt;
        }

        private void MoveSpotlightTo(RectTransform target)
        {
            if (spotlightCutout == null || target == null) return;
            
            spotlightCutout.position = target.position;
            spotlightCutout.sizeDelta = target.sizeDelta * 1.2f; // slightly larger than target
           spotlightCutout.gameObject.SetActive(true);
        }

        private void ClearSpotlight()
        {
            if (spotlightCutout != null)
                spotlightCutout.gameObject.SetActive(false);
        }

        // ── Tutorial Step Data ───────────────────────────────────────────────

        [System.Serializable]
        private class TutorialStep
        {
            public string prompt;
            public RectTransform targetTransform;
            public System.Func<bool> completion;
            public float autoAdvanceDelay = 0f;
            public bool isLastStep = false;
            public bool requiresManualAdvance = false;
            public bool showHandPointer = false;

            public TutorialStep(string prompt, RectTransform target, System.Func<bool> completionCheck)
            {
                this.prompt = prompt;
                this.targetTransform = target;
                this.completion = completionCheck;
            }
        }
    }
}
