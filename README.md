# PathfinderAttackSimulator

A pathfinder toolbox written in F#, designed to aid pathfinder players and gamemasters in managing multiple and/or complex characters.
Below you can find a list of the currently supported features. 

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

Below you can find two installation types. For most users the Quick Start variant will easily be enough to use.

### Quick Start

1. [Click here](https://code.visualstudio.com/download) to download VisualStudioCode
2. Install Ionide in visual studio code:
	(_open visual studio code -> Extensions -> type in Ionide-fsharp -> install_)
3. Download the Quickstart zip file [here](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/QuickStart.zip)
4. Unzip it with WinRAR or 7zip
5. You can now go the PathfinderAttackSimulator_qs folder and open the PathfinderCalculator.fsx and start using it.

### Full Installation

It is possible, that this won't work for Windows 10 Enterprise

1. [Click here](https://code.visualstudio.com/download) to download VisualStudioCode
2. [Click here](https://git-scm.com/download/win) to download Git
3. [Click here](https://dotnet.microsoft.com/download) to download .NET Core SDK AND .NET Framework Dev Pack
4. [Click here](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2017) to download Build Tools for Visual Studio 2019 
	While installing check the boxes for:
	Under "Workload":
	- .NET Core build tools
	Under "Individual components":
	- NuGet targets and build tasks
	- NuGet package manager
	- .NET Framework 4.7 SDK
	- .NET Framework 4.7 targeting pack
	- F# compiler
	...then install
5. Restart your computer
6. Install fake cli. Open [command prompt](https://en.wikipedia.org/wiki/Command-line_interface)(console) by searching in the windows search bar for "cmd" and type in the new window "dotnet tool install fake-cli -g" (without the quotation marks)
7. [Click here](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/developer.zip) or scroll up to download either master or developer branch of this repository.
	Master branch should be a fully functionable variant, while the developer branch often has more features which are not fully tested yet.
	At this point i recommend downloading the developer branch, as it will be updated the most.
	Unzip the file in any folder, except the Desktop!
8. Open command prompt(console) and navigate to the Folder _(Copy path to this folder)_ with the build.cmd inside. 
		_(console command: cd __PathToYourFolder__)_
9. Console command: fake build
10. Install Ionide in visual studio code:
	(_open visual studio code -> Extensions -> type in Ionide-fsharp -> install_)
11. Thats finally it, you can now go the PathfinderAttackSimulator folder and open the PathfinderCalculator.fsx and start using it.

