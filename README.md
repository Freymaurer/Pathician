# PathfinderAttackSimulator

Hello, this attack calculator/simulator is currently under construction.
For the time being it can already be used to calculate both standard attacks and full round attack actions.
Because this is meant to be very variable it is also not completly hand-held when using it.
To calculate these attack routines the function needs information about the character, the used weapon/weapons and the used modifications, 
which can be spells, feats, class abilitys, debuffs and so on.

- [How To Create Character Information](#how-to-create-character-information)
- [How To Create Weapon Information](#how-to-create-weapon-information)
- [How To Create Modification Information](#how-to-create-modification-information)
- [Standard Attack Action](#standard-attack-action)
- [Full Round Attack Action](#full-round-attack-action)
- [How To Install PathfinderAttackSimulator](#installation)

## How To Create Character Information

Example for **Character Information** 
For a detailed explanation to this topic please see the related documentation [Create Character](https://freymaurer.github.io/PathfinderAttackSimulator/CreateCharacter.html).

```fsharp
open PathfinderLib.Library

///example on character information
let myCharacter =  {
        CharacterName = "Best Character Name Ever"
        BAB = 6
        Strength = 7
        Dexterity = 3
        Constitution = 2
        Intelligence = -3
        Wisdom = 0
        Charisma = -3
        CasterLevel1 = 0
        CasterLevel2 = 0
    }
```

## How To Create Weapon Information

Example for **Weapon Information**
For a detailed explanation to this topic please see the related documentation [Weapon Creation](https://freymaurer.github.io/PathfinderAttackSimulator/CreateWeapon.html)

```fsharp
open PathfinderLib.Library
///example of weapon information
let keenflaming2greatsword = {
        Name                = "+2 keen flaming greatsword"
        Damage              = createDamage 2 6 Slashing
        DamageBonus         = 2
        ExtraDamage         = createDamage 1 6 Fire
        BonusAttackRolls    = 2
        CriticalRange       = [|17 .. 20|] 
        CriticalModifier    = 2
        Modifier            = createUsedModifier Strength Strength TwoHanded 1.5
        ManufacturedOrNatural = Manufactured
        }
```

## How To Create Modification Information

Example for **Modification information**
For a detailed explanation to this topic please see the related documentation [Modification Creation](https://freymaurer.github.io/PathfinderAttackSimulator/CreateModification.html)
```fsharp
open PathfinderLib.Library

///Example modification information
let Haste = {
    Name = "Haste"
    BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
    BonusAttackRoll = createBonus 1 Flat
    BonusDamage = createBonus 0 Flat
    ExtraDamage = createDamage 0 0 Untyped
    AppliedTo = [|All|], -20
    StatChanges = [||]
    SizeChanges = createSizechange 0 Flat false
    Description = "Range: Close (25 ft. + 5 ft./2 levels). One creature/level, no two of which can be more than 30 ft. apart."
    }
```
## Standard Attack Action

This function returns the output of a standard attack action for one weapon.
> The following character is a 8th level rogue like character with 22 strength and a mutagen to buff this even more. You might remember that orc in the assasins guild in oblivion. Well more or less like that.
```fsharp
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.StandardAttackAction

myStandardAttack myRogue Medium greatswordRogue [|Flanking; SneakAttack 8; PowerAttack myRogue.BAB; EnlargePerson; MutagenStrength; FuriousFocus myRogue.BAB|]

Output: > You hit the enemy with a 20 (rolled 3) for 31 Slashing damage +16 Precision Schaden !
```
So the function needs a character, the size the creature has without any buffs (e.g. enlarge person), a weapon (the character wields at that size) and an array ( **[| |]** ) of modifications. At this point i already made some modifications with automatic further calculations. See for example "SneakAttack _rogue level_" or "PowerAttack _character BAB_".
If you only use weapons and characters not found in the library then you don't need to open those modules.

## Full Round Attack Action

This function returns the output of a full round attack action routine with multiple weapons. It can apply bonus attacks to a specific weapon and also have modifications only apply to one weapon type. This will be further enhanced in the future.
> let's see what our rogue can do in a full round attack action. Only that he somehow got some Haste buffed and now also has an additional bite attack... and somehow dual wields two of his greatswords ... .
```fsharp
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.FullRoundAttackAction

myFullAttack myRogue Medium [|greatswordRogue,PrimaryMain; greatswordRogue,Primary; Weapons.bite,Secondary|] [|Flanking; SneakAttack 8; MutagenStrength; Haste; TwoWeaponFighting|]

> You attack with a Large +1 Keen Greatsword and (hopefully) critically hit the enemy with a 36 (rolled 20) and confirm your crit with a 26 (rolled 10) for 22 Slashing damage +15 Precision Schaden (crit * 2)!
> You attack with a Large +1 Keen Greatsword and (hopefully) critically hit the enemy with a 34 (rolled 18) and confirm your crit with a 26 (rolled 10) for 24 Slashing damage +16 Precision Schaden (crit * 2)!
> You attack with a Large +1 Keen Greatsword and hit the enemy with a 18 (rolled 7) for 16 Slashing damage +19 Precision Schaden !
> You attack with a Large +1 Keen Greatsword and hit the enemy with a 22 (rolled 6) for 17 Slashing damage +13 Precision Schaden !
> You attack with a Bite and hit the enemy with a 31 (rolled 19) for 5 BludgeoningOrPiercingOrSlashing damage +11 Precision Schaden !
```
> Oh nice! Some crits! As you can see a crit is not automatically calculated, to make it easier in case you could'nt confirm it.

The function is made in a way to provide maximum flexibility, thats why you need to add the information, as what type of weapon a weapon should be used.
Here are some examples:
- Haste/FlurryOfBlows adds an additional attack to the _PrimaryMain_ weapon.
- Two-Weapon-Fighting has a _PrimaryMain_ weapon for e.g. Haste and the other Weapon needs to be classified as a _Primary_ weapon.
- Secondary natural attacks are always classified as _Secondary_.
- Primary natural attacks without any manufactured weapons are the only case when there is __no__ _PrimaryMain_ weapon and all primary natural attacks are classified as _Primary_.

## Installation

It is possible, that this won't work for Windows 10 Enterprise

1. [Download VisualStudioCode](https://code.visualstudio.com/download)
2. [Download Git](https://git-scm.com/download/win)
3. [Download .NET Core SDK AND .NET Framework Dev Pack](https://dotnet.microsoft.com/download)
4. Restart your computer
5. [Download Fake cli](https://fake.build/fake-gettingstarted.html). tl;dr: open console and type in "dotnet tool install fake-cli -g"
6. [Download Master branch Zip of this repository](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/developer.zip) & unzip. Importan to not safe it on the Desktop!
7. open console and navigate to the Folder _(Copy path to this folder)_ with the build.cmd inside. 
		_(console command: cd __PathToYourFolder__)_
8. console command: fake build
9. install Ionide in visual studio code:
	(_open visual studio code -> Extensions -> type in Ionide-fsharp -> install_)
10. restart and open new .fsx file (File->New File -> Save As -> Name.fsx)
11. reference the PathfinderAttackSimulator.dll (#r @"HERECOMESYOURPATH\PathfinderAttackSimulator\src\PathfinderAttackSimulator\bin\Release\netstandard2.0\PathfinderAttackSimulator.dll")
