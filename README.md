# FeexRanks
Rank system to your server, creates a new table rank to store all players points, you can receive points by killing players/zombies/mega zombies, totally configurable,
also you can receive Uconomy balance when ranking up and group permissions rewards.

Requirements:
- Mysql
- Uconomy (Optional)

### Configuration
- PointsLoseWhenDie: amount of points the player will lose when die, losing points will not reduce points lower than rank
(reducing ranks will cause problems to player receiving the same reward as before)
- RankGlobalNotify: Globably notify player rank when any player rank up
- RankLocalNotify: Notify only the player when he rank up
- RankLoginGlobalNotify: Globably notify the player rank when he logging into server
- RankLoginLocalNotify: Notify the player rank when he join in the server
- RankLogoutGlobalNotify: Globably notify the player rank when he disconnect the server
- Kill...Points: Quantity of points a player will earn for killing
- PointsLoseWhenDie: Points to lose when the player dies
- PointsEarnPerTime: Points to earn every TickratePointsEarnPerTime
- TickratePointsEarnPerTime: Every amount of ticks in the server to earn points, calculation: Seconds * ServerTickrate, you can view tickrate in ``Rocket.config``

# Building

*Windows*: The project uses dotnet 4.8, consider installing into your machine, you need visual studio, simple open the solution file open the Build section and hit the build button (ctrl + shift + b) or you can do into powershell the command dotnet build -c Debug if you have installed dotnet 4.8.

*Linux*: Install dotnet-sdk from your distro package manager, open the root folder of this project and type ``dotnet build -c Debug``.

FTM License.