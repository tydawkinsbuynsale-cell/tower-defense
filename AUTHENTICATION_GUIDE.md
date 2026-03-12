# Authentication System Guide

Complete guide for the Robot Tower Defense authentication system with multi-backend support and seamless integration with cloud save and leaderboards.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Setup](#setup)
- [Authentication Methods](#authentication-methods)
- [Backend Integration](#backend-integration)
- [UI Components](#ui-components)
- [Integration with Game Systems](#integration-with-game-systems)
- [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The authentication system manages player identity and session management across multiple backends. It provides a unified API for signing in with various methods and automatically integrates with cloud save and leaderboard systems.

**File:** `Assets/Scripts/Online/AuthenticationManager.cs` (850 lines)

### Key Characteristics

- **Multi-Backend Support**: Unity Gaming Services, PlayFab, or custom server
- **Multiple Auth Methods**: Anonymous, Email/Password, Device ID
- **Session Persistence**: Automatic session restore on app restart
- **Token Management**: Automatic token refresh and validation
- **Offline Support**: Guest mode allows playing without authentication
- **Account Linking**: Upgrade anonymous accounts to permanent accounts

---

## Features

### Core Functionality

✅ **Anonymous Sign-In**
- Instant guest account creation
- No credentials required
- Tied to device for persistence
- Upgradeable to full account later

✅ **Email/Password Sign-In**
- Traditional account creation
- Secure password handling (delegated to backend)
- Account recovery support (backend-dependent)

✅ **Device ID Sign-In**
- Automatic unique device identifier
- Silent sign-in on repeat launches
- Cross-platform device tracking

✅ **Session Management**
- Automatic session persistence
- Token refresh on expiration
- Sign-out with full session cleanup

✅ **Cloud Integration**
- Seamless CloudSaveManager integration
- Automatic LeaderboardManager player ID sync
- Authentication state events for UI updates

---

## Architecture

### System Flow

```
┌─────────────────┐
│   MainMenu UI   │
└────────┬────────┘
         │ Show LoginUI
         ▼
┌──────────────────────┐      ┌──────────────────┐
│  AuthenticationMgr   │◄────►│  Backend Adapter │
│  (Signin/Signout)    │      │ (Unity/PF/Custom)│
└──────────────────────┘      └──────────────────┘
         │
         │ Events: OnSignInSuccess
         │         OnAuthenticationStateChanged
         ▼
┌──────────────────────┐      ┌──────────────────┐
│  CloudSaveManager    │      │ LeaderboardManager│
│ (Uses auth token)    │      │ (Uses player ID)  │
└──────────────────────┘      └──────────────────┘
```

### Auth State Machine

```
[Not Initialized]
      │
      │ Awake()
      ▼
[Initializing] ──────────► [Check Persisted Session]
      │                            │
      │                            ├─ Valid? ──► [Authenticated]
      │                            └─ Invalid ──► [Auto Sign-In (Guest)]
      ▼                                                  │
[Initialized - Not Auth]                                │
      │                                                  │
      │ User clicks Sign-In                             │
      ▼                                                  ▼
[Signing In...] ────────────────────────────────► [Authenticated]
      │                                                  │
      │ OnSignInFailed                                  │ User clicks Sign-Out
      ▼                                                  ▼
[Sign-In Error]                              [Signing Out...] ──► [Not Authenticated]
```

### Key Components

1. **AuthenticationManager** (Singleton)
   - Manages auth state and sessions
   - Provides backend-agnostic sign-in API
   - Fires events for UI and system integration

2. **Backend Adapters**
   - Unity Gaming Services: Uses Unity.Services.Authentication
   - PlayFab: Uses PlayFabClientAPI
   - Custom: HTTP REST API with JWT tokens

3. **Session Persistence**
   - Stores player ID, name, and token in PlayerPrefs
   - Validates token expiration on app start
   - Auto-refreshes expired tokens

4. **UI Components**
   - LoginUI: Sign-in interface
   - AccountUI: Display player info and sign-out

---

## Setup

### 1. Add AuthenticationManager to Scene

Add to your persistent scene (Bootstrap or MainMenu):

```csharp
GameObject authObj = new GameObject("AuthenticationManager");
authObj.AddComponent<AuthenticationManager>();
DontDestroyOnLoad(authObj);
```

Or create a prefab and add to scene hierarchy.

### 2. Configure in Inspector

**AuthenticationManager Settings:**
- ✅ **Auto Sign In On Start**: Sign in automatically on app launch
- ✅ **Allow Guest Mode**: Enable anonymous/guest accounts
- ✅ **Persist Session**: Remember player between sessions
- **Preferred Backend**: `UnityGamingServices` / `PlayFab` / `Custom`
- **Custom Auth URL**: (if using custom backend) `https://your-server.com/api/auth`
- **Verbose Logging**: Enable for debugging

### 3. Choose Backend

Set scripting define symbol in:
**Edit → Project Settings → Player → Scripting Define Symbols**

Add one of:
- `UNITY_GAMING_SERVICES` (for Unity backend)
- `PLAYFAB` (for PlayFab)
- Leave empty (for custom HTTP backend)

### 4. Add UI Components

Add LoginUI and AccountUI to your MainMenu scene:
1. Right-click Hierarchy → UI → Panel
2. Add `LoginUI` component
3. Configure UI references in Inspector
4. Repeat for `AccountUI`

---

## Authentication Methods

### 1. Anonymous Sign-In

Creates a guest account without credentials.

**When to use:**
- Quick onboarding (no signup required)
- Temporary accounts for testing/demos
- Accounts that can be upgraded later

**Usage:**
```csharp
AuthenticationManager.Instance.SignInAnonymous();
```

**Backend Behavior:**
- **Unity**: Uses `SignInAnonymouslyAsync()`
- **PlayFab**: Uses `LoginWithCustomID` (device-based)
- **Custom**: POST to `/anonymous` with device ID

**Example Response:**
```json
{
  "playerId": "abc123-def456-gh789",
  "playerName": "Player_abc123",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400
}
```

---

### 2. Email/Password Sign-In

Traditional account with email and password.

**When to use:**
- Permanent accounts
- Cross-device sync with cloud save
- Competitive leaderboards with verified players

**Usage:**
```csharp
AuthenticationManager.Instance.SignInWithEmail("player@example.com", "password");
```

**Validation:**
- Email format validated (must contain @)
- Password minimum 6 characters
- Backend performs full validation

**Backend Behavior:**
- **Unity**: Uses `SignInWithUsernamePasswordAsync()`
- **PlayFab**: Uses `LoginWithEmailAddress`
- **Custom**: POST to `/email` with credentials

---

### 3. Device ID Sign-In

Uses unique device identifier for silent sign-in.

**When to use:**
- Mobile games (auto-sign on app launch)
- Single-device players
- Silent authentication without UI prompts

**Usage:**
```csharp
AuthenticationManager.Instance.SignInWithDeviceId();
```

**Device ID Sources:**
1. `SystemInfo.deviceUniqueIdentifier` (primary)
2. Generated GUID (fallback if device ID unavailable)
3. Stored in PlayerPrefs for consistency

**Backend Behavior:**
- **Unity**: Falls back to anonymous sign-in
- **PlayFab**: Uses `LoginWithAndroidDeviceID` / `LoginWithIOSDeviceID`
- **Custom**: POST to `/device` with device ID

---

## Backend Integration

### Option 1: Unity Gaming Services

**Requirements:**
- Unity Services account
- Authentication package: `com.unity.services.authentication`
- Project linked to Unity Cloud

**Setup:**

1. Install packages:
```
Window → Package Manager → Unity Registry → Authentication → Install
Window → Package Manager → Unity Registry → Core → Install
```

2. Link project:
```
Window → General → Services → Create/Link Unity Project
```

3. Enable authentication:
```csharp
#define UNITY_GAMING_SERVICES
```

4. Initialize (already implemented):
```csharp
await Unity.Services.Core.UnityServices.InitializeAsync();
await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
string playerId = AuthenticationService.Instance.PlayerId;
string token = await AuthenticationService.Instance.GetAccessTokenAsync();
```

**Authentication Methods:**
- ✅ Anonymous: `SignInAnonymouslyAsync()`
- ✅ Email/Password: `SignInWithUsernamePasswordAsync()`
- ❌ Device ID: Not natively supported (falls back to anonymous)

**Token Lifetime:** 24 hours (auto-refresh on API calls)

---

### Option 2: PlayFab

**Requirements:**
- PlayFab account and Title ID
- PlayFab SDK for Unity

**Setup:**

1. Install PlayFab SDK:
```
Download: https://github.com/PlayFab/UnitySDK/releases
Assets → Import Package → Custom Package → PlayFabSDK.unitypackage
```

2. Configure Title ID:
```csharp
// In PlayFabSettings or initialization
PlayFabSettings.TitleId = "YOUR_TITLE_ID";
```

3. Enable PlayFab:
```csharp
#define PLAYFAB
```

4. Implement authentication (already in AuthenticationManager):

**Anonymous:**
```csharp
var request = new LoginWithCustomIDRequest
{
    CustomId = deviceId,
    CreateAccount = true
};
PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
```

**Email/Password:**
```csharp
var request = new LoginWithEmailAddressRequest
{
    Email = email,
    Password = password
};
PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
```

**Device ID:**
```csharp
var request = new LoginWithAndroidDeviceIDRequest
{
    AndroidDeviceId = deviceId,
    CreateAccount = true
};
PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnSuccess, OnError);
```

**Token Lifetime:** 24 hours (SessionTicket)

---

### Option 3: Custom HTTP Backend

**Requirements:**
- HTTP server with authentication endpoints
- JWT or custom token system

**Implementation:**

1. Set up server endpoints:
```
POST /api/auth/anonymous   - Anonymous sign-in
POST /api/auth/email       - Email/password sign-in
POST /api/auth/device      - Device ID sign-in
POST /api/auth/refresh     - Token refresh
```

2. Server Example (Node.js/Express):

```javascript
const express = require('express');
const jwt = require('jsonwebtoken');
const app = express();

app.post('/api/auth/anonymous', (req, res) => {
    const { deviceId } = req.body;
    
    // Generate player ID
    const playerId = generatePlayerId(deviceId);
    const playerName = `Player_${playerId.substring(0, 6)}`;
    
    // Generate JWT token
    const token = jwt.sign(
        { playerId, deviceId },
        process.env.JWT_SECRET,
        { expiresIn: '24h' }
    );
    
    res.json({
        playerId,
        playerName,
        token,
        expiresIn: 86400 // 24 hours in seconds
    });
});

app.post('/api/auth/email', (req, res) => {
    const { email, password } = req.body;
    
    // Validate credentials (check database)
    const user = authenticateUser(email, password);
    
    if (!user) {
        return res.status(401).json({ error: 'Invalid credentials' });
    }
    
    const token = jwt.sign(
        { playerId: user.id, email },
        process.env.JWT_SECRET,
        { expiresIn: '24h' }
    );
    
    res.json({
        playerId: user.id,
        playerName: user.name,
        token,
        expiresIn: 86400
    });
});

app.listen(3000);
```

3. Client Integration (already in AuthenticationManager):

```csharp
private IEnumerator SignInCustomAnonymous()
{
    string url = $"{customAuthUrl}/anonymous";
    var requestData = new { deviceId = GetOrCreateDeviceId() };
    string json = JsonUtility.ToJson(requestData);

    using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
    {
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<CustomAuthResponse>(www.downloadHandler.text);
            playerId = response.playerId;
            authToken = response.token;
            // ...
        }
    }
}
```

**Security Best Practices:**
- Use HTTPS for all auth endpoints
- Implement rate limiting (prevent brute force)
- Hash passwords with bcrypt/argon2
- Validate JWT signatures
- Use secure random token generation

---

## UI Components

### LoginUI

Located at: `Assets/Scripts/UI/LoginUI.cs`

**Features:**
- Anonymous and email/password login forms
- Form validation (email format, password length)
- Loading state with spinner animation
- Error display panel
- Tab switching between login methods
- Enter key submission

**Setup:**

1. Create UI hierarchy:
```
LoginPanel (Canvas)
├─ MainPanel
│  ├─ AnonymousLoginPanel
│  │  ├─ AnonymousLoginButton
│  │  ├─ DeviceIdLoginButton
│  │  └─ SwitchToEmailButton
│  └─ EmailLoginPanel
│     ├─ EmailInput (TMP_InputField)
│     ├─ PasswordInput (TMP_InputField)
│     ├─ EmailLoginButton
│     └─ SwitchToAnonymousButton
├─ LoadingPanel
│  ├─ LoadingText (TextMeshPro)
│  └─ LoadingSpinner (Image)
└─ ErrorPanel
   ├─ ErrorText (TextMeshPro)
   └─ ErrorCloseButton
```

2. Assign references in Inspector
3. Call `LoginUI.Instance.Show()` from MainMenu

**Usage:**
```csharp
// Show login UI
LoginUI loginUI = FindObjectOfType<LoginUI>();
loginUI.Show();

// Hide after successful auth
AuthenticationManager.Instance.OnSignInSuccess += (id, name) =>
{
    loginUI.Hide();
};
```

---

### AccountUI

Located at: `Assets/Scripts/UI/AccountUI.cs`

**Features:**
- Display player name and ID
- Auth status indicator (Signed In / Guest / Not Signed In)
- Sign-out button
- Account linking button (for guest accounts)
- Color-coded status (green = authenticated, yellow = guest, gray = not signed in)

**Setup:**

1. Create UI hierarchy:
```
AccountPanel
├─ SignedInPanel
│  ├─ PlayerNameText (TextMeshPro)
│  ├─ PlayerIdText (TextMeshPro)
│  ├─ StatusText (TextMeshPro)
│  ├─ SignOutButton
│  └─ LinkAccountButton
└─ SignedOutPanel
   └─ StatusText (TextMeshPro)
```

2. Assign references in Inspector
3. Add to Settings or Main Menu UI

**Usage:**
```csharp
// Manual refresh
AccountUI accountUI = FindObjectOfType<AccountUI>();
accountUI.RefreshDisplay();
```

---

## Integration with Game Systems

### CloudSaveManager Integration

CloudSaveManager automatically integrates with AuthenticationManager:

**Authentication Check:**
```csharp
// CloudSaveManager waits for authentication
var authManager = AuthenticationManager.Instance;
if (authManager != null && authManager.IsAuthenticated)
{
    // Proceed with cloud sync using auth token
    PushToCloud();
}
else
{
    // Work offline only
    LogDebug("Not authenticated - offline mode only");
}
```

**Auth Token Usage:**
```csharp
// Cloud Save uses auth token for API calls
string token = AuthenticationManager.Instance.AuthToken;
www.SetRequestHeader("Authorization", $"Bearer {token}");
```

**Auto-Sync on Sign-In:**
```csharp
AuthenticationManager.Instance.OnSignInSuccess += (id, name) =>
{
    // Cloud save automatically pulls latest data
    CloudSaveManager.Instance.PullFromCloud();
};
```

---

### LeaderboardManager Integration

LeaderboardManager uses authenticated player info:

**Player ID Sync:**
```csharp
// LeaderboardManager uses auth player ID
var authManager = AuthenticationManager.Instance;
if (authManager != null && authManager.IsAuthenticated)
{
    playerId = authManager.PlayerId;
    playerName = authManager.PlayerName;
}
else
{
    // Fall back to local PlayerPrefs ID
    playerId = GetOrCreatePlayerId();
}
```

**Score Submission:**
```csharp
// Scores are tied to authenticated player
LeaderboardManager.Instance.SubmitScore("endless", score);
// Backend receives playerId from AuthenticationManager
```

---

### MainMenu Integration Example

```csharp
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private LoginUI loginUI;
    [SerializeField] private AccountUI accountUI;
    [SerializeField] private Button playButton;

    private void Start()
    {
        var authManager = AuthenticationManager.Instance;

        // Show login UI if not authenticated
        if (authManager == null || !authManager.IsAuthenticated)
        {
            loginUI.Show();
            playButton.interactable = false;
        }
        else
        {
            loginUI.Hide();
            playButton.interactable = true;
        }

        // Update UI on auth state change
        authManager.OnAuthenticationStateChanged += (isAuth) =>
        {
            if (isAuth)
            {
                loginUI.Hide();
                playButton.interactable = true;
                accountUI.RefreshDisplay();
            }
            else
            {
                loginUI.Show();
                playButton.interactable = false;
            }
        };
    }

    public void OnPlayButtonClicked()
    {
        // Game can only start if authenticated
        if (AuthenticationManager.Instance.IsAuthenticated)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
```

---

## Testing

### Test Scenarios

#### 1. First-Time Launch

1. Fresh install, no persisted session
2. Auto sign-in as guest (if allowGuestMode = true)
3. Should see "Player_XXXXXX" name
4. Player ID stored in PlayerPrefs

**Expected:**
- Sign-in completes within 2 seconds
- `OnSignInSuccess` event fires
- Cloud save and leaderboards initialize
- Account UI shows "Guest Account" status

---

#### 2. Session Persistence

1. Sign in with any method
2. Close app
3. Reopen app
4. Should auto-restore previous session

**Expected:**
- No login UI shown
- Player name/ID restored from PlayerPrefs
- Token validated (not expired)
- Cloud save auto-syncs if enabled

---

#### 3. Token Expiration

1. Sign in, get token (24h expiration)
2. Manually set expiration to past (for testing)
3. Try cloud operation
4. Should trigger token refresh

**Expected:**
- Token refresh attempted
- If refresh fails, user prompted to re-authenticate
- Operations queue until re-auth complete

---

#### 4. Account Linking

1. Sign in anonymously (guest account)
2. Click "Link Account" button
3. Enter email/password
4. Guest account upgraded to permanent

**Expected:**
- Player ID remains same
- Player name changes to email prefix
- Session persisted with new credentials
- Cloud save continues working

---

#### 5. Sign-Out

1. Authenticated user clicks "Sign Out"
2. Session cleared
3. UI updates to signed-out state

**Expected:**
- `OnSignOut` event fires
- Player ID/name/token cleared
- PlayerPrefs session deleted
- Login UI shown again

---

### Debug Tools

**Print Auth State:**
```csharp
// Use context menu in Inspector
// Right-click AuthenticationManager → Print Auth State
```

**Console Output:**
```
Authentication Status:
  Initialized: True
  Authenticated: True
  Player ID: abc123-def456-gh789
  Player Name: Player_abc123
  Token Valid: True
  Backend: UnityGamingServices
```

**Manual Sign-In Test:**
```csharp
[ContextMenu("Test Anonymous Sign-In")]
private void TestSignIn()
{
    AuthenticationManager.Instance.SignInAnonymous();
}
```

---

## Best Practices

### 1. Always Handle Auth Failures Gracefully

```csharp
AuthenticationManager.Instance.OnSignInFailed += (error) =>
{
    // NEVER block gameplay on auth failure
    Debug.LogWarning($"Auth failed: {error}");
    ShowToast("Couldn't sign in - playing offline", ToastType.Warning);
    
    // Allow offline play
    EnableOfflineMode();
};
```

### 2. Prioritize Guest Mode for Onboarding

```csharp
// Enable guest mode for smooth onboarding
authManager.allowGuestMode = true;
authManager.autoSignInOnStart = true;

// Users can upgrade to full account later
ShowUpgradePrompt("Link your email to save progress across devices!");
```

### 3. Validate Tokens Before API Calls

```csharp
if (!AuthenticationManager.Instance.IsTokenValid)
{
    // Refresh token before making cloud/leaderboard calls
    AuthenticationManager.Instance.RefreshToken();
}
```

### 4. Use Events for UI Updates

```csharp
// Subscribe to auth events for reactive UI
authManager.OnAuthenticationStateChanged += (isAuth) =>
{
    UpdateAllUI(isAuth);
};

authManager.OnSignInSuccess += (id, name) =>
{
    ShowWelcomeMessage($"Welcome back, {name}!");
};
```

### 5. Handle Backend-Specific Errors

```csharp
authManager.OnSignInFailed += (error) =>
{
    if (error.Contains("Email already exists"))
    {
        ShowError("Account already exists. Try signing in instead.");
    }
    else if (error.Contains("Invalid password"))
    {
        ShowError("Incorrect password.");
    }
    else
    {
        ShowError($"Sign-in failed: {error}");
    }
};
```

---

## Troubleshooting

### Problem: Sign-In Hangs Forever

**Symptoms:** Sign-in never completes, no error or success

**Solutions:**
1. Check backend is reachable:
   ```csharp
   // Test URL manually
   curl https://your-server.com/api/auth/anonymous
   ```
2. Verify scripting define symbol matches backend
3. Enable verbose logging to see where it stops
4. Check Unity console for exceptions

---

### Problem: Session Not Persisting

**Symptoms:** User signed out after app restart

**Solutions:**
1. Verify `persistSession = true` in Inspector
2. Check PlayerPrefs are being saved:
   ```csharp
   Debug.Log(PlayerPrefs.GetString("Auth_PlayerId"));
   ```
3. Ensure PlayerPrefs.Save() is called
4. Check token expiration (tokens may expire between sessions)

---

### Problem: Token Expired Error

**Symptoms:** Cloud operations fail with "401 Unauthorized"

**Solutions:**
1. Check token expiration:
   ```csharp
   Debug.Log($"Token valid: {AuthenticationManager.Instance.IsTokenValid}");
   ```
2. Call RefreshToken() if expiring
3. Reduce session duration if users play infrequently
4. Implement automatic token refresh before API calls

---

### Problem: Multiple Sign-In Attempts

**Symptoms:** Sign-in called multiple times simultaneously

**Solutions:**
1. Add isSigningIn flag:
   ```csharp
   if (isSigningIn) return;
   isSigningIn = true;
   SignInAnonymous();
   ```
2. Disable sign-in buttons while loading
3. Implement cooldown timer

---

### Problem: Backend Returns 500 Error

**Symptoms:** Sign-in fails with "HTTP 500: Internal Server Error"

**Solutions:**
1. Check backend server logs for exceptions
2. Verify request payload format matches server expectations
3. Test with Postman/curl to isolate Unity client issues
4. Check backend database connection

---

## API Reference

### AuthenticationManager Public Methods

| Method | Description |
|--------|-------------|
| `SignInAnonymous()` | Sign in as guest (device-tied account) |
| `SignInWithEmail(email, password)` | Sign in with email/password |
| `SignInWithDeviceId()` | Sign in with unique device identifier |
| `SignOut()` | Sign out and clear session |
| `RefreshToken()` | Refresh auth token if expiring |
| `LinkEmail(email, password)` | Upgrade guest account to email account |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsAuthenticated` | bool | Is player signed in? |
| `IsInitialized` | bool | Has system finished initializing? |
| `PlayerId` | string | Unique player ID |
| `PlayerName` | string | Display name |
| `AuthToken` | string | Current auth token (for API calls) |
| `IsTokenValid` | bool | Is token not expired? |

### Events

| Event | Parameters | Description |  
|-------|------------|-------------|
| `OnAuthenticationStateChanged` | bool isAuth | Auth state changed |
| `OnSignInSuccess` | string id, string name | Sign-in succeeded |
| `OnSignInFailed` | string error | Sign-in failed |
| `OnSignOut` | none | Player signed out |
| `OnError` | string error | General error occurred |

---

## Additional Resources

- [Unity Gaming Services Authentication](https://docs.unity.com/authentication/)
- [PlayFab Authentication Guide](https://learn.microsoft.com/en-us/gaming/playfab/features/authentication/)
- [JWT Token Guide](https://jwt.io/introduction)
- [Cloud Save Integration](CLOUD_SAVE_GUIDE.md)
- [Leaderboard Integration](LEADERBOARD_GUIDE.md)

---

**Built for Robot Tower Defense | Version 1.3**
