# Multiplayer Co-op Guide

Comprehensive guide for implementing and testing the multiplayer co-op system in Robot Tower Defense.

---

## Table of Contents

1. [Overview](#overview)
2. [Package Installation](#package-installation)
3. [Unity Netcode Setup](#unity-netcode-setup)
4. [Architecture](#architecture)
5. [Room System](#room-system)
6. [Player Synchronization](#player-synchronization)
7. [Game State Sync](#game-state-sync)
8. [Testing Guide](#testing-guide)
9. [Troubleshooting](#troubleshooting)
10. [Best Practices](#best-practices)

---

## Overview

The multiplayer co-op system allows 2-4 players to team up and defend against enemy waves together. Built on **Unity Netcode for GameObjects**, it features:

- **Host-Client Architecture**: One player hosts, others connect as clients
- **Room Code System**: 6-character alphanumeric codes for easy joining
- **Quick Match**: Automatic matchmaking to find or create games
- **Shared Resources**: Optional shared credits and lives
- **Co-op Balance**: Enemy health/spawn rates scale with player count
- **Real-time Sync**: Tower placement, power-ups, and game state
- **In-game Chat**: Communication between players

---

## Package Installation

### 1. Install Unity Netcode for GameObjects

**Option A: Package Manager (Recommended)**

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` (top-left) → `Add package by name...`
3. Enter: `com.unity.netcode.gameobjects`
4. Click `Add`

**Option B: Manual Installation**

1. Open `Packages/manifest.json`
2. Add to dependencies:
```json
{
  "dependencies": {
    "com.unity.netcode.gameobjects": "1.7.1"
  }
}
```
3. Save and return to Unity

### 2. Install Unity Transport (UTP)

1. Package Manager → `+` → `Add package by name...`
2. Enter: `com.unity.transport`
3. Click `Add`

### 3. Verify Installation

Check that these packages appear in Package Manager:
- ✅ Netcode for GameObjects (1.7.1+)
- ✅ Unity Transport (2.2.0+)

---

## Unity Netcode Setup

### 1. Create NetworkManager GameObject

1. In your **MainMenu** or **Lobby** scene:
   - Right-click Hierarchy → `Create Empty`
   - Name it `NetworkManager`

2. Add **NetworkManager** component:
   - `Add Component` → `Netcode` → `NetworkManager`

3. Configure **NetworkManager**:
   - **NetworkConfig**:
     - `Player Prefab`: Assign your `NetworkedPlayer` prefab
     - `Enable Scene Management`: ✅ (checked)
   
4. Add **Unity Transport** component:
   - `Add Component` → `Netcode` → `Unity Transport`
   - **Connection Data**:
     - `Address`: 127.0.0.1 (localhost for testing)
     - `Port`: 7777
     - `Max Connect Attempts`: 60
     - `Connect Timeout MS`: 1000

5. Link Transport to NetworkManager:
   - In **NetworkManager** → `Network Transport`: Drag the `Unity Transport` component

### 2. Create NetworkedPlayer Prefab

1. Create empty GameObject: `NetworkedPlayer`
2. Add components:
   - `Scripts > Multiplayer > NetworkedPlayer`
   - `Netcode > NetworkObject`
3. Configure **NetworkObject**:
   - `Is Player Object`: ✅ (checked)
   - `Destroy With Scene`: ❌ (unchecked)
4. Save as prefab: `Assets/Prefabs/NetworkedPlayer.prefab`
5. Assign to NetworkManager's `Player Prefab` field

### 3. Add Multiplayer Scripts to Scene

1. Create empty GameObject: `MultiplayerManager`
   - Add: `Scripts > Multiplayer > MultiplayerManager`
   - Ensure `Don't Destroy On Load` is enabled

2. Create empty GameObject: `LobbyManager`
   - Add: `Scripts > Multiplayer > LobbyManager`
   - Configure:
     - `Matchmaking Timeout`: 30
     - `Preferred Player Count`: 2
     - `Server Address`: 127.0.0.1
     - `Server Port`: 7777

3. Create Canvas: `MultiplayerUI`
   - Add: `Scripts > UI > MultiplayerUI`
   - Build UI layout (see [UI Setup](#ui-setup))

---

## Architecture

### Component Overview

```
┌─────────────────────────────────────────┐
│          Unity Netcode                  │
│  (NetworkManager + UnityTransport)      │
└────────────┬────────────────────────────┘
             │
    ┌────────┴────────┐
    │                 │
┌───▼────┐      ┌────▼─────┐
│Lobby   │      │Multiplayer│
│Manager │◄────►│Manager   │
└────┬───┘      └────┬─────┘
     │               │
     │          ┌────▼────────┐
     │          │Networked    │
     │          │Player       │
     │          └─────────────┘
     │
┌────▼─────┐
│Multiplayer│
│UI        │
└──────────┘
```

### Key Classes

| Class | Purpose | Type |
|-------|---------|------|
| **MultiplayerManager** | Core networking, RPCs, game state sync | NetworkBehaviour (Singleton) |
| **LobbyManager** | Room creation, matchmaking, room discovery | MonoBehaviour (Singleton) |
| **NetworkedPlayer** | Individual player sync, actions, cursor | NetworkBehaviour |
| **MultiplayerUI** | Lobby interface, room panels, chat | MonoBehaviour (Singleton) |

---

## Room System

### Room Flow

```
Player A (Host)                    Player B (Client)
       │                                  │
       ├─► Create Room                    │
       │   └─ Generate code: "ABC123"     │
       │                                  │
       │◄─────────────────────────────────┤
       │        Enter code "ABC123"       │
       │                                  │
       ├─► Start Host                     ├─► Start Client
       │                                  │
       ├───────── Connection ─────────────┤
       │                                  │
       ├─► Both players in room           │
       │                                  │
       ├─► Player A ready                 │
       │                                  ├─► Player B ready
       │                                  │
       ├─────────► Game Starts ◄──────────┤
```

### Creating a Room

```csharp
// Host creates room
LobbyManager.Instance.CreateRoom(
    roomName: "My Game",
    hostPlayerName: "Player1",
    maxPlayers: 4,
    isPublic: true
);

// Listen for room created event
LobbyManager.Instance.OnRoomCreatedSuccess += (roomCode) => {
    Debug.Log($"Room created with code: {roomCode}");
};
```

### Joining a Room

```csharp
// Join by room code
LobbyManager.Instance.JoinRoomByCode(
    roomCode: "ABC123",
    playerName: "Player2"
);

// Listen for join success
LobbyManager.Instance.OnRoomJoinedSuccess += (roomCode) => {
    Debug.Log($"Joined room: {roomCode}");
};
```

### Quick Match

```csharp
// Automatically find or create a room
LobbyManager.Instance.StartQuickMatch(playerName: "Player1");

// Listen for match found
LobbyManager.Instance.OnQuickMatchFound += (roomInfo) => {
    Debug.Log($"Match found! Room: {roomInfo.roomCode}");
};
```

---

## Player Synchronization

### Network Variables

**NetworkedPlayer** automatically syncs:
- **Cursor Position**: Updated 10 times/second
- **Tower Placement State**: Is player placing a tower?
- **Selected Tower Type**: Which tower is selected

### Registering Players

```csharp
// Automatically handled in NetworkedPlayer.Start()
MultiplayerManager.Instance.RegisterNetworkedPlayer(clientId, this);

// Get all connected players
Dictionary<ulong, NetworkedPlayer> players = 
    MultiplayerManager.Instance.GetConnectedPlayers();
```

### Player Actions

```csharp
// Set ready state
networkedPlayer.SetReady(true);

// Send chat message
networkedPlayer.SendChatMessage("Hello!");

// Check if local player
bool isLocal = networkedPlayer.IsLocalPlayer();
```

---

## Game State Sync

### Tower Placement

Towers are placed through the server to ensure synchronization:

```csharp
// Client requests tower placement
MultiplayerManager.Instance.PlaceTowerServerRpc(position, towerType);

// Server validates and broadcasts to all clients
[ServerRpc(RequireOwnership = false)]
public void PlaceTowerServerRpc(Vector3 position, int towerType) {
    // Server-side validation
    if (CanPlaceTowerAt(position)) {
        // Place tower
        PlaceTower(position, towerType);
        
        // Notify all clients
        TowerPlacedClientRpc(position, towerType);
    }
}
```

### Game State Synchronization

```csharp
// Sync credits, lives, and wave number
MultiplayerManager.Instance.SyncGameStateClientRpc(
    credits: 500,
    lives: 20,
    waveNumber: 5
);
```

### Co-op Balance

Enemy scaling based on player count:

```csharp
// Enemy health multiplier
float healthMultiplier = MultiplayerManager.Instance.GetEnemyHealthMultiplier();
// 1 player = 1.0x, 2 players = 2.5x, 3 players = 4.0x, 4 players = 5.5x

// Enemy spawn rate multiplier
float spawnMultiplier = MultiplayerManager.Instance.GetEnemySpawnMultiplier();
// 1 player = 1.0x, 2 players = 2.3x, 3 players = 3.6x, 4 players = 4.9x
```

---

## Testing Guide

### Local Testing (Same Machine)

1. **Build the Game**:
   - `File > Build Settings`
   - Click `Build` (not Build and Run)
   - Save executable

2. **Setup**:
   - Launch 1 **built executable** (Player 1 - Host)
   - Launch 1 **Unity Editor** (Player 2 - Client)

3. **Test Flow**:
   - **Built Executable**:
     - Enter name: "Player1"
     - Click "Create Room"
     - Note the room code (e.g., "ABC123")
   
   - **Unity Editor**:
     - Enter name: "Player2"
     - Click "Join Room"
     - Enter room code: "ABC123"
     - Click "Join"
   
   - Both players should see each other in the room
   - Both click "Ready"
   - Game should start

### Network Testing (Different Machines)

1. **Host Setup** (Player 1):
   - Find your local IP: `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
   - Example: `192.168.1.105`
   - Create room (becomes host)

2. **Client Setup** (Player 2):
   - In `LobbyManager`:
     - Set `Server Address`: Host's IP (e.g., `192.168.1.105`)
     - Set `Server Port`: `7777`
   - Join room by code

3. **Router/Firewall**:
   - Ensure port `7777` is open on host machine
   - Disable firewall temporarily for testing

### Testing Checklist

- [ ] Room creation generates unique code
- [ ] Room code join works correctly
- [ ] Quick match finds/creates room
- [ ] Player list updates when players join/leave
- [ ] Ready button toggles state
- [ ] Game starts when all players ready
- [ ] Tower placement syncs across clients
- [ ] Chat messages appear for all players
- [ ] Player cursors visible for remote players
- [ ] Game state (credits/lives/wave) syncs
- [ ] Disconnect handling (player leaves)

### Debug Tools

Enable debug logs in MultiplayerManager:

```csharp
// In MultiplayerManager.cs
private bool debugMode = true; // Set to true

// See connection events
OnClientConnectedCallback(ulong clientId) {
    if (debugMode) Debug.Log($"Client {clientId} connected");
}
```

---

## Troubleshooting

### "Unity Netcode not installed"

**Symptoms**: Console warning: `Unity Netcode not installed`

**Solution**:
1. Install `com.unity.netcode.gameobjects` via Package Manager
2. Add scripting define: `UNITY_NETCODE`
   - `Edit > Project Settings > Player > Script Compilation`
   - Add `UNITY_NETCODE` to `Scripting Define Symbols`
3. Restart Unity

### "Failed to start host"

**Symptoms**: Cannot create room, "Failed to start host" error

**Solution**:
- Ensure **NetworkManager** exists in scene
- Check **Unity Transport** is assigned to NetworkManager
- Verify port `7777` is not already in use
- Check NetworkManager's `Player Prefab` is assigned

### "Room code not found"

**Symptoms**: Cannot join room, "Room code not found" error

**Solution**:
- Verify room code is correct (case-sensitive)
- Ensure host has created room
- Check both host and client are using same network transport settings
- Refresh room list before joining

### "Connection timeout"

**Symptoms**: Client times out when joining

**Solution**:
- Verify host's IP address is correct
- Check firewall allows port `7777`
- Ensure both machines on same network (for local testing)
- Increase `Connect Timeout MS` in Unity Transport (default: 1000ms)

### "Players not syncing"

**Symptoms**: Other players not visible or actions not replicating

**Solution**:
- Verify **NetworkedPlayer** has `NetworkObject` component
- Check `Is Player Object` is enabled on NetworkObject
- Ensure `Player Prefab` is assigned in NetworkManager
- Verify players are registered with MultiplayerManager

### "Chat not working"

**Symptoms**: Chat messages not appearing

**Solution**:
- Check `OnChatMessageReceived` event is subscribed in MultiplayerUI
- Verify `SendChatMessage()` is called on local player
- Ensure MultiplayerManager's chat RPCs are not blocked

---

## Best Practices

### 1. Server Authority

Always validate actions on the server:

```csharp
[ServerRpc(RequireOwnership = false)]
public void PlaceTowerServerRpc(Vector3 position, int towerType) {
    // ✅ Server validates
    if (!IsValidPlacement(position)) return;
    if (!HasEnoughCredits(towerType)) return;
    
    // ✅ Server places tower
    PlaceTower(position, towerType);
    
    // ✅ Server broadcasts to clients
    TowerPlacedClientRpc(position, towerType);
}
```

### 2. Network Variable Usage

Use NetworkVariables for frequently changing data:

```csharp
// ✅ Good: Auto-synced position
private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

// ❌ Bad: Manual sync every frame
void Update() {
    SyncPositionServerRpc(transform.position); // Too expensive!
}
```

### 3. RPC Throttling

Limit RPC frequency to avoid network congestion:

```csharp
private float lastSyncTime = 0f;
private const float syncInterval = 0.1f; // 10 times/second

void Update() {
    if (Time.time - lastSyncTime >= syncInterval) {
        SyncCursorPosition(mousePosition);
        lastSyncTime = Time.time;
    }
}
```

### 4. Ownership

Only the owner should modify owned objects:

```csharp
void Update() {
    if (!IsOwner) return; // ✅ Only owner updates
    
    // Handle local player input
    HandleInput();
}
```

### 5. Scene Management

Use NetworkManager's scene management for loading:

```csharp
// ✅ Good: Network-synced scene loading
NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

// ❌ Bad: Only loads for local client
SceneManager.LoadScene("GameScene");
```

### 6. Disconnection Handling

Always clean up when players disconnect:

```csharp
private void OnClientDisconnectCallback(ulong clientId) {
    // Remove player from lists
    UnregisterNetworkedPlayer(clientId);
    
    // Update UI
    OnPlayerLeft?.Invoke(clientId, playerName);
    
    // Notify remaining players
    PlayerDisconnectedClientRpc(clientId);
}
```

### 7. Testing

Test with realistic conditions:
- ✅ Test with 2-4 players
- ✅ Test host disconnect (client promotion)
- ✅ Test latency simulation
- ✅ Test with different player counts
- ✅ Test co-op balance scaling

---

## UI Setup

### Lobby Panel Hierarchy

```
Canvas (MultiplayerUI)
├── LobbyPanel
│   ├── PlayerNameInput (TMP_InputField)
│   ├── CreateRoomButton (Button)
│   ├── JoinRoomButton (Button)
│   ├── QuickMatchButton (Button)
│   ├── BackToMenuButton (Button)
│   ├── RoomListPanel
│   │   ├── RoomListContainer (ScrollView)
│   │   │   └── RoomListItem (Prefab)
│   │   │       ├── RoomName (TextMeshProUGUI)
│   │   │       ├── HostName (TextMeshProUGUI)
│   │   │       ├── Players (TextMeshProUGUI)
│   │   │       └── JoinButton (Button)
│   │   └── RefreshButton (Button)
│   
├── CreateRoomPanel
│   ├── RoomNameInput (TMP_InputField)
│   ├── MaxPlayersDropdown (TMP_Dropdown)
│   ├── IsPublicToggle (Toggle)
│   ├── ConfirmCreateButton (Button)
│   └── CancelButton (Button)
│
├── JoinRoomPanel
│   ├── RoomCodeInput (TMP_InputField)
│   ├── ConfirmJoinButton (Button)
│   └── CancelButton (Button)
│
└── RoomPanel
    ├── RoomCodeText (TextMeshProUGUI)
    ├── RoomNameText (TextMeshProUGUI)
    ├── PlayerListPanel
    │   └── PlayerListContainer (ScrollView)
    │       └── PlayerListItem (Prefab)
    │           ├── PlayerName (TextMeshProUGUI)
    │           └── ReadyIndicator (Image)
    ├── ChatPanel
    │   ├── ChatContainer (ScrollView)
    │   │   └── ChatMessage (Prefab - TextMeshProUGUI)
    │   ├── ChatInput (TMP_InputField)
    │   └── SendButton (Button)
    ├── ReadyButton (Button)
    ├── LeaveRoomButton (Button)
    └── StatusText (TextMeshProUGUI)
```

### Prefab Requirements

Create these prefabs:

1. **RoomListItem.prefab**:
   - RoomName (TextMeshProUGUI)
   - HostName (TextMeshProUGUI)
   - Players (TextMeshProUGUI)
   - JoinButton (Button)

2. **PlayerListItem.prefab**:
   - PlayerName (TextMeshProUGUI)
   - ReadyIndicator (Image - green checkmark)

3. **ChatMessage.prefab**:
   - TextMeshProUGUI component (for message text)

---

## Integration with Existing Systems

### GameManager Integration

```csharp
// In GameManager.cs
void Start() {
    if (MultiplayerManager.Instance != null && 
        MultiplayerManager.Instance.IsMultiplayerActive()) {
        // Multiplayer mode
        InitializeMultiplayerGame();
    } else {
        // Single-player mode
        InitializeSinglePlayerGame();
    }
}

void InitializeMultiplayerGame() {
    // Use multiplayer settings
    useSharedCredits = MultiplayerManager.Instance.useSharedCredits;
    useSharedLives = MultiplayerManager.Instance.useSharedLives;
    
    // Apply co-op balance
    enemyHealthMultiplier = MultiplayerManager.Instance.GetEnemyHealthMultiplier();
}
```

### WaveManager Integration

```csharp
// In WaveManager.cs
void SpawnWave() {
    int enemyCount = baseEnemyCount;
    
    // Apply multiplayer scaling
    if (MultiplayerManager.Instance != null && 
        MultiplayerManager.Instance.IsMultiplayerActive()) {
        float spawnMultiplier = MultiplayerManager.Instance.GetEnemySpawnMultiplier();
        enemyCount = Mathf.RoundToInt(enemyCount * spawnMultiplier);
    }
    
    // Spawn enemies
    for (int i = 0; i < enemyCount; i++) {
        SpawnEnemy();
    }
}
```

### Enemy Integration

```csharp
// In Enemy.cs
void Start() {
    // Apply multiplayer health scaling
    if (MultiplayerManager.Instance != null && 
        MultiplayerManager.Instance.IsMultiplayerActive()) {
        float healthMultiplier = MultiplayerManager.Instance.GetEnemyHealthMultiplier();
        maxHealth *= healthMultiplier;
        currentHealth = maxHealth;
    }
}
```

---

## Performance Considerations

### Network Bandwidth

- **Cursor Position**: ~10 updates/second per player = ~40 bytes/s
- **Tower Placement**: Occasional = ~100 bytes per placement
- **Chat Messages**: Variable = ~50-200 bytes per message
- **Game State Sync**: Every 0.1s = ~50 bytes/s

**Total Bandwidth** (4 players): ~200-500 bytes/s ≈ **0.5 KB/s**

### CPU Performance

- **NetworkVariables**: Automatic delta compression (optimized)
- **RPCs**: Only sent when needed (event-driven)
- **Player Count**: Scales linearly (4 players = ~4x CPU)

### Optimization Tips

1. **Rate Limiting**: Throttle cursor updates to 10 Hz
2. **Delta Compression**: Unity Netcode handles automatically
3. **Object Pooling**: Reuse network objects when possible
4. **Scene Management**: Unload unused scenes to free memory

---

## Production Deployment

### Dedicated Server (Optional)

For production, consider a dedicated server:

1. **Headless Server Build**:
   - `File > Build Settings`
   - Platform: `Linux` (or Windows Server)
   - `Server Build`: ✅ (checked)
   - Build and deploy to cloud (AWS, Azure, Google Cloud)

2. **Server Configuration**:
   ```csharp
   // Auto-start as dedicated server
   void Start() {
       if (Application.isBatchMode) {
           StartDedicatedServer();
       }
   }
   ```

3. **Matchmaking Backend**:
   - Use Unity Services (Lobby, Relay, Matchmaker)
   - Or custom backend (Node.js, ASP.NET)

### Security Considerations

1. **Validate All Inputs**: Server must validate all client requests
2. **Anti-Cheat**: Implement server-side validation for credits/lives
3. **Rate Limiting**: Prevent spam (tower placement, chat)
4. **Encryption**: Use DTLS encryption (Unity Transport default)

---

## Additional Resources

### Unity Documentation
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
- [Unity Transport](https://docs.unity3d.com/Packages/com.unity.transport@latest)
- [Network Variables](https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/)
- [RPCs](https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/rpc/)

### Community
- [Unity Multiplayer Forum](https://forum.unity.com/forums/multiplayer.26/)
- [Netcode GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)
- [Discord: Unity Multiplayer](https://discord.gg/unity)

### Tutorials
- [Official Boss Room Sample](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)
- [Netcode Getting Started](https://docs-multiplayer.unity3d.com/netcode/current/tutorials/get-started/)

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial multiplayer co-op implementation |

---

**Need Help?** Check [TROUBLESHOOTING](#troubleshooting) or open an issue on GitHub.

**Ready to Test?** See [TESTING_GUIDE.md](TESTING_GUIDE.md) for co-op testing scenarios.
