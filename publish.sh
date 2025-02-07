#!/bin/bash
rm -f MouthKing.UI.Deskto/packages.lock.json
dotnet publish -r linux-x64 -c Release /p:RestoreLockedMode=true /p:Version=$1
cd MouthKing.UI.Deskto/bin/Release/net9.0/linux-x64/publish 
cp MouthKing.UI.Deskto MouthKing.UI.Deskto.bin 
