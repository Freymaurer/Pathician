# PathfinderAttackSimulator

Hello, this attack calculator/simulator is currently under construction.
For the time being it can already be used to calculate both standard attacks and full round attack actions.
Because this is meant to be very variable it is also not completly hand-held when using it.
To calculate these attack routines the function needs information about the character, the used weapon/weapons and the used modifications, 
which can be spells, feats, class abilitys, debuffs and so on.

	* [How To Create Character Information](#How-To-Create-Character-Information)
	* [How To Create Weapon Information](#How-To-Create-Weapon-Information)
	* [How To Create Modification Information](#How-To-Create-Modification-Information)
	* [How To Install PathfinderAttackSimulator](#Installation)

## How To Create Character Information

Example for **Character Information** 

```fsharp
open PathfinderLib.Library
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

```fsharp
open PathfinderLib.Library

///example on character information
let myCharacter =
    {
	///the name of the character (the createStringForLib function makes it all upper case, 
	///only used for future web application)
        CharacterName = createStringForLib "characterName"
	///Base attack bonus
        BAB = 6
	///the following terms will determine the corresponding ability score **modifier**(!)
        Strength = 6
        Dexterity = 0
        Constitution = 0
        Intelligence = 0
        Wisdom = 0
        Charisma = 0
	///this can be used to calculate spellstrike's spelldamage for e.g. magus
		CasterLevel1 = 0
        CasterLevel2 = 0
    }
```

## How To Create Weapon Information

Example for **Weapon Information**

```fsharp
open PathfinderLib.Library

///example of weapon information
let greatswordKeenHuge =
    {
		///just the name/description
        Name                = "Huge +1 Keen Flaming Greatsword"
		///creates dice damage, here: 4d6 slashing
        Damage              = createDamage 4 6 Slashing
		///+1 flat damage bonus from enchantment
        DamageBonus         = 1
		///creates extra dice damage, like from the flaming ability, 			here: 1d6 Fire Damage. 
		///This sort of damage will be mentioned separatly in the output
        ExtraDamage         = createDamage 1 6 Feuer
		///+1 flat bonus to hot attacks from enchantment
        BonusAttackRolls    = 1
		///critical range; can also be written as [|17..20]
        CriticalRange       = [|17;18;19;20|]
		///modifier for crit which will be part of the output. Critical damage will not get calculated automatically yet.
        CriticalModifier    = 2
		///determines the ability score to hit, to damage and the multiplier to damage (in this order)
		///the multiplier should always only either be 0.5, 1. or 1.5 for a two-handed weapon.
        Modifier            = createUsedModifier "Str" "Str" 1.5
		///this is necessary to differenciate between natural and manufactured weapons (either "manufactured" or "natural")
        ManufacturedOrNatural = createStringForLib "Manufactured"
    }
```

## How To Create Modification Information

Example for **Modification information**

```fsharp
open PathfinderLib.Library

///Example modification information
let Haste =
    {
        Name = "Haste"
		///How many bonus attacks, the type of bonus attack and what weapon should be used for it.
        BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
		///bonus to attack roll to hit and the bonus type
        BonusAttackRoll = createBonus 1 Flat
		///bonus to attack damage and the bonus type
        BonusDamage = createBonus 0 Flat
		///extra damage as described under weapon (e.g. Elemental Fist)
        ExtraDamage = createDamage 0 0 Untyped
		///To what weapon type this bonus modifier applies (either ALL, PrimaryMain, 
		///Primary, Secondary) and to how many of these attacks.
		///type -20 if to all attacks or e.g. 1 for a sneak attack out of invisibility
        AppliedTo = [|All|], -20
		///creates ability score changes; e.g. alchemist mutagen: 
		///"(createStatChange "Str" 2 Alchemical); (createStatChange "Int" -2 Alchemical)"
        StatChanges = [|createStatChange "0" 0 Flat|]
		///this will be used in a future web application to show a descriptive tooltip
        Description = ""
    }
```
## Standard Attack Action

This function returns the output of a standard attack action for one weapon.
> The following character is a 8th level rouge like character with 22 strength and a mutagen to buff this even more. You might remember that orc in the assasins guild in oblivion. Well more or less like that.
```fsharp
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.StandardAttackAction

myStandardAttack myRogue greatswordRogue [|SneakAttack 8; PowerAttack myRouge.BAB; EnlargePerson; MutagenStrength; Heroism|]
Output: > Du triffst den Gegner mit 23 (gewuerfelt 8) fuer 33 Slashing Schaden +14 Precision Schaden !
```
So the function needs a character, a weapon and an array ( **[| |]** ) of modifications. At this point i already made some modifications with automatic further calculations. See for example "SneakAttack _rogue level_" or "PowerAttack _character BAB_".
If you only use weapons and characters not found in the library then you don'
t need to open those modules.

## Full Round Attack Action

This function returns the output of a full round attack action routine with multiple weapons. It can apply bonus attacks to a specific weapon and also have modifications only apply to one weapon type. This will be further enhance in the future.
> let's see what our rogue can do in a full round attack action. Only that he somehow got some Haste buffed and now also has an additional bite attack
```fsharp
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.StandardAttackAction

myFullAttack myRogue [|greatswordRogue,PrimaryMain; Weapons.bite,Secondary|] [|Flanking;SneakAttack 8; MutagenStrength; Haste|]
> Du greifst mit Large +1 Keen Greatsword an und crittest (hoffentlich) den Gegner mit 37 (gewuerfelt 19) und bestaetigst mit 26 (gewuerfelt 8) fuer 27 Slashing Schaden +18 Precision Schaden (crit * 2)!
> Du greifst mit Large +1 Keen Greatsword an und triffst den Gegner mit 22 (gewuerfelt 4) fuer 22 Slashing Schaden +14 Precision Schaden !
> Du greifst mit Large +1 Keen Greatsword an und triffst den Gegner mit 25 (gewuerfelt 7) fuer 24 Slashing Schaden +20 Precision Schaden !
> Du greifst mit Bite an und triffst den Gegner mit 26 (gewuerfelt 9) fuer 8 BludgeoningOrPiercingOrSlashing Schaden +9 Precision Schaden !
```
> Oh nice a crit! As you can see a crit is not automatically calculated, to make it easier in case you could'nt confirm it.

The function is made in a way to provide maximum flexibility, thats why you need to add the information, as what type of weapon a weapon should be used.
Here are some examples:
- Haste/FlurryOfBlows adds an additional attack to the _PrimaryMain_ weapon.
- Two-Weapon-Fighting has a _PrimaryMain_ weapon for e.g. Haste and the other Weapon needs to be classified as a _Primary_ weapon.
- Secondary natural attacks are always classified as _Secondary_.
- Primary natural attacks without any manufactured weapons are the only case when there is __no__ _PrimaryMain_ weapon and all primary natural attacks are classified as _Primary_.

## Installation

1.[Download VisualStudioCode](https://code.visualstudio.com/download)
2.[Download Git](https://git-scm.com/download/win)
3.[Download .NET core AND .NET Framework dev packs](https://dotnet.microsoft.com/download)
4.[Download Fake cli](https://fake.build/fake-gettingstarted.html). tl;dr: open console and type in "dotnet tool install fake-cli -g"
5. [Download Master branch Zip of this repository](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/developer.zip) & unzip
6. open console and navigate ot the Folder withe the build.cmd inside. 
		(_console command: cd folderPath_)
7. console command: fake build
8. install Ionide in visual studio code:
	(_open visual studio code -> Extensions -> type in Ionide-fsharp -> install_)
9. restart and open new .fsx file (File->New File -> Save As -> Name.fsx)
10. reference the PathfinderAttackSimulator.dll (#r @"HERECOMESYOURPATH\PathfinderAttackSimulator\src\PathfinderAttackSimulator\bin\Release\netstandard2.0\PathfinderAttackSimulator.dll")
///example on character information
let myCharacter =
    {
	///the name of the character (the createStringForLib function makes it all upper case, 
	///only used for future web application)
        CharacterName = createStringForLib "characterName"
	///Base attack bonus
        BAB = 6
	///the following terms will determine the corresponding ability score **modifier**(!)
        Strength = 6
        Dexterity = 0
        Constitution = 0
        Intelligence = 0
        Wisdom = 0
        Charisma = 0
	///this can be used to calculate spellstrike's spelldamage for e.g. magus
		CasterLevel1 = 0
        CasterLevel2 = 0
    }
```

## How To Create Weapon Information

Example for **Weapon Information**

```fsharp
open PathfinderLib.Library

///example of weapon information
let greatswordKeenHuge =
    {
	///just the name/description
        Name                = "Huge +1 Keen Flaming Greatsword"
	///creates dice damage, here: 4d6 slashing
        Damage              = createDamage 4 6 Slashing
	///+1 flat damage bonus from enchantment
        DamageBonus         = 1
	///creates extra dice damage, like from the flaming ability, here: 1d6 Fire Damage. 
	///This sort of damage will be mentioned separatly in the output
        ExtraDamage         = createDamage 1 6 Feuer
	///+1 flat bonus to hot attacks from enchantment
        BonusAttackRolls    = 1
	///critical range; can also be written as [|17..20]
        CriticalRange       = [|17;18;19;20|]
	///modifier for crit which will be part of the output. Critical damage will not get calculated automatically yet.
        CriticalModifier    = 2
	///determines the ability score to hit, to damage and the multiplier to damage (in this order)
	///the multiplier should always only either be 0.5, 1. or 1.5 for a two-handed weapon.
        Modifier            = createUsedModifier "Str" "Str" 1.5
	///this is necessary to differenciate between natural and manufactured weapons (either "manufactured" or "natural")
        ManufacturedOrNatural = createStringForLib "Manufactured"
    }
```

## How To Create Modification Information

Example for **Modification information**

```fsharp
open PathfinderLib.Library

///Example modification information
let Haste =
    {
        Name = "Haste"
	///How many bonus attacks, the type of bonus attack and what weapon should be used for it.
        BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
	///bonus to attack roll to hit and the bonus type
        BonusAttackRoll = createBonus 1 Flat
	///bonus to attack damage and the bonus type
        BonusDamage = createBonus 0 Flat
	///extra damage as described under weapon (e.g. Elemental Fist)
        ExtraDamage = createDamage 0 0 Untyped
	///To what weapon type this bonus modifier applies (either ALL, PrimaryMain, 
	///Primary, Secondary) and to how many of these attacks.
	///type -20 if to all attacks or e.g. 1 for a sneak attack out of invisibility
        AppliedTo = [|All|], -20
	///creates ability score changes; e.g. alchemist mutagen: 
	///"(createStatChange "Str" 2 Alchemical); (createStatChange "Int" -2 Alchemical)"
        StatChanges = [|createStatChange "0" 0 Flat|]
	///this will be used in a future web application to show a descriptive tooltip
        Description = ""
    }
```

## Installation

	1. [Download VisualStudioCode](https://code.visualstudio.com/download)
	2. [Download Git](https://git-scm.com/download/win)
	3. [Download .NET core AND .NET Framework dev packs](https://dotnet.microsoft.com/download)
	4. [Download Fake cli](https://fake.build/fake-gettingstarted.html)
		* tl;dr: open console and type in "dotnet tool install fake-cli -g"
	5. [Download Master branch Zip of this repository](https://github.com/Freymaurer/PathfinderAttackSimulator/archive/developer.zip) & unzip
	6. open console and navigate ot the Folder withe the build.cmd inside 
		* console command: cd folderPath
	7. console command: fake build
	8. install Ionide in visual studio code:
		* open visual studio code -> Extensions -> type in Ionide-fsharp -> install
	9. restart and open new .fsx file (File->New File -> Save As -> Name.fsx)
	10. reference the PathfinderAttackSimulator.dll (#r @"HERECOMESYOURPATH\PathfinderAttackSimulator\src\PathfinderAttackSimulator\bin\Release\netstandard2.0\PathfinderAttackSimulator.dll")