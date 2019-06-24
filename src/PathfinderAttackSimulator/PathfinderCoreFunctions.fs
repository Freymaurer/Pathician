namespace PathfinderAttackSimulator

open System

open PathfinderAttackSimulator
open Library.AuxLibFunctions
open LibraryModifications

/// Library for all pre-written modifications
module CoreFunctions =

    /// Attack calculator helper functions
    module AuxCoreFunctions =
        
        /// This function returns "count" randomized values from 1 to "diceSides"
        let rollDice count (diceSides:int) =
            let rnd = System.Random()
            if diceSides = 0 
            then [|0|]
            else Array.init count (fun _ -> rnd.Next (1, diceSides+1))
        
        /// This function picks a random value from an array; used as an additional layer of "randomn" for the dice rolls.
        let getRndArrElement =
            let rnd = Random()
            fun (arr : int[]) -> arr.[rnd.Next(arr.Length)]
    

        /// rolls dice for weapon
        let getDamageRolls die =
            rollDice 100000 die

        let findSizes = [1,createSizeAttributes 8 1 Fine;
                         2,createSizeAttributes 4 2 Diminuitive;
                         3,createSizeAttributes 2 3 Tiny;
                         4,createSizeAttributes 1 4 Small;
                         5,createSizeAttributes 0 5 Medium;
                         6,createSizeAttributes -1 6 Large;
                         7,createSizeAttributes -2 7 Huge;
                         8,createSizeAttributes -4 8 Gargantuan;
                         9,createSizeAttributes -8 9 Colossal
                         ] |> Map.ofSeq

        /// calculates real size changes due to modifications and applies them to the start size.
        /// This function returns an integer representing the new size (The map of size integer to size is "findSizes"
        let calculateSize (size: SizeType) (modifications: AttackModification [])=

            let startSize =
                match size with
                | Fine          -> 1
                | Diminuitive   -> 2
                | Tiny          -> 3
                | Small         -> 4
                | Medium        -> 5
                | Large         -> 6
                | Huge          -> 7
                | Gargantuan    -> 8
                | Colossal      -> 9

            let changeSizeBy =
                modifications
                |> Array.filter (fun x -> x.SizeChanges.EffectiveSizeChange = false)
                |> Array.map (fun x -> x.SizeChanges)
                |> Array.groupBy (fun x -> x.SizeChangeBonustype)
                |> Array.map (fun (header,bonusArr) -> if header <> BonusTypes.Flat 
                                                       then bonusArr
                                                            |> Array.sortByDescending (fun x -> x.SizeChangeValue) 
                                                            |> fun x -> Array.head x
                                                            |> fun x -> x.SizeChangeValue
                                                       elif header = BonusTypes.Flat
                                                       then bonusArr
                                                            |> Array.map (fun x -> x.SizeChangeValue)
                                                            |> Array.sum
                                                       else failwith "Unrecognized Pattern of size changes in 'calculateSize'" 
                             )
                |> Array.sum

            (startSize + changeSizeBy)
            |> fun x -> if x > 9 then 9
                        elif x < 1 then 1
                        else x

    module OneAttack =

        module toHit =

            open AuxCoreFunctions

            /// calculates size bonus to attack rolls (eg. +1 for small)
            let addSizeBonus newSizeInt =
                newSizeInt
                |> fun x -> Map.find x findSizes
                |> fun x -> x.SizeModifier

            
            /// calculates bonus on attack rolls due to the ability score used by the weapon. 
            /// This function includes changes to these ability score modifiers due to modifications.
            let getUsedModifierToHit (char: CharacterStats) (weapon: Weapon) (modifications: AttackModification []) =
            
                let getStatChangesToHit =
                    modifications
                    |> Array.collect (fun x -> x.StatChanges)
                    |> Array.filter (fun statChange -> statChange.Attribute = weapon.Modifier.ToHit)
                    |> Array.groupBy (fun statChange -> statChange.Bonustype)
                    |> Array.map (fun (uselessHeader,x) -> x)
                    ///Next step should take the highest stat change to remove non-stacking boni
                    ///But what if a negative and a positive bonus of the same type exist?
                    |> Array.map (fun x -> Array.sortByDescending (fun statChange -> statChange.AttributeChange) x)
                    |> Array.map (fun x -> Array.head x)
                    |> Array.map (fun statChange -> statChange.AttributeChange)
                    |> Array.sum
            
                (match weapon.Modifier.ToHit with
                | Strength      -> char.Strength
                | Dexterity     -> char.Dexterity
                | Constitution  -> char.Constitution
                | Intelligence  -> char.Intelligence
                | Wisdom        -> char.Wisdom
                | Charisma      -> char.Charisma
                | _             -> 10
                )
                |> fun x -> x + getStatChangesToHit
                |> fun x -> (float x-10.)/2.
                |> floor |> int
            
            /// calculates all boni to attack rolls from modifications and checks if they stack or not
            let addBoniToAttack (modifications: AttackModification []) = 
                modifications 
                |> Array.map (fun x -> x.BonusAttackRoll.OnHit)
                |> Array.groupBy (fun x -> x.BonusType)
                |> Array.map (fun (header,bonusArr) -> if header <> BonusTypes.Flat 
                                                       then bonusArr
                                                            |> Array.sortByDescending (fun x -> x.Value) 
                                                            |> fun x -> Array.head x
                                                            |> fun x -> x.Value
                                                       elif header = BonusTypes.Flat
                                                       then bonusArr
                                                            |> Array.map (fun x -> x.Value)
                                                            |> Array.sum
                                                       else failwith "Unrecognized Pattern of attackBoni in 'addBoniToAttack'"
                              )
                |> Array.sum

            /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
            let getTotalAttackCritBonus (modifications: AttackModification []) bonusToAttack =
                let critSpecificBonus =
                    modifications
                    |> Array.map (fun x -> x.BonusAttackRoll.OnCrit)
                    |> Array.groupBy (fun x -> x.BonusType)
                    |> Array.map (fun (header,bonusArr) -> if header <> BonusTypes.Flat 
                                                           then bonusArr
                                                                |> Array.sortByDescending (fun x -> x.Value) 
                                                                |> fun x -> Array.head x
                                                                |> fun x -> x.Value
                                                           elif header = BonusTypes.Flat
                                                           then bonusArr
                                                                |> Array.map (fun x -> x.Value)
                                                                |> Array.sum
                                                           else failwith "Unrecognized Pattern of attackBoni in 'addBoniToAttack'"
                                  )
                    |> Array.sum
                bonusToAttack + critSpecificBonus
        
        module toDmg =

            open AuxCoreFunctions

            /// calculates bonus on damage rolls due to the ability score used by the weapon and the related multiplied
            /// calculates stat changes due to modifications
            let getStatChangesToDmg (weapon: Weapon) (modifications: AttackModification []) =
                modifications
                |> Array.collect (fun x -> x.StatChanges)
                |> Array.filter (fun statChange -> statChange.Attribute = weapon.Modifier.ToDmg)
                |> Array.groupBy (fun statChange -> statChange.Bonustype)
                |> Array.map (fun (useless,x) -> x)
                |> Array.map (fun x -> Array.sortByDescending (fun statChange -> statChange.AttributeChange) x)
                |> Array.map (fun x -> Array.head x)
                |> Array.map (fun statChange -> statChange.AttributeChange)
                |> Array.sum
                |> float

            ///
            let addDamageMod (char: CharacterStats) (weapon: Weapon) statChangeToDamageAbilityScore =

                match weapon.Modifier.ToDmg with
                | Strength      -> char.Strength
                | Dexterity     -> char.Dexterity
                | Constitution  -> char.Constitution
                | Intelligence  -> char.Intelligence
                | Wisdom        -> char.Wisdom
                | Charisma      -> char.Charisma
                | _             -> 0
                |> fun stat -> float stat + statChangeToDamageAbilityScore 
                |> fun x -> (x-10.)/2.

            /// calculates size change and resizes weapon damage dice.
            let adjustWeaponDamage (size: SizeType) (weaponDmgDie: int) (weaponDmgNOfDie:int) (modifications: AttackModification [])=

                let startSize =
                    match size with
                    | Fine          -> 1
                    | Diminuitive   -> 2
                    | Tiny          -> 3
                    | Small         -> 4
                    | Medium        -> 5
                    | Large         -> 6
                    | Huge          -> 7
                    | Gargantuan    -> 8
                    | Colossal      -> 9

                let effectiveSize =

                    let changeSizeBy =
                        modifications
                        |> Array.map (fun x -> x.SizeChanges)
                        |> Array.groupBy (fun x -> x.SizeChangeBonustype)
                        |> Array.map (fun (header,bonusArr) -> if header <> BonusTypes.Flat 
                                                               then bonusArr
                                                                    |> Array.sortByDescending (fun x -> x.SizeChangeValue) 
                                                                    |> fun x -> Array.head x
                                                                    |> fun x -> x.SizeChangeValue
                                                               elif header = BonusTypes.Flat
                                                               then bonusArr
                                                                    |> Array.map (fun x -> x.SizeChangeValue)
                                                                    |> Array.sum
                                                               else failwith "Unrecognized Pattern of attackBoni in 'addBoniToAttack'" 
                                     )
                        |> Array.sum

                    (startSize + changeSizeBy)
                    |> fun x -> if x > 9 then 9
                                elif x < 1 then 1
                                else x

                let diceRow = 
                    [|(1,1);(1,2);(1,3);(1,4);(1,6);(1,8);(1,10);(2,6);(2,8);(3,6);(3,8);
                    (4,6);(4,8);(6,6);(6,8);(8,6);(8,8);(12,6);(12,8);(16,6);(16,8);(24,6);(24,8);(36,6);(36,8)|] 

                ///https://paizo.com/paizo/faq/v5748nruor1fm#v5748eaic9t3f
                let getSizeChange weaponDmgDie weaponDmgNOfDie (startS: int) (modifiedS: int) =
                    let snowFlakeIncrease numberofdice (die: int) =
                        match numberofdice with 
                        | 1 -> 2,die
                        | _ -> (numberofdice + int (floor (float numberofdice)*(1./3.))), die
                    let snowFlakeDecrease numberofdice (die: int) =
                        match numberofdice with 
                        | 2 -> 1,die
                        | _ -> (numberofdice - int (floor (float numberofdice)*(1./3.))), die
                    let isEven x = (x % 2) = 0         
                    let isOdd x = (x % 2) = 1
                    let sizeDiff = modifiedS - startS
                    let decInc = if sizeDiff < 0 then (-1.)
                                 elif sizeDiff > 0 then (1.)
                                 else 0.
                    let adjustedDie = match weaponDmgDie with
                                      | 2   -> weaponDmgNOfDie, weaponDmgDie
                                      | 3   -> weaponDmgNOfDie, weaponDmgDie
                                      | 4   -> match weaponDmgNOfDie with
                                               | 1                                       -> weaponDmgNOfDie, weaponDmgDie
                                               | odd when isOdd weaponDmgNOfDie = true   -> int (ceil (float weaponDmgNOfDie/2.)), 6
                                               | even when isEven weaponDmgNOfDie = true -> (weaponDmgNOfDie/2), 8
                                               | _                                       -> failwith "unknown combination for reCalcWeapon damage dice calculator according to size; Error4"
                                      | 6   -> weaponDmgNOfDie, weaponDmgDie
                                      | 8   -> weaponDmgNOfDie, weaponDmgDie
                                      | 10  -> weaponDmgNOfDie, weaponDmgDie
                                      | 12  -> (weaponDmgNOfDie*2), 6
                                      | 20  -> (weaponDmgNOfDie*2), 10
                                      | _   -> if weaponDmgDie % 10 = 0
                                               then ((weaponDmgDie / 10) * weaponDmgNOfDie), 10
                                               elif weaponDmgDie % 6 = 0
                                               then ((weaponDmgDie / 6) * weaponDmgNOfDie), 6
                                               elif weaponDmgDie % 4 = 0 
                                               then ((weaponDmgDie / 4) * weaponDmgNOfDie), 4
                                               else weaponDmgNOfDie, weaponDmgDie
                    let adjustedDieNum = fst adjustedDie
                    let adjustedDietype = snd adjustedDie

                    let rec loopResizeWeapon (n:int) (nDice:int) (die:int) = 

                        let stepIncrease = if startS + (int decInc*n) < 5 || (nDice * die) < 6 
                                           then 1
                                           else 2
                        let stepDecrease = if startS + (int decInc*n) < 6 || (nDice * die) < 8 
                                           then 1
                                           else 2
                        let findRowPosition =
                            Array.tryFindIndex (fun (x,y) -> x = nDice && y = die) diceRow
                        if sizeDiff = 0 || n >= abs sizeDiff
                        then nDice, die
                        else findRowPosition
                             |> fun x -> if (x.IsSome) 
                                         then match decInc with 
                                              | dec when decInc < 0. -> if x.Value < 1 then diceRow.[0] else diceRow.[x.Value - stepDecrease]
                                              | inc when decInc > 0. -> if x.Value > (diceRow.Length-3) then (snowFlakeIncrease nDice die) else diceRow.[x.Value + stepIncrease]
                                              | _ -> failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error1"
                                         elif x.IsSome = false 
                                         then match decInc with 
                                              | dec when decInc < 0. -> snowFlakeDecrease nDice die
                                              | inc when decInc > 0. -> snowFlakeIncrease nDice die
                                              | _ -> failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error2"
                                         else failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error3"
                             |> fun (nDie,die) -> loopResizeWeapon (n+1) nDie die

                    loopResizeWeapon 0 adjustedDieNum adjustedDietype

                getSizeChange weaponDmgDie weaponDmgNOfDie startSize effectiveSize

            /// calculates all boni to damage rolls from modifications and checks if they stack or not
            let addDamageBoni (modifications:AttackModification []) =
                modifications
                |> Array.map (fun x -> x.BonusDamage)
                |> Array.groupBy (fun x -> x.BonusType)
                |> Array.map (fun (x,bonusArr) -> if x <> BonusTypes.Flat 
                                                  then bonusArr
                                                       |> Array.sortByDescending (fun x -> x.Value) 
                                                       |> fun x -> Array.head x
                                                       |> fun x -> x.Value
                                                  else bonusArr
                                                       |> Array.map (fun x -> x.Value)
                                                       |> Array.sum                   
                              )
                |> Array.sum

            /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
            let getExtraDamageOnHit (weapon:Weapon) (modifications:AttackModification []) (resizedWeaponDmg:Damage) = 
                let rec getRandRoll listOfRolls die number =
                    (getRndArrElement (getDamageRolls die))::listOfRolls
                    |> fun rollList -> if rollList.Length >= number
                                       then rollList
                                       else getRandRoll rollList die number
                [|weapon.ExtraDamage.OnHit,weapon.Name|]
                |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage.OnHit,x.Name) )
                |> Array.map (fun (extraD,str) -> getRandRoll [] extraD.Die extraD.NumberOfDie |> List.toArray |> Array.sum
                                                  , extraD.DamageType, str
                             )
                |> fun x -> x
                ///Vital Strike hardcode
                |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                               || x = VitalStrikeImproved 
                                                                               || x = VitalStrikeGreater) modifications)
                                   then Array.filter (fun x -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                        |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                        |> Array.head
                                        |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                            yield getRandRoll [] resizedWeaponDmg.Die resizedWeaponDmg.NumberOfDie|], vitalS.Name
                                        |> fun (intList,str) -> Array.map List.sum intList, str
                                        |> fun x -> x
                                        |> fun (intList,str) -> Array.sum intList, str
                                        |> fun (bonus,str) -> Array.append [|bonus,resizedWeaponDmg.DamageType,str|] extraDmg
                                   else extraDmg
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0,Untyped) && (bonus,dType) <> (0,VitalStrikeDamage) )

            /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
            let getExtraDamageOnCrit attackRoll (weapon:Weapon) (modifications:AttackModification[]) (resizedWeaponDmg:Damage) = 
                let rec getRandRoll listOfRolls die number =
                    (getRndArrElement (getDamageRolls die))::listOfRolls
                    |> fun rollList -> if rollList.Length >= number
                                       then rollList
                                       else getRandRoll rollList die number
                if (Array.contains attackRoll weapon.CriticalRange) = false
                // stop function right here if there is no crit
                then [||]
                else [|weapon.ExtraDamage.OnCrit,weapon.Name|]
                     |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage.OnCrit,x.Name) )
                     |> Array.map (fun (extraD,str) -> getRandRoll [] extraD.Die extraD.NumberOfDie |> List.toArray |> Array.sum
                                                       , extraD.DamageType, str
                                  )
                     |> fun x -> x
                     ///Vital Strike hardcode
                     |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                                    || x = VitalStrikeImproved 
                                                                                    || x = VitalStrikeGreater) modifications)
                                        then Array.filter (fun x -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                             |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                             |> Array.head
                                             |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                                 yield getRandRoll [] resizedWeaponDmg.Die resizedWeaponDmg.NumberOfDie|], vitalS.Name
                                             |> fun (intList,str) -> Array.map List.sum intList, str
                                             |> fun x -> x
                                             |> fun (intList,str) -> Array.sum intList, str
                                             |> fun (bonus,str) -> Array.append [|bonus,resizedWeaponDmg.DamageType,str|] extraDmg
                                        else extraDmg
                     |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0,Untyped) && (bonus,dType) <> (0,VitalStrikeDamage) )
            
            /// combines the extra damage and the extra damage on crit
            let combineExtraDamage (extraDamage:(int*DamageTypes*string)[]) (critExtraDamage:(int*DamageTypes*string)[])=
                let getValue (triple:(int*DamageTypes*string)) = 
                    triple |> fun (value,dType,string) -> value
                let getDmgType (triple:(int*DamageTypes*string)) = 
                    triple |> fun (value,dType,string) -> dType
                let getName (triple:(int*DamageTypes*string)) = 
                    triple |> fun (value,dType,string) -> string
                if critExtraDamage = [||]
                then extraDamage
                else Array.map2 (fun onHit onCrit -> (getValue onHit) + (getValue onCrit), getDmgType onHit, getName onHit) extraDamage critExtraDamage

            /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something
            let extraDamageToString (extraDmgArr:(int*DamageTypes*string)[]) = 
                extraDmgArr
                |> Array.map (fun (value,dType,name) -> "+" + (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
                |> Array.fold (fun strArr x -> strArr + x) "" 
                |> fun x -> x.TrimEnd [|' ';','|]          
    
    module FullAttack =
        
        let getBonusAttacksFor (weaponType:WeaponType) (modifications:AttackModification [])  = 
            modifications
            |> Array.map (fun x -> x.BonusAttacks)
            |> Array.filter (fun bAttacks -> bAttacks.WeaponTypeWithBonusAttacks = weaponType)
            |> Array.groupBy (fun x -> x.TypeOfBonusAttacks)
            |> Array.map (fun (bTypes,bAttacks) -> if bTypes <> FlatBA
                                                   then bAttacks
                                                        |> Array.sortByDescending (fun x -> x.NumberOfBonusAttacks) 
                                                        |> fun x -> Array.head x
                                                        |> fun x -> x.NumberOfBonusAttacks
                                                   else bAttacks
                                                        |> Array.map (fun x -> x.NumberOfBonusAttacks)
                                                        |> Array.sum
                         )
            |> Array.sum

