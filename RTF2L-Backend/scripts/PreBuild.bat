@echo off

cd "..\..\..\.."

echo Copying frontend files to server.
copy "config.cfg" "RTF2L-Backend\RandomTF2Loadout\bin\%1\config.cfg"