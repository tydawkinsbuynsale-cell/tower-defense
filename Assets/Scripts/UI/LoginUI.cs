using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace RobotTD.UI
{
    /// <summary>
    /// Login/Sign-in UI panel with anonymous, email, and device ID authentication.
    /// Works with AuthenticationManager for player authentication.
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;

        [Header("Login Forms")]
        [SerializeField] private GameObject anonymousLoginPanel;
        [SerializeField] private GameObject emailLoginPanel;

        [Header("Anonymous Login")]
        [SerializeField] private Button anonymousLoginButton;
        [SerializeField] private Button deviceIdLoginButton;
        [SerializeField] private Button switchToEmailButton;

        [Header("Email Login")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button emailLoginButton;
        [SerializeField] private Button switchToAnonymousButton;
        [SerializeField] private Toggle rememberMeToggle;

        [Header("Error Display")]
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button errorCloseButton;

        [Header("Loading")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image loadingSpinner;

        [Header("Settings")]
        [SerializeField] private bool closeOnSuccess = true;
        [SerializeField] private float spinnerSpeed = 180f;

        private Online.AuthenticationManager authManager;
        private bool isLoading = false;

        private void Awake()
        {
            // Setup button listeners
            anonymousLoginButton?.onClick.AddListener(OnAnonymousLoginClicked);
            deviceIdLoginButton?.onClick.AddListener(OnDeviceIdLoginClicked);
            switchToEmailButton?.onClick.AddListener(SwitchToEmailLogin);
            emailLoginButton?.onClick.AddListener(OnEmailLoginClicked);
            switchToAnonymousButton?.onClick.AddListener(SwitchToAnonymousLogin);
            errorCloseButton?.onClick.AddListener(CloseErrorPanel);

            // Setup input field listeners
            emailInput?.onEndEdit.AddListener(OnEmailInputEnd);
            passwordInput?.onEndEdit.AddListener(OnPasswordInputEnd);

            // Start with anonymous login panel
            SwitchToAnonymousLogin();
            HideLoadingPanel();
            HideErrorPanel();
        }

        private void Start()
        {
            authManager = Online.AuthenticationManager.Instance;

            if (authManager == null)
            {
                Debug.LogError("[LoginUI] AuthenticationManager not found!");
                ShowError("Authentication system not available");
                return;
            }

            // Subscribe to auth events
            authManager.OnSignInSuccess += OnSignInSuccess;
            authManager.OnSignInFailed += OnSignInFailed;

            // Auto-hide if already authenticated
            if (authManager.IsAuthenticated)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (authManager != null)
            {
                authManager.OnSignInSuccess -= OnSignInSuccess;
                authManager.OnSignInFailed -= OnSignInFailed;
            }
        }

        private void Update()
        {
            // Rotate loading spinner
            if (isLoading && loadingSpinner != null)
            {
                loadingSpinner.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
            }
        }

        // ── Button Handlers ───────────────────────────────────────────────────

        private void OnAnonymousLoginClicked()
        {
            if (isLoading) return;

            ShowLoadingPanel("Signing in as guest...");
            authManager.SignInAnonymous();
        }

        private void OnDeviceIdLoginClicked()
        {
            if (isLoading) return;

            ShowLoadingPanel("Signing in with device ID...");
            authManager.SignInWithDeviceId();
        }

        private void OnEmailLoginClicked()
        {
            if (isLoading) return;

            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            // Validate inputs
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Invalid email format");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return;
            }

            ShowLoadingPanel("Signing in...");
            authManager.SignInWithEmail(email, password);
        }

        // ── Auth Callbacks ────────────────────────────────────────────────────

        private void OnSignInSuccess(string playerId, string playerName)
        {
            HideLoadingPanel();

            Debug.Log($"[LoginUI] Sign-in success: {playerName} ({playerId})");

            // Show success briefly
            StartCoroutine(ShowSuccessAndClose(playerName));
        }

        private void OnSignInFailed(string error)
        {
            HideLoadingPanel();
            ShowError($"Sign-in failed: {error}");
        }

        private IEnumerator ShowSuccessAndClose(string playerName)
        {
            if (loadingText != null)
            {
                loadingText.text = $"Welcome, {playerName}!";
            }

            ShowLoadingPanel($"Welcome, {playerName}!");

            yield return new WaitForSeconds(1.5f);

            HideLoadingPanel();

            if (closeOnSuccess)
            {
                gameObject.SetActive(false);
            }
        }

        // ── Panel Switching ───────────────────────────────────────────────────

        private void SwitchToAnonymousLogin()
        {
            anonymousLoginPanel?.SetActive(true);
            emailLoginPanel?.SetActive(false);
            HideErrorPanel();
        }

        private void SwitchToEmailLogin()
        {
            anonymousLoginPanel?.SetActive(false);
            emailLoginPanel?.SetActive(true);
            HideErrorPanel();
        }

        // ── UI State Management ───────────────────────────────────────────────

        private void ShowLoadingPanel(string message)
        {
            isLoading = true;
            
            mainPanel?.SetActive(false);
            loadingPanel?.SetActive(true);
            errorPanel?.SetActive(false);

            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }

        private void HideLoadingPanel()
        {
            isLoading = false;
            
            loadingPanel?.SetActive(false);
            mainPanel?.SetActive(true);
        }

        private void ShowError(string message)
        {
            errorPanel?.SetActive(true);

            if (errorText != null)
            {
                errorText.text = message;
            }

            Debug.LogWarning($"[LoginUI] Error: {message}");
        }

        private void HideErrorPanel()
        {
            errorPanel?.SetActive(false);
        }

        private void CloseErrorPanel()
        {
            HideErrorPanel();
        }

        // ── Input Handlers ────────────────────────────────────────────────────

        private void OnEmailInputEnd(string value)
        {
            // Submit on Enter key
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                passwordInput?.ActivateInputField();
            }
        }

        private void OnPasswordInputEnd(string value)
        {
            // Submit on Enter key
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnEmailLoginClicked();
            }
        }

        // ── Validation ────────────────────────────────────────────────────────

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Show the login UI.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            SwitchToAnonymousLogin();
        }

        /// <summary>
        /// Hide the login UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Clear all input fields.
        /// </summary>
        public void ClearInputs()
        {
            if (emailInput != null) emailInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
        }
    }
}
