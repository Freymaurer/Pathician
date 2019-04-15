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
open PathfinderAttackSimulator

(**
The most difficult part might actually be to think of a nice name for the type-binding, as you can't start with a number.
Because a weapon is a type it is really easy to handle, as you can just dot into the weapon name if you are unsure if you made some mistake creating the weapon.
*)

let showName = "something"


