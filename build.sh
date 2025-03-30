#!/bin/sh
game_dir=".../SteamLibrary/steamapps/common/U3DS/Servers/Test/Rocket/Plugins/"
library_dir=".../SteamLibrary/steamapps/common/U3DS/Servers/Test/Rocket/Libraries/"

dotnet build -c Release
cp -r ./bin/Release/net48/FeexRanks.dll "$game_dir"
cp -r ./Libs/MySql.Data.dll "$library_dir"
cp -r ./Libs/Ubiety.Dns.Core.dll "$library_dir"