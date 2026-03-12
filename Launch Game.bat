@echo off
title Robot Tower Defense - Unity Launcher

echo ========================================
echo   ROBOT TOWER DEFENSE
echo   Unity Project Launcher
echo ========================================
echo.

REM Try to find Unity Hub
set "UNITY_HUB=C:\Program Files\Unity Hub\Unity Hub.exe"

if exist "%UNITY_HUB%" (
    echo Launching via Unity Hub...
    start "" "%UNITY_HUB%" -- --projectPath "%~dp0"
) else (
    echo Unity Hub not found at standard location.
    echo Opening project folder instead...
    echo.
    echo To play the game:
    echo 1. Open Unity Hub
    echo 2. Click "Open" and select this folder
    echo 3. Press Play in the Unity Editor
    echo.
    start "" "%~dp0"
    pause
)
