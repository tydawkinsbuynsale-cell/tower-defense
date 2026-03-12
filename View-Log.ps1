<#
.SYNOPSIS
    View Robot Tower Defense Setup and Error Logs
.DESCRIPTION
    Interactive log viewer with filtering, search, and analysis capabilities
#>

param(
    [switch]$ShowErrors,
    [switch]$ShowAll,
    [switch]$Tail,
    [int]$Lines = 50
)

$LOG_FILE = Join-Path $PSScriptRoot "Setup-Log.txt"
$ERROR_LOG = Join-Path $PSScriptRoot "Setup-Errors.txt"

$COLOR_HEADER = "Cyan"
$COLOR_INFO = "White"
$COLOR_SUCCESS = "Green"
$COLOR_WARNING = "Yellow"
$COLOR_ERROR = "Red"
$COLOR_STEP = "Magenta"

function Show-Banner {
    Clear-Host
    Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║                                                                ║
║           ROBOT TOWER DEFENSE - LOG VIEWER                    ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝

"@ -ForegroundColor $COLOR_HEADER
}

function Show-LogFile {
    param(
        [string]$FilePath,
        [string]$Title,
        [switch]$ColorCode
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "No $Title found at: $FilePath" -ForegroundColor $COLOR_WARNING
        Write-Host ""
        return
    }

    $fileInfo = Get-Item $FilePath
    Write-Host "═══ $Title ═══" -ForegroundColor $COLOR_HEADER
    Write-Host "Location: " -NoNewline -ForegroundColor Gray
    Write-Host $FilePath -ForegroundColor White
    Write-Host "Size: " -NoNewline -ForegroundColor Gray
    Write-Host "$([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor White
    Write-Host "Modified: " -NoNewline -ForegroundColor Gray
    Write-Host $fileInfo.LastWriteTime -ForegroundColor White
    Write-Host ""

    $content = Get-Content $FilePath
    
    if ($Tail) {
        $content = $content | Select-Object -Last $Lines
    }

    if ($ColorCode) {
        foreach ($line in $content) {
            if ($line -match "\[ERROR\]") {
                Write-Host $line -ForegroundColor $COLOR_ERROR
            }
            elseif ($line -match "\[WARNING\]") {
                Write-Host $line -ForegroundColor $COLOR_WARNING
            }
            elseif ($line -match "\[SUCCESS\]") {
                Write-Host $line -ForegroundColor $COLOR_SUCCESS
            }
            elseif ($line -match "\[STEP\]") {
                Write-Host $line -ForegroundColor $COLOR_STEP
            }
            elseif ($line -match "^=+$" -or $line -match "^===") {
                Write-Host $line -ForegroundColor $COLOR_HEADER
            }
            else {
                Write-Host $line -ForegroundColor $COLOR_INFO
            }
        }
    }
    else {
        Write-Host $content -ForegroundColor $COLOR_INFO
    }
    
    Write-Host ""
}

function Get-LogStats {
    if (-not (Test-Path $LOG_FILE)) {
        return
    }

    $content = Get-Content $LOG_FILE
    $errors = ($content | Select-String -Pattern "\[ERROR\]").Count
    $warnings = ($content | Select-String -Pattern "\[WARNING\]").Count
    $successes = ($content | Select-String -Pattern "\[SUCCESS\]").Count
    $steps = ($content | Select-String -Pattern "\[STEP\]").Count

    Write-Host "═══ Log Statistics ═══" -ForegroundColor $COLOR_HEADER
    Write-Host "Total Lines:    " -NoNewline -ForegroundColor Gray
    Write-Host $content.Count -ForegroundColor White
    Write-Host "Steps:          " -NoNewline -ForegroundColor Gray
    Write-Host $steps -ForegroundColor $COLOR_STEP
    Write-Host "Successes:      " -NoNewline -ForegroundColor Gray
    Write-Host $successes -ForegroundColor $COLOR_SUCCESS
    Write-Host "Warnings:       " -NoNewline -ForegroundColor Gray
    Write-Host $warnings -ForegroundColor $COLOR_WARNING
    Write-Host "Errors:         " -NoNewline -ForegroundColor Gray
    Write-Host $errors -ForegroundColor $COLOR_ERROR
    Write-Host ""
}

function Show-RecentErrors {
    if (-not (Test-Path $ERROR_LOG)) {
        Write-Host "No errors logged yet! 🎉" -ForegroundColor $COLOR_SUCCESS
        Write-Host ""
        return
    }

    Write-Host "═══ Recent Errors ═══" -ForegroundColor $COLOR_HEADER
    Write-Host ""
    
    $content = Get-Content $ERROR_LOG -Raw
    $errors = $content -split "---" | Where-Object { $_.Trim() -ne "" }
    
    $recentErrors = $errors | Select-Object -Last 5
    
    foreach ($error in $recentErrors) {
        Write-Host $error.Trim() -ForegroundColor $COLOR_ERROR
        Write-Host ""
    }
}

function Show-Diagnostics {
    Write-Host "═══ System Diagnostics ═══" -ForegroundColor $COLOR_HEADER
    Write-Host ""
    
    # Check Unity Hub
    $unityHubPath = "C:\Program Files\Unity Hub\Unity Hub.exe"
    Write-Host "Unity Hub:           " -NoNewline -ForegroundColor Gray
    if (Test-Path $unityHubPath) {
        Write-Host "✓ Installed" -ForegroundColor $COLOR_SUCCESS
    } else {
        Write-Host "✗ Not Found" -ForegroundColor $COLOR_ERROR
    }
    
    # Check Unity Editors
    $unityBasePath = "C:\Program Files\Unity\Hub\Editor"
    Write-Host "Unity Editors:       " -NoNewline -ForegroundColor Gray
    if (Test-Path $unityBasePath) {
        $editors = Get-ChildItem -Path $unityBasePath -Directory -ErrorAction SilentlyContinue
        if ($editors) {
            Write-Host "✓ $($editors.Count) installed" -ForegroundColor $COLOR_SUCCESS
            foreach ($editor in $editors) {
                Write-Host "  - $($editor.Name)" -ForegroundColor Gray
            }
        } else {
            Write-Host "⚠ Directory exists but no editors found" -ForegroundColor $COLOR_WARNING
        }
    } else {
        Write-Host "✗ Not Found" -ForegroundColor $COLOR_ERROR
    }
    
    # Check Project Structure
    Write-Host "Project Structure:   " -NoNewline -ForegroundColor Gray
    $projectOK = $true
    $requiredFolders = @("Assets", "ProjectSettings", "Packages")
    foreach ($folder in $requiredFolders) {
        if (-not (Test-Path (Join-Path $PSScriptRoot $folder))) {
            $projectOK = $false
            break
        }
    }
    if ($projectOK) {
        Write-Host "✓ Complete" -ForegroundColor $COLOR_SUCCESS
    } else {
        Write-Host "⚠ Incomplete" -ForegroundColor $COLOR_WARNING
    }
    
    # Check Disk Space
    $drive = (Get-Item $PSScriptRoot).PSDrive
    $freeSpaceGB = [math]::Round($drive.Free / 1GB, 2)
    Write-Host "Free Disk Space:     " -NoNewline -ForegroundColor Gray
    if ($freeSpaceGB -gt 10) {
        Write-Host "$freeSpaceGB GB ✓" -ForegroundColor $COLOR_SUCCESS
    } elseif ($freeSpaceGB -gt 5) {
        Write-Host "$freeSpaceGB GB ⚠" -ForegroundColor $COLOR_WARNING
    } else {
        Write-Host "$freeSpaceGB GB ✗ LOW!" -ForegroundColor $COLOR_ERROR
    }
    
    # Check Internet Connection
    Write-Host "Internet Connection: " -NoNewline -ForegroundColor Gray
    $ping = Test-Connection -ComputerName "8.8.8.8" -Count 1 -Quiet -ErrorAction SilentlyContinue
    if ($ping) {
        Write-Host "✓ Connected" -ForegroundColor $COLOR_SUCCESS
    } else {
        Write-Host "✗ Not Connected" -ForegroundColor $COLOR_ERROR
    }
    
    Write-Host ""
}

function Show-Menu {
    Write-Host "═══ Options ═══" -ForegroundColor $COLOR_HEADER
    Write-Host "[1] View Full Setup Log" -ForegroundColor White
    Write-Host "[2] View Error Log" -ForegroundColor White
    Write-Host "[3] View Last 50 Lines" -ForegroundColor White
    Write-Host "[4] View Statistics" -ForegroundColor White
    Write-Host "[5] View Recent Errors Only" -ForegroundColor White
    Write-Host "[6] Run Diagnostics" -ForegroundColor White
    Write-Host "[7] Open Log File in Notepad" -ForegroundColor White
    Write-Host "[8] Clear All Logs" -ForegroundColor White
    Write-Host "[9] Export Logs for Support" -ForegroundColor White
    Write-Host "[Q] Quit" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "Select option"
    return $choice
}

function Export-LogsForSupport {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $exportFile = Join-Path $PSScriptRoot "RobotTD-Logs-$timestamp.txt"
    
    $exportContent = @"
ROBOT TOWER DEFENSE - LOG EXPORT
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
========================================

SYSTEM INFORMATION:
- OS: $([System.Environment]::OSVersion.VersionString)
- PowerShell: $($PSVersionTable.PSVersion)
- User: $env:USERNAME
- Computer: $env:COMPUTERNAME

========================================
SETUP LOG:
========================================

"@
    
    if (Test-Path $LOG_FILE) {
        $exportContent += Get-Content $LOG_FILE -Raw
    } else {
        $exportContent += "No setup log found.`n"
    }
    
    $exportContent += @"

========================================
ERROR LOG:
========================================

"@
    
    if (Test-Path $ERROR_LOG) {
        $exportContent += Get-Content $ERROR_LOG -Raw
    } else {
        $exportContent += "No error log found.`n"
    }
    
    Set-Content -Path $exportFile -Value $exportContent -Encoding UTF8
    
    Write-Host ""
    Write-Host "Logs exported to: " -NoNewline -ForegroundColor $COLOR_SUCCESS
    Write-Host $exportFile -ForegroundColor White
    Write-Host "You can share this file when seeking support." -ForegroundColor Gray
    Write-Host ""
    
    Start-Process "notepad.exe" -ArgumentList $exportFile
}

function Clear-AllLogs {
    Write-Host ""
    $confirm = Read-Host "Are you sure you want to clear all logs? (yes/no)"
    
    if ($confirm -eq "yes") {
        if (Test-Path $LOG_FILE) {
            Remove-Item $LOG_FILE -Force
            Write-Host "Setup log cleared." -ForegroundColor $COLOR_SUCCESS
        }
        if (Test-Path $ERROR_LOG) {
            Remove-Item $ERROR_LOG -Force
            Write-Host "Error log cleared." -ForegroundColor $COLOR_SUCCESS
        }
        Write-Host ""
    } else {
        Write-Host "Cancelled." -ForegroundColor $COLOR_WARNING
        Write-Host ""
    }
}

# Main execution
Show-Banner

if ($ShowErrors) {
    Show-RecentErrors
    Read-Host "Press Enter to exit"
    exit
}

if ($ShowAll) {
    Show-LogFile -FilePath $LOG_FILE -Title "Setup Log" -ColorCode
    Write-Host ""
    Show-LogFile -FilePath $ERROR_LOG -Title "Error Log" -ColorCode:$false
    Read-Host "Press Enter to exit"
    exit
}

# Interactive mode
while ($true) {
    $choice = Show-Menu
    
    switch ($choice.ToUpper()) {
        "1" {
            Show-Banner
            Show-LogFile -FilePath $LOG_FILE -Title "Full Setup Log" -ColorCode
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "2" {
            Show-Banner
            Show-LogFile -FilePath $ERROR_LOG -Title "Error Log"
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "3" {
            Show-Banner
            Show-LogFile -FilePath $LOG_FILE -Title "Last 50 Lines" -ColorCode -Tail
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "4" {
            Show-Banner
            Get-LogStats
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "5" {
            Show-Banner
            Show-RecentErrors
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "6" {
            Show-Banner
            Show-Diagnostics
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "7" {
            if (Test-Path $LOG_FILE) {
                Start-Process "notepad.exe" -ArgumentList $LOG_FILE
            } else {
                Write-Host "No log file found." -ForegroundColor $COLOR_WARNING
                Start-Sleep -Seconds 2
            }
            Show-Banner
        }
        "8" {
            Clear-AllLogs
            Start-Sleep -Seconds 2
            Show-Banner
        }
        "9" {
            Export-LogsForSupport
            Read-Host "Press Enter to continue"
            Show-Banner
        }
        "Q" {
            Write-Host "Goodbye!" -ForegroundColor $COLOR_SUCCESS
            exit
        }
        default {
            Write-Host "Invalid option. Please try again." -ForegroundColor $COLOR_ERROR
            Start-Sleep -Seconds 1
            Show-Banner
        }
    }
}
