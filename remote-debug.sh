#!/bin/bash
echo "====== Building ======"
./build.sh
echo "====== Stopping Service ======"
#sudo service emby-server stop
#rm -f backend/SmartPlaylist/bin/Release/netstandard2.0/publish/SmartPlaylist.dll
#rm -f backend/SmartPlaylist/bin/Release/netstandard2.0/publish/SmartPlaylist.pdb

cd backend
dotnet publish --configuration debug
cd ..

echo "====== Copying ======"
rm -f /var/lib/emby/plugins/SmartPlaylist.dll
rm -f /var/lib/emby/plugins/SmartPlaylist.pdb
cp -v backend/SmartPlaylist/bin/Debug/netstandard2.0/SmartPlaylist.dll /var/lib/emby/plugins/SmartPlaylist.dll
cp -v backend/SmartPlaylist/bin/Debug/netstandard2.0/SmartPlaylist.pdb /var/lib/emby/plugins/SmartPlaylist.pdb
#sudo chmod 777 /var/lib/emby/plugins/SmartPlaylist.*
echo "====== !!DONE!! ======"