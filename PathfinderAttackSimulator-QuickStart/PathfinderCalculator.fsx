#r @"src\PathfinderAttackSimulator.dll"
#r "netstandard"

open System.IO
open PathfinderAttackSimulator
open Library.AuxLibFunctions
open LibraryModifications
open StandardAttackAction
open FullRoundAttackAction
open BestiaryReader
open BestiaryReader.AuxFunctions
open BestiaryCalculator
open DamagePerRound
open DamagePerRound.AuxDPRFunctions

//ignore this part as it contains helper functions for the dpr calculator
let baseDirectory = __SOURCE_DIRECTORY__
let fileName = "src\Pathfinder Bestiary with Statistics - Statistics.tsv"
let filePath = Path.Combine(baseDirectory, fileName)
////////////////////////////////////////////////////////////////////////

//     Hello,
// below are some "empty" templates for modifications, weapons and characters 

let modificationTemplate = 
    {
        Name = "Modification Template"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20 
        StatChanges = [|(createStatChange Strength 0 Flat)|] // use: [||] if no ability score change is applied
        SizeChanges = createSizechange 0 Flat false
        Description = ""
    }

let characterTemplate = { 
        CharacterName = ""
        BAB = 0
        Strength = 10
        Dexterity = 10
        Constitution = 10
        Intelligence = 10
        Wisdom = 10
        Charisma = 10
        CasterLevel1 = 10
        CasterLevel2 = 10
        }

let weaponTemplate = {
        Name                = "Test"
        Damage              = createDamage 1 6 Slashing
        DamageBonus         = 0
        ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        BonusAttackRolls    = 0
        CriticalRange       = [|20|]
        CriticalModifier    = 2
        Modifier            = createUsedModifier Strength Strength OneHanded 1.
        ManufacturedOrNatural = Manufactured
        Description         = ""
        }

// Below you can find the d20pfsrd/archives of nethys reader & calculator

let importantMonster = getMonsterInformation "https://www.d20pfsrd.com/bestiary/monster-listings/animals/rat/rat-common/"

calculateFullAttack importantMonster Melee 1 [|Flanking|]
calculateStandardAttack importantMonster Melee 1 [|Flanking|]


// below you can find the attack calculator

myStandardAttack characterTemplate Medium weaponTemplate [|Flanking|]

myFullAttack characterTemplate Medium [|weaponTemplate,PrimaryMain;weaponTemplate,Primary|] [|Flanking;TwoWeaponFighting|]

// next a small example for damage per round calculation with the above examples

myStandardAttackDPR characterTemplate Medium weaponTemplate [|Flanking|] 1 ArmorClass Mean filePath

myFullAttackDPR characterTemplate Medium [|weaponTemplate,PrimaryMain|] [|Flanking|] 1 ArmorClass Mean filePath

// now for real beginners ;)
// at the beginning you should mark all with Strg + a and then execute it in Interactive by pressing alt + enter
// a new window should open showing some stuff *cough*
// now you can click on the function you are interested in and mark everything from it and then alt + enter.
// for the following example this should give you something similiar to this:
// You attack with a Test and hit the enemy with a 1 (rolled 1) for 6 Slashing damage!
// You attack with a Test and hit the enemy with a 8 (rolled 8) for 1 Slashing damage!
// val it : unit [] = [|(); ()|]

let test = myFullAttack characterTemplate Medium [|weaponTemplate,PrimaryMain;weaponTemplate,Primary|] [|Flanking;TwoWeaponFighting|]

// if you have questions about how to use anything here, even if the question is really simple, pls message me as i plan to make a FAQ.
// PS: everything written behind "//" is not recogniced as code and is not executed, use it to write notes or whatever you want.
// PPS: you can try and hover over different parts of these functions for more informartion, albeit a bit difficult to read.

//Version 0.0.1