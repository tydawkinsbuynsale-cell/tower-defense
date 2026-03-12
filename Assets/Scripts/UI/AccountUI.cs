using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// Displays player account information and auth status.
    /// Shows player name, ID, and provides sign-out/manage options.
    /// </summary>
    public class AccountUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button signOutButton;
        [SerializeField] private Button linkAccountButton;
        [SerializeField] private GameObject signedInPanel;
        [SerializeField] private GameObject signedOutPanel;

        [Header("Status Colors")]
        [SerializeField] private Color authenticatedColor = Color.green;
        [SerializeField] private Color guestColor = Color.yellow;
        [SerializeField] private Color signedOutColor = Color.gray;

        private Online.AuthenticationManager authManager;

        private void Awake()
        {
            signOutButton?.onClick.AddListener(OnSignOutClicked);
            linkAccountButton?.onClick.AddListener(OnLinkAccountClicked);
        }

        private void Start()
        {
            authManager = Online.AuthenticationManager.Instance;

            if (authManager == null)
            {
                Debug.LogError("[AccountUI] AuthenticationManager not found!");
                return;
            }

            // Subscribe to auth events
            authManager.OnAuthenticationStateChanged += OnAuthStateChanged;
            authManager.OnSignInSuccess += OnSignInSuccess;
            authManager.OnSignOut += OnSignOut;

            // Initial update
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (authManager != null)
            {
                authManager.OnAuthenticationStateChanged -= OnAuthStateChanged;
                authManager.OnSignInSuccess -= OnSignInSuccess;
                authManager.OnSignOut -= OnSignOut;
            }
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void OnAuthStateChanged(bool isAuthenticated)
        {
            UpdateUI();
        }

        private void OnSignInSuccess(string playerId, string playerName)
        {
            UpdateUI();
        }

        private void OnSignOut()
        {
            UpdateUI();
        }

        // ── Button Handlers ───────────────────────────────────────────────────

        private void OnSignOutClicked()
        {
            if (authManager != null)
            {
                authManager.SignOut();
            }
        }

        private void OnLinkAccountClicked()
        {
            // Show link account UI (to be implemented)
            Debug.Log("[AccountUI] Link account clicked (UI not implemented yet)");
            
            // Example: Upgrade guest account to email/password
            // authManager.LinkEmail("email@example.com", "password");
        }

        // ── UI Update ─────────────────────────────────────────────────────────

        private void UpdateUI()
        {
            if (authManager == null) return;

            bool isAuth = authManager.IsAuthenticated;

            // Show/hide panels
            signedInPanel?.SetActive(isAuth);
            signedOutPanel?.SetActive(!isAuth);

            if (isAuth)
            {
                // Update player info
                if (playerNameText != null)
                {
                    playerNameText.text = authManager.PlayerName ?? "Unknown";
                }

                if (playerIdText != null)
                {
                    string id = authManager.PlayerId ?? "N/A";
                    // Show shortened ID
                    if (id.Length > 16)
                    {
                        id = id.Substring(0, 8) + "..." + id.Substring(id.Length - 4);
                    }
                    playerIdText.text = $"ID: {id}";
                }

                // Update status
                bool isGuest = authManager.PlayerName != null && authManager.PlayerName.StartsWith("Player_");
                
                if (statusText != null)
                {
                    if (isGuest)
                    {
                        statusText.text = "Guest Account";
                        statusText.color = guestColor;
                    }
                    else
                    {
                        statusText.text = "Signed In";
                        statusText.color = authenticatedColor;
                    }
                }

                // Show/hide link button for guest accounts
                if (linkAccountButton != null)
                {
                    linkAccountButton.gameObject.SetActive(isGuest);
                }
            }
            else
            {
                // Signed out state
                if (statusText != null)
                {
                    statusText.text = "Not Signed In";
                    statusText.color = signedOutColor;
                }
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Manually refresh the UI display.
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateUI();
        }
    }
}
