@echo off
chcp 65001 >nul
REM ============================================================
REM  Rebuilds the game from source and starts the FRESH build.
REM  Needed because after editing the code the old .exe in
REM  bin\Debug keeps running, so it looks like nothing changed.
REM
REM  In Visual Studio the same thing happens with F5
REM  (Build first, then Run).
REM ============================================================

set "PROJECT=%~dp0SuperMarioBros.csproj"
set "EXE=%~dp0bin\Debug\SuperMarioBros.exe"

REM --- find MSBuild ---
set "MSBUILD="
if exist "%ProgramFiles%\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\amd64\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\amd64\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles% (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=%ProgramFiles% (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
if not defined MSBUILD for /f "delims=" %%M in ('where msbuild 2^>nul') do if not defined MSBUILD set "MSBUILD=%%M"
if not defined MSBUILD (
  echo [ERROR] MSBuild not found. Open the project in Visual Studio and press F5.
  pause
  exit /b 1
)

REM --- remove the web safety mark, it breaks the build with MSB3821 ---
powershell -NoProfile -Command "Get-ChildItem -LiteralPath '%~dp0' -Recurse -File -Force -ErrorAction SilentlyContinue ^| Unblock-File" >nul 2>&1

REM --- close a running game, otherwise the .exe cannot be overwritten ---
taskkill /IM SuperMarioBros.exe /F >nul 2>&1

echo === Build ===
"%MSBUILD%" "%PROJECT%" /t:Build /p:Configuration=Debug /v:minimal /nologo
if errorlevel 1 (
  echo.
  echo [ERROR] Build failed, see the errors above.
  pause
  exit /b 1
)

echo.
echo === Run ===
start "" "%EXE%"
exit /b 0
