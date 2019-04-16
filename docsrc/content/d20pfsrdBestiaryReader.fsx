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
...But wait... isn't this a bit _weak_ for a CR 19 npc? Well, then let us add his lvl 20 sneak attack ability
*)

calculateFullAttack bladeLord Melee 1 [|Modifications.SneakAttack 20|]
