namespace PathfinderAttackSimulator

open Library
open Library.Modifications
open StandardAttackAction


module FullRoundAttackAction =

    ///This function returns the output of a full round attack action based on the used character stats, weapons and modifications.
    ///Weapons need an additional WeaponType: PrimaryMain for the weapon which should be used with things like haste, Primary for Primary natural attacks or two weapon fighting, and Secondary for secondary natural attacks.
    let myFullAttack (char: CharacterStats) (weapons: (Weapon * WeaponType) []) (modifications: AttackModification []) =
    
        // erzeugt extra Angriffe durch BAB und gibt Array mit extra attacks als boni von 0, 5, 10 wieder
        let calculateBabExtraAttacks =
            floor ( (float char.BAB - 1.)/5. )
            |> int
            |> fun x -> [|1 .. 1 .. (x+1)|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        // checks for additional attacks for primaryMain weapon, gives back an array of zero.
        // If there are no additional attacks due to modifications then  an empty array is given back
        // else its an array of x+1 additional attacks (e.g. haste gives [| 0 ; 0 |])
        let getBonusAttacksForPrimaryMain = 
            modifications
            |> Array.map (fun x -> x.BonusAttacks)
            |> Array.filter (fun bAttacks -> bAttacks.WeaponTypeWithBonusAttacks = PrimaryMain)
            |> Array.groupBy (fun x -> x.TypeOfBonusAttacks)
            |> Array.map (fun (bTypes,bAttacks) -> if bTypes <> Flat 
                                                    then bAttacks
                                                            |> Array.sortByDescending (fun x -> x.NumberOfBonusAttacks) 
                                                            |> fun x -> Array.head x
                                                            |> fun x -> x.NumberOfBonusAttacks
                                                    else bAttacks
                                                            |> Array.map (fun x -> x.NumberOfBonusAttacks)
                                                            |> Array.sum
                         )
            |> Array.sum
            |> fun x -> if x = 0 then [||] else Array.create (x+1) 0
    
    
        // is absolutly necessary to produce any attacks with "Primary" weapon(?). gives back array similiar to extra BAB in the style of x+1 [|0; 5; 10|]
        let getAttacksForPrimary = 
            modifications
            |> Array.map (fun x -> x.BonusAttacks)
            |> Array.filter (fun bAttacks -> bAttacks.WeaponTypeWithBonusAttacks = Primary)
            |> Array.groupBy (fun x -> x.TypeOfBonusAttacks)
            |> Array.map (fun (bTypes,bAttacks) -> if bTypes <> Flat 
                                                    then bAttacks
                                                            |> Array.sortByDescending (fun x -> x.NumberOfBonusAttacks) 
                                                            |> fun x -> Array.head x
                                                            |> fun x -> x.NumberOfBonusAttacks
                                                    else bAttacks
                                                            |> Array.map (fun x -> x.NumberOfBonusAttacks)
                                                            |> Array.sum 
                         )
            |> Array.sum
            |> fun x -> if x = 0 then [||] else [|1 .. 1 .. x|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        //
        let getAttackArray =
            weapons
            |> Array.groupBy (fun (weap,wType) -> wType)
            |> Array.map (fun (wType,tuple) -> if wType = Primary || wType = PrimaryMain
                                                    then Array.map ( fun (weap,wType) -> (weap,wType, 0) ) tuple
                                               elif wType = Secondary
                                                    then Array.map ( fun (weap,wType) -> (weap,wType, -5) ) tuple
                                                    else failwith "Unknown WeaponType-pattern; pls contact Kevin"
                          )
            |> Array.concat
            |> Array.map (fun (w, wType, modifier) -> if wType = PrimaryMain
                                                            then calculateBabExtraAttacks |> Array.map (fun x -> w,wType, modifier-x)
                                                            else [|w,wType,modifier|]
                         )
            |> Array.concat
            |> fun arr -> if (Array.contains PrimaryMain (Array.map (fun (w,wType) -> wType) weapons
                                                         )
                             ) = true
                            then if getBonusAttacksForPrimaryMain <> [||]
                                    then ( Array.map (fun (w, wType, modifier) -> if wType = PrimaryMain && modifier = 0
                                                                                    then getBonusAttacksForPrimaryMain |> Array.map (fun x -> w,wType, modifier)
                                                                                    else [|w,wType,modifier|]
                                                     ) arr 
                                         )|> Array.concat
                                 elif getBonusAttacksForPrimaryMain = [||] 
                                    then arr
                                    else failwith "Unknown Problem related to Bonus Attacks from Modifications for PrimaryMain; pls contact Kevin"
                          elif (Array.contains PrimaryMain (Array.map (fun (w,wType) -> wType) weapons
                                                           )
                               ) = false &&
                               (Array.contains "NATURAL" (Array.map (fun ((w: Weapon),wType) -> w.ManufacturedOrNatural) weapons
                                                         )
                               ) = true
                                then if getBonusAttacksForPrimaryMain <> [||]
                                        /// filter for primary weapon
                                        then (arr |> Array.head
                                                  |> fun (w, wType, modifier) -> if wType = Primary && modifier = 0 && w.ManufacturedOrNatural = "NATURAL"
                                                                                              then getBonusAttacksForPrimaryMain.[0 .. getBonusAttacksForPrimaryMain.Length-2]
                                                                                                    |> Array.map (fun x -> w,wType, modifier)
                                                                                              else [|w,wType,modifier|]
                                             )|> fun x -> Array.append x arr                                       
                                     elif getBonusAttacksForPrimaryMain = [||] 
                                        then arr
                                     else failwith "Unknown Problem related to Bonus Attacks from Modifications for PrimaryMain; pls contact Kevin"
                                else failwith "Unknown Problem related to not having the right WeaponTypes"
            |> fun arr -> if getAttacksForPrimary <> [||]
                            then (Array.map (fun (w, wType, modifier) -> if wType = Primary 
                                                                                then getAttacksForPrimary |> Array.map (fun x -> w,wType, modifier-x)
                                                                                else [|w,wType,modifier|]
                                            ) arr 
                                 )|> Array.concat
                          //// hier weitermachen
                          elif getAttacksForPrimary = [||]  
                               && (Array.contains "NATURAL" (Array.map (fun (x,y) -> x.ManufacturedOrNatural) weapons)) = false
                            then arr |> Array.filter (fun (w,wType,modifier) -> wType <> Primary)
                          elif getAttacksForPrimary = [||]
                               && (Array.contains "NATURAL" (Array.map (fun (x,y) -> x.ManufacturedOrNatural) weapons)) = true
                            then arr
                            else failwith "Unknown Problem related to Primary Weapons (two-Weapon-Fighting); pls contact Kevin)"
            |> Array.sortByDescending (fun (w,wType,modi) -> modi )
    
        ///Get all AttackModifications constant for All weapons
        let getAttackModificationsForAll = 
            modifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo = -20)
            |> fun x -> x
        ///Get all AttackModifications with limited numbers for All weapons
        let getAttackModificationsForAllLimited =
            modifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getAttackArray.Length
                                                then (Array.create getAttackArray.Length arr),getAttackArray.Length
                                           elif int <= getAttackArray.Length
                                                then (Array.create int arr),int
                                                else failwith "Unknown Problem related to Limited Attack Modifiers added to All Attacks; pls contact Kevin" 
                         )
            |> fun arr -> if arr <> [||]
                                then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getAttackArray.Length-int) ZeroMod 
                                                                                               )
                                                ) arr
                                     ) |> fun x -> x
                          elif arr = [||]
                                then [|Array.create getAttackArray.Length ZeroMod|]
                                else failwith "Unknown Problem related to limited attack modifiers for all weapons; pls contact Kevin)"
            |> fun x -> x
        ///adds all modifications from "getAttackModificationsForAll" and "getAttackModificationsForAllLimited"
        let addAllAttackModificationsForAll =
            getAttackModificationsForAllLimited
            |> Array.map (fun x -> Array.mapi (fun i x -> i, x) x)
            |> Array.concat
            |> Array.groupBy (fun x -> fst x)
            |> Array.map (fun (header,tuple) -> tuple)
            |> Array.map (fun x -> Array.map (fun x -> snd x) x)
            |> Array.map (fun arr -> Array.append getAttackModificationsForAll arr)
            |> fun x -> x 
        ///adds all modifications for All weapons to the several weapon attacks from "getAttackArray"
        let addAllUnspecifcModificationsToAttackArray =
            addAllAttackModificationsForAll
            |> Array.zip getAttackArray
            |> Array.map (fun ((x,y,z),arr) -> x,y,z,arr)
    
        ///begin calculating Secondary specific modifications
        let getNumberOfSecondaryAttacks =
            getAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = Secondary)
            |> fun x -> x.Length
        let getSecondaryAttackModificationsForAll =
            modifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getSecondaryAttackModificationsLimited =
            modifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map ( fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfSecondaryAttacks 
                                                then (Array.create getNumberOfSecondaryAttacks arr),getNumberOfSecondaryAttacks
                                           elif int <= getNumberOfSecondaryAttacks
                                                then (Array.create int arr),int
                                                else failwith "Unknown Problem related to Limited Attack Modifiers added to Secondary Attacks; pls contact Kevin" 
                         )
            |> fun arr -> if arr <> [||]
                                then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfSecondaryAttacks-int) ZeroMod 
                                                                                               )
                                                ) arr
                                     ) 
                          elif arr = [||]
                                then [|Array.create getNumberOfSecondaryAttacks ZeroMod|]
                                else failwith "Unknown Problem related to limited attack modifiers for Secondary weapons; pls contact Kevin"
        let addAllSecondaryAttackModifications =
            getSecondaryAttackModificationsLimited
            |> Array.map (fun x -> Array.mapi (fun i x ->i, x )x )
            |> Array.concat
            |> Array.groupBy (fun x -> fst x)
            |> Array.map (fun (header,tuple) -> tuple)
            |> Array.map (fun x ->Array.map (fun x -> snd x) x)
            |> Array.map (fun arr -> Array.append getSecondaryAttackModificationsForAll arr)
        
        ///begin calculating Primary specific modifications
        let getNumberOfPrimaryAttacks =
            getAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = Primary)
            |> fun x -> x.Length
        let getPrimaryAttackModificationsForAll =
            modifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryAttackModificationsLimited =
            modifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfPrimaryAttacks 
                                                then (Array.create getNumberOfPrimaryAttacks arr),getNumberOfPrimaryAttacks
                                           elif int <= getNumberOfPrimaryAttacks
                                                then (Array.create int arr),int
                                                else failwith "Unknown Problem related to Limited Attack Modifiers added to Primary Attacks; pls contact Kevin" 
                         )
            |> fun arr -> if arr <> [||]
                                then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfPrimaryAttacks-int) ZeroMod 
                                                                                               )
                                                ) arr
                                     ) |> fun x -> x
                          elif arr = [||]
                                then [|Array.create getNumberOfPrimaryAttacks ZeroMod|]
                                else failwith "Unknown Problem related to limited attack modifiers for Primary weapons; pls contact Kevin"
        let addAllPrimaryAttackModifications =
            getPrimaryAttackModificationsLimited
            |> Array.map (fun x -> Array.mapi (fun i x ->i, x )x )
            |> Array.concat
            |> Array.groupBy (fun x -> fst x)
            |> Array.map (fun (header,tuple) -> tuple)
            |> Array.map (fun x ->Array.map (fun x -> snd x) x)
            |> Array.map (fun arr -> Array.append getPrimaryAttackModificationsForAll arr)
    
        ///begin calculating PrimaryMain specific modifications
        let getNumberOfPrimaryMainAttacks =
            getAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = PrimaryMain)
            |> fun x -> x.Length
        let getPrimaryMainAttackModificationsForAll =
            modifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryMainAttackModificationsLimited =
            modifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfPrimaryMainAttacks 
                                                then (Array.create getNumberOfPrimaryMainAttacks arr),getNumberOfPrimaryMainAttacks
                                           elif int <= getNumberOfPrimaryMainAttacks
                                                then (Array.create int arr),int
                                                else failwith "Unknown Problem related to Limited Attack Modifiers added to PrimaryMain Attacks; pls contact Kevin" 
                         )
            |> fun arr -> if arr <> [||]
                                then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfPrimaryMainAttacks-int) ZeroMod 
                                                                                               )
                                                ) arr
                                     ) |> fun x -> x
                          elif arr = [||]
                                then [|Array.create getNumberOfPrimaryMainAttacks ZeroMod|]
                                else failwith "Unknown Problem related to limited attack modifiers for PrimaryMain weapons; pls contact Kevin"
        let addAllPrimaryMainAttackModifications =
            getPrimaryMainAttackModificationsLimited
            |> Array.map (fun x -> Array.mapi (fun i x ->i, x )x )
            |> Array.concat
            |> Array.groupBy (fun x -> fst x)
            |> Array.map (fun (header,tuple) -> tuple)
            |> Array.map (fun x ->Array.map (fun x -> snd x) x)
            |> Array.map (fun arr -> Array.append getPrimaryMainAttackModificationsForAll arr)
    
        let addAllWeaponTypeSpecificModifications =
            addAllUnspecifcModificationsToAttackArray
            |> Array.groupBy (fun (w,wType,modi,modArr) -> wType)
            |> Array.map (fun (wType,arr) -> match wType with
                                             | PrimaryMain -> arr 
                                                                |> Array.zip addAllPrimaryMainAttackModifications
                                                                |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | Primary -> arr 
                                                                |> Array.zip addAllPrimaryAttackModifications
                                                                |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | Secondary -> arr 
                                                                |> Array.zip addAllSecondaryAttackModifications
                                                                |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | _ -> failwith "Unknown Problem related to adding weaponType specific modifiers; pls contact Kevin"
                         )
            |> Array.concat
    
        ///get One Attack per Attack Array
        let getOneAttack (weapon: Weapon) (wType: WeaponType) (modifier: int) (modifications: AttackModification []) =
    
            ///
            let getUsedModifierToHit =
                match weapon.Modifier.ToHit with
                | "Str" -> char.Strength
                | "Dex" -> char.Dexterity
                | "Con" -> char.Constitution
                | "Int" -> char.Intelligence
                | "Wis" -> char.Wisdom
                | "Cha" -> char.Charisma
                | _ -> 0
    
            ///
            let getStatChangesToHit =
                modifications
                |> Array.collect (fun x -> x.StatChanges)
                |> Array.map (fun statChange -> if statChange.Attribute = weapon.Modifier.ToHit
                                                            then statChange
                                                            else createStatChange "0" 0 Flat
                             )
                |> Array.groupBy (fun statChange -> statChange.Bonustype)
                |> Array.map (fun (uselessHeader,x) -> x)
                ///nächster Schritt soll höchsten Statchange nehmen um nicht stackende boni auszusortieren. 
                ///Aber was wenn ein negativer und ein positiver des selben Typs exisiteren?
                |> Array.map (fun x -> Array.sortByDescending (fun statChange -> statChange.AttributeChange) x)
                |> Array.map (fun x -> Array.head x)
                |> Array.map (fun statChange -> statChange.AttributeChange)
                |> Array.sum
    
            ///
            let addBoniToAttack = 
                modifications 
                |> Array.map (fun x -> x.BonusAttackRoll)
                |> Array.groupBy (fun x -> x.BonusType)
                |> Array.map (fun (header,bonusArr) -> if header <> Flat 
                                                            then bonusArr
                                                                |> Array.sortByDescending (fun x -> x.Value) 
                                                                |> fun x -> Array.head x
                                                                |> fun x -> x.Value
                                                       elif header = Flat
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
                |> fun (x,y) -> x, Array.contains true y
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
                |> Array.map (fun statChange -> if statChange.Attribute = weapon.Modifier.ToDmg
                                                            then statChange
                                                            else createStatChange "0" 0 Flat
                             )
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
    
            let addDamageMod =
                if Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) && 
                                (wType = Primary || wType = Secondary)
                    then
                        match weapon.Modifier.ToDmg with
                            | "Str" -> char.Strength
                            | "Dex" -> char.Dexterity
                            | "Con" -> char.Constitution
                            | "Int" -> char.Intelligence
                            | "Wis" -> char.Wisdom
                            | "Cha" -> char.Charisma
                            | _ -> 0
                        |> fun stat -> ((float stat + getStatChangesToDmg) * 0.5) |> floor |> int
                elif Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) && 
                                wType = PrimaryMain
                    then 
                        match weapon.Modifier.ToDmg with
                            | "Str" -> char.Strength
                            | "Dex" -> char.Dexterity
                            | "Con" -> char.Constitution
                            | "Int" -> char.Intelligence
                            | "Wis" -> char.Wisdom
                            | "Cha" -> char.Charisma
                            | _ -> 0
                        |> fun stat -> ((float stat + getStatChangesToDmg) * weapon.Modifier.MultiplicatorOnDamage) |> floor |> int
                elif Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) = false && 
                                wType = Primary
                    then 
                        match weapon.Modifier.ToDmg with
                            | "Str" -> char.Strength
                            | "Dex" -> char.Dexterity
                            | "Con" -> char.Constitution
                            | "Int" -> char.Intelligence
                            | "Wis" -> char.Wisdom
                            | "Cha" -> char.Charisma
                            | _ -> 0
                        |> fun stat -> ((float stat + getStatChangesToDmg) * weapon.Modifier.MultiplicatorOnDamage) |> floor |> int
                    else failwith "Unknown Weapon Combination to know if off-hand or not"
                    
            let addWeaponDamage = 
                let rec getRandRoll listOfRolls=
                    (getRandArrElement (getDamageRolls weapon.Damage.Die) )::listOfRolls
                    |> fun rollList -> if rollList.Length >= (weapon.Damage.NumberOfDie)
                                            then rollList
                                            else getRandRoll rollList
                getRandRoll [] |> List.toArray |> Array.sum
                |> fun damageDice -> damageDice + weapon.DamageBonus
    
            let addDamageBoni =
                modifications
                |> Array.map (fun x -> x.BonusDamage)
                |> Array.groupBy (fun x -> x.BonusType)
                |> Array.map (fun (x,bonusArr) -> if x <> Flat 
                                                        then bonusArr
                                                                |> Array.sortByDescending (fun x -> x.Value) 
                                                                |> fun x -> x.[0]
                                                                |> fun x -> x.Value
                                                        else bonusArr
                                                                |> Array.map (fun x -> x.Value)
                                                                |> Array.sum                   
                              )
                |> Array.sum
                |> fun bonus -> if (Array.contains (PowerAttack char.BAB) modifications) = true && 
                                        weapon.Modifier.MultiplicatorOnDamage = 1.5 &&
                                        wType = PrimaryMain
                                    then float bonus + ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                elif (Array.contains (PowerAttack char.BAB) modifications) = true &&
                                        Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) &&
                                        (wType = Primary || wType = Secondary)
                                    then float bonus - ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                    else bonus
            let getDamage = 
                addDamageMod + addWeaponDamage + addDamageBoni
                |> fun x -> if x <= 0 then 1 else x
    
            if (calculateRolls |> fun (x,y,z,u) -> u) = -20 && getExtraDamage = [||]
                then printfn "Du greifst mit %s an und triffst den Gegner mit %i (gewürfelt %i) für %i %A Schaden!" weapon.Name (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) getDamage weapon.Damage.DamageType
            elif (calculateRolls |> fun (x,y,z,u) -> u) <> -20 && getExtraDamage = [||] 
                then printfn "Du greifst mit %s an und crittest (hoffentlich) den Gegner mit %i (gewürfelt %i) und bestätigst mit %i (gewürfelt %i) für %i %A Schaden (crit * %i)!" weapon.Name (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) (calculateRolls |> fun (x,y,z,u) -> z) (calculateRolls |> fun (x,y,z,u) -> u) getDamage weapon.Damage.DamageType weapon.CriticalModifier
            elif (calculateRolls |> fun (x,y,z,u) -> u) = -20 && getExtraDamage <> [||]
                then printfn "Du greifst mit %s an und triffst den Gegner mit %i (gewürfelt %i) für %i %A Schaden %s !" weapon.Name (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) getDamage weapon.Damage.DamageType extraDamageToString
            elif (calculateRolls |> fun (x,y,z,u) -> u) <> -20 && getExtraDamage <> [||] 
                then printfn "Du greifst mit %s an und crittest (hoffentlich) den Gegner mit %i (gewürfelt %i) und bestätigst mit %i (gewürfelt %i) für %i %A Schaden %s (crit * %i)!" weapon.Name (calculateRolls |> fun (x,y,z,u) -> x) (calculateRolls |> fun (x,y,z,u) -> y) (calculateRolls |> fun (x,y,z,u) -> z) (calculateRolls |> fun (x,y,z,u) -> u) getDamage weapon.Damage.DamageType extraDamageToString weapon.CriticalModifier
     
        addAllWeaponTypeSpecificModifications
        |> Array.map (fun (w,wType,modi,modArr) -> getOneAttack w wType modi modArr)