@echo off
cls

.paket\paket.exe install
if errorlevel 1 (
  exit /b %errorlevel%
)

"packages\build\FAKE\tools\Fake.exe" .\Builder\build.fsx %1
