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

### +2 Keen Flaming Greatsword
*)

#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.Library.AuxLibFunctions

let greatswordPlus2KeenFlamingBurst = {
        Name                    = "+2 keen flaming greatsword"
        Damage                  = createDamage 2 6 Slashing
        DamageBonus             = 2
        ExtraDamage             = createDamageHitAndCrit 1 6 Fire 1 10 Fire
        BonusAttackRolls        = 2
        CriticalRange           = [|17 .. 20|] 
        CriticalModifier        = 2
        Modifier                = createUsedModifier Strength Strength TwoHanded 1.5
        ManufacturedOrNatural   = Manufactured
        Description             = ""
        }

(**
Now to explain the different categories:

* Name = The actual correct name of the weapon, currently not relevant for the code.
* Damage = The core weapon damage. For 2d6 slashing this needs to be "createDamage 2 6 Slashing".
* DamageBonus = Enhancement bonus to dmg, or weapon specific boni like Weapon Specialization.
* ExtraDamage = This field contains damage that will be calculated and displayed separately, e.g. the flaming burst weapon enchantment.
    The first 3 variables are used on non crit attacks, the latter 3 variables are used in addition to the first values but only(!) for critical hits.
* BonusAttackRolls = Enhancement bonus to attack roll and weapon specific boni like Weapon Focus.
* CriticalRange = All rolled dice representing a crit.
* CriticalModifier = The critical hit modifier for that weapon.
* Modifier = The modifier used to hit something, to calculate damage, either OneHanded or TwoHanded and the multiplicator for dmg. 
    These variables are shown above in this order.
* ManufacturedOrNatural = Either Manufactured or Natural, this is necessary for a correct full-round attack action.
* Description = Whatever you want to take note of. For special weapon you can copy pase additional effects in here. 
    Or you might want to take note that the weapon has weapon focus applied to it.

*)

