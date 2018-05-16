@echo off
cls

.paket\paket.exe install
if errorlevel 1 (
  exit /b %errorlevel%
)

"packages\build\FAKE\tools\Fake.exe" .\build.fsx %1
