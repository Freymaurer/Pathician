namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.Library.Modifications

module StandardAttackAction =
    ///Pathfinder Standard Attack Action////

    ////All dice generators////
    module AuxFunctions =

        let rollDice count (diceSides:int) =
            let rnd = System.Random()
            if diceSides = 0 
                then [|0|]
                else Array.init count (fun _ -> rnd.Next (1, diceSides+1))
        
        let getRandArrElement =
          let rnd = Random()
          fun (arr : int[]) -> arr.[rnd.Next(arr.Length)]
    
    open AuxFunctions
    
    let myStandardAttack (char: CharacterStats) (size: SizeType) (weapon: Weapon) (modifications: AttackModification []) =
    
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

        ///
        let getUsedModifierToHit =
            match weapon.Modifier.ToHit with
            | Strength -> char.Strength
            | Dexterity -> char.Dexterity
            | Constitution -> char.Constitution
            | Intelligence -> char.Intelligence
            | Wisdom -> char.Wisdom
            | Charisma -> char.Charisma
            | _ -> 0
    
        ///
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
    
        ///
        let addBoniToAttack = 
            modifications 
            |> Array.map (fun x -> x.BonusAttackRoll)
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
        
        ///
        let addSizeBonus =
            calculatedSize
            |> fun x -> Map.find x findSizes
            |> fun x -> x.SizeModifier
        
        ///
        let getBonusToAttack =
            char.BAB + weapon.BonusAttackRolls + getUsedModifierToHit + getStatChangesToHit + addBoniToAttack + addSizeBonus
    
        ///
        let getAttackRolls =
            rollDice 100000 20

        let calculateRolls =
            getRandArrElement getAttackRolls
            |> fun roll -> roll, Array.map (fun x -> roll = x) weapon.CriticalRange
            |> fun (x,y) -> x, Array.contains true y ///TODO: this can be done shorter
            |> fun (firstRoll,crit) -> firstRoll, if crit = true
                                                        then getRandArrElement getAttackRolls
                                                        else -20
            |> fun (firstRoll,crit) -> if crit <> -20 
                                            then firstRoll + getBonusToAttack, firstRoll, crit + getBonusToAttack, crit
                                            else firstRoll + getBonusToAttack, firstRoll,-20,-20
    
        ///
        let getDamageRolls die =
            rollDice 100000 die
    
        ///
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
    
        ///getRandRoll is not so good; try find something better
        let getExtraDamage = 
            let rec getRandRoll listOfRolls die number =
                (getRandArrElement (getDamageRolls die))::listOfRolls
                |> fun rollList -> if rollList.Length >= number
                                        then rollList
                                        else getRandRoll rollList die number
            weapon.ExtraDamage
            |> Array.create 1
            |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage) )
            |> Array.map (fun extraD -> getRandRoll [] extraD.Die extraD.NumberOfDie |> List.toArray |> Array.sum
                                                       , 
                                                       extraD.DamageType
                         )
            |> Array.groupBy snd
            |> Array.map snd
            |> Array.map Array.unzip
            |> Array.map (fun (value,dType) -> Array.sum value, Array.head dType)
            |> Array.filter (fun x -> x <> (0,Untyped))
    
        ///
        let extraDamageToString = 
            getExtraDamage
            |> Array.map (fun (value,types) -> "+" + (string value) + " " + (string types) + " " + "Schaden, ")
            |> Array.fold (fun strArr x -> strArr + x) "" 
            |> fun x -> x.TrimEnd [|' ';','|]      
            
        ///
        let addDamageMod =
            match weapon.Modifier.ToDmg with
                | Strength -> char.Strength
                | Dexterity -> char.Dexterity
                | Constitution -> char.Constitution
                | Intelligence -> char.Intelligence
                | Wisdom -> char.Wisdom
                | Charisma -> char.Charisma
                | _ -> 0
            |> fun stat -> ((float stat + getStatChangesToDmg) * weapon.Modifier.MultiplicatorOnDamage.Multiplicator) |> floor |> int
    
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
                                    | 2 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                    | 3 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                    | 4 -> match reCalcWeapon.Damage.NumberOfDie with
                                                    | 1 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                                    | odd when isOdd reCalcWeapon.Damage.NumberOfDie = true -> int (ceil (float reCalcWeapon.Damage.NumberOfDie/2.)), 6
                                                    | even when isEven reCalcWeapon.Damage.NumberOfDie = true -> (reCalcWeapon.Damage.NumberOfDie/2), 8
                                                    | _ -> failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error4"
                                    | 6 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                    | 8 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                    | 10 -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                    | 12 -> (reCalcWeapon.Damage.NumberOfDie*2), 6
                                    | 20 -> (reCalcWeapon.Damage.NumberOfDie*2), 10
                                    | _ -> if reCalcWeapon.Damage.Die % 10 = 0
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

        ///
        let addWeaponDamage = 
            let rec getRandRoll listOfRolls =
                (getRandArrElement (getDamageRolls sizeAdjustedWeaponDamage.Die) )::listOfRolls
                |> fun rollList -> if rollList.Length >= (sizeAdjustedWeaponDamage.NumberOfDie)
                                        then rollList
                                        else getRandRoll rollList
            getRandRoll [] |> List.toArray |> Array.sum
            |> fun damageDice -> damageDice + weapon.DamageBonus
    
        ///
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
        ///
        let getDamage = 
            addDamageMod + addWeaponDamage + addDamageBoni
            |> fun x -> if x <= 0 then 1 else x
        
        ///
        if (calculateRolls |> fun (x,y,z,u) -> u) = -20 && getExtraDamage = [||]
            then printfn "You hit the enemy with a %i (rolled %i) for %i %A damage!" (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) getDamage weapon.Damage.DamageType
        elif (calculateRolls |> fun (x,y,z,u) -> u) <> -20 && getExtraDamage = [||] 
            then printfn "You (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A Damage (crit * %i)!" (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) (calculateRolls |> fun (x,y,z,u) -> z) (calculateRolls |> fun (x,y,z,u) -> u) getDamage weapon.Damage.DamageType weapon.CriticalModifier
        elif (calculateRolls |> fun (x,y,z,u) -> u) = -20 && getExtraDamage <> [||]
            then printfn "You hit the enemy with a %i (rolled %i) for %i %A damage %s !" (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) getDamage weapon.Damage.DamageType extraDamageToString
        elif (calculateRolls |> fun (x,y,z,u) -> u) <> -20 && getExtraDamage <> [||] 
            then printfn ("You (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage %s (crit * %i)!") (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) (calculateRolls |> fun (x,y,z,u) -> z) (calculateRolls |> fun (x,y,z,u) -> u) getDamage weapon.Damage.DamageType extraDamageToString weapon.CriticalModifier
    