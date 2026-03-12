# Troubleshooting Guide - Robot Tower Defense

## 🔍 Diagnostic Tools

### Quick Diagnostics
**Double-click:** `Quick-Diagnose.bat`

This instantly shows:
- Unity Hub installation status
- Unity Editor installations
- Project structure validation
- System requirements check

### View Detailed Logs
**Double-click:** `View-Log.ps1` or run `.\View-Log.ps1`

Interactive log viewer with:
- Color-coded log entries
- Error filtering
- Statistics and analysis
- System diagnostics
- Log export for support

### Log Files
- `Setup-Log.txt` - Detailed setup process log
- `Setup-Errors.txt` - Error-only log for quick debugging

---

## ❌ Common Issues & Solutions

### Issue: Launcher closes immediately

**Symptoms:**
- Double-click `Launch Game.bat`
- Window appears briefly then closes
- No Unity window opens

**Solutions:**

1. **Check Logs**
   ```
   Double-click: View-Log.ps1
   Select: [5] View Recent Errors Only
   ```

2. **Run Diagnostics**
   ```
   Double-click: Quick-Diagnose.bat
   ```

3. **Run Setup Manually**
   ```powershell
   .\Setup-Game.ps1 -Verbose
   ```
   The -Verbose flag shows detailed progress

4. **Check PowerShell Execution Policy**
   ```powershell
   Get-ExecutionPolicy
   ```
   If it shows "Restricted", run:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

---

### Issue: "Unity Hub not found"

**Solutions:**

1. **Let installer download it**
   - Just run `Launch Game.bat` again
   - It will auto-download Unity Hub

2. **Manual installation**
   - Download from: https://unity.com/download
   - Install to default location
   - Run `Launch Game.bat` again

3. **Check installation path**
   Expected location: `C:\Program Files\Unity Hub\Unity Hub.exe`
   
   If installed elsewhere, Unity Hub should still work via PATH

---

### Issue: "Unity 2022.3 not installed"

**Solutions:**

1. **Follow guided installation**
   - The setup script will open Unity Hub
   - Install Unity 2022.3 LTS from Hub
   - Press any key in the terminal when done

2. **Manual installation**
   - Open Unity Hub
   - Go to "Installs" tab
   - Click "Install Editor"
   - Select "Unity 2022.3 LTS"
   - Install required modules (Android, iOS, Windows)

3. **Use compatible version**
   - Any 2022.3.x version works
   - The script auto-detects compatible versions

---

### Issue: Script execution errors

**Error:** "Running scripts is disabled on this system"

**Solution:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Error:** "Access denied" or "Administrator required"

**Solution:**
- Right-click `Launch Game.bat`
- Select "Run as administrator"

---

### Issue: Unity won't open project

**Symptoms:**
- Unity opens but shows error
- "Invalid project folder" message
- Assets won't import

**Solutions:**

1. **Verify project structure**
   ```
   Run: Quick-Diagnose.bat
   ```
   Should show:
   - [OK] Assets folder exists
   - [OK] ProjectSettings folder exists
   - [OK] Packages folder exists

2. **Recreate project files**
   ```powershell
   .\Setup-Game.ps1 -SkipUnityInstall
   ```

3. **Clear Unity cache**
   - Close Unity completely
   - Delete `Library` folder (if it exists)
   - Delete `Temp` folder (if it exists)
   - Open project again

4. **Check Unity version**
   - Open `ProjectSettings/ProjectVersion.txt`
   - Should show: `m_EditorVersion: 2022.3.x`
   - Install matching Unity version

---

### Issue: Low disk space

**Error:** "Not enough disk space"

**Requirements:**
- Unity Hub: ~150 MB
- Unity 2022.3 LTS: ~3-6 GB
- Project workspace: ~2 GB
- **Total needed:** ~10 GB free

**Solutions:**
- Free up disk space
- Install on a different drive (manual installation)
- Move project folder to drive with more space

---

### Issue: Internet connection problems

**Error:** "Failed to download Unity Hub"

**Solutions:**

1. **Check connection**
   ```powershell
   Test-Connection -ComputerName 8.8.8.8 -Count 4
   ```

2. **Try manual download**
   - Download Unity Hub from: https://unity.com/download
   - Install manually
   - Run `Launch Game.bat` again

3. **Check firewall**
   - Ensure PowerShell can access internet
   - Allow Unity Hub through firewall

---

### Issue: Slow performance in Unity

**Solutions:**

1. **Lower quality settings**
   - In Unity: Edit → Project Settings → Quality
   - Select "Low" preset
   - Reduce resolution

2. **Close background applications**
   - Check Task Manager
   - Close unnecessary programs
   - Ensure 8GB+ RAM available

3. **Update graphics drivers**
   - Check manufacturer website
   - Install latest drivers

---

## 🔧 Advanced Troubleshooting

### View Full Setup Log
```powershell
.\View-Log.ps1
Select: [1] View Full Setup Log
```

### Export Logs for Support
```powershell
.\View-Log.ps1
Select: [9] Export Logs for Support
```
This creates a complete log file you can share when asking for help.

### Reset Everything
```powershell
# Clear logs
.\View-Log.ps1
Select: [8] Clear All Logs

# Delete Unity cache (careful!)
Remove-Item Library -Recurse -Force
Remove-Item Temp -Recurse -Force

# Run setup fresh
.\Setup-Game.ps1
```

### Manual Project Setup
If automated setup fails completely:

1. Install Unity Hub manually
2. Install Unity 2022.3 LTS manually
3. Open Unity Hub → "Open" → Select this folder
4. Wait for import (5-10 minutes)
5. Open MainMenu scene
6. Press Play

---

## 📊 System Requirements Check

Run diagnostics to verify:
```
Quick-Diagnose.bat
```

**Minimum Requirements:**
- Windows 10 64-bit
- 8 GB RAM
- 10 GB free disk space
- Intel HD 4000 or equivalent GPU
- Internet connection

**Recommended:**
- Windows 11 64-bit
- 16 GB RAM
- 20 GB free SSD space
- Dedicated GPU (GTX 1050 or better)
- Stable internet connection

---

## 🆘 Still Having Issues?

### Check Documentation
- [INSTALL.md](INSTALL.md) - Complete installation guide
- [GETTING_STARTED.md](GETTING_STARTED.md) - How to play
- [README.md](README.md) - Full project documentation

### Export Debug Info
```powershell
.\View-Log.ps1
# Select option [9] Export Logs for Support
```

This creates a file with:
- System information
- Complete setup log
- All errors encountered
- Diagnostic results

### Get Help
1. Export your logs (see above)
2. Open an issue on GitHub
3. Attach the exported log file
4. Describe what happens when you run the launcher

**GitHub Issues:** https://github.com/tydawkinsbuynsale-cell/tower-defense/issues

---

## 📝 Debugging Commands

### Check if Unity Hub is running
```powershell
Get-Process | Where-Object {$_.Name -like "*Unity*"}
```

### Check Unity installations
```powershell
Get-ChildItem "C:\Program Files\Unity\Hub\Editor"
```

### Test PowerShell script directly
```powershell
.\Setup-Game.ps1 -Verbose -SkipUnityInstall
```

### View real-time log
```powershell
Get-Content Setup-Log.txt -Wait -Tail 20
```

---

## ✅ Verification Checklist

Before reporting issues, verify:
- [ ] Ran `Quick-Diagnose.bat`
- [ ] Checked `View-Log.ps1` for errors
- [ ] At least 10 GB free disk space
- [ ] Internet connection working
- [ ] PowerShell execution policy set
- [ ] Tried running as administrator
- [ ] Unity Hub shows in diagnostics
- [ ] Unity 2022.3.x detected
- [ ] All project folders exist
- [ ] Reviewed Setup-Errors.txt (if exists)
