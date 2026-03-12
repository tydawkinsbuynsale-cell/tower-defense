@echo off
title Robot Tower Defense - Auto Setup ^& Launcher

echo ========================================
echo   ROBOT TOWER DEFENSE
echo   Automatic Setup ^& Launcher
echo ========================================
echo.

REM Enable delayed expansion for error handling
setlocal enabledelayedexpansion

REM Check if Unity is installed
set "UNITY_HUB=C:\Program Files\Unity Hub\Unity Hub.exe"

if exist "%UNITY_HUB%" (
    REM Unity Hub found - use quick launcher
    echo Unity Hub detected. Launching game...
    echo.
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Game.ps1" -SkipUnityInstall
    set EXITCODE=!ERRORLEVEL!
) else (
    REM Unity Hub not found - run full setup
    echo Unity Hub not detected. Starting automatic setup...
    echo This will download and install Unity automatically.
    echo.
    pause
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Game.ps1"
    set EXITCODE=!ERRORLEVEL!
)

echo.
echo ========================================
if !EXITCODE! EQU 0 (
    echo   Setup completed successfully!
) else (
    echo   Setup encountered issues.
    echo   Exit code: !EXITCODE!
)
echo ========================================
echo.

REM Check for log files
if exist "%~dp0Setup-Log.txt" (
    echo Log file created: Setup-Log.txt
)
if exist "%~dp0Setup-Errors.txt" (
    echo Error log created: Setup-Errors.txt
    echo.
    echo To view errors, run: View-Log.ps1
)

echo.
echo Press any key to close this window...
pause >nul

