@echo off
cd "..\..\..\.."

echo Copying configuration file to %1.
copy "config.cfg" "RTF2L-Backend\RandomTF2Loadout\bin\%1\config.cfg"