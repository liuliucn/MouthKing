set version=%1
del MouthKing.UI.Desktop\packages.lock.json
dotnet publish -r win-x64 -c Release -f net9.0 /p:RestoreLockedMode=true /p:Version=%version%