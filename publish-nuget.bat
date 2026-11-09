@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title monorepo-dotnet — publishing
color 0B

echo.
echo   ============================================
echo     monorepo-dotnet  -  nuget.org
echo   ============================================
echo.
echo   Starting...

set "BUMP=%~1"
if "%BUMP%"=="" set "BUMP=patch"

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\publish-nuget.ps1" -Bump "%BUMP%"
set "EXITCODE=%ERRORLEVEL%"

echo.
if not "%EXITCODE%"=="0" (
  echo Publish failed.
  pause
  exit /b %EXITCODE%
)

echo Done.
pause
exit /b 0
