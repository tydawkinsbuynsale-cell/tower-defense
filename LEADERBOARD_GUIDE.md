# Leaderboard System Guide

Complete implementation guide for the Robot Tower Defense competitive scoring system.

## 📋 Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Backend Integration](#backend-integration)
- [UI Components](#ui-components)
- [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The Leaderboard System provides competitive scoring across multiple game modes with support for offline-first gameplay and flexible backend integration.

### Features

✅ **Offline-First Design** — Local score storage works without internet connection  
✅ **Multiple Leaderboards** — Endless mode, daily challenges, weekly challenges  
✅ **Player Identity** — Auto-generated unique IDs and customizable player names  
✅ **Score Caching** — 5-minute local cache reduces API calls and costs  
✅ **Backend Agnostic** — Support for Unity Gaming Services, PlayFab, or custom HTTP backend  
✅ **Rich Queries** — Top scores, player rank, nearby players  
✅ **Analytics Integration** — Automatic event tracking for engagement metrics  
✅ **Name Validation** — Sanitization and length limits for safety  

### System Components

**Core:**
- `LeaderboardManager.cs` — Singleton managing all leaderboard operations
- `LeaderboardEntry.cs` — Score data structure with metadata support

**UI:**
- `LeaderboardUI.cs` — Main display panel with tabs for multiple boards
- `LeaderboardEntryUI.cs` — Individual score row component
- `PlayerNameDialog.cs` — Name input on first launch

**Integration:**
- `EndlessMode.cs` — Posts scores on game over
- `GameManager.cs` — Triggers score submission
- `AnalyticsManager.cs` — Tracks leaderboard events

---

## Quick Start

### 5-Minute Setup

**1. Add LeaderboardManager to Scene**

```csharp
// Create GameObject in your scene bootstrapper
GameObject leaderboardObj = new GameObject("LeaderboardManager");
leaderboardObj.AddComponent<LeaderboardManager>();
```

**2. Configure Leaderboard IDs**

Select the `LeaderboardManager` GameObject in the hierarchy and configure in Inspector:

```
Enable Leaderboards: ✓
Enable Debug Logs: ✓ (for development)
Offline Mode: ✓ (optional, forces local-only)

Endless Leaderboard ID: "endless_high_score"
Daily Leaderboard ID: "daily_challenge"
Weekly Leaderboard ID: "weekly_challenge"

Max Local Scores: 100
Max Fetched Scores: 50
```

**3. Test Score Submission**

Right-click `LeaderboardManager` in Inspector:
- **Submit Test Score** — Submit random score (1000-100000)
- **Fetch Endless Leaderboard** — Load top scores
- **Print Local Scores** — View console output

**4. Add UI to Scene**

Create leaderboard UI hierarchy:

```
Canvas
└── LeaderboardPanel (LeaderboardUI.cs)
    ├── TitleText (TMP_Text)
    ├── PlayerInfoPanel
    │   ├── PlayerNameText (TMP_Text)
    │   ├── PlayerRankText (TMP_Text)
    │   └── ChangeNameButton (Button)
    ├── TabButtons
    │   ├── EndlessButton (Button)
    │   ├── DailyButton (Button)
    │   └── WeeklyButton (Button)
    ├── ScrollView (ScrollRect)
    │   └── EntryContainer (Vertical Layout Group)
    ├── LoadingText (TMP_Text)
    ├── ErrorText (TMP_Text)
    ├── RefreshButton (Button)
    └── CloseButton (Button)
```

Assign all references in `LeaderboardUI` Inspector, then assign the `LeaderboardEntryPrefab`.

**5. Test in Play Mode**

Press Play and use context menu commands to test:
- Submit Test Score → Check "Print Local Scores" to verify
- Open LeaderboardUI → Verify scores display
- Click "Change Name" → Test PlayerNameDialog

---

## Architecture

### Data Flow

```
Game Event (EndlessMode game over)
    ↓
GameManager.TriggerGameOver()
    ↓
EndlessMode.PostEndlessScore()
    ↓
LeaderboardManager.SubmitEndlessScore(wave, score)
    ↓
├── SaveScoreLocally() → PlayerPrefs
└── SubmitScoreOnline() → Backend API
        ↓
    OnScoreSubmitted event
        ↓
    LeaderboardUI updates display
```

### Class Hierarchy

**LeaderboardManager** (Singleton)
- Manages all leaderboard operations
- Handles player identity (ID, name)
- Local score storage via PlayerPrefs
- Online submission routing
- Score fetching with caching
- Analytics integration

**LeaderboardEntry** (Data Structure)
```csharp
public class LeaderboardEntry
{
    public string playerId;
    public string playerName;
    public long score;
    public int rank;
    public DateTime timestamp;
    public Dictionary<string, string> metadata;
}
```

**LeaderboardScope** (Enum)
```csharp
public enum LeaderboardScope
{
    Global,    // All players worldwide
    Friends,   // Player's friends only
    Regional   // Players in same region
}
```

---

## Configuration

### LeaderboardManager Settings

**Leaderboard Control:**
- `enableLeaderboards` — Master switch for leaderboard functionality
- `enableDebugLogs` — Verbose console output for development
- `offlineMode` — Force local-only mode (no backend calls)

**Leaderboard IDs:**
- `endlessLeaderboardId` — Endless mode high scores (default: "endless_high_score")
- `dailyLeaderboardId` — Daily challenge scores (default: "daily_challenge")
- `weeklyLeaderboardId` — Weekly challenge scores (default: "weekly_challenge")

**Limits:**
- `maxLocalScores` — Maximum scores stored locally per leaderboard (default: 100)
- `maxFetchedScores` — Maximum scores fetched from backend (default: 50)

**Backend URLs (Custom HTTP only):**
- `customSubmitUrl` — POST endpoint for score submission
- `customFetchUrl` — GET endpoint for fetching scores

### PlayerPrefs Keys

The system uses the following PlayerPrefs keys:

- `LeaderboardPlayerId` — Unique player GUID (auto-generated)
- `PlayerName` — Player display name (auto-generated or custom)
- `LeaderboardScores_{leaderboardId}` — JSON array of local scores per board

**Example PlayerPrefs data:**
```json
{
  "LeaderboardPlayerId": "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6",
  "PlayerName": "CaptainMech42",
  "LeaderboardScores_endless_high_score": "[{\"playerId\":\"a1b2c3d4...\",\"playerName\":\"CaptainMech42\",\"score\":75000,\"rank\":1,\"timestamp\":\"2024-01-15T10:30:00Z\",\"metadata\":{\"wave\":\"35\"}}]"
}
```

### Name Generation

**Auto-Generated Format:** `Player{1000-9999}` (e.g., "Player4273")

**Random Name Format:** `{Prefix}{Suffix}{Number}` (e.g., "CaptainMech42")

Random name lists in `PlayerNameDialog.cs`:
- **Prefixes:** Captain, General, Commander, Major, Admiral, Chief, Mega, Super, Ultra, Hyper...
- **Suffixes:** Bot, Mech, Drone, Tank, Turret, Defender, Guardian, Warrior, Hero...

**Name Validation Rules:**
- Min length: 3 characters
- Max length: 20 characters
- Allowed characters: Letters, numbers, spaces, underscores
- Sanitization: Special characters removed automatically

---

## Backend Integration

The system supports three backend options via preprocessor directives:

### Option 1: Unity Gaming Services

**Requirements:**
- Package: `com.unity.services.leaderboards` (version 2.0+)
- Unity Gaming Services account
- Project linked to Unity Cloud Project ID

**Setup:**

1. Install Unity Gaming Services package:
```
Window → Package Manager → Unity Registry → Leaderboards → Install
```

2. Configure Project ID:
```
Edit → Project Settings → Services → Link Project
```

3. Create leaderboards in Unity Dashboard:
   - Navigate to: https://dashboard.unity3d.com
   - Select project → Leaderboards
   - Create leaderboards with IDs matching your configuration:
     - `endless_high_score`
     - `daily_challenge`
     - `weekly_challenge`

4. Enable Unity Gaming Services in code:
```csharp
// Add #define directive at top of LeaderboardManager.cs
#define UNITY_GAMING_SERVICES
```

5. Initialize in LeaderboardManager:
```csharp
private async void InitializeUnityGamingServices()
{
    await UnityServices.InitializeAsync();
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    Debug.Log("[LeaderboardManager] Unity Gaming Services initialized");
}
```

6. Submit scores:
```csharp
private IEnumerator SubmitScoreUGS(string leaderboardId, long score, Dictionary<string, string> metadata)
{
    Task<AddPlayerScoreResponse> task = LeaderboardsService.Instance.AddPlayerScoreAsync(
        leaderboardId,
        score,
        new AddPlayerScoreOptions { Metadata = metadata }
    );
    
    yield return new WaitUntil(() => task.IsCompleted);
    
    if (task.Exception != null)
        Debug.LogError($"[LeaderboardManager] UGS submit failed: {task.Exception.Message}");
}
```

**Cost:** Free tier includes 50k DAU (daily active users)

---

### Option 2: PlayFab

**Requirements:**
- PlayFab SDK (Unity Package)
- PlayFab account and Title ID

**Setup:**

1. Install PlayFab SDK:
```
Download from: https://github.com/PlayFab/UnitySDK/releases
Assets → Import Package → Custom Package → Select PlayFabSDK.unitypackage
```

2. Configure Title ID:
```csharp
// Assets/PlayFabSDK/Shared/PlayFabClientAPI.cs
public static PlayFabSettings settings = new PlayFabSettings
{
    TitleId = "YOUR_TITLE_ID_HERE"
};
```

3. Create leaderboards in PlayFab Dashboard:
   - Navigate to: https://developer.playfab.com
   - Select title → Leaderboards → Create Statistic
   - Create statistics:
     - `endless_high_score` (Aggregation: Max)
     - `daily_challenge` (Aggregation: Max, Reset: Daily)
     - `weekly_challenge` (Aggregation: Max, Reset: Weekly)

4. Enable PlayFab in code:
```csharp
// Add #define directive at top of LeaderboardManager.cs
#define PLAYFAB
```

5. Initialize:
```csharp
private void InitializePlayFab()
{
    var request = new LoginWithCustomIDRequest
    {
        CustomId = GetOrCreatePlayerId(),
        CreateAccount = true
    };
    
    PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnPlayFabError);
}
```

6. Submit scores:
```csharp
private IEnumerator SubmitScorePlayFab(string leaderboardId, long score, Dictionary<string, string> metadata)
{
    var request = new UpdatePlayerStatisticsRequest
    {
        Statistics = new List<StatisticUpdate>
        {
            new StatisticUpdate { StatisticName = leaderboardId, Value = (int)score }
        }
    };
    
    bool completed = false;
    PlayFabClientAPI.UpdatePlayerStatistics(request,
        result => { completed = true; },
        error => { Debug.LogError($"[LeaderboardManager] PlayFab error: {error.GenerateErrorReport()}"); completed = true; }
    );
    
    yield return new WaitUntil(() => completed);
}
```

**Cost:** Free tier includes 100k players/month

---

### Option 3: Custom HTTP Backend

**Requirements:**
- Your own backend server
- REST API endpoints for submit/fetch

**Backend API Specification:**

**POST /api/leaderboards/submit** — Submit score
```json
Request:
{
  "player_id": "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6",
  "leaderboard_id": "endless_high_score",
  "score": 75000,
  "player_name": "CaptainMech42",
  "metadata": {
    "wave": "35",
    "timestamp": "2024-01-15T10:30:00Z"
  }
}

Response:
{
  "success": true,
  "rank": 12,
  "message": "Score submitted successfully"
}
```

**GET /api/leaderboards/fetch** — Fetch scores
```
Request Parameters:
?leaderboard_id=endless_high_score&scope=Global&limit=50

Response:
{
  "success": true,
  "scores": [
    {
      "player_id": "...",
      "player_name": "TopPlayer",
      "score": 150000,
      "rank": 1,
      "timestamp": "2024-01-15T12:00:00Z",
      "metadata": { "wave": "75" }
    },
    ...
  ]
}
```

**Setup:**

1. Configure backend URLs in LeaderboardManager Inspector:
```
Custom Submit URL: https://yourserver.com/api/leaderboards/submit
Custom Fetch URL: https://yourserver.com/api/leaderboards/fetch
```

2. Custom backend is default (no #define needed)

3. Implementation uses UnityWebRequest:
```csharp
private IEnumerator SubmitScoreCustom(string leaderboardId, long score, Dictionary<string, string> metadata)
{
    var jsonData = new Dictionary<string, object>
    {
        { "player_id", GetOrCreatePlayerId() },
        { "leaderboard_id", leaderboardId },
        { "score", score },
        { "player_name", GetPlayerName() },
        { "metadata", metadata }
    };
    
    string jsonString = JsonUtility.ToJson(jsonData);
    
    using (UnityWebRequest request = new UnityWebRequest(customSubmitUrl, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[LeaderboardManager] Score submitted: {score}");
        }
        else
        {
            Debug.LogError($"[LeaderboardManager] Submit failed: {request.error}");
        }
    }
}
```

**Example Backend (Node.js + Express + MongoDB):**

```javascript
// server.js
const express = require('express');
const mongoose = require('mongoose');

const app = express();
app.use(express.json());

// Score schema
const scoreSchema = new mongoose.Schema({
  playerId: String,
  leaderboardId: String,
  playerName: String,
  score: Number,
  timestamp: { type: Date, default: Date.now },
  metadata: Object
});
const Score = mongoose.model('Score', scoreSchema);

// Submit endpoint
app.post('/api/leaderboards/submit', async (req, res) => {
  const { player_id, leaderboard_id, score, player_name, metadata } = req.body;
  
  // Update or insert score
  await Score.findOneAndUpdate(
    { playerId: player_id, leaderboardId: leaderboard_id },
    { score, playerName: player_name, metadata },
    { upsert: true }
  );
  
  // Calculate rank
  const rank = await Score.countDocuments({
    leaderboardId: leaderboard_id,
    score: { $gt: score }
  }) + 1;
  
  res.json({ success: true, rank });
});

// Fetch endpoint
app.get('/api/leaderboards/fetch', async (req, res) => {
  const { leaderboard_id, scope, limit } = req.query;
  
  const scores = await Score.find({ leaderboardId: leaderboard_id })
    .sort({ score: -1 })
    .limit(parseInt(limit) || 50);
  
  const rankedScores = scores.map((s, i) => ({
    player_id: s.playerId,
    player_name: s.playerName,
    score: s.score,
    rank: i + 1,
    timestamp: s.timestamp,
    metadata: s.metadata
  }));
  
  res.json({ success: true, scores: rankedScores });
});

app.listen(3000, () => console.log('Leaderboard server running on port 3000'));
```

---

## UI Components

### LeaderboardUI

Main panel displaying leaderboard scores with tab switching.

**Key Methods:**

```csharp
// Show specific leaderboard
leaderboardUI.ShowLeaderboard("endless_high_score");

// Show endless mode (shortcut)
leaderboardUI.ShowEndlessLeaderboard();

// Hide panel
leaderboardUI.Hide();

// Refresh current board (clears cache)
leaderboardUI.RefreshCurrentLeaderboard();
```

**Inspector Configuration:**

- **References:** Panel, entry container, entry prefab, title text, loading/error text, close/refresh buttons
- **Tab Buttons:** Endless, daily, weekly tab buttons
- **Player Info:** Player name text, player rank text, change name button
- **Scroll Settings:** Scroll rect, max displayed entries (default: 50)
- **Highlight Colors:** Player entry color (gold), top 3 color (blue)

**Events:**

- Subscribes to `LeaderboardManager.OnScoresLoaded`
- Subscribes to `LeaderboardManager.OnError`

---

### LeaderboardEntryUI

Individual score row component (prefab).

**Data Display:**

- Rank (formatted: "#1", "1st", "2nd", "3rd")
- Player name
- Score (formatted with commas: "75,000")
- Metadata (wave number or challenge date)
- Background color (player highlight, top 3 tint)
- Rank icons (optional gold/silver/bronze medals)

**Visual Customization:**

- Gold color for 1st place text
- Silver color for 2nd place text
- Bronze color for 3rd place text
- Optional rank medal sprites

**Setup Prefab:**

```
LeaderboardEntryPrefab (GameObject)
├── BackgroundImage (Image)
├── RankIcon (Image) [Optional]
├── RankText (TMP_Text)
├── NameText (TMP_Text)
├── ScoreText (TMP_Text)
└── MetadataText (TMP_Text)
```

Assign all components to `LeaderboardEntryUI` script, then assign prefab to `LeaderboardUI.entryPrefab`.

---

### PlayerNameDialog

Name input dialog shown on first launch or when changing name.

**Key Methods:**

```csharp
// Show dialog for first launch (no cancel button)
PlayerNameDialog.Instance.Show(
    isFirstLaunch: true,
    currentName: "",
    onConfirm: (name) => Debug.Log($"Name set: {name}")
);

// Show dialog for name change (with cancel)
PlayerNameDialog.Instance.Show(
    isFirstLaunch: false,
    currentName: "CaptainMech42",
    onConfirm: (name) => UpdateUI()
);

// Hide dialog
PlayerNameDialog.Instance.Hide();
```

**Inspector Configuration:**

- **References:** Dialog panel, name input field, error text, character count text, buttons
- **Validation:** Min name length (default: 3), max name length (default: 20)
- **Allow Skip:** Allow first launch skip (default: false)
- **Random Names:** Prefix/suffix arrays for name generation

**Auto-Show Behavior:**

The dialog automatically shows on first launch if player name is auto-generated (starts with "Player").

Disable auto-show by commenting out:
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void CheckFirstLaunch() { ... }
```

---

## Testing

### Local Testing (Offline)

**1. Enable Offline Mode**

Select `LeaderboardManager` → Enable `Offline Mode`

**2. Submit Test Scores**

Right-click `LeaderboardManager` in Inspector:
- **Submit Test Score** — Submits random score (1000-100000)
- Repeat 5-10 times to populate local leaderboard

**3. View Local Scores**

Right-click `LeaderboardManager` → **Print Local Scores**

Console output:
```
[LeaderboardManager] Local scores for endless_high_score:
1. Player4273 - 95000 (Wave 42)
2. Player8192 - 87000 (Wave 38)
3. Player1234 - 75000 (Wave 35)
...
```

**4. Test UI**

- Open LeaderboardUI in scene
- Should display local scores
- Test tab switching
- Test name change dialog

**5. Clear Data**

Right-click `LeaderboardManager` → **Clear All Data**

---

### Online Testing

**1. Disable Offline Mode**

Select `LeaderboardManager` → Uncheck `Offline Mode`

**2. Configure Backend**

Choose one of three options (see [Backend Integration](#backend-integration)):
- Unity Gaming Services (#define UNITY_GAMING_SERVICES)
- PlayFab (#define PLAYFAB)
- Custom HTTP (default, set customSubmitUrl/customFetchUrl)

**3. Monitor Network Requests**

Enable debug logs:
```csharp
LeaderboardManager.Instance.enableDebugLogs = true;
```

Console output:
```
[LeaderboardManager] Submitting score 75000 to endless_high_score
[LeaderboardManager] Score submitted successfully
[LeaderboardManager] Fetching leaderboard endless_high_score (Global, limit 50)
[LeaderboardManager] Loaded 50 scores from online
```

**4. Test Cache Behavior**

- Submit score → Fetch leaderboard (loads from backend)
- Wait 2 minutes → Fetch again (returns cached)
- Wait 6 minutes → Fetch again (cache expired, loads from backend)
- Click Refresh button → Force clears cache

**5. Test Error Handling**

- Disconnect internet → Submit score (should save locally and retry)
- Invalid backend URL → Should show error in LeaderboardUI
- Backend timeout → Should fallback to local scores

---

### EndlessMode Integration Testing

**1. Start Endless Mode**

- Play through normal waves until endless mode activates
- Or force-start: `EndlessMode.Instance.StartEndless()`

**2. Let Game End**

- Lose all lives → GameOver
- Or win final wave → Victory (if endless is disabled)

**3. Verify Score Submission**

Console output:
```
[EndlessMode] Final endless score: 75000 (endless portion: 50000, wave: 35)
[LeaderboardManager] Submitting score 75000 to endless_high_score
[AnalyticsManager] Event tracked: leaderboard_score_submit
```

**4. Check Leaderboard**

- Open LeaderboardUI
- Should display newly submitted score
- Player's entry should be highlighted in gold
- Rank should be calculated correctly

---

### Unit Test Examples

```csharp
using NUnit.Framework;
using RobotTD.Online;

public class LeaderboardTests
{
    [Test]
    public void SubmitScore_AddsToLocalScores()
    {
        // Arrange
        LeaderboardManager.Instance.ClearCache();
        
        // Act
        LeaderboardManager.Instance.SubmitEndlessScore(wave: 10, score: 5000);
        
        // Assert
        var localScores = LeaderboardManager.Instance.GetLocalScores("endless_high_score");
        Assert.IsNotNull(localScores);
        Assert.Greater(localScores.Count, 0);
        Assert.AreEqual(5000, localScores[0].score);
    }
    
    [Test]
    public void GetPlayerScore_ReturnsCorrectEntry()
    {
        // Arrange
        LeaderboardManager.Instance.SubmitEndlessScore(wave: 20, score: 15000);
        
        // Act
        var playerScore = LeaderboardManager.Instance.GetPlayerScore("endless_high_score");
        
        // Assert
        Assert.IsNotNull(playerScore);
        Assert.AreEqual(15000, playerScore.score);
    }
    
    [Test]
    public void SetPlayerName_SanitizesInput()
    {
        // Act
        LeaderboardManager.Instance.SetPlayerName("Test@Player#123!");
        
        // Assert
        string sanitized = LeaderboardManager.Instance.GetPlayerName();
        Assert.AreEqual("TestPlayer123", sanitized);
    }
}
```

---

## Best Practices

### 1. **Always Use Offline-First Design**

Mobile games face unreliable network conditions. Ensure all features work offline first.

```csharp
// ✅ Good: Save locally first, sync later
LeaderboardManager.Instance.SubmitEndlessScore(wave, score);
// → Saves to PlayerPrefs immediately
// → Attempts online submission asynchronously
// → Game continues normally even if online fails

// ❌ Bad: Block on network request
yield return WaitForOnlineSubmission(); // Freezes game if network is slow
```

---

### 2. **Respect Cache Lifetimes**

Caching reduces backend costs and improves UX. Use 5-minute default or customize.

```csharp
// ✅ Good: Let cache work
var scores = LeaderboardManager.Instance.FetchLeaderboard("endless", scope, limit);
// → Returns cached if less than 5 minutes old
// → Only hits backend when needed

// ❌ Bad: Clear cache on every fetch
LeaderboardManager.Instance.ClearCache();
var scores = LeaderboardManager.Instance.FetchLeaderboard(...);
// → Always hits backend, increases costs
```

---

### 3. **Validate Names on Client and Server**

Client-side validation provides instant feedback. Server-side validation prevents abuse.

**Client (LeaderboardManager.SanitizeName):**
```csharp
// Removes special chars, enforces length
string clean = Regex.Replace(name, @"[^a-zA-Z0-9_\s]", "");
return clean.Substring(0, Mathf.Min(clean.Length, 20));
```

**Server (backend validation):**
```javascript
// Additional checks
if (name.length < 3 || name.length > 20)
  return res.status(400).json({ error: "Invalid name length" });

if (/badword/.test(name))
  return res.status(400).json({ error: "Name contains prohibited content" });
```

---

### 4. **Use Metadata for Contextual Information**

Metadata provides rich data without cluttering score structure.

```csharp
// ✅ Good: Use metadata dictionary
var metadata = new Dictionary<string, string>
{
    { "wave", wave.ToString() },
    { "difficulty", "hard" },
    { "timestamp", DateTime.UtcNow.ToString("o") }
};
LeaderboardManager.Instance.SubmitScore("endless", score, metadata);

// UI can display: "Wave 35 (Hard Mode)"
// Analytics can track: Difficulty distribution
// Backend can filter: By time period
```

---

### 5. **Handle Errors Gracefully**

Network failures are normal on mobile. Handle them without disrupting gameplay.

```csharp
// LeaderboardManager automatically:
// 1. Saves scores locally first (always succeeds)
// 2. Attempts online submission asynchronously
// 3. Fires OnError event on failure (UI can show toast)
// 4. Retries on next app launch (background sync)

// In your UI:
LeaderboardManager.Instance.OnError += (message) => {
    // Show non-intrusive notification
    ToastUI.Show("Leaderboard temporarily unavailable");
    // Don't block gameplay or show scary error messages
};
```

---

### 6. **Track Engagement with Analytics**

Leaderboards generate valuable engagement metrics.

```csharp
// Automatically tracked by LeaderboardManager:
// - "leaderboard_score_submit" (leaderboard_id, score)

// Add your own tracking:
AnalyticsManager.Instance.TrackEvent("leaderboard_viewed", new Dictionary<string, object>
{
    { "leaderboard_id", "endless_high_score" },
    { "player_rank", playerRank }
});

AnalyticsManager.Instance.TrackEvent("leaderboard_name_changed", new Dictionary<string, object>
{
    { "new_name", newName }
});

// Key metrics to monitor:
// - Score submission rate (% of games that submit)
// - Leaderboard view rate (% of players who open leaderboards)
// - Name change rate (player engagement signal)
// - Score improvement over time (retention indicator)
```

---

### 7. **Implement Daily/Weekly Challenge Rotation**

Use leaderboard IDs with date suffixes for time-based challenges.

```csharp
// Generate dynamic leaderboard ID
string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
string leaderboardId = $"daily_challenge_{today}";

LeaderboardManager.Instance.SubmitScore(leaderboardId, score, metadata);

// Backend cleans up old leaderboards (7+ days)
// Players only see today's standings
```

---

### 8. **Optimize Backend Queries**

Leaderboard queries can be expensive. Use indexes and limits.

**MongoDB Indexes:**
```javascript
// Index on leaderboardId + score (descending)
db.scores.createIndex({ leaderboardId: 1, score: -1 });

// Rank queries now fast:
db.scores.find({ leaderboardId: "endless" }).sort({ score: -1 }).limit(50);
```

**Limit Fetch Sizes:**
```csharp
// ✅ Good: Reasonable limit
FetchLeaderboard("endless", scope, 50); // Top 50 is plenty for UI

// ❌ Bad: Excessive fetch
FetchLeaderboard("endless", scope, 10000); // Slow, expensive, wasteful
```

---

### 9. **Prevent Cheating**

Client-submitted scores are vulnerable. Add server-side validation.

**Score Validation:**
```javascript
// Server-side sanity checks
if (score > 1000000) // Unrealistic for your game
  return res.status(400).json({ error: "Score too high" });

if (metadata.wave && score / metadata.wave < 100) // Score/wave ratio check
  return res.status(400).json({ error: "Invalid score for wave" });
```

**Rate Limiting:**
```javascript
// Prevent spam
const recentSubmissions = await Score.countDocuments({
  playerId: player_id,
  timestamp: { $gt: new Date(Date.now() - 60000) } // Last minute
});

if (recentSubmissions > 5)
  return res.status(429).json({ error: "Too many submissions" });
```

**Timestamp Validation:**
```javascript
// Check submission timestamp is reasonable
const gameStartTime = metadata.game_start_time;
const gameDuration = Date.now() - new Date(gameStartTime);

if (gameDuration < 60000) // Game shorter than 1 minute
  return res.status(400).json({ error: "Game too short" });
```

---

### 10. **Test with Real-World Conditions**

Simulate poor network conditions during testing.

**Unity Network Simulation:**
```csharp
// Simulate 3G connection (300ms latency, 10% packet loss)
Application.backgroundLoadingPriority = ThreadPriority.Low;

// Or use Unity Profiler → Network → Network Emulation
```

**Chrome DevTools (for WebGL):**
```
F12 → Network tab → Throttling → Slow 3G
```

**Test Scenarios:**
- ✅ Airplane mode (offline submit → online sync later)
- ✅ Slow 3G (5+ second backend response)
- ✅ Intermittent connection (request fails mid-flight)
- ✅ Backend timeout (30+ second no response)
- ✅ Backend error 500 (server crash)

---

## Troubleshooting

### Problem: Leaderboard not showing scores

**Solution:**

1. Check LeaderboardManager is in scene:
```csharp
if (LeaderboardManager.Instance == null)
    Debug.LogError("LeaderboardManager not found in scene!");
```

2. Verify leaderboard ID matches:
```csharp
// Ensure IDs are consistent
LeaderboardManager: endlessLeaderboardId = "endless_high_score"
LeaderboardUI: ShowLeaderboard("endless_high_score")
```

3. Enable debug logs:
```csharp
LeaderboardManager.Instance.enableDebugLogs = true;
```

4. Check local scores exist:
```csharp
// Right-click LeaderboardManager → Print Local Scores
var scores = LeaderboardManager.Instance.GetLocalScores("endless_high_score");
Debug.Log($"Local scores count: {scores?.Count ?? 0}");
```

---

### Problem: Scores not submitting online

**Solution:**

1. Verify offline mode is disabled:
```csharp
LeaderboardManager.Instance.SetOfflineMode(false);
```

2. Check backend configuration:
```csharp
// For custom HTTP backend, ensure URLs are set
Debug.Log($"Submit URL: {LeaderboardManager.customSubmitUrl}");
Debug.Log($"Fetch URL: {LeaderboardManager.customFetchUrl}");
```

3. Test backend manually:
```bash
# Test POST submit endpoint
curl -X POST https://yourserver.com/api/leaderboards/submit \
  -H "Content-Type: application/json" \
  -d '{"player_id":"test","leaderboard_id":"endless_high_score","score":5000,"player_name":"Test"}'

# Expected response:
# {"success":true,"rank":1}
```

4. Check Unity console for errors:
```
[LeaderboardManager] Submit failed: Could not resolve host
[LeaderboardManager] Submit failed: Connection timeout

# → Indicates network or URL configuration issue
```

---

### Problem: Player name not saving

**Solution:**

1. Verify PlayerPrefs write permissions (Android/iOS):
```csharp
// Test write
PlayerPrefs.SetString("TestKey", "TestValue");
PlayerPrefs.Save();

// Test read
string value = PlayerPrefs.GetString("TestKey");
Debug.Log($"Test value: {value}"); // Should print "TestValue"
```

2. Check name validation:
```csharp
// Names with special chars get sanitized
LeaderboardManager.Instance.SetPlayerName("Test@Player#123!");
// Becomes: "TestPlayer123"
```

3. Ensure PlayerPrefs.Save() is called:
```csharp
// LeaderboardManager.SetPlayerName() calls this automatically
// But if you modify PlayerPrefs directly, call:
PlayerPrefs.Save();
```

---

### Problem: Cache not expiring

**Solution:**

1. Verify system time is correct:
```csharp
Debug.Log($"Current time: {DateTime.UtcNow}");
Debug.Log($"Cache time: {cacheExpiration[leaderboardId]}");
```

2. Force clear cache:
```csharp
LeaderboardManager.Instance.ClearCache();
```

3. Check cache expiration logic:
```csharp
// Default: 5 minutes
TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

// Customize in LeaderboardManager.cs:
private bool IsCacheValid(string leaderboardId)
{
    return cacheExpiration.TryGetValue(leaderboardId, out DateTime expiry)
        && DateTime.UtcNow < expiry;
}
```

---

### Problem: Rank not calculating correctly

**Solution:**

1. Ensure scores are sorted descending:
```csharp
localScores[leaderboardId].Sort((a, b) => b.score.CompareTo(a.score));
```

2. Verify rank assignment (1-based):
```csharp
for (int i = 0; i < scores.Count; i++)
{
    scores[i].rank = i + 1; // 1st place = rank 1
}
```

3. Check for duplicate scores:
```csharp
// Duplicate scores get same rank (handled by backend)
// But local ranking assigns sequential ranks
```

---

### Problem: Analytics events not tracking

**Solution:**

1. Verify AnalyticsManager is initialized:
```csharp
if (AnalyticsManager.Instance == null)
    Debug.LogError("AnalyticsManager not found!");
```

2. Check event is being tracked:
```csharp
// LeaderboardManager.SubmitScore() calls:
AnalyticsManager.Instance?.TrackEvent("leaderboard_score_submit", new Dictionary<string, object>
{
    { "leaderboard_id", leaderboardId },
    { "score", score }
});
```

3. Enable analytics debug mode:
```csharp
AnalyticsManager.Instance.enableDebugLogs = true;
```

4. Check AnalyticsDashboard:
```
Tools → Robot TD → Analytics Dashboard → Realtime Events
# Should show "leaderboard_score_submit" events
```

---

### Problem: UI not updating after score submission

**Solution:**

1. Ensure LeaderboardUI subscribes to events:
```csharp
private void OnEnable()
{
    LeaderboardManager.Instance.OnScoresLoaded += OnScoresLoaded;
    LeaderboardManager.Instance.OnError += OnLeaderboardError;
}
```

2. Force refresh after submission:
```csharp
LeaderboardManager.Instance.SubmitEndlessScore(wave, score);
yield return new WaitForSeconds(1f); // Wait for local save
leaderboardUI.RefreshCurrentLeaderboard();
```

3. Check event is being fired:
```csharp
// In LeaderboardManager.FetchLeaderboard():
OnScoresLoaded?.Invoke(leaderboardId, scores);
// → Should trigger LeaderboardUI.OnScoresLoaded()
```

---

### Problem: First launch dialog not showing

**Solution:**

1. Verify PlayerNameDialog is in scene:
```csharp
if (PlayerNameDialog.Instance == null)
    Debug.LogError("PlayerNameDialog not found in scene!");
```

2. Check auto-show logic:
```csharp
// PlayerNameDialog.CheckFirstLaunch() runs on scene load
// Shows dialog if player name is auto-generated (starts with "Player")
```

3. Manually trigger:
```csharp
PlayerNameDialog.Instance?.Show(isFirstLaunch: true);
```

4. Check PlayerPrefs key:
```csharp
string playerName = PlayerPrefs.GetString("PlayerName", "");
Debug.Log($"Current player name: {playerName}");
// If starts with "Player" and length < 10 → triggers dialog
```

---

## API Reference

### LeaderboardManager

**Initialization:**
- `InitializeLeaderboards()` — Setup player ID, load local scores, init backend

**Score Submission:**
- `SubmitScore(leaderboardId, score, metadata)` — Submit to any leaderboard
- `SubmitEndlessScore(wave, score)` — Specialized endless mode submission

**Score Fetching:**
- `FetchLeaderboard(leaderboardId, scope, maxResults)` — Get top scores
- `FetchPlayerRank(leaderboardId)` — Get player's rank
- `FetchScoresNearPlayer(leaderboardId, range)` — Get ±range scores near player

**Local Scores:**
- `GetLocalScores(leaderboardId)` — Get all local scores for board
- `GetPlayerScore(leaderboardId)` — Get player's score for board

**Player Identity:**
- `GetPlayerId()` — Get unique player GUID
- `GetPlayerName()` — Get player display name
- `SetPlayerName(newName)` — Set player name (sanitized)

**Cache Management:**
- `ClearCache()` — Clear all cached online scores
- `IsCacheValid(leaderboardId)` — Check if cache is still valid

**Mode Control:**
- `IsOfflineMode()` — Check if in offline mode
- `SetOfflineMode(enabled)` — Enable/disable offline mode

**Events:**
- `OnScoreSubmitted(leaderboardId, success)` — Score submission complete
- `OnScoresLoaded(leaderboardId, entries)` — Scores fetched successfully
- `OnError(message)` — Error occurred

---

### LeaderboardUI

**Display:**
- `ShowLeaderboard(leaderboardId)` — Show specific leaderboard
- `ShowEndlessLeaderboard()` — Show endless mode (shortcut)
- `Hide()` — Hide panel

**Refresh:**
- `RefreshCurrentLeaderboard()` — Reload current board (clears cache)

---

### PlayerNameDialog

**Display:**
- `Show(isFirstLaunch, currentName, onConfirm)` — Show dialog
- `Hide()` — Hide dialog

---

## Summary

The Leaderboard System provides a complete competitive scoring solution with:

✅ **Offline-first reliability** — Works without internet, syncs when available  
✅ **Flexible backend support** — Unity Gaming Services, PlayFab, or custom HTTP  
✅ **Rich UI components** — Ready-to-use panels and dialogs  
✅ **Analytics integration** — Automatic engagement tracking  
✅ **Production-ready** — Caching, error handling, name validation  

For questions or issues, refer to the [Troubleshooting](#troubleshooting) section or enable debug logs in LeaderboardManager.

---

**Related Documentation:**
- [ANALYTICS_GUIDE.md](ANALYTICS_GUIDE.md) — Event tracking and metrics
- [README.md](README.md) — Project overview
- [TESTING_GUIDE.md](TESTING_GUIDE.md) — Testing procedures

**Version:** 1.0  
**Last Updated:** January 2024
