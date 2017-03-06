@echo off

echo Deleting current files.
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
cd "..\..\..\.."

echo Copying frontend files to server.

xcopy "RTF2L-Frontend" "RTF2L-Backend\RandomTF2Loadout\bin\%1" /O /X /E /H /K