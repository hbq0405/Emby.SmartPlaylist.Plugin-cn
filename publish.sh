#!/bin/bash

cd frontend
yarn install
yarn build
cd ../backend
rm -rv SmartPlaylist/bin/Release
dotnet publish --configuration Release
cd SmartPlaylist/bin/Release/netstandard2.0/publish
zip SmartPlaylist-2.0.0.1.zip SmartPlaylist.dll
ls -lh .