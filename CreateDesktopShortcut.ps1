# Robot Tower Defense - Desktop Shortcut Creator
# This script creates a desktop shortcut to launch the game

$projectPath = "C:\Users\tydaw\OneDrive\Documents\RobotTowerDefense"
$shortcutPath = [Environment]::GetFolderPath("Desktop") + "\Robot Tower Defense.lnk"
$iconPath = "$projectPath\Assets\Icon\GameIcon.png"

# Check if Unity Editor executable exists (common paths)
$unityPaths = @(
    "C:\Program Files\Unity\Hub\Editor\2022.3.*\Editor\Unity.exe",
    "C:\Program Files\Unity\Editor\Unity.exe",
    "${env:ProgramFiles}\Unity\Hub\Editor\*\Editor\Unity.exe"
)

$unityExe = $null
foreach ($path in $unityPaths) {
    $found = Get-Item $path -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $unityExe = $found.FullName
        break
    }
}

if (-not $unityExe) {
    Write-Host "Unity Editor not found in standard locations." -ForegroundColor Yellow
    Write-Host "Please enter the path to Unity.exe:" -ForegroundColor Cyan
    $unityExe = Read-Host
}

# Create the shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $unityExe
$Shortcut.Arguments = "-projectPath `"$projectPath`""
$Shortcut.WorkingDirectory = $projectPath
$Shortcut.Description = "Launch Robot Tower Defense in Unity Editor"

# Try to set icon (Windows shortcuts with .lnk can't use .png directly, but Unity.exe icon will be used)
# For a proper icon, we'd need to convert to .ico format
$Shortcut.IconLocation = $unityExe + ",0"

$Shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green
Write-Host "Target: $unityExe" -ForegroundColor Green
Write-Host "Project: $projectPath" -ForegroundColor Green
Write-Host ""
Write-Host "Double-click Robot Tower Defense on your desktop to launch!" -ForegroundColor Cyan
