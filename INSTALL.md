# Robot Tower Defense - Installation Guide

## 🚀 Quick Install (Automatic)

**Double-click:** `Launch Game.bat`

That's it! The launcher will automatically:
- ✅ Download Unity Hub (if needed)
- ✅ Install Unity 2022.3 LTS (if needed)
- ✅ Set up the project structure
- ✅ Launch the game

---

## 📋 What Gets Installed

### Unity Hub (~150 MB)
- Download source: Unity's official CDN
- Installs to: `C:\Program Files\Unity Hub\`
- Purpose: Manages Unity installations

### Unity 2022.3 LTS (~3-6 GB depending on modules)
- Version: 2022.3.25f1 (or any 2022.3.x)
- Installs to: `C:\Program Files\Unity\Hub\Editor\`
- Modules included:
  - Android Build Support
  - iOS Build Support
  - Windows Build Support

---

## 🎮 Manual Installation

If you prefer to install manually:

### Step 1: Install Unity Hub
1. Download from: https://unity.com/download
2. Run the installer
3. Follow the installation wizard

### Step 2: Install Unity Editor
1. Open Unity Hub
2. Go to **Installs** tab
3. Click **Install Editor**
4. Select **Unity 2022.3 LTS** (latest patch version)
5. Choose modules:
   - ☑ Android Build Support
   - ☑ iOS Build Support
   - ☑ Windows Build Support
6. Click **Install** and wait for completion

### Step 3: Open Project
1. Open Unity Hub
2. Click **Open** button
3. Navigate to this folder
4. Select the `RobotTowerDefense` folder
5. Click **Open**

### Step 4: Play
1. Wait for Unity to import assets (first time takes 2-5 minutes)
2. Double-click `MainMenu` scene in Project window
3. Click the **Play** button (▶) at the top
4. Enjoy!

---

## 🔧 Command Line Usage

### Run Setup Script Manually
```powershell
.\Setup-Game.ps1
```

### Skip Unity Installation (if already installed)
```powershell
.\Setup-Game.ps1 -SkipUnityInstall
```

### Verbose Output
```powershell
.\Setup-Game.ps1 -Verbose
```

---

## 📦 System Requirements

### Minimum
- **OS:** Windows 10 (64-bit)
- **CPU:** Intel Core i5 / AMD equivalent
- **RAM:** 8 GB
- **GPU:** Intel HD 4000 / AMD Radeon HD 5000
- **Storage:** 10 GB free space
- **Internet:** Required for initial Unity download

### Recommended
- **OS:** Windows 11 (64-bit)
- **CPU:** Intel Core i7 / AMD Ryzen 5
- **RAM:** 16 GB
- **GPU:** NVIDIA GTX 1050 / AMD RX 560
- **Storage:** 20 GB free space (SSD recommended)

---

## ❓ Troubleshooting

### "Unity Hub not found" Error
**Solution:** Run `Launch Game.bat` again - it will download Unity Hub automatically.

### "Unity 2022.3 not installed" Error
**Solution:** 
1. Open Unity Hub
2. Go to Installs → Install Editor
3. Select Unity 2022.3 LTS
4. Run `Launch Game.bat` again

### Project Won't Open
**Solution:**
1. Delete `Library` and `Temp` folders in project directory
2. Open project again in Unity Hub
3. Wait for Unity to reimport assets

### Script Execution Error
**Solution:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
Then run `Launch Game.bat` again.

### Unity Crashes on Startup
**Solution:**
1. Update your graphics drivers
2. Check Windows is fully updated
3. Try running Unity as Administrator

### Need Help?
- Check: [README.md](README.md) for project documentation
- Check: [QUICKSTART.md](QUICKSTART.md) for gameplay guide
- Check: [TESTING_GUIDE.md](TESTING_GUIDE.md) for development info

---

## 🔒 Security & Privacy

- All downloads come from official Unity CDN
- No telemetry or tracking in the setup script
- Unity Hub may collect anonymous usage statistics (can be disabled in Unity Hub settings)
- No personal data is transmitted by this project

---

## 🗑️ Uninstallation

### Remove This Project
Simply delete the `RobotTowerDefense` folder.

### Remove Unity (keep Unity Hub)
1. Open Unity Hub
2. Go to **Installs** tab
3. Click ⋮ menu next to Unity 2022.3
4. Select **Uninstall**

### Remove Everything
1. Uninstall Unity Hub from Windows Settings → Apps
2. Delete `C:\Program Files\Unity`
3. Delete this project folder

---

## 📝 License

See [LICENSE](LICENSE) file for details.
