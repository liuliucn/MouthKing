set version=%1
del MouthKing.UI.Deskto\packages.lock.json
dotnet publish -r win-x64 -c Release /p:RestoreLockedMode=true /p:Version=%version%