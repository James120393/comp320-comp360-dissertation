# comp360 Dissertation Artefact Readme

## The code can be found in the following folder:
comp360_01_1506530\scbot.git\POSH-StarCraftBot\behaviours

## To run the Bot simply run the "run_proxy.bat" file located at:
comp360_01_1506530\scbot.git\POSH-Launcher\bin\Release
### Note: You will Need to install StarCraft: BroodWar with patch 1.16.1 
### plus ChaosLauncher which can be found [here](http://www.teamliquid.net/forum/brood-war/65196-chaoslauncher-for-1161)
### Note: The Bot must be run in admin mode otherwise it will not connect to the game.
The batch file should ask for admin. if it does not then it will not work, to circumven this you must run the POSH-Launcher.exe
in the command line while running in admin, with the following argument after "-a=POSH-SCBot.dll POSH-SCBot".


## To run the code "From Source" please follow this [link.](https://github.com/suegy/bwapi-mono-bridge2/wiki/StarCraft-Setup-BWAPI)

Once there follow the steps until you have closed ChaosLauncher, then follow this [link](https://github.com/suegy/bwapi-mono-bridge2/wiki/MonoBridge-Setup) and follow the instructions.

Do not complete part b of this page, instead follow this [link](https://github.com/suegy/bwapi-mono-bridge2/wiki/CsharpAI).

The once there follow the instructions until it asked you to create a "scbot.git" folder as you will be using the "scbot.git" folder located within this project instead.

Located within this folder should be two other folders called:
#### POSH-Launcher
#### POSH-StarCraftBot

Open the bwapi-clr-client Solution in \bwapi-mono-bridge2.git\Source\bwapi-clr-client

NOTE: currently all bots in Client Mode require Visual Studio to be run as Administrator. 

So, if you want to test your agent you need to start VS with admin rights. 

Otherwise your client is not able to inject the information into the game.

If you do not want to, or forgot to setup the POSHBots you will get an error message that two projects could > not be found: POSH-Launcher and POSH-StarCraftBot

If the POSH projects cannot be found within Visual Studio remove both projects but then add the two with the correct path by right clicking on the Solution in 

Visual Studio -> "Add..." -> "Existing Project" and then select the directory where you put each project.

now simply start the ChaosLauncher in admin mode and select the "Release" injector.

compile first bwapii-native then bwapi-clr and then tesbot right click on POSH-Launcher and select "Debug" and run in debug mode.



## A video of the Bot working can also be found in:
comp360_01_1506530\Video

POSH-botVSInBuiltTerran.mp4 was recorded in debug mode.

POSH-botVSCimex(Zerg).mp4 is a replay from the testing and was only available to record in StarCraft Remastered.

## The trello board can be found in the Images folder:
comp360_01_1506530\Images

## The link to the GitHub Repository:
[link]("https://github.com/James120393/Dissertation_Artefact")

## To open a plan please open the ABODEstar-004.jar located at:
"comp320_01_1506530\scbot.git\POSH-StarCraftBot\ABODEstar-004.jar"

## Then select open and navigate to:
"comp320_01_1506530\scbot.git\POSH-StarCraftBot\library\plans"
and select any of the plans within this folder.

## An image depicting the testing process can be found in the Images folder:
comp360_01_1506530\Images

This screenshot is showing the process taken for testing the software. At every implementation or change to a variable the Bot would be compiled and run,
this would open the command window as seen in the bottom right. This would then run in-game and the function would then be observed, for a pass/fail as to whether
it succeeded in its function. If it failed a breakpoint would be placed at the appropriate location in the code and the function would then be debugged.
This was the process taken for each test case, unfortunately, there are not any accompanying documents containing unit test tables.