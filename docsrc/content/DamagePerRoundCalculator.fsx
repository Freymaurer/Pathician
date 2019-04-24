(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
Damage Per Round Calculator
======================

Calculate Damage Per Round
-------

This tool is meant to help players compare different builds in their damage output per round.
It reads in the [Average MonsterStats](https://docs.google.com/spreadsheets/d/1E2-s8weiulPoBQjdI05LBzOUToyoZIdSsLKxHAvf8F8/edit#gid=3] and uses the 
information to calculate a players average damage per round against a given cr and armortype.

### Example 1

As a example we will use a simple character from the library.
> Parrn is a lvl 8 rouge/vivisectionist so his BAB is 6 and he has full sneak attack progression. He also has 22 Strength and wields a large greatsword. 
>
*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator
open Library.AuxLibFunctions
open Library.Characters
open Library.Modifications
open Library.Weapons
open DamagePerRound

myStandardAttackDPR myParrn 
                    Medium 
                    greatswordParrn 
                    [|PowerAttack myParrn.BAB; FuriousFocus myParrn.BAB; SneakAttack 8; Flanking|] 
                    8                              // Used against the average statistics for Cr 8 monsters.
                    AuxDPRFunctions.ArmorClass     // Defines the target armor, can be either ArmorClass, FlatFootedArmor or TouchArmor.
                    AuxDPRFunctions.Mean           // Determines which statistical value should be used, can be either Mean, Median or Mode

(**
> You hit the enemy for an average of 34.35 damage, the average enemy has 96 hp (15 = attack roll bonus; 22.275 damage from normal hits; 2.025 damage from threatened crits; 10.05 damage from confirmed crits (10.5 Precision damage (Sneak Attack))) !
>

And for the full-round attack action it works the same way.
*)
myFullAttackDPR myParrn 
                Medium 
                [|greatswordParrn,PrimaryMain|]
                [|PowerAttack myParrn.BAB; FuriousFocus myParrn.BAB; SneakAttack 8; Flanking|] 
                8                              // Used against the average statistics for Cr 8 monsters.
                AuxDPRFunctions.ArmorClass     // Defines the target armor, can be either ArmorClass, FlatFootedArmor or TouchArmor.
                AuxDPRFunctions.Mean           // Determines which statistical value should be used, can be either Mean, Median or Mode
(**
> You hit the enemy for an average of 34.35 damage (15 = attack roll bonus; 22.275 damage from normal hits; 2.025 damage from threatened crits; 10.05 damage from confirmed crits (10.5 Precision damage (Sneak Attack))) !
>
> You hit the enemy for an average of 18.32 damage (8 = attack roll bonus; 8.1 damage from normal hits; 4.86 damage from threatened crits; 5.36 damage from confirmed crits (5.6 Precision damage (Sneak Attack))) !
>
> Your combined damage per round is 52.67 damage, the average enemy has 96 hp (30.375 damage from normal hits; 6.885 damage from threatened crits; 15.41 damage from confirmed crits)
>

For more information on how to set up characters, weapon, modifications or how a normal standard/full-round attack action works please see the respective documentation.
*)
