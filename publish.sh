#!/bin/bash
rm -f MouthKing.UI.Desktop/packages.lock.json
dotnet publish -r linux-x64 -c Release -f net9.0 /p:RestoreLockedMode=true /p:Version=$1
cd MouthKing.UI.Desktop/bin/Release/net9.0/linux-x64/publish 
cp MouthKing.UI.Desktop MouthKing.UI.Desktop.bin 
