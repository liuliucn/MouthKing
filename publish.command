﻿#!/bin/bash 
dir=${0%/*} 
if [ -d "$dir" ]; then 
  cd "$dir" 
fi 
rm -f MouthKing.UI.Desktop/packages.lock.json 
dotnet publish -r osx-x64 -c Release -f net9.0 /p:RestoreLockedMode=true /p:Version=$1 -t:BundleApp
rm -rf MouthKing.UI.Desktop/bin/Release/net9.0/osx-x64/publish/Assets/
rm -rf MouthKing.UI.Desktop/bin/Release/net9.0/osx-x64/publish/MouthKing.UI.Desktop.app/Contents/MacOS/Assets/