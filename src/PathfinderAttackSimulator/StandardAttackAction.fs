namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.Library.Modifications

/// This module contains some smaller helper functions for all attack calculator functions and also the calculator for standard attack actions "myStandardAttack"
module StandardAttackAction =

    /// Attack calculator helper functions
    module AuxFunctions =
        
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
    
    open AuxFunctions
    

    /// This function returns the output of a standard attack action based on the used character stats, size, weapons and modifications.
    let myStandardAttack (char: CharacterStats) (size: SizeType) (weapon: Weapon) (modifications: AttackModification []) =
    
        /// calculates size changes due to modifications and applies them to the start size
        let calculatedSize =

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
                                                       else failwith "Unrecognized Pattern of attackBoni in 'addBoniToAttack'" 
                             )
                |> Array.sum

            (startSize + changeSizeBy)
            |> fun x -> if x > 9 then 9
                        elif x < 1 then 1
                        else x

        /// calculates bonus on attack rolls due to the ability score used by the weapon
        let getUsedModifierToHit =

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
        let addBoniToAttack = 
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
        
        /// calculates size bonus to attack rolls (eg. +1 for small)
        let addSizeBonus =
            calculatedSize
            |> fun x -> Map.find x findSizes
            |> fun x -> x.SizeModifier
        
        /// Sums up all different boni to attack rolls
        let getBonusToAttack =
            char.BAB + weapon.BonusAttackRolls + getUsedModifierToHit + addBoniToAttack + addSizeBonus
   
        /// rolls two dice; one for the regular hit and one for a possible crit confirmation roll
        let (attackRoll,critConfirmationRoll) = 
            let getAttackRolls =
                    rollDice 10000 20
            getRndArrElement getAttackRolls,getRndArrElement getAttackRolls
    
        /// complete bonus on attack = dice roll + Sum of all boni (getBonusToAttack)
        let totalAttackBonus =
            attackRoll + getBonusToAttack

        /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
        let totalAttackCritBonus =
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
            critConfirmationRoll + getBonusToAttack + critSpecificBonus
    
        /// rolls dice for weapon
        let getDamageRolls die =
            rollDice 100000 die
    
        /// calculates stat changes due to modifications
        let getStatChangesToDmg =
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
            
        /// calculates bonus on damage rolls due to the ability score used by the weapon and the related multiplied
        let addDamageMod =
            match weapon.Modifier.ToDmg with
                | Strength      -> char.Strength
                | Dexterity     -> char.Dexterity
                | Constitution  -> char.Constitution
                | Intelligence  -> char.Intelligence
                | Wisdom        -> char.Wisdom
                | Charisma      -> char.Charisma
                | _             -> 0
            |> fun stat -> float stat + getStatChangesToDmg
            |> fun x -> (x-10.)/2.
            |> fun x -> x * weapon.Modifier.MultiplicatorOnDamage.Multiplicator |> floor |> int
    
        /// calculates size change and resizes weapon damage dice.
        let sizeAdjustedWeaponDamage =
            
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
            let getSizeChange reCalcWeapon (startS: int) (modifiedS: int) =
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
                let adjustedDie = match reCalcWeapon.Damage.Die with
                                  | 2   -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                  | 3   -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                  | 4   -> match reCalcWeapon.Damage.NumberOfDie with
                                           | 1                                                       -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                           | odd when isOdd reCalcWeapon.Damage.NumberOfDie = true   -> int (ceil (float reCalcWeapon.Damage.NumberOfDie/2.)), 6
                                           | even when isEven reCalcWeapon.Damage.NumberOfDie = true -> (reCalcWeapon.Damage.NumberOfDie/2), 8
                                           | _                                                       -> failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error4"
                                  | 6   -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                  | 8   -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                  | 10  -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                  | 12  -> (reCalcWeapon.Damage.NumberOfDie*2), 6
                                  | 20  -> (reCalcWeapon.Damage.NumberOfDie*2), 10
                                  | _   -> if reCalcWeapon.Damage.Die % 10 = 0
                                           then ((reCalcWeapon.Damage.Die / 10) * reCalcWeapon.Damage.NumberOfDie), 10
                                           elif reCalcWeapon.Damage.Die % 6 = 0
                                           then ((reCalcWeapon.Damage.Die / 6) * reCalcWeapon.Damage.NumberOfDie), 6
                                           elif reCalcWeapon.Damage.Die % 4 = 0 
                                           then ((reCalcWeapon.Damage.Die / 4) * reCalcWeapon.Damage.NumberOfDie), 4
                                           else reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
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
                |> fun (n,die) -> createDamage n die reCalcWeapon.Damage.DamageType

            getSizeChange weapon startSize effectiveSize

        /// Rolls dice for resized weapon damage dice
        let addWeaponDamage = 
            let rec getRandRoll listOfRolls =
                (getRndArrElement (getDamageRolls sizeAdjustedWeaponDamage.Die) )::listOfRolls
                |> fun rollList -> if rollList.Length >= (sizeAdjustedWeaponDamage.NumberOfDie)
                                   then rollList
                                   else getRandRoll rollList
            getRandRoll [] |> List.toArray |> Array.sum
            |> fun damageDice -> damageDice + weapon.DamageBonus
    
        /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
        let getExtraDamageOnHit = 
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
                                                        yield getRandRoll [] sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                    |> fun (intList,str) -> Array.map List.sum intList, str
                                    |> fun x -> x
                                    |> fun (intList,str) -> Array.sum intList, str
                                    |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                               else extraDmg
            |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0,Untyped) && (bonus,dType) <> (0,VitalStrikeDamage) )

        /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
        let getExtraDamageOnCrit = 
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
                                                             yield getRandRoll [] sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                         |> fun (intList,str) -> Array.map List.sum intList, str
                                         |> fun x -> x
                                         |> fun (intList,str) -> Array.sum intList, str
                                         |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                                    else extraDmg
                 |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0,Untyped) && (bonus,dType) <> (0,VitalStrikeDamage) )
        
        /// combines the extra damage and the extra damage on crit
        let extraDamageCombined =
            let getValue (triple:(int*DamageTypes*string)) = 
                triple |> fun (value,dType,string) -> value
            let getDmgType (triple:(int*DamageTypes*string)) = 
                triple |> fun (value,dType,string) -> dType
            let getName (triple:(int*DamageTypes*string)) = 
                triple |> fun (value,dType,string) -> string
            if getExtraDamageOnCrit = [||]
            then getExtraDamageOnHit
            else Array.map2 (fun onHit onCrit -> (getValue onHit) + (getValue onCrit), getDmgType onHit, getName onHit) getExtraDamageOnHit getExtraDamageOnCrit

        /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something
        let extraDamageToString extraDmgArr= 
            extraDmgArr
            |> Array.map (fun (value,dType,name) -> "+" + (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
            |> Array.fold (fun strArr x -> strArr + x) "" 
            |> fun x -> x.TrimEnd [|' ';','|]          

        /// calculates all boni to damage rolls from modifications and checks if they stack or not
        let addDamageBoni =
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
            |> fun bonus -> if (Array.contains (PowerAttack char.BAB) modifications) = true 
                                && weapon.Modifier.MultiplicatorOnDamage.Hand = TwoHanded
                            then float bonus + ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) 
                                 |> int
                            else bonus

        /// Sums up all different boni to damage
        let getDamage = 
            addDamageMod + addWeaponDamage + addDamageBoni
            |> fun x -> if x <= 0 then 1 else x

        /////
        if (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined = [||]
            then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage!" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType
        elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined = [||] 
            then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)!" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier
        elif (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined <> [||]
            then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage %s !" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType (extraDamageToString extraDamageCombined)
        elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined <> [||] 
            then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)(%s on a crit) / (%s when not confirmed) !" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier (extraDamageToString extraDamageCombined) (extraDamageToString getExtraDamageOnHit) 
            else printfn "You should not see this message, please open an issue with your input as a bug report"