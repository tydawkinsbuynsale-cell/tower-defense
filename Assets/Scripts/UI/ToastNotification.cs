using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace RobotTD.UI
{
    /// <summary>
    /// Toast notification system for in-game messages.
    /// Shows messages that slide in, stay for a duration, then fade out.
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        public static ToastNotification Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject toastContainer;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Vector2 offScreenPosition = new Vector2(0, -100);
        [SerializeField] private Vector2 onScreenPosition = new Vector2(0, 100);

        [Header("Colors")]
        [SerializeField] private Color infoColor = new Color(0.2f, 0.5f, 0.8f);
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color warningColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color errorColor = new Color(0.9f, 0.2f, 0.2f);

        private RectTransform rectTransform;
        private Queue<ToastMessage> messageQueue = new Queue<ToastMessage>();
        private bool isShowing = false;

        private struct ToastMessage
        {
            public string text;
            public ToastType type;

            public ToastMessage(string text, ToastType type)
            {
                this.text = text;
                this.type = type;
            }
        }

        public enum ToastType
        {
            Info,
            Success,
            Warning,
            Error
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            rectTransform = toastContainer?.GetComponent<RectTransform>();
            if (toastContainer != null)
            {
                toastContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Show a toast message
        /// </summary>
        public void Show(string message, ToastType type = ToastType.Info)
        {
            messageQueue.Enqueue(new ToastMessage(message, type));

            if (!isShowing)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        /// <summary>
        /// Shorthand methods for different toast types
        /// </summary>
        public void ShowInfo(string message) => Show(message, ToastType.Info);
        public void ShowSuccess(string message) => Show(message, ToastType.Success);
        public void ShowWarning(string message) => Show(message, ToastType.Warning);
        public void ShowError(string message) => Show(message, ToastType.Error);

        private IEnumerator ProcessQueue()
        {
            while (messageQueue.Count > 0)
            {
                isShowing = true;
                var msg = messageQueue.Dequeue();

                yield return StartCoroutine(ShowToast(msg));

                // Small delay between toasts
                if (messageQueue.Count > 0)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }

            isShowing = false;
        }

        private IEnumerator ShowToast(ToastMessage message)
        {
            // Setup
            if (messageText != null)
            {
                messageText.text = message.text;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = GetColorForType(message.type);
            }

            if (toastContainer != null)
            {
                toastContainer.SetActive(true);
            }

            // Slide in
            if (rectTransform != null)
            {
                yield return StartCoroutine(SlideIn());
            }

            // Display
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeOut());
            }

            // Hide
            if (toastContainer != null)
            {
                toastContainer.SetActive(false);
            }

            // Reset
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private IEnumerator SlideIn()
        {
            float elapsed = 0f;
            Vector2 startPos = offScreenPosition;
            Vector2 endPos = onScreenPosition;

            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / slideInDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothT);
                }

                yield return null;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = endPos;
            }
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private Color GetColorForType(ToastType type)
        {
            return type switch
            {
                ToastType.Success => successColor,
                ToastType.Warning => warningColor,
                ToastType.Error => errorColor,
                _ => infoColor
            };
        }
    }
}
