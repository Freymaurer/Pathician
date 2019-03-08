namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
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
    
    let myStandardAttack (char: CharacterStats) (weapon: Weapon) (modifications: AttackModification []) =
    
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
        let getBonusToAttack =
            char.BAB + weapon.BonusAttackRolls + getUsedModifierToHit + getStatChangesToHit + addBoniToAttack
    
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
    
        ///
        let addWeaponDamage = 
            let rec getRandRoll listOfRolls =
                (getRandArrElement (getDamageRolls weapon.Damage.Die) )::listOfRolls
                |> fun rollList -> if rollList.Length >= (weapon.Damage.NumberOfDie)
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
    