(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
Creating a Modification
======================

Example on how a Modifcation should be created.
-------

A modification needs certain informations for a correct calculation. 
Right now this is not hand-held at all, but might get better in a future version.

This is by far the most complex variant of information-type.
Therefore, i create a library of pre-written modifications.

Let's see how this works for the famous Haste spell

### Haste
*)

#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.Library

let Haste =
    {
        Name = "Haste"
        BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
        BonusAttackRoll = createBonus 1 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|createStatChange "0" 0 Flat|]
        Description = "Range: Close (25 ft. + 5 ft./2 levels). One creature/level, no two of which can be more than 30 ft. apart."
    }

(**
So a short introduction to the spell for all who might not know it (so like complete beginners, i guess?).
> Haste gives an extra attack, that does not stack with weapon enhancements like Speed and so on, and also an 
> flat bonus to all attack rolls, and also some non - attack related boni.

So how does this work:
* Name = the actual correct name of the weapon, currently not relevant for the code
* BonusAttacks = the number of bonus attacks granted by the modification and which WeaponType should be used for the extra attack
> Imagine a dual-wielding fighter with a silver,PrimaryMain and an cold iron sword,Primary fighting against a fae creature.
* BonusDamage = bonus damage that is added to the weapon damage and the bonus type (think: Prayer or InspireCourage)
* ExtraDamage = Damage that will be calculated and displayed separately, e.g. Sneak Attack from rogue.
* AppliedTo = apply this modification to the WeaponTypes specified here and how often. "-20" is currently used as a placeholder for "to all attacks". Here you could use 1 for Spellstrike-ShockingGrasp.
* StatChanges = e.g. the alchemist's mutagen (createStatChange "Str" 2 Alchemical) the number again, represents the modifier increase.
* Description = A short Description for additional information. Not used in the script right now.

Some further Examples:

### Alchemist's Strength Mutagen
*)
let MutagenStrength =
    {
        Name = "Strength Mutagen"
        BonusAttacks = createBonusAttacks 0 Flat All
        BonusAttackRoll = createBonus 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|(createStatChange "Str" 2 Alchemical); (createStatChange "Int" -2 Alchemical)|]
        Description = ""
    }

(**
### Enlarge Person
*)
let EnlargePerson =
    {
        Name = "Enlarge Person"
        BonusAttacks = createBonusAttacks 0 Flat All
        BonusAttackRoll = createBonus -1 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|(createStatChange "Str" 1 Size);(createStatChange "Dex" -1 Size)|]
        Description = ""
    }

(**
### Sneak Attack (from invisibility)
*)
let SneakAttack (rogueLevel:int) =
    {
    Name = "Sneak Attack auf dem ersten Angriff"
    BonusAttacks = createBonusAttacks 0 Flat All
    BonusAttackRoll = createBonus 0 Flat
    BonusDamage = createBonus 0 Flat
    ExtraDamage = createDamage (int (ceil (float rogueLevel/2.))) 6 Precision
    AppliedTo = [|All|], 1        
    StatChanges = [|createStatChange "0" 0 Flat|]
    Description = ""
    }
(**
As you can see you can also have modifications with further automatic calculations.
*)