@echo off
echo Copying frontend files to server.

xcopy "RTF2L-Frontend" "RTF2L-Backend\RandomTF2Loadout\bin\Release" /O /X /E /H /K

echo Starting Server
start RTF2L-Backend\RandomTF2Loadout\bin\Release\RandomTF2Loadout.exe