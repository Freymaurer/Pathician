(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
d20pfsrd Bestiary Reader/Calculator
======================

Read d20pfsrd Bestiary Entries
-------

This tool is meant to help gamemasters when having to manage huge fights with lots of different enemy types.
It reads in the [d20pfsrd bestiary](https://www.d20pfsrd.com/bestiary/) entry and uses regex pattern matching to extract the necessary 
information from it. This is based on certain writing patterns, so a 3rd party entry that does not follow these specific patterns will not work with this function.

### Example Lunar Dragon

As a example we will use the [Ancient Lunar Dragon](https://www.d20pfsrd.com/bestiary/monster-listings/dragons/dragon-outer/outer-dragon-lunar/lunar-dragon-ancient/).
*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator
open D20pfsrdReader

let ancientLunarDragon = getMonsterInformation "https://www.d20pfsrd.com/bestiary/monster-listings/dragons/dragon-outer/outer-dragon-lunar/lunar-dragon-ancient/"
(**
This function extracts the necessary information from the bestiary entry. We can now give this information to the calculator function.
*)
open D20pfsrdReader.AuxFunctions
open D20pfsrdCalculator
open PathfinderAttackSimulator.Library

calculateFullAttack ancientLunarDragon Melee 1 [||]
(**
> You attack with a bite and hit with a 36 (rolled 4) for 34 damage !
>
> You attack with a claws and hit with a 51 (rolled 19) for 16 damage !
>
> You attack with a claws and hit with a 35 (rolled 3) for 21 damage !
>
> You attack with a wings and hit with a 49 (rolled 19) for 21 damage !
>
> You attack with a wings and hit with a 33 (rolled 3) for 19 damage !
>
> You attack with a tail slap  Space ft. and hit with a 48 (rolled 18) for 26 damage !
>

This is fast and easily calculated, without the gm juggling several dozen links/pages and die while also trying to tell a story.
Altough it is obviously not perfect, as you can see in the last calculated attack, in which a " Space ft." sneaked into the weapon name, hinting at a important detail reliant on an x-amount of feet.
After looking it up on the bestiary entry we can see that this was part of "tail slap +30 (2d8+18) Space 20 ft.", meaning... well i am actually not sure about this one.
This could either be 20 ft reach or that this attack hits everyone in a 20 ft space. Anyhow let's move to some better examples.
*)
(**
You can also let the enemy just do one standard attack with the function below. That version will also try and erase possible mali due to Rapid Shot or Two-Weapon Fighting, which will not apply to standard action attacks.
*)
calculateStandardAttack ancientLunarDragon Melee 1 [||]
// > You attack with a bite and hit with a 46 (rolled 14) for 33 damage !
(**
### Example Blade Lord

For the related link please click [here](https://www.d20pfsrd.com/bestiary/npc-s/npcs-cr-19/blade-lord-elf-rogue-20/)
*)

let bladeLord = getMonsterInformation "https://www.d20pfsrd.com/bestiary/npc-s/npcs-cr-19/blade-lord-elf-rogue-20/"

calculateFullAttack bladeLord Melee 1 [||]

(**
> You attack with a brilliant energy longsword and hit with a 25 (rolled 6) for 7 damage !
> 
> You attack with a brilliant energy longsword and hit with a 30 (rolled 16) for 12 damage !
> 
> You attack with a brilliant energy longsword and hit with a 18 (rolled 9) for 13 damage !
> 
> You attack with a frost short sword and hit the enemy with a 35 (rolled 15) for 6 damage +1 cold damage  !
> 
> You attack with a frost short sword and hit the enemy with a 24 (rolled 9) for 8 damage +5 cold damage  !
> 
> You attack with a frost short sword and hit the enemy with a 20 (rolled 10) for 6 damage +3 cold damage  !
>

If we now change the given "1" in the function above to "2" we get his second attack scheme ("or +2 brilliant energy longsword +21/+16/+11 (1d8+5/19–20)")
and so on.
...But wait... isn't this a bit _weak_ for a CR 19 npc? Well, then let us add his lvl 20 sneak attack ability to the AttackModificationArray
*)

calculateFullAttack bladeLord Melee 1 [|Modifications.SneakAttack 20|]
(**
> You attack with a brilliant energy longsword and (hopefully) critically hit the enemy with a 39 (rolled 20) and confirm your crit with a 32 (rolled 13) for 6 damage +35 Precision damage (crit * 2) !
> 
> You attack with a brilliant energy longsword and hit the enemy with a 32 (rolled 18) for 10 damage +31 Precision damage !
> 
> You attack with a brilliant energy longsword and hit the enemy with a 25 (rolled 16) for 6 damage +33 Precision damage !
> 
> You attack with a frost short sword and (hopefully) critically hit the enemy with a 40 (rolled 20) and confirm your crit with a 21 (rolled 1) for 7 damage +38 Precision damage, +5 cold damage (crit * 2) !
> 
> You attack with a frost short sword and hit the enemy with a 21 (rolled 6) for 9 damage +28 Precision damage, +6 cold damage !
> 
> You attack with a frost short sword and hit the enemy with a 12 (rolled 2) for 10 damage +37 Precision damage, +5 cold damage !
>

Ah, way better, this can be done with nearly any modification. At this moment this tool cannot calculate stat-changing modifications (alchemist mutagen), as it is difficult to determine which stat is used for dmg/attack.
And the tool has problems with Power Attack, as this feat gives different boni depending on how the weapon is wielded.

Try and circumvent the missing stat change modifications with the BonusAttackDamage modification:
*)

let testBoni1 = Modifications.BonusAttackDamage 2 5

(**
This would create a modification with +2 to attack rolls and +5 to damage rolls. You can also write this, without any namebinding, directly into the ModificationArray.

In the end one more example of a more complex variant.

### Kryton Eremite Overlord

Click [here](https://www.d20pfsrd.com/bestiary/unique-monsters/cr-22/kyton-eremite-overlord) for the link.
*)

let krytonEremiteOverlord = getMonsterInformation "https://www.d20pfsrd.com/bestiary/unique-monsters/cr-22/kyton-eremite-overlord"

calculateFullAttack krytonEremiteOverlord Melee 1 [|Modifications.EnlargePerson; Modifications.InspireCourage 15; Modifications.Shaken|]

(**
> You attack with a bite and hit with a 56 (rolled 19) for 19 damage plus pain!
> 
> You attack with a claws and hit with a 50 (rolled 13) for 18 damage plus pain!
> 
> You attack with a claws and hit with a 54 (rolled 17) for 22 damage plus pain!
> 
> You attack with a thorn vines touch and hit with a 37 (rolled 5) for 0 damage plus pain plus grab, constrict and energy drain!
> 
> You attack with a thorn vines touch and hit with a 40 (rolled 8) for 0 damage plus pain plus grab, constrict and energy drain!
> 
> You attack with a thorn vines touch and hit with a 51 (rolled 19) for 0 damage plus pain plus grab, constrict and energy drain!
> 
> You attack with a thorn vines touch and hit with a 49 (rolled 17) for 0 damage plus pain plus grab, constrict and energy drain!
> 
> You attack with a thorn vines touch and hit with a 42 (rolled 10) for 0 damage plus pain plus grab, constrict and energy drain!
> 
> You attack with a thorn vines touch and hit with a 43 (rolled 11) for 0 damage plus pain plus grab, constrict and energy drain!
>

So the enemy got enlarged (automatically resizes used weapons) and got a lvl 15 bard-like character at his side. 
Naturally, despite his buffs, he is still shaken, as he knows he does not stand a chance against a well organized party ;).
*)