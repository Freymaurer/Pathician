# PathfinderAttackSimulator

Hello, this attack calculator/simulator is currently under construction.
For the time being it can already be used to calculate both standard attacks and full round attack actions.
Because this is meant to be very variable it is also not completly hand-held when using it.
To calculate these attack routines the function needs information about the character, the used weapon/weapons and the used modifications, 
which can be spells, feats, class abilitys, debuffs and so on.

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