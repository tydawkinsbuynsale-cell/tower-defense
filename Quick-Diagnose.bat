@echo off
title Robot Tower Defense - Quick Diagnostics

echo ========================================
echo   ROBOT TOWER DEFENSE
echo   Quick Diagnostics
echo ========================================
echo.

echo Running diagnostics...
echo.

powershell.exe -ExecutionPolicy Bypass -Command "& '%~dp0View-Log.ps1' -NonInteractive" 2>nul

if errorlevel 1 (
    echo Running basic diagnostics...
    echo.
    
    REM Check Unity Hub
    if exist "C:\Program Files\Unity Hub\Unity Hub.exe" (
        echo [OK] Unity Hub is installed
    ) else (
        echo [!!] Unity Hub NOT found
    )
    
    REM Check Unity Editors
    if exist "C:\Program Files\Unity\Hub\Editor\" (
        echo [OK] Unity Editor directory exists
        dir /b "C:\Program Files\Unity\Hub\Editor\" 2>nul
    ) else (
        echo [!!] No Unity Editors found
    )
    
    REM Check Project Structure
    if exist "%~dp0Assets\" (
        echo [OK] Assets folder exists
    ) else (
        echo [!!] Assets folder missing
    )
    
    if exist "%~dp0ProjectSettings\" (
        echo [OK] ProjectSettings folder exists
    ) else (
        echo [!!] ProjectSettings folder missing
    )
    
    if exist "%~dp0Packages\" (
        echo [OK] Packages folder exists
    ) else (
        echo [!!] Packages folder missing
    )
    
    echo.
    echo For detailed diagnostics, run: View-Log.ps1
)

echo.
echo ========================================
echo   Diagnostics Complete
echo ========================================
echo.
pause
