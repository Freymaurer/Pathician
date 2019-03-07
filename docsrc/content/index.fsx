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

Example for a automatic calculated full round attack action.
-------

This example demonstrates the full round attack action calculater, which is able to automatically: 

* the number of attacks
* boni from all kind of modifications
* crits and confirmation rolls
* damage rolls
* which boni stack and which not

to just list a few of the implemented options.

*)
#r "PathfinderAttackSimulator.dll"
open PathfinderAttackSimulator.FullRoundAttackAction
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.Characters
open PathfinderAttackSimulator.Library.Modifications
open PathfinderAttackSimulator.Library.Weapons

myFullAttack myParrn    [|greatswordParrnHuge,PrimaryMain;
                        bite,Secondary|]                                
                        
                        [|EnlargePerson;
                        MutagenStrength;
                        Haste;
                        SneakAttack 8;
                        PowerAttack myParrn.BAB;
                        FuriousFocus 8|]

(**
> Du greifst mit Huge +1 Keen Greatsword an und triffst den Gegner mit 23 (gewürfelt 6) für 34 Slashing Schaden +9 Precision Schaden !
> Du greifst mit Huge +1 Keen Greatsword an und crittest (hoffentlich) den Gegner mit 31 (gewürfelt 17) und bestätigst mit 29 (gewürfelt 15) für 33 Slashing Schaden +17 Precision Schaden (crit * 2)!
> Du greifst mit Huge +1 Keen Greatsword an und triffst den Gegner mit 15 (gewürfelt 1) für 31 Slashing Schaden +19 Precision Schaden !
> Du greifst mit Bite an und crittest (hoffentlich) den Gegner mit 33 (gewürfelt 20) und bestätigst mit 32 (gewürfelt 19) für 10 BludgeoningOrPiercingOrSlashing Schaden +17 Precision Schaden (crit * 2)!


Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this sample library.

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
