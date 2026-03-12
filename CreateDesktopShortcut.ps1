<#
.SYNOPSIS
    Creates a desktop shortcut for Robot Tower Defense
.DESCRIPTION
    Creates shortcuts on the desktop that launch the game and diagnostics automatically
#>

$ShortcutPath = [System.IO.Path]::Combine([Environment]::GetFolderPath("Desktop"), "Robot Tower Defense.lnk")
$TargetPath = Join-Path $PSScriptRoot "Launch Game.bat"
$IconPath = Join-Path $PSScriptRoot "Assets\Icon\GameIcon.png"

# Check if icon exists, if not use default
if (-not (Test-Path $IconPath)) {
    Write-Host "Game icon not found, using default launcher icon" -ForegroundColor Yellow
    $IconPath = $TargetPath
}

Write-Host "Creating desktop shortcuts..." -ForegroundColor Cyan
Write-Host ""

try {
    $WshShell = New-Object -ComObject WScript.Shell
    
    # Main game launcher shortcut
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $TargetPath
    $Shortcut.WorkingDirectory = $PSScriptRoot
    $Shortcut.Description = "Launch Robot Tower Defense - Auto Setup & Play"
    
    # Set icon if PNG exists (convert to ICO if needed)
    if ((Test-Path $IconPath) -and $IconPath -like "*.png") {
        # For now, use the .bat icon - you can convert PNG to ICO separately
        $Shortcut.IconLocation = $TargetPath + ",0"
    } else {
        $Shortcut.IconLocation = $IconPath + ",0"
    }
    
    $Shortcut.Save()
    
    Write-Host "✓ Main launcher shortcut created!" -ForegroundColor Green
    Write-Host "  Location: $ShortcutPath" -ForegroundColor White
    Write-Host ""
    
    # Diagnostics shortcut
    $DiagShortcutPath = [System.IO.Path]::Combine([Environment]::GetFolderPath("Desktop"), "Robot TD - Diagnostics.lnk")
    $DiagTargetPath = Join-Path $PSScriptRoot "Quick-Diagnose.bat"
    
    if (Test-Path $DiagTargetPath) {
        $DiagShortcut = $WshShell.CreateShortcut($DiagShortcutPath)
        $DiagShortcut.TargetPath = $DiagTargetPath
        $DiagShortcut.WorkingDirectory = $PSScriptRoot
        $DiagShortcut.Description = "Run diagnostics and view logs for Robot Tower Defense"
        $DiagShortcut.IconLocation = $DiagTargetPath + ",0"
        $DiagShortcut.Save()
        
        Write-Host "✓ Diagnostics shortcut created!" -ForegroundColor Green
        Write-Host "  Location: $DiagShortcutPath" -ForegroundColor White
        Write-Host "  Use this to check logs and troubleshoot issues." -ForegroundColor Gray
        Write-Host ""
    }
    
    Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Setup Complete!" -ForegroundColor Green
    Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now launch the game from your desktop!" -ForegroundColor White
    Write-Host ""
    Write-Host "Desktop icons created:" -ForegroundColor Gray
    Write-Host "  • Robot Tower Defense (launches game)" -ForegroundColor White
    Write-Host "  • Robot TD - Diagnostics (troubleshooting)" -ForegroundColor White
    
}
catch {
    Write-Host "✗ Failed to create shortcut: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Read-Host "Press Enter to exit"
