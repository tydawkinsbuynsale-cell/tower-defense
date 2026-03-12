using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// Reusable confirmation dialog for important actions.
    /// Shows title, message, and Yes/No buttons.
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        public static ConfirmationDialog Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TextMeshProUGUI yesButtonText;
        [SerializeField] private TextMeshProUGUI noButtonText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 8f;

        private Action onConfirm;
        private Action onCancel;
        private bool isShowing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Setup button listeners
            yesButton?.onClick.AddListener(OnYesClicked);
            noButton?.onClick.AddListener(OnNoClicked);

            Hide();
        }

        private void Update()
        {
            // Fade animation
            if (canvasGroup != null)
            {
                float targetAlpha = isShowing ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);

                if (!isShowing && canvasGroup.alpha <= 0.01f)
                {
                    dialogPanel?.SetActive(false);
                }
            }

            // ESC to cancel
            if (isShowing && Input.GetKeyDown(KeyCode.Escape))
            {
                OnNoClicked();
            }
        }

        /// <summary>
        /// Show confirmation dialog with custom message and callbacks
        /// </summary>
        public void Show(string title, string message, Action onConfirm, Action onCancel = null, 
                         string yesText = "Yes", string noText = "No")
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (yesButtonText != null)
            {
                yesButtonText.text = yesText;
            }

            if (noButtonText != null)
            {
                noButtonText.text = noText;
            }

            isShowing = true;
            dialogPanel?.SetActive(true);

            // Pause game if instance exists
            if (Core.GameManager.Instance != null && 
                Core.GameManager.Instance.CurrentState == Core.GameManager.GameState.Playing)
            {
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// Hide the dialog
        /// </summary>
        public void Hide()
        {
            isShowing = false;

            // Resume game if it was paused by us
            if (Time.timeScale == 0f && Core.GameManager.Instance != null &&
                Core.GameManager.Instance.CurrentState != Core.GameManager.GameState.Paused)
            {
                Time.timeScale = 1f;
            }
        }

        private void OnYesClicked()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
            Hide();
            onConfirm?.Invoke();
        }

        private void OnNoClicked()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            Hide();
            onCancel?.Invoke();
        }

        // ── Quick show methods ───────────────────────────────────────────────

        /// <summary>
        /// Show simple yes/no confirmation
        /// </summary>
        public static void ShowConfirm(string message, Action onConfirm, Action onCancel = null)
        {
            Instance?.Show("Confirm", message, onConfirm, onCancel);
        }

        /// <summary>
        /// Show confirmation for destructive action
        /// </summary>
        public static void ShowWarning(string message, Action onConfirm, Action onCancel = null)
        {
            Instance?.Show("Warning", message, onConfirm, onCancel, "Delete", "Cancel");
        }

        /// <summary>
        /// Show restart confirmation
        /// </summary>
        public static void ShowRestart(Action onConfirm, Action onCancel = null)
        {
            Instance?.Show("Restart Game", 
                "Are you sure you want to restart? All progress in this level will be lost.", 
                onConfirm, onCancel, "Restart", "Cancel");
        }

        /// <summary>
        /// Show quit to main menu confirmation
        /// </summary>
        public static void ShowQuit(Action onConfirm, Action onCancel = null)
        {
            Instance?.Show("Return to Main Menu", 
                "Are you sure? Unsaved progress will be lost.", 
                onConfirm, onCancel, "Quit", "Cancel");
        }
    }
}
