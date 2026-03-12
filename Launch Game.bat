@echo off
title Robot Tower Defense - Auto Setup & Launcher

echo ========================================
echo   ROBOT TOWER DEFENSE
echo   Automatic Setup ^& Launcher
echo ========================================
echo.

REM Check if Unity is installed
set "UNITY_HUB=C:\Program Files\Unity Hub\Unity Hub.exe"

if exist "%UNITY_HUB%" (
    REM Unity Hub found - use quick launcher
    echo Unity Hub detected. Launching game...
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Game.ps1" -SkipUnityInstall
) else (
    REM Unity Hub not found - run full setup
    echo Unity Hub not detected. Starting automatic setup...
    echo This will download and install Unity automatically.
    echo.
    pause
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Game.ps1"
)

