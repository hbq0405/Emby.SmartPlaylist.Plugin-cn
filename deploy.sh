#!/bin/bash

./build.sh
sudo service emby-server stop
sudo cp -v backend/SmartPlaylist/bin/Release/netstandard2.0/SmartPlaylist.dll /var/lib/emby/plugins/SmartPlaylist.dll
sudo service emby-server start