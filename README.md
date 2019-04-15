# PathfinderAttackSimulator

Hello, this attack calculator/simulator is currently under construction.
For the time being it can already be used to calculate both standard attacks and full round attack actions.
Because this is meant to be very variable it is also not completly hand-held when using it.
To calculate these attack routines the function needs information about the character, the used weapon/weapons and the used modifications, 
which can be spells, feats, class abilitys, debuffs and so on.

- [Features](#features)
- [How To Contribute](#how-to-contribute)
- [How To Install PathfinderAttackSimulator](#installation)

## Features

- Standard attack action and full-round attack action calculator.
- Calculator for d20pfsrd bestiary entries.

For in depth information on how to use these features please see [here](https://freymaurer.github.io/PathfinderAttackSimulator/)

## How To Contribute

If you want to participate in this project, either by reporting a found bug/error or if you want to request a feature please open a issue [here](https://github.com/Freymaurer/PathfinderAttackSimulator/issues).
If you are not sure about opening an issue you can also E-Mail me directly (Freymaurer@gmx.de).

## Installation

It is possible, that this won't work for Windows 10 Enterprise

1. [Click here](https://code.visualstudio.com/download) to download VisualStudioCode
2. [Click here](https://git-scm.com/download/win) to download Git
3. [Click here](https://dotnet.microsoft.com/download) to download .NET Core SDK AND .NET Framework Dev Pack
4. [Click here](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2017) to download Build Tools for Visual Studio 2019 //HERE STILL MISSING INDIVIDUAL COMPONENTS
5. Restart your computer
6. Install fake cli. Open [command prompt](https://en.wikipedia.org/wiki/Command-line_interface) by searching in the windows search bar for "cmd" and type in the new window "dotnet tool install fake-cli -g" (without the quotation marks)
7. [Click here](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/developer.zip) or scroll up to download either master or developer branch of this repository.
	Master branch should be a fully functionable variant, while the developer branch often has more features which are not fully tested yet.
	At this point i recommend downloading the developer branch, as it will be updated the most.
	Unzip the file ina any folder, except the Desktop!
8. open command prompt and navigate to the Folder _(Copy path to this folder)_ with the build.cmd inside. 
		_(console command: cd __PathToYourFolder__)_
9. console command: fake build
10. install Ionide in visual studio code:
	(_open visual studio code -> Extensions -> type in Ionide-fsharp -> install_)
11. restart and open new .fsx file (File->New File -> Save As -> Name.fsx)
12. reference the PathfinderAttackSimulator.dll (#r @"HERECOMESYOURPATH\PathfinderAttackSimulator\src\PathfinderAttackSimulator\bin\Release\netstandard2.0\PathfinderAttackSimulator.dll")
