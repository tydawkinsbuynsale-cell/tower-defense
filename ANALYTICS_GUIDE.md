# Analytics System

**Robot Tower Defense - Analytics & Telemetry**

Complete analytics and telemetry system for tracking player behavior, performance metrics, and engagement.

---

## 🎯 Overview

The Analytics System provides comprehensive tracking of:
- **Session Management** - Track sessions, play time, retention
- **Gameplay Events** - Tower placement, waves, enemies, progression
- **Performance Metrics** - FPS, memory, battery, quality settings
- **Progression Tracking** - Achievements, tech tree, level-ups
- **Error Logging** - Crashes, exceptions, errors
- **Monetization** - IAP tracking (for future implementation)

---

## 📦 Components

### AnalyticsManager.cs
**Location:** `Assets/Scripts/Analytics/AnalyticsManager.cs`

**Purpose:** Core analytics singleton that tracks all events and manages sessions.

**Key Features:**
- Auto session management (start/end/timeout)
- Event tracking with parameters
- Performance metrics sampling
- Error/crash logging
- User properties
- Backend integration ready

### AnalyticsEvents.cs
**Location:** `Assets/Scripts/Analytics/AnalyticsEvents.cs`

**Purpose:** Centralized event name constants to prevent typos.

**Categories:**
- Session events
- Gameplay events
- Tower/enemy events
- Progression events
- Tutorial events
- UI events
- Monetization events
- Performance events
- Error events

---

## 🚀 Setup

### 1. Add Manager to Scene

The AnalyticsManager is a singleton that persists across scenes (DontDestroyOnLoad).

**Option A: Add to Bootstrapper**
```csharp
// In SceneBootstrapper.cs or GameManager.cs
void Awake()
{
    if (AnalyticsManager.Instance == null)
    {
        var analyticsObj = new GameObject("AnalyticsManager");
        analyticsObj.AddComponent<AnalyticsManager>();
    }
}
```

**Option B: Manual Setup**
1. Create empty GameObject in first scene
2. Name it "AnalyticsManager"
3. Add `AnalyticsManager` component
4. Configure settings in Inspector:
   - **Enable Analytics:** true (production), false (development)
   - **Enable Debug Logs:** true (development), false (production)
   - **Send In Editor:** false (don't track in-editor testing)
   - **Session Timeout Minutes:** 5 (how long before new session)

### 2. Configure Settings

**Inspector Settings:**

```
Enable Analytics: ✓ (check for production builds)
Enable Debug Logs: □ (uncheck for production)
Send In Editor: □ (leave unchecked)
Session Timeout Minutes: 5
```

**Build Settings:**

For production builds, ensure:
```csharp
#if UNITY_EDITOR
    [SerializeField] private bool sendInEditor = false;
#else
    [SerializeField] private bool sendInEditor = true;
#endif
```

### 3. Integration (Already Done)

Analytics tracking is already integrated into:
- ✅ GameManager (game start, game end, victory, defeat)
- ✅ WaveManager (wave start, wave complete)
- ✅ TowerPlacementManager (tower placed)
- ✅ AchievementManager (achievement unlocked)
- ✅ PerformanceManager (quality changes)

---

## 📊 Tracked Events

### Session Events

**session_start**
```csharp
Parameters:
- session_id: Unique GUID
- session_number: Lifetime session count
- is_new_user: First time player
- platform: iOS, Android, etc.
- device_model: Device name
- os_version: Operating system
```

**session_end**
```csharp
Parameters:
- session_id: Same as session_start
- duration_seconds: Session length
- total_play_time: Lifetime play time
```

### Gameplay Events

**game_start**
```csharp
Parameters:
- map_name: Map identifier
- difficulty: Difficulty level
- is_tutorial: Tutorial mode flag
```
**Triggered:** GameManager.InitializeGame()

**game_end**
```csharp
Parameters:
- result: "victory", "defeat", or "quit"
- final_wave: Last wave reached
- final_score: Final score
- credits_earned: Credits earned this session
- play_time_seconds: Game duration
```
**Triggered:** GameManager.TriggerGameOver() or TriggerVictory()

**wave_started**
```csharp
Parameters:
- wave_number: Current wave index
- enemies_count: Number of enemies
```
**Triggered:** WaveManager.StartNextWave()

**wave_complete**
```csharp
Parameters:
- wave_number: Completed wave index
- enemies_killed: Total enemies killed
- lives_remaining: Lives left
- credits_earned: Credits from wave
```
**Triggered:** WaveManager.CompleteWave()

### Tower Events

**tower_placed**
```csharp
Parameters:
- tower_type: TowerType enum as string
- tower_level: Starting level (always 1)
- cost: Credit cost
- position_x: X coordinate
- position_y: Z coordinate (2D position on map)
```
**Triggered:** TowerPlacementManager.PlaceTower()

**tower_upgraded**
```csharp
Parameters:
- tower_type: TowerType enum as string
- old_level: Level before upgrade
- new_level: Level after upgrade
- cost: Upgrade credit cost
```
**Triggered:** Tower.Upgrade() method

**tower_sold**
```csharp
Parameters:
- tower_type: TowerType enum as string
- level: Tower level when sold
- refund: Credits received
```
**Triggered:** Tower.Sell() method

### Progression Events

**achievement_unlocked**
```csharp
Parameters:
- achievement_id: AchievementId enum as string
- achievement_name: Achievement title
- session_number: Session when unlocked
```
**Triggered:** AchievementManager.TryUnlock()

**tech_upgraded**
```csharp
Parameters:
- upgrade_name: TechUpgrade enum as string
- new_level: New upgrade level
- cost: Tech points spent
```
**Triggered:** TechTree.UnlockUpgrade()

**tutorial_step**
```csharp
Parameters:
- step_number: Step index (1-9)
- step_name: Step description
- completed: true/false
```
**Triggered:** TutorialManager.CompleteStep()

**tutorial_complete**
```csharp
Parameters:
- completion_time_seconds: Total tutorial duration
```
**Triggered:** TutorialManager.CompleteTutorial()

### Performance Events

**quality_changed**
```csharp
Parameters:
- old_preset: Previous QualityPreset
- new_preset: New QualityPreset
- reason: "manual", "auto", "battery"
```
**Triggered:** PerformanceManager.ApplyQualityPreset()

**battery_save_activated**
```csharp
Parameters:
- battery_level: Battery percentage (0-100)
```
**Triggered:** PerformanceManager.BatteryMonitor()

**performance_sample**
```csharp
Parameters:
- avg_fps: Average FPS
- min_fps: Minimum FPS
- avg_frame_time_ms: Average frame time
- memory_mb: Memory usage in MB
- quality_level: Unity quality level
```
**Triggered:** Manual call (for periodic sampling)

### Error Events

**error_logged**
```csharp
Parameters:
- error_type: Error or Exception
- error_message: Error text
- stack_trace: Full stack trace
- error_count: Session error count
```
**Triggered:** Automatically via Application.logMessageReceived

**crash**
```csharp
Parameters:
- crash_message: Crash description
- stack_trace: Full stack trace
- crash_count: Lifetime crash count
```
**Triggered:** Manual call before crash

---

## 💻 Usage Examples

### Track Custom Events

```csharp
using RobotTD.Analytics;

// Simple event
AnalyticsManager.Instance?.TrackEvent(AnalyticsEvents.BUTTON_CLICKED);

// Event with single parameter
AnalyticsManager.Instance?.TrackEvent(
    AnalyticsEvents.MENU_OPENED,
    "menu_name",
    "settings"
);

// Event with multiple parameters
AnalyticsManager.Instance?.TrackEvent(
    AnalyticsEvents.ENEMY_KILLED,
    new Dictionary<string, object>
    {
        { "enemy_type", EnemyType.Scout.ToString() },
        { "wave_number", currentWave },
        { "credits_earned", 10 }
    }
);
```

### Track Gameplay Events

```csharp
// Game start (already integrated in GameManager)
AnalyticsManager.Instance?.TrackGameStart("City_01", 1, false);

// Game end (already integrated in GameManager)
AnalyticsManager.Instance?.TrackGameEnd("victory", 15, 5000, 1200, 480f);

// Wave complete (already integrated in WaveManager)
AnalyticsManager.Instance?.TrackWaveComplete(5, 25, 18, 500);

// Tower placed (already integrated in TowerPlacementManager)
AnalyticsManager.Instance?.TrackTowerPlaced(
    TowerType.LaserTower.ToString(),
    1,
    150,
    new Vector2(10.5f, 5.2f)
);
```

### Track Progression

```csharp
// Achievement (already integrated in AchievementManager)
AnalyticsManager.Instance?.TrackAchievementUnlock(
    AchievementId.FirstVictory.ToString(),
    "First Victory"
);

// Tech tree upgrade
AnalyticsManager.Instance?.TrackTechUpgrade(
    TechUpgrade.DamageBoost.ToString(),
    2,
    100
);

// Tutorial progress
AnalyticsManager.Instance?.TrackTutorialStep(3, "Place First Tower", true);
```

### Track Performance

```csharp
// Quality change (already integrated in PerformanceManager)
AnalyticsManager.Instance?.TrackQualityChange("Medium", "Low", "battery");

// Battery save activation
AnalyticsManager.Instance?.TrackBatterySaveActivated(18f);

// Performance sample (call periodically)
AnalyticsManager.Instance?.TrackPerformanceMetrics(
    avgFPS: 58.5f,
    minFPS: 45.2f,
    avgFrameTime: 17.1f,
    memoryMB: 456.3f
);
```

### Get Session Info

```csharp
// Get current session ID
string sessionId = AnalyticsManager.Instance?.GetSessionId();

// Get session statistics
var sessionInfo = AnalyticsManager.Instance?.GetSessionInfo();
Debug.Log($"Session #{sessionInfo["session_number"]}");
Debug.Log($"Duration: {sessionInfo["session_duration"]}s");
Debug.Log($"Total play time: {sessionInfo["total_play_time"]}s");

// Check if new user
bool isNew = AnalyticsManager.Instance?.IsNewUser() ?? false;
```

---

## 🔌 Backend Integration

### Unity Analytics

```csharp
// In AnalyticsManager.cs, SendEventToBackend method:

#if UNITY_ANALYTICS
using UnityEngine.Analytics;

private void SendEventToBackend(string eventName, Dictionary<string, object> parameters)
{
    // Convert to Unity Analytics format
    Analytics.CustomEvent(eventName, parameters);
}
#endif
```

**Setup:**
1. Enable Unity Analytics in Services window
2. Import Unity Analytics package
3. Add `UNITY_ANALYTICS` scripting define symbol
4. Build and deploy

### Firebase Analytics

```csharp
#if FIREBASE_ANALYTICS
using Firebase.Analytics;

private void SendEventToBackend(string eventName, Dictionary<string, object> parameters)
{
    // Convert parameters to Firebase format
    var firebaseParams = new Parameter[parameters.Count];
    int index = 0;
    foreach (var kvp in parameters)
    {
        firebaseParams[index++] = new Parameter(kvp.Key, kvp.Value.ToString());
    }
    
    FirebaseAnalytics.LogEvent(eventName, firebaseParams);
}
#endif
```

**Setup:**
1. Import Firebase SDK
2. Add `google-services.json` (Android) or `GoogleService-Info.plist` (iOS)
3. Add `FIREBASE_ANALYTICS` scripting define symbol
4. Build and deploy

### Custom Backend

```csharp
private void SendEventToBackend(string eventName, Dictionary<string, object> parameters)
{
    StartCoroutine(SendToServer(eventName, parameters));
}

private IEnumerator SendToServer(string eventName, Dictionary<string, object> parameters)
{
    string url = "https://your-analytics-server.com/track";
    
    // Create JSON payload
    var payload = new Dictionary<string, object>
    {
        { "event", eventName },
        { "parameters", parameters },
        { "timestamp", DateTime.UtcNow.ToString("o") },
        { "session_id", sessionId },
        { "user_id", SystemInfo.deviceUniqueIdentifier }
    };
    
    string json = JsonUtility.ToJson(payload);
    
    using (UnityWebRequest www = UnityWebRequest.Post(url, json))
    {
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Analytics send failed: {www.error}");
        }
    }
}
```

---

## 📈 Key Metrics to Monitor

### Engagement Metrics
- **DAU/MAU** - Daily/Monthly Active Users
- **Session Length** - Average session duration
- **Session Frequency** - Sessions per user per day
- **Retention** - D1, D7, D30 return rates

### Progression Metrics
- **Tutorial Completion Rate** - % who finish tutorial
- **Wave Progression** - How far players get
- **Achievement Unlock Rate** - % unlocking each achievement
- **Tech Tree Use** - Which upgrades are popular

### Monetization Metrics (Future)
- **Conversion Rate** - % users who purchase
- **ARPU** - Average Revenue Per User
- **ARPPU** - Average Revenue Per Paying User
- **LTV** - Lifetime Value

### Performance Metrics
- **Average FPS** - Across devices
- **Crash Rate** - Crashes per session
- **Error Rate** - Errors per session
- **Memory Usage** - Peak memory by device

### Gameplay Balance
- **Win Rate** - % games won vs lost
- **Tower Usage** - Which towers placed most
- **Difficulty Curve** - Where players struggle
- **Session End Points** - Where players quit

---

## 🛠️ Debugging & Testing

### Enable Debug Logging

In Inspector:
```
Enable Analytics: ✓
Enable Debug Logs: ✓  ← Turn this on
Send In Editor: □
```

Console output will show:
```
[AnalyticsManager] Analytics initialized - New User: False, Session: 42
[AnalyticsManager] New session started: 550e8400-e29b-41d4-a716-446655440000
[Analytics] game_start | map_name=City_01, difficulty=1, is_tutorial=False
[Analytics] wave_started | wave_number=1, enemies_count=10
[Analytics] tower_placed | tower_type=LaserTower, cost=150
```

### Context Menu Commands

Right-click AnalyticsManager component:
- **Print Session Info** - Log current session details
- **Reset Analytics Data** - Clear all saved analytics data

### Test Events

```csharp
// In test script
void TestAnalytics()
{
    // Test session tracking
    Debug.Log($"Session ID: {AnalyticsManager.Instance.GetSessionId()}");
    Debug.Log($"Session #: {AnalyticsManager.Instance.GetSessionNumber()}");
    
    // Test event tracking
    AnalyticsManager.Instance.TrackEvent("test_event", new Dictionary<string, object>
    {
        { "test_param", "test_value" },
        { "test_number", 42 }
    });
    
    // Test gameplay tracking
    AnalyticsManager.Instance.TrackGameStart("TestMap", 1, false);
    AnalyticsManager.Instance.TrackWaveComplete(1, 10, 20, 100);
    AnalyticsManager.Instance.TrackGameEnd("victory", 15, 5000, 1000, 300f);
}
```

---

## 📝 Best Practices

### Event Naming
- Use snake_case for event names: `tower_placed`, not `TowerPlaced`
- Use constants from AnalyticsEvents.cs
- Be consistent across platforms

### Parameter Naming
- Use descriptive keys: `tower_type`, not `type`
- Use consistent units: always seconds for time, always MB for memory
- Include context: `final_score`, not just `score`

### Performance
- Batch events if possible
- Avoid tracking every frame
- Use sampling for performance metrics
- Don't track PII (personally identifiable information)

### Privacy
- Ask for tracking consent if required by region (GDPR, CCPA)
- Anonymize user identifiers
- Don't track sensitive data
- Provide opt-out mechanism

### Testing
- Always test with `SendInEditor = false` during development
- Use debug logs extensively
- Verify events reach backend before production
- Test on real devices, not just editor

---

## 🚀 Production Checklist

Before releasing:

- [ ] Analytics backend configured (Unity/Firebase/Custom)
- [ ] `Enable Analytics` set to true
- [ ] `Enable Debug Logs` set to false
- [ ] `Send In Editor` set to false
- [ ] Privacy policy updated with analytics disclosure
- [ ] Consent mechanism implemented (if required)
- [ ] All critical events firing correctly
- [ ] Backend dashboards configured
- [ ] Alerts set up for crashes/errors
- [ ] Team has access to analytics dashboard

---

## 📚 Related Files

- [AnalyticsManager.cs](Assets/Scripts/Analytics/AnalyticsManager.cs) - Core manager
- [AnalyticsEvents.cs](Assets/Scripts/Analytics/AnalyticsEvents.cs) - Event constants
- [GameManager.cs](Assets/Scripts/Core/GameManager.cs) - Game event integration
- [WaveManager.cs](Assets/Scripts/Core/WaveManager.cs) - Wave event integration
- [TowerPlacementManager.cs](Assets/Scripts/Towers/TowerPlacementManager.cs) - Tower event integration
- [AchievementManager.cs](Assets/Scripts/Progression/AchievementManager.cs) - Achievement integration
- [PerformanceManager.cs](Assets/Scripts/Core/PerformanceManager.cs) - Performance integration

---

**Last Updated:** 2024
**Version:** 1.0.0
