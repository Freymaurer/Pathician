(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
# Automatic Attack Simulation

<a name="StandardAttackAction"></a>

## Standard Attack Action

To calculate an attack action you need a character, a weapon and the relevant modifications. For the modifications you might already find everything in the library, but for the character and weapon you will most likely have to write your own.

Let us start with the character creation of an example character.
For this i will build a lvl 6 elf magus, who will use dexterity to hit the enemy but still use strength to damage.
*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator
open Library.AuxLibFunctions

let myMagus =  {
    CharacterName = "Best Character Name Ever"
    BAB = 5
    Strength = 1
    Dexterity = 5
    Constitution = 0
    Intelligence = 3
    Wisdom = 0
    Charisma = -2
    CasterLevel1 = 6
    CasterLevel2 = 0
    }
(** 
> This character has 12 Str, 20 dex (17 buy in, +2 racial, +1 from lvl 4 ability score increase) and caster level 6 with BAB 4.
>

Now we need to decide for a weapon. Like every magus we decide to use a Scimitar.
*)

let Scimitar = {
        Name                    = "Scimitar"
        Damage                  = createDamage 1 6 Slashing
        DamageBonus             = 0
        ExtraDamage             = createDamage 0 0 Untyped
        BonusAttackRolls        = 0
        CriticalRange           = [|18 .. 20|] 
        CriticalModifier        = 2
        Modifier                = createUsedModifier Dexterity Strength OneHanded 1.
        ManufacturedOrNatural   = Manufactured
        Description             = "Normal Scimitar"
        }

(** 
Now we want to calculate the Standard Attack Action with some modifications from the library (for a full list see For a full list of all prebuild modifications please see [here](https://freymaurer.github.io/PathfinderAttackSimulator/reference/pathfinderattacksimulator-library-modifications.html))
and also a new one: The modification for an intensified, empowered Shocking Grasp.
So how do we create this modification, which is a quite complex modification if we want it to be flexibel.
*)

let ShockingGraspEmpowered casterLevel metalTF = {
    Name = "Intensified Empowered Shocking Grasp"
    BonusAttacks = createBonusAttacks 0 NoBA All
    BonusAttackRoll = createBonus (if metalTF = true then 3 else 0) Flat
    BonusDamage = createBonus 0 Flat
    ExtraDamage = createDamage ((if casterLevel > 10 then 10 else casterLevel) 
                               |> fun x -> x + int (float x * 0.5) ) 6 Electricity
    AppliedTo = [|All|], 1
    StatChanges = [||]
    SizeChanges = createSizechange 0 Flat false
    Description = "Shocking Grasp deals 1d6 / level electricity damage up to a maximum of 10d6 for this intensified version. Empowered increases the number of all rolled dice by 50%"
    }
(** 
Now This is the most flexible version. It calculates the amount of d6 electricity damage depending on the caster level up to a maximum of 10d6
and also has asks wether or not the modification is used against someone with a metal armor/weapon.

The other modifications we will use are: Haste and Weapon Focus 

> If you use different weapons, don't use the Weapon Focus modification, but add its bonus directly to the weapon!
>

As we only use one weapon we can use Weapon Focus as a modification.
*)

open StandardAttackAction
open Library.Modifications

myStandardAttack myMagus Medium Scimitar [|ShockingGraspEmpowered myMagus.CasterLevel1 true; Haste; WeaponFocus|]

(** 
> You hit the enemy with a 21 (rolled 6) for 7 Slashing damage +27 Electricity damage (Intensified Empowered Shocking Grasp) !
>


For this attack we assume the target has some metal so we add the "true" parameter to ShockingGraspEmpowered (otherwise false). 
If modifications change or our weapon/character "upgrades" we simply need to update the related types or add new modifications to the modification array.

This gives high flexibility and a fast calculation of all relevant boni!
*)

(**
<a name="FullRoundAttackAction"></a>

## Full Round Attack Action

To calculate a full round attack action you need a character, a weapon/weapons and the relevant modifications. 
For the modifications you might already find everything in the library, but for the character and weapon you will most likely have to write your own.
A full-round attack action also needs additional information. Especially as what type of weapon a weapon should be used. 
Because this is difficult to explain let me first show you the difference between a full round attack action and a standard attack action:

*)

open FullRoundAttackAction

/// This is the previous standard attack action
myStandardAttack myMagus Medium Scimitar [|ShockingGraspEmpowered myMagus.CasterLevel1 true; Haste; WeaponFocus|]

myFullAttack myMagus Medium [|Scimitar,PrimaryMain|] [|ShockingGraspEmpowered myMagus.CasterLevel1 true; Haste; WeaponFocus|] 

(**
As you can see the difference is an (weapon * WeaponType) array instead of simply a weapon.
This is necessary for the calculator to know which weapon is should be used for additional attacks, e.g. Haste, Two Weapon Fighting.
Here are some example for this:

- Haste/FlurryOfBlows adds an additional attack to the _PrimaryMain_ weapon.
- Two-Weapon-Fighting has a _PrimaryMain_ weapon for e.g. Haste and the other Weapon needs to be classified as a _Primary_ weapon.
- Natural attacks (together with manufactured attacks) are always classified as _Secondary_.
- Primary natural attacks without any manufactured weapons are the only case when there is __no__ _PrimaryMain_ weapon and all primary natural attacks are classified as _Primary_.

Now let us skip some level and look at our Magus at a level 16
*)

let myMagus2 =  {
    CharacterName = "Best Character Name Ever^2"
    BAB = 12
    Strength = 3
    Dexterity = 7
    Constitution = 0
    Intelligence = 6
    Wisdom = 0
    Charisma = -2
    CasterLevel1 = 16
    CasterLevel2 = 0
    }

let ShinyBlingBlingScimitar = {
    Name                = "Really Shiny +5 Flaming Keen Ghost Touch Scimitar"
    Damage              = createDamage 1 6 Slashing
    DamageBonus         = 5
    ExtraDamage         = createDamage 1 6 Fire
    BonusAttackRolls    = 6
    CriticalRange       = [|15 .. 20|] 
    CriticalModifier    = 2
    Modifier            = createUsedModifier Dexterity Strength OneHanded 1.
    ManufacturedOrNatural = Manufactured
    Description         = "Really, really shiny."
    }

(**
But not only our character and his weapon changed but also the amount of modification that we have. And not only the modification that we have, but also our group's buffs.
We Somehow also got some nice Tentacles because why not.
*)

let Tentacle = {
    Name                = "Huge Tentacle"
    Damage              = createDamage 3 8 Bludgeoning
    DamageBonus         = 0
    ExtraDamage         = createDamage 1 6 Cold
    BonusAttackRolls    = 0
    CriticalRange       = [|20|] 
    CriticalModifier    = 3
    Modifier            = createUsedModifier Dexterity Strength OneHanded 1.
    ManufacturedOrNatural = Natural
    Description         = "We propably became the herold of an ancient god or something"
    }

myFullAttack myMagus2 Medium [| ShinyBlingBlingScimitar,PrimaryMain;
                                Tentacle,Secondary;
                                Tentacle,Secondary |] 

                             [| ShockingGraspEmpowered myMagus.CasterLevel1 true; 
                                Haste; 
                                EnlargePerson; 
                                PowerAttack myMagus2.BAB; 
                                FuriousFocus myMagus2.BAB;
                                BlessingOfFervorAttackBonus;
                                InspireCourage 15|]

(** 
> You attack with a Really Shiny +5 Flaming Keen Ghost Touch Scimitar and (hopefully) critically hit the enemy with a 49 (rolled 17) and confirm your crit with a 45 (rolled 13) for 24 Slashing damage +33 Electricity damage (Intensified Empowered Shocking Grasp), +5 Fire damage (Really Shiny +5 Flaming Keen Ghost Touch Scimitar) (crit * 2)!
> 
> You attack with a Really Shiny +5 Flaming Keen Ghost Touch Scimitar and (hopefully) critically hit the enemy with a 43 (rolled 18) and confirm your crit with a 32 (rolled 7) for 29 Slashing damage +3 Fire damage (Really Shiny +5 Flaming Keen Ghost Touch Scimitar) (crit * 2)!
> 
> You attack with a Really Shiny +5 Flaming Keen Ghost Touch Scimitar and hit the enemy with a 30 (rolled 10) for 31 Slashing damage +5 Fire damage (Really Shiny +5 Flaming Keen Ghost Touch Scimitar) !
> 
> You attack with a Really Shiny +5 Flaming Keen Ghost Touch Scimitar and hit the enemy with a 29 (rolled 14) for 27 Slashing damage +5 Fire damage (Really Shiny +5 Flaming Keen Ghost Touch Scimitar) !
> 
> You attack with a Huge Tentacle and hit the enemy with a 15 (rolled 1) for 22 Bludgeoning damage +2 Cold damage (Huge Tentacle) !
> 
> You attack with a Huge Tentacle and hit the enemy with a 22 (rolled 8) for 30 Bludgeoning damage +5 Cold damage (Huge Tentacle) !
>


Such a calculation could easily take 5 minutes, which can be reduced to several seconds with this script.

</br>

[Attack Simulator Repository](https://github.com/Freymaurer/PathfinderAttackSimulator)

<br>
*)