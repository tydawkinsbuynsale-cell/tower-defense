# Social Features Guide

This guide covers the social features system in Robot Tower Defense, including friends management, friend leaderboards, and sharing.

## Table of Contents

1. [Overview](#overview)
2. [Backend Setup](#backend-setup)
3. [Scene Setup](#scene-setup)
4. [Friend System](#friend-system)
5. [Friend Leaderboards](#friend-leaderboards)
6. [Score & Achievement Sharing](#score--achievement-sharing)
7. [Testing](#testing)
8. [Analytics](#analytics)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The social features system provides:

- **Friend Management**: Send, accept, decline friend requests and manage friend list
- **Friend Leaderboards**: View friends-only leaderboards alongside global leaderboards
- **Score Sharing**: Share high scores with friends
- **Achievement Sharing**: Share unlocked achievements with friends
- **Player Search**: Search for other players by name or ID

### Architecture

The system consists of:

- **SocialManager**: Core social features logic (friend management, requests, sharing)
- **LeaderboardManager**: Extended with friend leaderboard filtering
- **UI Components**: FriendsUI, LeaderboardUI with friend tabs, UI card components
- **Backend Integration**: Supports Unity Gaming Services, PlayFab, or custom HTTP API

---

## Backend Setup

### Backend Options

Choose one backend implementation:

#### Option 1: Unity Gaming Services

```csharp
// In ProjectSettings, add UNITY_GAMING_SERVICES to Scripting Define Symbols
// Install packages:
// - com.unity.services.core
// - com.unity.services.authentication
// - com.unity.services.cloudcode (for friend features)
// - com.unity.services.leaderboards
```

#### Option 2: PlayFab

```csharp
// In ProjectSettings, add PLAYFAB to Scripting Define Symbols
// Install PlayFab SDK from Asset Store or Unity Package Manager
// Configure PlayFab Title ID in SocialManager and LeaderboardManager
```

#### Option 3: Custom HTTP Backend

```csharp
// Default mode (no define symbols needed)
// Update server URLs in SocialManager and LeaderboardManager:

// SocialManager:
private string serverUrl = "https://your-server.com/api";

// LeaderboardManager:
string url = $"https://your-server.com/api/leaderboards/{leaderboardId}";
```

### Custom Backend API Reference

If using a custom backend, implement these endpoints:

#### Friend Requests

```http
POST /api/social/friends/request
{
  "sender_id": "string",
  "receiver_id": "string"
}

Response: { "request_id": "string", "status": "success" }
```

#### Accept Friend Request

```http
POST /api/social/friends/accept
{
  "request_id": "string",
  "player_id": "string"
}

Response: { "status": "success" }
```

#### Get Friend List

```http
GET /api/social/friends/{player_id}

Response: {
  "friends": [
    {
      "player_id": "string",
      "player_name": "string",
      "is_online": boolean,
      "last_seen_online": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### Get Friend Requests

```http
GET /api/social/friends/requests/{player_id}

Response: {
  "requests": [
    {
      "request_id": "string",
      "sender_id": "string",
      "sender_name": "string",
      "timestamp": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### Friend Leaderboards

```http
POST /api/leaderboards/{leaderboard_id}/friends
{
  "friend_ids": ["id1", "id2", "id3"],
  "limit": 50
}

Response: {
  "entries": [
    {
      "player_id": "string",
      "player_name": "string",
      "score": 12345,
      "rank": 1,
      "timestamp": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

## Scene Setup

### 1. Social Manager Setup

Add **SocialManager** to a persistent scene or use the provided prefab:

1. Create empty GameObject named "SocialManager"
2. Add `SocialManager` component
3. Configure in inspector:
   - **Enable Friends**: ✓ (enabled)
   - **Enable Sharing**: ✓ (enabled)
   - **Max Friends**: 100
   - **Max Pending Requests**: 50
   - **Enable Debug Logs**: ✓ (for development)
   - **Editor Simulation**: ✓ (for testing without backend)

### 2. Friends UI Setup

Add **FriendsUI** panel to your main menu or social screen:

1. Create UI Canvas if not exists
2. Add `FriendsUI` prefab or create manually:
   - Panel GameObject with `FriendsUI` component
   - Tab buttons (Friends, Requests, Search)
   - Scroll views for each tab
   - Search input field
   - Close button

#### Required Prefabs

Create these UI card prefabs:

**FriendCardUI Prefab:**
- Player name text
- Status text (Online/Offline)
- Online indicator image
- View Profile button
- Remove Friend button

**FriendRequestCardUI Prefab:**
- Player name text
- Time ago text
- Accept button
- Decline button

**PlayerSearchResultCardUI Prefab:**
- Player name text
- Status text
- Online indicator
- Add Friend button
- Friend status text

### 3. Leaderboard UI Setup

Enhanced **LeaderboardUI** with friend filtering:

1. Open existing LeaderboardUI prefab/scene
2. Add new UI elements:
   - **Global Scope Button**: Shows all players
   - **Friends Scope Button**: Shows friends only
3. Assign buttons in LeaderboardUI inspector:
   - Drag buttons to "Global Scope Button" and "Friends Scope Button" fields
4. Configure scope button colors (optional):
   - Active Scope Button Color: Blue
   - Inactive Scope Button Color: Gray

---

## Friend System

### Sending Friend Requests

```csharp
// Search for player
SocialManager.Instance.SearchPlayers("PlayerName", (results) => {
    foreach (var player in results)
    {
        Debug.Log($"Found: {player.playerName}");
    }
});

// Send friend request
SocialManager.Instance.SendFriendRequest("target_player_id", (success) => {
    if (success)
        Debug.Log("Friend request sent!");
});
```

### Accepting Friend Requests

```csharp
// Get pending requests
List<FriendRequest> requests = SocialManager.Instance.GetPendingRequests();

// Accept request
SocialManager.Instance.AcceptFriendRequest("request_id", (success) => {
    if (success)
        Debug.Log("Friend added!");
});

// Decline request
SocialManager.Instance.DeclineFriendRequest("request_id", (success) => {
    if (success)
        Debug.Log("Request declined");
});
```

### Managing Friends

```csharp
// Get friend list
List<FriendInfo> friends = SocialManager.Instance.GetFriends();

// Remove friend
SocialManager.Instance.RemoveFriend("friend_player_id", (success) => {
    if (success)
        Debug.Log("Friend removed");
});

// Check if player is friend
bool isFriend = SocialManager.Instance.IsFriend("player_id");
```

### Events

Subscribe to social events:

```csharp
void OnEnable()
{
    SocialManager.Instance.OnFriendRequestReceived += HandleFriendRequest;
    SocialManager.Instance.OnFriendAdded += HandleFriendAdded;
    SocialManager.Instance.OnFriendRemoved += HandleFriendRemoved;
}

void OnDisable()
{
    if (SocialManager.Instance != null)
    {
        SocialManager.Instance.OnFriendRequestReceived -= HandleFriendRequest;
        SocialManager.Instance.OnFriendAdded -= HandleFriendAdded;
        SocialManager.Instance.OnFriendRemoved -= HandleFriendRemoved;
    }
}

void HandleFriendRequest(FriendRequest request)
{
    Debug.Log($"Friend request from {request.senderPlayerName}");
    // Show notification to player
}
```

---

## Friend Leaderboards

### Viewing Friend Leaderboards

```csharp
// Fetch global leaderboard (all players)
LeaderboardManager.Instance.FetchLeaderboard(
    "endless_high_score",
    LeaderboardScope.Global,
    maxResults: 50
);

// Fetch friend leaderboard (friends only)
LeaderboardManager.Instance.FetchFriendLeaderboard(
    "endless_high_score",
    maxResults: 50
);
```

### UI Integration

The **LeaderboardUI** automatically handles friend filtering:

1. Click "Global" button → Shows all players
2. Click "Friends" button → Shows friends only
3. Leaderboard refreshes automatically

Friend leaderboards include:
- All friends who have scores
- Player's own score (even if not in top N)
- Ranks recalculated for friends-only context

### Offline Mode

In offline mode (no backend), friend leaderboards use local cache:

```csharp
// LeaderboardManager filters local scores by friend IDs
var friendScores = localScores[leaderboardId]
    .Where(e => friendIds.Contains(e.playerId))
    .OrderByDescending(e => e.score)
    .ToList();
```

---

## Score & Achievement Sharing

### Sharing Scores

```csharp
// Share score with specific friends
List<string> friendIds = new List<string> { "friend1_id", "friend2_id" };
string message = "Just beat wave 50!";

SocialManager.Instance.ShareScore(
    score: 123456,
    gameMode: "endless",
    friendIds: friendIds,
    message: message,
    callback: (success) => {
        if (success)
            Debug.Log("Score shared!");
    }
);

// Share with all friends (pass null for friendIds)
SocialManager.Instance.ShareScore(123456, "endless", null, "New high score!");
```

### Sharing Achievements

```csharp
SocialManager.Instance.ShareAchievement(
    achievementId: "first_victory",
    achievementName: "First Victory",
    friendIds: null, // Share with all friends
    callback: (success) => {
        if (success)
            Debug.Log("Achievement shared!");
    }
);
```

### Receiving Shares

Listen for shared content:

```csharp
void OnEnable()
{
    SocialManager.Instance.OnScoreShared += HandleScoreShared;
    SocialManager.Instance.OnAchievementShared += HandleAchievementShared;
}

void HandleScoreShared(string friendId, long score, string gameMode)
{
    Debug.Log($"{friendId} scored {score} in {gameMode}!");
    // Show notification or feed entry
}
```

---

## Testing

### Editor Testing Mode

SocialManager includes editor simulation for testing without a backend:

1. Enable **Editor Simulation** in SocialManager inspector
2. Use context menu commands:
   - Right-click SocialManager → **Add Test Friend**
   - Right-click SocialManager → **Add Test Friend Request**
   - Right-click SocialManager → **Simulate Friend Request**

### Context Menu Commands

**SocialManager Commands:**
```
[Context Menu] Refresh Friends List
[Context Menu] Refresh Pending Requests
[Context Menu] Add Test Friend
[Context Menu] Add Test Friend Request
[Context Menu] Clear All Friends
[Context Menu] Print Friends List
```

**LeaderboardManager Commands:**
```
[Context Menu] Submit Test Score
[Context Menu] Fetch Endless Leaderboard
[Context Menu] Print Local Scores
[Context Menu] Clear All Data
```

### Test Scenarios

#### Scenario 1: Friend Request Flow

1. Open FriendsUI
2. Search for player (will generate test results in editor mode)
3. Send friend request
4. Accept request (simulate receiving request first)
5. Verify friend appears in friends list

#### Scenario 2: Friend Leaderboard

1. Add multiple test friends via context menu
2. Submit test scores for different "players"
3. Open LeaderboardUI
4. Switch between Global and Friends tabs
5. Verify filtering works correctly

#### Scenario 3: Offline Mode

1. Set LeaderboardManager to offline mode
2. Submit scores locally
3. Add friends
4. View friend leaderboard (should show local cachefiltered by friends)

---

## Analytics

Social features track these events:

### Friend Events
- `friend_request_sent`: When player sends friend request
- `friend_request_received`: When player receives request
- `friend_request_accepted`: When player accepts request
- `friend_request_declined`: When player declines request
- `friend_removed`: When player removes friend
- `player_searched`: When player searches for others
- `friends_list_viewed`: When player opens friends list

### Leaderboard Events
- `leaderboard_viewed`: When viewing global leaderboard
- `friend_leaderboard_viewed`: When viewing friend leaderboard

### Sharing Events
- `score_shared`: When player shares score
- `achievement_shared_social`: When player shares achievement

### Event Parameters

```csharp
// Example: Friend request sent
{
    "friend_id": "target_player_id",
    "friend_name": "TargetPlayer",
    "timestamp": "2024-01-15T10:30:00Z"
}

// Example: Friend leaderboard viewed
{
    "leaderboard_id": "endless_high_score",
    "leaderboard_scope": "Friends",
    "friend_count": 24
}
```

---

## Troubleshooting

### Friends Not Loading

**Issue**: Friends list is empty after logging in

**Solutions**:
1. Check AuthenticationManager is initialized first
2. Verify backend URL is correct
3. Check SocialManager.OnError event for error messages
4. Enable Debug Logs in SocialManager to see backend responses

### Friend Leaderboard Empty

**Issue**: Friend leaderboard shows no scores

**Solutions**:
1. Verify friends have submitted scores
2. Check SocialManager.GetFriendIds() returns IDs
3. Confirm LeaderboardManager is not in offline mode (unless intended)
4. Check backend friend leaderboard endpoint

### UI Not Updating

**Issue**: UI doesn't refresh after friend actions

**Solutions**:
1. Verify UI subscribes to SocialManager events in OnEnable
2. Check event unsubscribe in OnDisable to avoid null references
3. Call RefreshFriendsList() manually after actions
4. Check Canvas Group/GameObject active state

### Editor Simulation Not Working

**Issue**: Test friends not appearing

**Solutions**:
1. Enable "Editor Simulation" in SocialManager inspector
2. Use context menu commands (right-click component)
3. Check Application.isEditor is true
4. Verify UNITY_EDITOR symbol is defined

### Backend Connection Fails

**Issue**: Network errors when connecting to backend

**Solutions**:
1. Check firewall/antivirus settings
2. Verify SSL certificates (use UnityWebRequest.certificateHandler if needed)
3. Test backend URLs in browser/Postman first
4. Check backend CORS settings for WebGL builds
5. Enable Debug Logs to see exact error messages

### Performance Issues

**Issue**: UI lags when loading large friend lists

**Solutions**:
1. Implement pagination for friends list (load in batches)
2. Use object pooling for UI card elements
3. Limit max friends displayed at once
4. Cache friend data locally to reduce backend calls
5. Use async/await for smoother loading

---

## Best Practices

### Friend List Management

1. **Limit Friend Count**: Default max is 100, adjust based on game needs
2. **Pagination**: Load friends in batches for large lists
3. **Caching**: Cache friend data locally, refresh periodically
4. **Offline Sync**: Queue friend actions when offline, sync when online

### Leaderboard Performance

1. **Cache Leaderboards**: Use 5-minute cache by default
2. **Limit Results**: Fetch only top N scores (50 default)
3. **Smart Refreshing**: Only refresh when user explicitly requests
4. **Local Fallback**: Show cached/local scores if backend fails

### Security

1. **Validate Requests**: Always validate friend requests on backend
2. **Rate Limiting**: Limit friend requests per hour
3. **Block List**: Implement block/report functionality
4. **Privacy Settings**: Allow users to control who can send requests

### UX Guidelines

1. **Friend Limits**: Show "Friend slots: 42/100" progress
2. **Notifications**: Notify users of new friend requests
3. **Search**: Provide autocomplete for player search
4. **Filters**: Allow sorting friends (online, recent, alphabetical)
5. **Confirmation**: Confirm before removing friends

---

## Future Enhancements

Potential additions to social features:

- **Guilds/Clans**: Create and join player groups
- **Chat System**: Direct messaging between friends
- **Gifting**: Send resources to friends
- **Cooperative Modes**: Team up with friends
- **Social Feed**: Activity stream of friend actions
- **Spectate Mode**: Watch friends play
- **Friend Challenges**: Challenge friends to beat your score

---

## Related Documentation

- [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md) - Account system setup
- [LEADERBOARD_GUIDE.md](LEADERBOARD_GUIDE.md) - Leaderboard configuration
- [CLOUD_SAVE_GUIDE.md](CLOUD_SAVE_GUIDE.md) - Cloud save system
- [ANALYTICS_GUIDE.md](ANALYTICS_GUIDE.md) - Analytics integration

---

## Support

For issues or questions:

1. Check Unity console for error messages
2. Enable Debug Logs in SocialManager
3. Review backend API logs
4. Test with Editor Simulation mode first
5. Verify authentication is working

**File**: `SOCIAL_FEATURES_GUIDE.md`  
**Last Updated**: January 2024
