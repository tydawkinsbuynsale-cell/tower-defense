using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace RobotTD.Online
{
    /// <summary>
    /// Manages player authentication across multiple backends.
    /// Supports Unity Gaming Services, PlayFab, and custom authentication servers.
    /// Works with CloudSaveManager and LeaderboardManager for authenticated operations.
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        public static AuthenticationManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool autoSignInOnStart = true;
        [SerializeField] private bool allowGuestMode = true;
        [SerializeField] private bool persistSession = true;
        [SerializeField] private AuthBackend preferredBackend = AuthBackend.UnityGamingServices;
        [SerializeField] private bool verboseLogging = false;

        [Header("Custom Backend Settings")]
        [SerializeField] private string customAuthUrl = "https://your-server.com/api/auth";

        // Authentication state
        private bool isAuthenticated = false;
        private bool isInitialized = false;
        private AuthBackend currentBackend;
        private string playerId;
        private string playerName;
        private string authToken;
        private DateTime tokenExpiration;

        // Events
        public event Action<bool> OnAuthenticationStateChanged; // isAuthenticated
        public event Action<string, string> OnSignInSuccess; // playerId, playerName
        public event Action<string> OnSignInFailed; // error message
        public event Action OnSignOut;
        public event Action<string> OnError;

        // Properties
        public bool IsAuthenticated => isAuthenticated;
        public bool IsInitialized => isInitialized;
        public string PlayerId => playerId;
        public string PlayerName => playerName;
        public string AuthToken => authToken;
        public bool IsTokenValid => !string.IsNullOrEmpty(authToken) && DateTime.UtcNow < tokenExpiration;

        public enum AuthBackend
        {
            UnityGamingServices,
            PlayFab,
            Custom
        }

        public enum AuthMethod
        {
            Anonymous,
            EmailPassword,
            Guest,
            DeviceId,
            CustomToken
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAuthentication();
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void InitializeAuthentication()
        {
            LogDebug("Initializing authentication system...");

            currentBackend = preferredBackend;

            // Load persisted session
            if (persistSession)
            {
                LoadPersistedSession();
            }

            // Auto sign-in
            if (autoSignInOnStart)
            {
                StartCoroutine(AutoSignIn());
            }
            else
            {
                isInitialized = true;
                LogDebug("Authentication initialized (manual sign-in required)");
            }
        }

        private IEnumerator AutoSignIn()
        {
            LogDebug("Attempting auto sign-in...");

            // Try to restore previous session
            if (!string.IsNullOrEmpty(playerId) && IsTokenValid)
            {
                LogDebug($"Restored session for player: {playerId}");
                isAuthenticated = true;
                isInitialized = true;
                OnAuthenticationStateChanged?.Invoke(true);
                OnSignInSuccess?.Invoke(playerId, playerName);
                yield break;
            }

            // Sign in anonymously/guest
            if (allowGuestMode)
            {
                yield return StartCoroutine(SignInAnonymous());
            }
            else
            {
                isInitialized = true;
                LogDebug("Auto sign-in disabled, awaiting manual authentication");
            }
        }

        // ── Sign-In Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Sign in anonymously (no credentials required).
        /// Creates a guest account tied to the device.
        /// </summary>
        public void SignInAnonymous()
        {
            if (isAuthenticated)
            {
                LogDebug("Already authenticated");
                return;
            }

            StartCoroutine(SignInAnonymousCoroutine());
        }

        private IEnumerator SignInAnonymousCoroutine()
        {
            LogDebug("Signing in anonymously...");

            switch (currentBackend)
            {
                case AuthBackend.UnityGamingServices:
                    yield return StartCoroutine(SignInUnityAnonymous());
                    break;
                case AuthBackend.PlayFab:
                    yield return StartCoroutine(SignInPlayFabAnonymous());
                    break;
                case AuthBackend.Custom:
                    yield return StartCoroutine(SignInCustomAnonymous());
                    break;
            }
        }

        /// <summary>
        /// Sign in with email and password.
        /// </summary>
        public void SignInWithEmail(string email, string password)
        {
            if (isAuthenticated)
            {
                LogDebug("Already authenticated");
                return;
            }

            StartCoroutine(SignInWithEmailCoroutine(email, password));
        }

        private IEnumerator SignInWithEmailCoroutine(string email, string password)
        {
            LogDebug($"Signing in with email: {email}");

            switch (currentBackend)
            {
                case AuthBackend.UnityGamingServices:
                    yield return StartCoroutine(SignInUnityEmail(email, password));
                    break;
                case AuthBackend.PlayFab:
                    yield return StartCoroutine(SignInPlayFabEmail(email, password));
                    break;
                case AuthBackend.Custom:
                    yield return StartCoroutine(SignInCustomEmail(email, password));
                    break;
            }
        }

        /// <summary>
        /// Sign in with device ID (auto-generated unique identifier).
        /// </summary>
        public void SignInWithDeviceId()
        {
            if (isAuthenticated)
            {
                LogDebug("Already authenticated");
                return;
            }

            StartCoroutine(SignInWithDeviceIdCoroutine());
        }

        private IEnumerator SignInWithDeviceIdCoroutine()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            LogDebug($"Signing in with device ID: {deviceId.Substring(0, 8)}...");

            switch (currentBackend)
            {
                case AuthBackend.UnityGamingServices:
                    yield return StartCoroutine(SignInUnityDeviceId(deviceId));
                    break;
                case AuthBackend.PlayFab:
                    yield return StartCoroutine(SignInPlayFabDeviceId(deviceId));
                    break;
                case AuthBackend.Custom:
                    yield return StartCoroutine(SignInCustomDeviceId(deviceId));
                    break;
            }
        }

        // ── Sign-Out ──────────────────────────────────────────────────────────

        /// <summary>
        /// Sign out and clear session.
        /// </summary>
        public void SignOut()
        {
            if (!isAuthenticated)
            {
                LogDebug("Not authenticated");
                return;
            }

            LogDebug("Signing out...");

            // Clear session data
            isAuthenticated = false;
            playerId = null;
            playerName = null;
            authToken = null;
            tokenExpiration = DateTime.MinValue;

            // Clear persisted session
            if (persistSession)
            {
                ClearPersistedSession();
            }

            // Notify listeners
            OnAuthenticationStateChanged?.Invoke(false);
            OnSignOut?.Invoke();

            LogDebug("Signed out successfully");
        }

        // ── Unity Gaming Services Backend ─────────────────────────────────────

#if UNITY_GAMING_SERVICES
        private IEnumerator SignInUnityAnonymous()
        {
            // Unity Gaming Services anonymous sign-in
            var signInTask = Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            while (!signInTask.IsCompleted)
            {
                yield return null;
            }

            if (signInTask.IsFaulted)
            {
                string error = signInTask.Exception?.Message ?? "Unknown error";
                LogDebug($"Unity anonymous sign-in failed: {error}");
                OnSignInFailed?.Invoke(error);
                OnError?.Invoke(error);
                isInitialized = true;
                yield break;
            }

            // Success
            playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            playerName = $"Player_{playerId.Substring(0, 6)}";
            authToken = await Unity.Services.Authentication.AuthenticationService.Instance.GetAccessTokenAsync();
            tokenExpiration = DateTime.UtcNow.AddHours(24); // Unity tokens typically valid for 24h
            isAuthenticated = true;
            isInitialized = true;

            SavePersistedSession();

            OnAuthenticationStateChanged?.Invoke(true);
            OnSignInSuccess?.Invoke(playerId, playerName);
            LogDebug($"Unity anonymous sign-in success: {playerId}");
        }

        private IEnumerator SignInUnityEmail(string email, string password)
        {
            // Unity Gaming Services email/password sign-in
            var signInTask = Unity.Services.Authentication.AuthenticationService.Instance
                .SignInWithUsernamePasswordAsync(email, password);
            
            while (!signInTask.IsCompleted)
            {
                yield return null;
            }

            if (signInTask.IsFaulted)
            {
                string error = signInTask.Exception?.Message ?? "Unknown error";
                LogDebug($"Unity email sign-in failed: {error}");
                OnSignInFailed?.Invoke(error);
                OnError?.Invoke(error);
                yield break;
            }

            // Success (same as anonymous, just different method)
            playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            playerName = email.Split('@')[0]; // Use email prefix as name
            authToken = await Unity.Services.Authentication.AuthenticationService.Instance.GetAccessTokenAsync();
            tokenExpiration = DateTime.UtcNow.AddHours(24);
            isAuthenticated = true;

            SavePersistedSession();

            OnAuthenticationStateChanged?.Invoke(true);
            OnSignInSuccess?.Invoke(playerId, playerName);
            LogDebug($"Unity email sign-in success: {playerId}");
        }

        private IEnumerator SignInUnityDeviceId(string deviceId)
        {
            // Use anonymous sign-in for device ID in Unity
            yield return StartCoroutine(SignInUnityAnonymous());
        }
#else
        private IEnumerator SignInUnityAnonymous()
        {
            LogDebug("Unity Gaming Services not available (package not installed)");
            OnSignInFailed?.Invoke("Unity Gaming Services not configured");
            isInitialized = true;
            yield break;
        }

        private IEnumerator SignInUnityEmail(string email, string password)
        {
            LogDebug("Unity Gaming Services not available (package not installed)");
            OnSignInFailed?.Invoke("Unity Gaming Services not configured");
            yield break;
        }

        private IEnumerator SignInUnityDeviceId(string deviceId)
        {
            LogDebug("Unity Gaming Services not available (package not installed)");
            OnSignInFailed?.Invoke("Unity Gaming Services not configured");
            yield break;
        }
#endif

        // ── PlayFab Backend ───────────────────────────────────────────────────

#if PLAYFAB
        private IEnumerator SignInPlayFabAnonymous()
        {
            string customId = GetOrCreateDeviceId();
            
            var request = new PlayFab.ClientModels.LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true
            };

            bool completed = false;
            bool success = false;
            string errorMsg = null;

            PlayFab.PlayFabClientAPI.LoginWithCustomID(request,
                result =>
                {
                    playerId = result.PlayFabId;
                    playerName = $"Player_{playerId.Substring(0, 6)}";
                    authToken = result.SessionTicket;
                    tokenExpiration = DateTime.UtcNow.AddHours(24);
                    isAuthenticated = true;
                    isInitialized = true;
                    success = true;
                    completed = true;

                    SavePersistedSession();
                },
                error =>
                {
                    errorMsg = error.ErrorMessage;
                    completed = true;
                });

            while (!completed)
            {
                yield return null;
            }

            if (success)
            {
                OnAuthenticationStateChanged?.Invoke(true);
                OnSignInSuccess?.Invoke(playerId, playerName);
                LogDebug($"PlayFab anonymous sign-in success: {playerId}");
            }
            else
            {
                LogDebug($"PlayFab anonymous sign-in failed: {errorMsg}");
                OnSignInFailed?.Invoke(errorMsg);
                OnError?.Invoke(errorMsg);
                isInitialized = true;
            }
        }

        private IEnumerator SignInPlayFabEmail(string email, string password)
        {
            var request = new PlayFab.ClientModels.LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password
            };

            bool completed = false;
            bool success = false;
            string errorMsg = null;

            PlayFab.PlayFabClientAPI.LoginWithEmailAddress(request,
                result =>
                {
                    playerId = result.PlayFabId;
                    playerName = email.Split('@')[0];
                    authToken = result.SessionTicket;
                    tokenExpiration = DateTime.UtcNow.AddHours(24);
                    isAuthenticated = true;
                    success = true;
                    completed = true;

                    SavePersistedSession();
                },
                error =>
                {
                    errorMsg = error.ErrorMessage;
                    completed = true;
                });

            while (!completed)
            {
                yield return null;
            }

            if (success)
            {
                OnAuthenticationStateChanged?.Invoke(true);
                OnSignInSuccess?.Invoke(playerId, playerName);
                LogDebug($"PlayFab email sign-in success: {playerId}");
            }
            else
            {
                LogDebug($"PlayFab email sign-in failed: {errorMsg}");
                OnSignInFailed?.Invoke(errorMsg);
                OnError?.Invoke(errorMsg);
            }
        }

        private IEnumerator SignInPlayFabDeviceId(string deviceId)
        {
            var request = new PlayFab.ClientModels.LoginWithAndroidDeviceIDRequest
            {
                AndroidDeviceId = deviceId,
                CreateAccount = true
            };

            bool completed = false;
            bool success = false;
            string errorMsg = null;

            PlayFab.PlayFabClientAPI.LoginWithAndroidDeviceID(request,
                result =>
                {
                    playerId = result.PlayFabId;
                    playerName = $"Player_{playerId.Substring(0, 6)}";
                    authToken = result.SessionTicket;
                    tokenExpiration = DateTime.UtcNow.AddHours(24);
                    isAuthenticated = true;
                    success = true;
                    completed = true;

                    SavePersistedSession();
                },
                error =>
                {
                    errorMsg = error.ErrorMessage;
                    completed = true;
                });

            while (!completed)
            {
                yield return null;
            }

            if (success)
            {
                OnAuthenticationStateChanged?.Invoke(true);
                OnSignInSuccess?.Invoke(playerId, playerName);
                LogDebug($"PlayFab device ID sign-in success: {playerId}");
            }
            else
            {
                LogDebug($"PlayFab device ID sign-in failed: {errorMsg}");
                OnSignInFailed?.Invoke(errorMsg);
                OnError?.Invoke(errorMsg);
            }
        }
#else
        private IEnumerator SignInPlayFabAnonymous()
        {
            LogDebug("PlayFab SDK not available (package not installed)");
            OnSignInFailed?.Invoke("PlayFab not configured");
            isInitialized = true;
            yield break;
        }

        private IEnumerator SignInPlayFabEmail(string email, string password)
        {
            LogDebug("PlayFab SDK not available (package not installed)");
            OnSignInFailed?.Invoke("PlayFab not configured");
            yield break;
        }

        private IEnumerator SignInPlayFabDeviceId(string deviceId)
        {
            LogDebug("PlayFab SDK not available (package not installed)");
            OnSignInFailed?.Invoke("PlayFab not configured");
            yield break;
        }
#endif

        // ── Custom Backend ────────────────────────────────────────────────────

        private IEnumerator SignInCustomAnonymous()
        {
            string deviceId = GetOrCreateDeviceId();
            string url = $"{customAuthUrl}/anonymous";

            var requestData = new
            {
                deviceId = deviceId,
                platform = Application.platform.ToString(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonUtility.ToJson(requestData);

            using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CustomAuthResponse>(www.downloadHandler.text);

                    playerId = response.playerId;
                    playerName = response.playerName ?? $"Player_{playerId.Substring(0, 6)}";
                    authToken = response.token;
                    tokenExpiration = DateTime.UtcNow.AddSeconds(response.expiresIn);
                    isAuthenticated = true;
                    isInitialized = true;

                    SavePersistedSession();

                    OnAuthenticationStateChanged?.Invoke(true);
                    OnSignInSuccess?.Invoke(playerId, playerName);
                    LogDebug($"Custom anonymous sign-in success: {playerId}");
                }
                else
                {
                    string error = $"HTTP {www.responseCode}: {www.error}";
                    LogDebug($"Custom anonymous sign-in failed: {error}");
                    OnSignInFailed?.Invoke(error);
                    OnError?.Invoke(error);
                    isInitialized = true;
                }
            }
        }

        private IEnumerator SignInCustomEmail(string email, string password)
        {
            string url = $"{customAuthUrl}/email";

            var requestData = new
            {
                email = email,
                password = password,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonUtility.ToJson(requestData);

            using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CustomAuthResponse>(www.downloadHandler.text);

                    playerId = response.playerId;
                    playerName = response.playerName ?? email.Split('@')[0];
                    authToken = response.token;
                    tokenExpiration = DateTime.UtcNow.AddSeconds(response.expiresIn);
                    isAuthenticated = true;

                    SavePersistedSession();

                    OnAuthenticationStateChanged?.Invoke(true);
                    OnSignInSuccess?.Invoke(playerId, playerName);
                    LogDebug($"Custom email sign-in success: {playerId}");
                }
                else
                {
                    string error = $"HTTP {www.responseCode}: {www.error}";
                    LogDebug($"Custom email sign-in failed: {error}");
                    OnSignInFailed?.Invoke(error);
                    OnError?.Invoke(error);
                }
            }
        }

        private IEnumerator SignInCustomDeviceId(string deviceId)
        {
            // Same as anonymous for custom backend
            yield return StartCoroutine(SignInCustomAnonymous());
        }

        [Serializable]
        private class CustomAuthResponse
        {
            public string playerId;
            public string playerName;
            public string token;
            public int expiresIn; // seconds
        }

        // ── Session Persistence ───────────────────────────────────────────────

        private void LoadPersistedSession()
        {
            playerId = PlayerPrefs.GetString("Auth_PlayerId", null);
            playerName = PlayerPrefs.GetString("Auth_PlayerName", null);
            authToken = PlayerPrefs.GetString("Auth_Token", null);
            
            long expirationTicks = long.Parse(PlayerPrefs.GetString("Auth_TokenExpiration", "0"));
            tokenExpiration = expirationTicks > 0 ? new DateTime(expirationTicks) : DateTime.MinValue;

            if (!string.IsNullOrEmpty(playerId) && IsTokenValid)
            {
                LogDebug($"Loaded persisted session for: {playerId}");
            }
        }

        private void SavePersistedSession()
        {
            if (!persistSession) return;

            PlayerPrefs.SetString("Auth_PlayerId", playerId);
            PlayerPrefs.SetString("Auth_PlayerName", playerName);
            PlayerPrefs.SetString("Auth_Token", authToken);
            PlayerPrefs.SetString("Auth_TokenExpiration", tokenExpiration.Ticks.ToString());
            PlayerPrefs.Save();

            LogDebug("Session persisted");
        }

        private void ClearPersistedSession()
        {
            PlayerPrefs.DeleteKey("Auth_PlayerId");
            PlayerPrefs.DeleteKey("Auth_PlayerName");
            PlayerPrefs.DeleteKey("Auth_Token");
            PlayerPrefs.DeleteKey("Auth_TokenExpiration");
            PlayerPrefs.Save();

            LogDebug("Persisted session cleared");
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        private string GetOrCreateDeviceId()
        {
            string deviceId = PlayerPrefs.GetString("Auth_DeviceId", "");
            
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
                
                // Fallback if device ID not available
                if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
                {
                    deviceId = Guid.NewGuid().ToString();
                }
                
                PlayerPrefs.SetString("Auth_DeviceId", deviceId);
                PlayerPrefs.Save();
            }

            return deviceId;
        }

        /// <summary>
        /// Refresh the auth token if it's about to expire.
        /// Call this periodically if maintaining long sessions.
        /// </summary>
        public void RefreshToken()
        {
            if (!isAuthenticated)
            {
                LogDebug("Cannot refresh token: not authenticated");
                return;
            }

            // Check if token is expiring soon (within 1 hour)
            if ((tokenExpiration - DateTime.UtcNow).TotalHours > 1)
            {
                LogDebug("Token still valid, refresh not needed");
                return;
            }

            LogDebug("Token expiring soon, refreshing...");
            StartCoroutine(RefreshTokenCoroutine());
        }

        private IEnumerator RefreshTokenCoroutine()
        {
            // Re-authenticate to get new token
            switch (currentBackend)
            {
                case AuthBackend.UnityGamingServices:
                case AuthBackend.PlayFab:
                case AuthBackend.Custom:
                    // Most backends automatically refresh on API calls
                    // If explicit refresh needed, implement here
                    LogDebug("Token refresh handled by backend");
                    break;
            }

            yield break;
        }

        /// <summary>
        /// Link current anonymous account to email/password.
        /// Allows upgrading guest account to permanent account.
        /// </summary>
        public void LinkEmail(string email, string password)
        {
            if (!isAuthenticated)
            {
                OnError?.Invoke("Must be authenticated to link email");
                return;
            }

            StartCoroutine(LinkEmailCoroutine(email, password));
        }

        private IEnumerator LinkEmailCoroutine(string email, string password)
        {
            LogDebug($"Linking email to account: {email}");

            switch (currentBackend)
            {
                case AuthBackend.UnityGamingServices:
                    // Unity: Use UpdatePlayerName or AddUsernamePassword
                    LogDebug("Unity account linking not implemented yet");
                    break;
                case AuthBackend.PlayFab:
                    // PlayFab: Use AddUsernamePassword API
                    LogDebug("PlayFab account linking not implemented yet");
                    break;
                case AuthBackend.Custom:
                    // Custom: Implement your linking endpoint
                    LogDebug("Custom account linking not implemented yet");
                    break;
            }

            yield break;
        }

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[AuthenticationManager] {message}");
            }
        }

        // ── Context Menu Commands (Testing) ───────────────────────────────────

        [ContextMenu("Sign In Anonymous")]
        private void TestSignInAnonymous()
        {
            SignInAnonymous();
        }

        [ContextMenu("Sign Out")]
        private void TestSignOut()
        {
            SignOut();
        }

        [ContextMenu("Print Auth State")]
        private void TestPrintState()
        {
            Debug.Log($"Authentication Status:");
            Debug.Log($"  Initialized: {isInitialized}");
            Debug.Log($"  Authenticated: {isAuthenticated}");
            Debug.Log($"  Player ID: {playerId}");
            Debug.Log($"  Player Name: {playerName}");
            Debug.Log($"  Token Valid: {IsTokenValid}");
            Debug.Log($"  Backend: {currentBackend}");
        }
    }
}
