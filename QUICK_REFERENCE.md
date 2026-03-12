# Quick Reference - Robot Tower Defense

## 🚀 Launch the Game

**Desktop Icon (Easiest):**
- Double-click: **Robot Tower Defense** icon on desktop

**Or from folder:**
- Double-click: `Launch Game.bat`

---

## 🔍 Troubleshooting Tools

### If game won't launch:

**1. Quick Diagnostics (Fastest)**
- Double-click: **Robot TD - Diagnostics** icon on desktop
- Or run: `Quick-Diagnose.bat`

**2. View Detailed Logs**
- Run: `View-Log.ps1`
- Select from menu:
  - [5] View Recent Errors Only (quickest)
  - [6] Run Diagnostics (system check)
  - [9] Export Logs for Support (if asking for help)

**3. Read Troubleshooting Guide**
- Open: [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- Find your issue and solution

---

## 📁 Important Files

| File | Purpose |
|------|---------|
| `Launch Game.bat` | Start the game |
| `Setup-Game.ps1` | Full installer (auto-runs from launcher) |
| `View-Log.ps1` | Interactive log viewer |
| `Quick-Diagnose.bat` | Instant system check |
| `CreateDesktopShortcut.ps1` | Create desktop icons |
| `Setup-Log.txt` | Detailed activity log (auto-created) |
| `Setup-Errors.txt` | Error log (auto-created) |

---

## 📝 Documentation

| Document | When to Use |
|----------|-------------|
| [INSTALL.md](INSTALL.md) | Complete installation guide |
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | Problems and solutions |
| [GETTING_STARTED.md](GETTING_STARTED.md) | How to play |
| [README.md](README.md) | Full project documentation |
| [QUICKSTART.md](QUICKSTART.md) | Developer quick start |

---

## ⚙️ Common Commands

### Launch with logging
```powershell
.\Setup-Game.ps1 -Verbose
```

### Skip Unity install check (if already installed)
```powershell
.\Setup-Game.ps1 -SkipUnityInstall
```

### View logs in real-time
```powershell
Get-Content Setup-Log.txt -Wait -Tail 20
```

### Check what's installed
```powershell
.\Quick-Diagnose.bat
```

### Export debug info
```powershell
.\View-Log.ps1
# Select: [9] Export Logs for Support
```

---

## 🆘 Getting Help

### Before asking for help:

1. **Run diagnostics:**
   ```
   Quick-Diagnose.bat
   ```

2. **Check logs:**
   ```
   View-Log.ps1
   Select: [5] View Recent Errors Only
   ```

3. **Export logs:**
   ```
   View-Log.ps1
   Select: [9] Export Logs for Support
   ```

4. **Read troubleshooting:**
   - Open [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
   - Search for your error message

### When asking for help:
- Include the exported log file
- Describe what you see when running the launcher
- Mention any error messages

---

## ✅ Verification Checklist

Run `Quick-Diagnose.bat` - should show:
- ✅ Unity Hub is installed
- ✅ Unity Editor directory exists
- ✅ Assets folder exists
- ✅ ProjectSettings folder exists
- ✅ Packages folder exists

If any show ❌, run `Launch Game.bat` to auto-fix.

---

## 🎮 First Time Setup

1. Double-click: `Launch Game.bat`
2. Wait for Unity Hub to download (~150 MB)
3. Follow prompts to install Unity 2022.3 LTS (~3-6 GB)
4. Wait for Unity to open project
5. Press Play (▶) button in Unity
6. Enjoy!

**Total time:** 15-30 minutes depending on internet speed

---

## 💡 Pro Tips

- **Desktop icons created?** Run `CreateDesktopShortcut.ps1`
- **Logs cluttering?** Use View-Log.ps1 → [8] Clear All Logs
- **Unity slow?** Lower quality: Edit → Project Settings → Quality → Low
- **Need space?** Delete `Library` and `Temp` folders (auto-regenerated)
- **Fresh start?** Delete Setup-Log.txt and Setup-Errors.txt, run launcher again

---

## 🔗 Links

- **GitHub Repository:** https://github.com/tydawkinsbuynsale-cell/tower-defense
- **Unity Download:** https://unity.com/download
- **Report Issues:** https://github.com/tydawkinsbuynsale-cell/tower-defense/issues

---

**Last Updated:** March 12, 2026
