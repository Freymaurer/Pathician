(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
Creating a Weapon
======================

Example on how a Weapon should be created.
-------

A weapon needs certain informations for a correct calculation. 
Right now this is not hand-held at all, but might get better in a future version.

Let's start with an easy example a +2 keen flaming greatsword

### +2 keen flaming greatsword
*)

#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.Library

let keenflaming2greatsword = {
        Name                = "+2 keen flaming greatsword"
        Damage              = createDamage 2 6 Slashing
        DamageBonus         = 2
        ExtraDamage         = createDamage 1 6 Fire
        BonusAttackRolls    = 2
        CriticalRange       = [|17 .. 20|] 
        CriticalModifier    = 2
        Modifier            = createUsedModifier "Str" "Str" 1.5
        ManufacturedOrNatural = createStringForLib "Manufactured" 
        }

(**
The most difficult part might actually be to think of a nice name for the type-binding, as you can't start with a number.
Because a weapon is a type it is really easy to handle, as you can just dot into the weapon name if you are unsure if you made some mistake creating the weapon
*)

let showName = keenflaming2greatsword.Name

(*** include-value:showName ***)

let showDamageDie = keenflaming2greatsword.Damage.Die

(*** include-value:showDamageDie ***)

(**
Now to explain the different categories:
* Name = the actual correct name of the weapon, currently not relevant for the code
* Damage = the core weapon damage for 2d6 slashing this needs to be "createDamage 2 6 Slashing"
* DamageBonus = enhancement bonus to dmg
* ExtraDamage = Damage that will be calculated and displayed separately, e.g. 1d6 fire from flaming
* BonusAttackRolls = enhancement bonus to attack roll
* CriticalRange = all rolled dice, representing a crit
* CriticalModifier = currently just displayed and not automatically calculated
* Modifier = the modifier used to hit something, to damage and the multiplicator for dmg in this order
* ManufacturedOrNatural = either manufactured or natural, this is necessary for a correct fullround attack action. 
    Always use this in combination with "createStringForLib".
*)

