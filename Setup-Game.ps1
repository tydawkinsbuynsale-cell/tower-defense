<#
.SYNOPSIS
    Robot Tower Defense - Automatic Setup and Launcher
.DESCRIPTION
    This script automatically downloads and installs all dependencies needed to run the game:
    - Unity Hub
    - Unity 2022.3 LTS
    - Creates required project structure
    - Launches the game
#>

param(
    [switch]$SkipUnityInstall,
    [switch]$Verbose,
    [switch]$ShowLog
)

$ErrorActionPreference = "Continue"  # Changed to Continue to log errors instead of stopping
$ProgressPreference = "SilentlyContinue"

# Configuration
$UNITY_VERSION = "2022.3.25f1"  # Update to latest 2022.3 LTS patch
$UNITY_HUB_URL = "https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe"
$REQUIRED_MODULES = @("android", "ios", "windows-mono")

# Paths
$PROJECT_ROOT = $PSScriptRoot
$UNITY_HUB_PATH = "C:\Program Files\Unity Hub\Unity Hub.exe"
$UNITY_INSTALL_BASE = "C:\Program Files\Unity\Hub\Editor"
$TEMP_DIR = "$env:TEMP\RobotTDSetup"
$LOG_FILE = Join-Path $PROJECT_ROOT "Setup-Log.txt"
$ERROR_LOG = Join-Path $PROJECT_ROOT "Setup-Errors.txt"

# Colors for output
$COLOR_INFO = "Cyan"
$COLOR_SUCCESS = "Green"
$COLOR_WARNING = "Yellow"
$COLOR_ERROR = "Red"

# Initialize logging
function Initialize-Logging {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $separator = "="*80
    $logHeader = @"
$separator
ROBOT TOWER DEFENSE - Setup Log
Started: $timestamp
$separator

"@
    Set-Content -Path $LOG_FILE -Value $logHeader -Encoding UTF8
    
    Write-Log "Setup started from: $PROJECT_ROOT"
    Write-Log "PowerShell Version: $($PSVersionTable.PSVersion)"
    Write-Log "OS: $([System.Environment]::OSVersion.VersionString)"
    Write-Log "User: $env:USERNAME"
    Write-Log ""
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    $timestamp = Get-Date -Format "HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Add-Content -Path $LOG_FILE -Value $logMessage -Encoding UTF8
    
    if ($Verbose) {
        Write-Host $logMessage -ForegroundColor Gray
    }
}

function Write-ErrorLog {
    param(
        [string]$Message,
        [string]$Exception = ""
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $errorMessage = @"
[$timestamp] ERROR
Message: $Message
$(if ($Exception) { "Exception: $Exception`n" })
---
"@
    Add-Content -Path $ERROR_LOG -Value $errorMessage -Encoding UTF8
    Write-Log $Message "ERROR"
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n===================================" -ForegroundColor $COLOR_INFO
    Write-Host " $Message" -ForegroundColor $COLOR_INFO
    Write-Host "===================================`n" -ForegroundColor $COLOR_INFO
    Write-Log "STEP: $Message" "STEP"
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor $COLOR_SUCCESS
    Write-Log $Message "SUCCESS"
}

function Write-Info {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor $COLOR_INFO
    Write-Log $Message "INFO"
}

function Write-Warn {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor $COLOR_WARNING
    Write-Log $Message "WARNING"
}

function Write-Err {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor $COLOR_ERROR
    Write-ErrorLog $Message
}

function Test-AdminPrivileges {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}Write-Log "Checking Unity Hub at: $UNITY_HUB_PATH"
    
    if (Test-Path $UNITY_HUB_PATH) {
        Write-Success "Unity Hub already installed at: $UNITY_HUB_PATH"
        return $true
    }

    Write-Info "Unity Hub not found. Starting download..."
    Write-Log "Creating temp directory: $TEMP_DIR"
    
    try {
        New-Item -ItemType Directory -Force -Path $TEMP_DIR | Out-Null
        $installerPath = "$TEMP_DIR\UnityHubSetup.exe"
        Write-Log "Installer path: $installerPath"
        
        Write-Info "Downloading Unity Hub from: $UNITY_HUB_URL"
        Write-Log "Starting download..."
        
        Invoke-WebRequest -Uri $UNITY_HUB_URL -OutFile $installerPath -UseBasicParsing
        
        if (Test-Path $installerPath) {
            $fileSize = (Get-Item $installerPath).Length / 1MB
            Write-Success "Unity Hub downloaded ($([math]::Round($fileSize, 2)) MB)"
            Write-Log "Download complete. File size: $fileSize MB"
        } else {
            throw "Downloaded file not found at $installerPath"
        }
        
        Write-Info "Installing Unity Hub (this may take a few minutes)..."
        Write-Warn "A UAC prompt may appear - please click 'Yes' to continue"
        Write-Log "Launching installer with /S flag"
        
        $process = Start-Process -FilePath $installerPath -ArgumentList "/S" -Wait -PassThru
        Write-Log "Installer exit code: $($process.ExitCode)"
        
        if ($process.ExitCode -eq 0 -and (Test-Path $UNITY_HUB_PATH)) {
            Write-Success "Unity Hub installed successfully"
            Write-Log "Unity Hub verified at: $UNITY_HUB_PATH"
            return $true
        } else {
            Write-Err "Unity Hub installation failed with exit code: $($process.ExitCode)"
            Write-ErrorLog "Unity Hub installation failed" "Exit code: $($process.ExitCode)"
            return $false
        }
    }
    catch {
        Write-Err "Failed to download/install Unity Hub: $_"
        Write-ErrorLog "Unity Hub installation exception" $_.Exception.Message
        Write-Log "Stack trace: $($_.ScriptStackTrace)
        }
    }
    catch {
        Write-Err "Failed to download/install Unity Hub: $_"
        return $false
    }
}

function Get-InstalledUnityVersions {
    if (-not (Test-Path $UNITY_INSTALL_BASE)) {
        return @()
    }
    return Get-ChildItem -Path $UNITY_INSTALL_BASE -Directory | Select-Object -ExpandProperty Name
}

function Test-UnityVersion {
    param([string]$Version)
    $unityPath = Join-Path $UNITY_INSTALL_BASE $Version
    return Test-Path (Join-Path $unityPath "Editor\Unity.exe")
}

function Install-UnityEditor {
    Write-Step "Installing Unity $UNITY_VERSION"
    
    $installedVersions = Get-InstalledUnityVersions
    
    # Check if exact version is installed
    if (Test-UnityVersion $UNITY_VERSION) {
        Write-Success "Unity $UNITY_VERSION already installed"
        return $true
    }
    
    # Check if any 2022.3.x version is installed
    $compatible = $installedVersions | Where-Object { $_ -like "2022.3.*" } | Select-Object -First 1
    if ($compatible) {
        Write-Success "Compatible Unity version found: $compatible"
        $global:UNITY_VERSION = $compatible
        return $true
    }

    Write-Info "Unity 2022.3 LTS not found. Installation required."
    Write-Warn @"

IMPORTANT: Unity installation requires Unity Hub's GUI
Please follow these steps:

1. Unity Hub will open automatically
2. Click 'Installs' in the left sidebar
3. Click 'Install Editor'
4. Select 'Unity 2022.3 LTS' (latest patch version)
5. Click 'Next'
6. Select these modules:
   ☑ Android Build Support
   ☑ iOS Build Support  
   ☑ Windows Build Support (IL2CPP)
7. Click 'Done' and wait for installation to complete

Press any key when ready to open Unity Hub...
"@
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # Launch Unity Hub to Installs page
    Start-Process -FilePath $UNITY_HUB_PATH
    
    Write-Info "Waiting for Unity installation..."
    Write-Info "Press any key once Unity 2022.3 LTS installation is complete..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # Verify installation
    $installedVersions = Get-InstalledUnityVersions
    $installed = $installedVersions | Where-Object { $_ -like "2022.3.*" } | Select-Object -First 1
    
    if ($installed) {
        $global:UNITY_VERSION = $installed
        Write-Success "Unity $installed detected"
        return $true
    } else {
        Write-Err "Unity 2022.3 LTS not found. Please ensure installation completed."
        return $false
    }
}

function Initialize-UnityProject {
    Write-Step "Initializing Unity Project Structure"
    
    # Check if project already initialized
    if ((Test-Path "$PROJECT_ROOT\ProjectSettings") -and (Test-Path "$PROJECT_ROOT\Packages")) {
        Write-Success "Unity project structure already exists"
        return $true
    }

    Write-Info "Creating Unity project folders..."
    
    # Create essential directories
    $directories = @(
        "ProjectSettings",
        "Packages", 
        "Assets/Scenes",
        "Assets/Prefabs",
        "Assets/Materials",
        "Assets/Textures",
        "Assets/Audio",
        "Assets/Resources"
    )
    
    foreach ($dir in $directories) {
        $path = Join-Path $PROJECT_ROOT $dir
        if (-not (Test-Path $path)) {
            New-Item -ItemType Directory -Force -Path $path | Out-Null
            Write-Info "Created: $dir"
        }
    }

    # Create ProjectSettings/ProjectVersion.txt
    $projectVersionPath = Join-Path $PROJECT_ROOT "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $projectVersionPath)) {
        $versionContent = @"
m_EditorVersion: $UNITY_VERSION
m_EditorVersionWithRevision: $UNITY_VERSION
"@
        Set-Content -Path $projectVersionPath -Value $versionContent -Encoding UTF8
        Write-Info "Created: ProjectSettings/ProjectVersion.txt"
    }

    # Create Packages/manifest.json
    $manifestPath = Join-Path $PROJECT_ROOT "Packages\manifest.json"
    if (-not (Test-Path $manifestPath)) {
        $manifestContent = @'
{
  "dependencies": {
    "com.unity.collab-proxy": "2.0.5",
    "com.unity.feature.2d": "2.0.0",
    "com.unity.ide.rider": "3.0.24",
    "com.unity.ide.visualstudio": "2.0.18",
    "com.unity.ide.vscode": "1.2.5",
    "com.unity.test-framework": "1.1.33",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.timeline": "1.7.4",
    "com.unity.ugui": "1.0.0",
    "com.unity.visualscripting": "1.8.0",
    "com.unity.modules.ai": "1.0.0",
    "com.unity.modules.androidjni": "1.0.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.cloth": "1.0.0",
    "com.unity.modules.director": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.screencapture": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.terrainphysics": "1.0.0",
    "com.unity.modules.tilemap": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unityanalytics": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
    "com.unity.modules.unitywebrequestaudio": "1.0.0",
    "com.unity.modules.unitywebrequesttexture": "1.0.0",
    "com.unity.modules.unitywebrequestwww": "1.0.0",
    "com.unity.modules.vehicles": "1.0.0",
    "com.unity.modules.video": "1.0.0",
    "com.unity.modules.vr": "1.0.0",
    "com.unity.modules.wind": "1.0.0",
    "com.unity.modules.xr": "1.0.0"
  }
}
'@
        Set-Content -Path $manifestPath -Value $manifestContent -Encoding UTF8
        Write-Info "Created: Packages/manifest.json"
    }

    # Create a simple README in Assets
    $assetsReadmePath = Join-Path $PROJECT_ROOT "Assets\README.txt"
    if (-not (Test-Path $assetsReadmePath)) {
        $readmeContent = @"
Robot Tower Defense - Unity Project

This project contains:
- Scripts: C# game logic
- Scenes: Game levels and menus  
- Prefabs: Reusable game objects
- Materials: Visual shaders
- Audio: Music and sound effects
Write-Log "Looking for Unity at: $unityExePath"
    
    if (-not (Test-Path $unityExePath)) {
        Write-Err "Unity executable not found at: $unityExePath"
        Write-Log "Searching for any Unity 2022.3.x installation..."
        
        # Try to find any 2022.3.x version
        $installedVersions = Get-InstalledUnityVersions
        $compatible = $installedVersions | Where-Object { $_ -like "2022.3.*" } | Select-Object -First 1
        
        if ($compatible) {
            $global:UNITY_VERSION = $compatible
            $unityExePath = Join-Path $UNITY_INSTALL_BASE "$UNITY_VERSION\Editor\Unity.exe"
            Write-Info "Found compatible version: $UNITY_VERSION"
            Write-Log "Using Unity version: $UNITY_VERSION"
        } else {
            Write-ErrorLog "No Unity 2022.3.x installation found"
            return $false
        }
    }

    Write-Info "Opening project in Unity $UNITY_VERSION..."
    Write-Info "This may take a few minutes on first launch..."
    Write-Log "Launching Unity with project path: $PROJECT_ROOT"
    
    try {
        $arguments = "-projectPath `"$PROJECT_ROOT`""
        Write-Log "Unity arguments: $arguments"
        
        Start-Process -FilePath $unityExePath -ArgumentList $arguments -WindowStyle Normal
        Write-Success "Unity Editor launched!"
        Write-Log "Unity process started successfully"
        
        Write-Info @"

Next steps:
1. Wait for Unity to import assets (first launch takes longer)
2. Open MainMenu scene from Assets/Scenes/
3. Press the Play button (▶) at the top
4. Enjoy the game!

Log file saved to: $LOG_FILE

"@Show-LogSummary {
    Write-Host "`n" -NoNewline
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  SETUP COMPLETE" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Log files saved:" -ForegroundColor White
    Write-Host "  Setup Log:  " -NoNewline -ForegroundColor Gray
    Write-Host "$LOG_FILE" -ForegroundColor Yellow
    
    if (Test-Path $ERROR_LOG) {
        Write-Host "  Error Log:  " -NoNewline -ForegroundColor Gray
        Write-Host "$ERROR_LOG" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "To view logs:" -ForegroundColor White
    Write-Host "  .\View-Log.ps1" -ForegroundColor Cyan
    Write-Host ""
}

function Main {
    # Initialize logging first
    Initialize-Logging
    
    Show-Banner
    
    Write-Info "Project: $PROJECT_ROOT"
    Write-Info "Required Unity Version: 2022.3 LTS"
    Write-Info "Log file: $LOG_FILE"
    Write-Info ""

    Write-Log "=== DIAGNOSTICS ==="
    Write-Log "Unity Hub Path: $UNITY_HUB_PATH"
    Write-Log "Unity Hub Exists: $(Test-Path $UNITY_HUB_PATH)"
    Write-Log "Unity Install Base: $UNITY_INSTALL_BASE"
    Write-Log "Unity Install Base Exists: $(Test-Path $UNITY_INSTALL_BASE)"
    Write-Log "Project Root: $PROJECT_ROOT"
    Write-Log "Admin Privileges: $(Test-AdminPrivileges)"
    Write-Log ""

    # Check admin privileges
    if (-not (Test-AdminPrivileges)) {
        Write-Warn "This script may require administrator privileges for installation."
        Write-Info "If prompted by UAC, please click 'Yes' to continue."
        Write-Log "Running without admin privileges"
        Write-Info ""
    }

    try {
        # Step 1: Install Unity Hub
        if (-not (Install-UnityHub)) {
            Write-Err "Setup failed: Could not install Unity Hub"
            Write-Log "FAILED: Unity Hub installation"
            Show-LogSummary
            Read-Host "Press Enter to exit"
            exit 1
        }

        # Step 2: Install Unity Editor
        if (-not $SkipUnityInstall) {
            if (-not (Install-UnityEditor)) {
                Write-Err "Setup failed: Could not install Unity Editor"
                Write-Log "FAILED: Unity Editor installation"
                Show-LogSummary
                Read-Host "Press Enter to exit"
                exit 1
            }
        } else {
            Write-Log "Skipping Unity installation (SkipUnityInstall flag set)"
        }

        # Step 3: Initialize project structure
        if (-not (Initialize-UnityProject)) {
            Write-Err "Setup failed: Could not initialize project"
            Write-Log "FAILED: Project initialization"
            Show-LogSummary
            Read-Host "Press Enter to exit"
            exit 1
        }

        # Step 4: Launch Unity
        if (-not (Open-UnityProject)) {
            Write-Err "Setup failed: Could not launch Unity"
            Write-Log "FAILED: Unity launch"
            Show-LogSummary
            Read-Host "Press Enter to exit"
            exit 1
        }

        Write-Host "`n"
        Write-Success "Setup complete! Unity is now starting..."
        Write-Log "=== SETUP COMPLETED SUCCESSFULLY ==="
        Write-Info "You can run this script again anytime with: .\Setup-Game.ps1"
        
        Show-LogSummary
        
        Start-Sleep -Seconds 3
        
    } catch {
        Write-Err "Unexpected error during setup: $_"
        Write-ErrorLog "Unexpected exception in Main" $_.Exception.Message
        Write-Log "Stack trace: $($_.ScriptStackTrace)"
        Show-LogSummary
        Read-Host "Press Enter to exit"
        exit 1
    }nity Version: 2022.3 LTS"
    Write-Info ""

    # Check admin privileges
    if (-not (Test-AdminPrivileges)) {
        Write-Warn "This script may require administrator privileges for installation."
        Write-Info "If prompted by UAC, please click 'Yes' to continue."
        Write-Info ""
    }

    # Step 1: Install Unity Hub
    if (-not (Install-UnityHub)) {
        Write-Err "Setup failed: Could not install Unity Hub"
        Read-Host "Press Enter to exit"
        exit 1
    }

    # Step 2: Install Unity Editor
    if (-not $SkipUnityInstall) {
        if (-not (Install-UnityEditor)) {
            Write-Err "Setup failed: Could not install Unity Editor"
            Read-Host "Press Enter to exit"
            exit 1
        }
    }

    # Step 3: Initialize project structure
    if (-not (Initialize-UnityProject)) {
        Write-Err "Setup failed: Could not initialize project"
        Read-Host "Press Enter to exit"
        exit 1
    }

    # Step 4: Launch Unity
    if (-not (Open-UnityProject)) {
        Write-Err "Setup failed: Could not launch Unity"
        Read-Host "Press Enter to exit"
        exit 1
    }

    Write-Host "`n"
    Write-Success "Setup complete! Unity is now starting..."
    Write-Info "You can run this script again anytime with: .\Setup-Game.ps1"
    Write-Host "`n"
    
    Start-Sleep -Seconds 3
}

# Run main function
try {
    Main
}
catch {
    Write-Err "An unexpected error occurred: $_"
    Write-Err $_.ScriptStackTrace
    Read-Host "`nPress Enter to exit"
    exit 1
}
