(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
PathfinderAttackSimulator
======================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The PathfinderAttackSimulator library can be <a href="https://nuget.org/packages/PathfinderAttackSimulator">installed from NuGet</a>:
      <pre>PM> Install-Package PathfinderAttackSimulator</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Pathfinder is, and that is a great part of why i like it so much, very free in character design and allows for a variety of different options.
But with great freedom in character design comes great complexity and especially a attack calculation that can get quite complex.
Of course you can calculate all different options beforehand and write them down separatly ... but then the bard starts with his Inspire Courage,
the cleric adds Blessing of Fervor, arcane casters start throwing around Haste and Enlarge Person and .. yes you can prepare for that and write down each and every option
(although it is already getting out of hand) but then comes a nasty poison or conditions and calculating attack rolls really are a mess and take a lot of time.
That is why i wrote this this Attack Action Calculator to ease down all of this complexity and free up some brainpower for the story, strategy and roleplaying.

This is my first coding project, so there will be lots of style errors and inefficient functions. If you notice something like this, please feel free to open an [issue](https://github.com/Freymaurer/PathfinderAttackSimulator/issues) and i will gladly fix it.
The function itself should work without errors, but if you encounter any please (again) open an [issue](https://github.com/Freymaurer/PathfinderAttackSimulator/issues) and let me know.

The current version also features an d20pfsrd bestiary entry reader and a calculator for these bestiary entries. Allowing gamemasters more easily to fill a board with lots of nice and deadly minions.

Example for a automatic calculated full round attack action.
-------

This example demonstrates the full round attack action calculater, which is able to calculate automatically: 

* the number of attacks
* boni from all kind of modifications
* crits and confirmation rolls
* damage rolls
* which boni stack and which not
* calculate the size depending on size modifiers e.g. from enlarger person or improved natural attack
* resize the used weapons depending on the size change

to just list a few of the implemented options.

> Parrn is one of my characters, i currently play. He is a half-giant rogue with 20 strength (_please don't judge_) and likes to hit like a truck with his greatsword.
> And because he has some familiars that attack with him, using such a script is a nice option for me, because it frees up some mindpower for roleplaying and strategy, without having to worry about
> missing some modifiers or calculating everything wrong.

*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.FullRoundAttackAction
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons
open PathfinderAttackSimulator

myFullAttack myParrn Medium [|greatswordParrn,PrimaryMain; 
                            greatswordParrn,Primary; 
                            bite,Secondary|] 
                            
                            [|Flanking; 
                            SneakAttack 8; 
                            MutagenStrength; 
                            Haste; 
                            TwoWeaponFighting|]
(**
> You attack with a Large +1 Keen Greatsword and (hopefully) critically hit the enemy with a 35 (rolled 19) and confirm your crit with a 20 (rolled 4) for 26 Slashing damage +9 Precision Schaden (crit * 2)!
>
> You attack with a Large +1 Keen Greatsword and hit the enemy with a 19 (rolled 3) for 25 Slashing damage +12 Precision Schaden !
>
> You attack with a Large +1 Keen Greatsword and hit the enemy with a 12 (rolled 1) for 29 Slashing damage +8 Precision Schaden !
>
> You attack with a Large +1 Keen Greatsword and hit the enemy with a 30 (rolled 14) for 16 Slashing damage +12 Precision Schaden !
>
> You attack with a Bite and hit the enemy with a 30 (rolled 18) for 10 BludgeoningOrPiercingOrSlashing damage +10 Precision Schaden !
>

Next will be a short example for the d20pfsrd bestiary entry reader.
This will be shown with the help of [Kyton Eremite Overlord](https://www.d20pfsrd.com/bestiary/unique-monsters/cr-22/kyton-eremite-overlord).
*)
open D20pfsrdReader
open D20pfsrdReader.AuxFunctions
open D20pfsrdCalculator

let kytonEremiteOverlord = getMonsterInformation "https://www.d20pfsrd.com/bestiary/unique-monsters/cr-22/kyton-eremite-overlord"

calculateStandardAttack kytonEremiteOverlord Melee 1 [||]
// > You attack with a bite and hit with a 53 (rolled 16) for 24 damage plus pain!

calculateFullAttack kytonEremiteOverlord Melee 1 [||]
// > You attack with a bite and hit with a 41 (rolled 4) for 23 damage plus pain!
// > You attack with a claws and hit with a 43 (rolled 6) for 21 damage plus pain!
// > You attack with a claws and hit with a 55 (rolled 18) for 24 damage plus pain!
// > You attack with a thorn vines touch and hit with a 45 (rolled 13) for 0 damage plus pain plus grab, constrict and energy drain!
// > You attack with a thorn vines touch and hit with a 36 (rolled 4) for 0 damage plus pain plus grab, constrict and energy drain!
// > You attack with a thorn vines touch and hit with a 40 (rolled 8) for 0 damage plus pain plus grab, constrict and energy drain!
// > You attack with a thorn vines touch and hit with a 43 (rolled 11) for 0 damage plus pain plus grab, constrict and energy drain!
// > You attack with a thorn vines touch and hit with a 50 (rolled 18) for 0 damage plus pain plus grab, constrict and energy drain!
// > You attack with a thorn vines touch and hit with a 36 (rolled 4) for 0 damage plus pain plus grab, constrict and energy drain!

(**
It is also possible to add modifications to these attacks to easily adapt to the game.

Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/PathfinderAttackSimulator/tree/master/docs/content
  [gh]: https://github.com/fsprojects/PathfinderAttackSimulator
  [issues]: https://github.com/fsprojects/PathfinderAttackSimulator/issues
  [readme]: https://github.com/fsprojects/PathfinderAttackSimulator/blob/master/README.md
  [license]: https://github.com/fsprojects/PathfinderAttackSimulator/blob/master/LICENSE.txt
*)
