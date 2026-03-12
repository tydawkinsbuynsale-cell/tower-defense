# Quick Start Guide

Get Robot Tower Defense running in **under 5 minutes**!

## ⚡ Super Quick Start

```bash
# 1. Clone
git clone https://github.com/tydawkinsbuynsale-cell/tower-defense.git
cd tower-defense

# 2. Open in Unity Hub (2022.3 LTS)
# 3. Press Play in MainMenu scene
# 4. Done! 🎉
```

---

## 🎮 First Time Playing

1. **Open Scene:** `Assets/Scenes/MainMenu`
2. **Press Play** in Unity Editor
3. **Click "Play"** → Select first map
4. **Tutorial will auto-start** for first-time players

**Quick Controls:**
- **Left Click** - Select tower / Place tower
- **Right Click** - Deselect
- **Mouse Wheel** - Zoom camera
- **Middle Mouse Drag** - Pan camera
- **Escape** - Pause menu

---

## 🔧 Common Tasks

### Test a Specific Map

```csharp
// In GameManager.cs Start(), add:
MapManager.Instance.LoadMap("map_fortress");
```

### Skip to Wave 10

```
Tools → Robot TD → Dev Test Tools
→ Skip to Wave: 10
→ Click "Apply"
```

### Add 10,000 Credits

```
Tools → Robot TD → Dev Test Tools
→ Add Credits: 10000
→ Click "Add Credits"
```

### Reset Save Data

```
Tools → Robot TD → Dev Test Tools
→ Click "Reset Save Data"
```

### Change Quality Preset

```csharp
PerformanceManager.Instance.ApplyQualityPreset(
    PerformanceManager.QualityPreset.High
);
```

---

## 🏗️ Adding Content

### New Tower (5 minutes)

1. **Create Data Asset**
   ```
   Assets → Create → Robot TD → Tower Data
   Name: "TowerData_YourTower"
   ```

2. **Set Values**
   - Cost: 200
   - Damage: 50
   - Range: 6
   - Fire Rate: 1.0

3. **Create Prefab**
   - Create GameObject in scene
   - Add `Tower` component
   - Assign TowerData
   - Drag to `Assets/Prefabs/Towers/`

4. **Add to UI**
   - Add TowerButton in TowerPanel prefab
   - Assign TowerData reference

5. **Test**: Play scene, buy tower, verify it works

### New Enemy (5 minutes)

1. **Create Data Asset**
   ```
   Assets → Create → Robot TD → Enemy Data
   Name: "EnemyData_YourEnemy"
   ```

2. **Set Values**
   - HP: 150
   - Speed: 2.5
   - Armor: 20%
   - Credits: 25

3. **Create Prefab**
   - Create GameObject
   - Add `Enemy` component
   - Assign EnemyData
   - Add NavMeshAgent
   - Drag to `Assets/Prefabs/Enemies/`

4. **Add to Wave**
   - Open `WaveSetData` asset
   - Add to wave composition

5. **Test**: Start wave, verify spawning

### New Map (15 minutes)

1. **Duplicate Scene**
   ```
   Duplicate: Assets/Scenes/Map_Fortress
   Rename: Map_YourMap
   ```

2. **Modify Layout**
   - Move waypoints
   - Adjust placement grid
   - Add decorations

3. **Create MapData**
   ```
   Assets → Create → Robot TD → Map Data
   ```

4. **Configure**
   - Assign scene reference
   - Set WaveSetData
   - Configure starting resources

5. **Register**
   ```csharp
   // In MapRegistry.cs
   RegisterMap("map_yourname", mapDataAsset);
   ```

6. **Test**: Load map from main menu

---

## 🏃 Quick Build

### Build for Android (Test APK)

```
1. Tools → Robot TD → Android Build Config
2. Set Bundle ID: com.yourstudio.robottowerdefense
3. Development Build: ✅
4. Click "Build APK"
5. Wait 5-15 minutes
6. Install APK on device
```

### Build for Android (Release AAB)

```
1. Tools → Robot TD → Android Build Config
2. Development Build: ❌
3. Build AAB: ✅
4. Click "Build AAB"
5. Wait 15-30 minutes
6. Upload to Google Play Console
```

---

## 🐛 Troubleshooting

### "Cannot find ObjectPooler"

**Fix:** Open MainMenu scene, verify SceneBootstrapper exists.

### "Save file not loading"

**Fix:** 
```
Tools → Robot TD → Dev Test Tools → Reset Save Data
```

### "Towers not firing"

**Check:**
- Tower has projectile prefab assigned
- ObjectPooler has projectile pool configured
- Enemies have correct layer (Enemy)

### "Wave not starting"

**Check:**
- WaveManager exists in scene
- WaveSetData is assigned
- At least one spawn point exists

### "Low FPS in Editor"

**Fix:**
```csharp
// In PerformanceManager, set:
currentPreset = QualityPreset.Low;
```

Or disable VSync:
```
Edit → Project Settings → Quality → VSync Count → Don't Sync
```

---

## 📖 Learn More

- **Full Documentation:** See [README.md](README.md)
- **Game Design:** See [GAME_DESIGN_DOCUMENT.md](GAME_DESIGN_DOCUMENT.md)
- **Version History:** See [CHANGELOG.md](CHANGELOG.md)

---

## 💡 Pro Tips

1. **Use Dev Test Tools** for rapid iteration
2. **Test on real devices** early and often
3. **Profile on target hardware** (low-end Android)
4. **Enable Deep Profiling** sparingly (slow)
5. **Backup save data** before testing reset features
6. **Use Quality Presets** for consistent performance
7. **Version control** every working state
8. **Test tutorial** after any UI changes

---

## 🎯 Next Steps

Once you're comfortable:

1. Read the full [README.md](README.md) for system details
2. Review [GAME_DESIGN_DOCUMENT.md](GAME_DESIGN_DOCUMENT.md) for design rationale
3. Explore codebase starting with `GameManager.cs`
4. Add your own content (towers, enemies, maps)
5. Build and test on Android device
6. Iterate based on feedback

---

**Happy Developing! 🚀**
