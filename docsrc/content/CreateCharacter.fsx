(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../src/PathfinderAttackSimulator/bin/Release/netstandard2.0"

(**
Creating a Character
======================

Example on how a Character should be created.
-------

A character needs certain informations for a correct calculation. 
Right now this is not hand-held at all, but might get better in a future version.

This is most likely the easiest of the information types and really self-explanatory.
The first example is a fighter lvl 6 that wants to use bow and melee weapons.

### The Fighter
*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.Library.AuxLibFunctions

let myCharacter =  {
    CharacterName = "Best Character Name Ever"
    BAB = 6
    Strength = 7
    Dexterity = 3
    Constitution = 2
    Intelligence = -3
    Wisdom = 0
    Charisma = -3
    CasterLevel1 = 0
    CasterLevel2 = 0
    }

(**
> Without any doubt we have a nice and optimized fighter with "de inteligenz to crash samfing".

* Abilitie Scores: = The numbers given for the abilities are the modifier and not the score numbers (obviously).
* BAB = base attack bonus 
* CasteLevel = can be used for e.g. Magus Spellstrike with Shocking grasp
*)