namespace PathfinderAttackSimulator

open Library
open Library.AuxLibFunctions
open LibraryModifications
open StandardAttackAction

open CoreFunctions
open AuxCoreFunctions

/// This module contains the calculator for full-round attack actions "myFullAttack"
module FullRoundAttackAction =
    open CoreFunctions.OneAttack

    
    ///This function returns the output of a full round attack action based on the used character stats, weapons and modifications.
    ///Weapons need an additional WeaponType: PrimaryMain for the weapon which should be used with things like haste, Primary for Primary natural attacks or two weapon fighting, and Secondary for secondary natural attacks.
    let myFullAttack (char: CharacterStats) (size :SizeType) (weapons: (Weapon * WeaponType) []) (allModifications: AttackModification []) =
        
        /// creates bonus attacks due to BAB and yields them in an array of boni of 0, 5, 10
        let calculateBabExtraAttacks =
            floor ( (float char.BAB - 1.)/5. )
            |> int
            |> fun x -> if x <= 0 then 0 else x
            |> fun x -> [|1 .. 1 .. (x+1)|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        // checks for additional attacks for primaryMain weapon, gives back an array of zero.
        // If there are no additional attacks due to modifications then  an empty array is given back
        // else its an array of x+1 additional attacks (e.g. haste gives [| 0 ; 0 |])
        let getBonusAttacksForPrimaryMain = 
            allModifications
            |> Array.map (fun x -> x.BonusAttacks)
            |> Array.filter (fun bAttacks -> bAttacks.WeaponTypeWithBonusAttacks = PrimaryMain)
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
            |> fun x -> if x = 0 then [||] else Array.create (x+1) 0
    
        // is absolutly necessary to produce any attacks with "Primary" weapon. gives back array similiar to extra BAB in the style of x+1 [|0; 5; 10|]
        let getAttacksForPrimary = 
            allModifications
            |> Array.map (fun x -> x.BonusAttacks)
            |> Array.filter (fun bAttacks -> bAttacks.WeaponTypeWithBonusAttacks = Primary)
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
                                               else failwith "Unknown WeaponType-pattern; pls contact support."
                          )
            |> Array.concat
            |> Array.map (fun (w, wType, modifier) -> if wType = PrimaryMain
                                                      then calculateBabExtraAttacks |> Array.map (fun x -> w,wType, modifier-x)
                                                      else [|w,wType,modifier|]
                         )
            |> Array.concat
            |> fun arr -> if (Array.contains PrimaryMain (Array.map (fun (w,wType) -> wType) weapons)
                             ) = true
                          then if getBonusAttacksForPrimaryMain <> [||]
                               then ( Array.map (fun (w, wType, modifier) -> if wType = PrimaryMain && modifier = 0
                                                                             then getBonusAttacksForPrimaryMain |> Array.map (fun x -> w,wType, modifier)
                                                                             else [|w,wType,modifier|]
                                                ) arr 
                                    )|> Array.concat
                               elif getBonusAttacksForPrimaryMain = [||] 
                               then arr
                               else failwith "Unknown Problem related to Bonus Attacks from Modifications for PrimaryMain; pls contact support."
                          elif (Array.contains PrimaryMain (Array.map (fun (w,wType) -> wType) weapons)
                               ) = false &&
                               (Array.contains Natural (Array.map (fun ((w: Weapon),wType) -> w.ManufacturedOrNatural) weapons)
                               ) = true
                          then if getBonusAttacksForPrimaryMain <> [||]
                                        /// filter for primary weapon
                               then (arr |> Array.head
                                            |> fun (w, wType, modifier) -> if wType = Primary && modifier = 0 && w.ManufacturedOrNatural = Natural
                                                                           then getBonusAttacksForPrimaryMain.[0 .. getBonusAttacksForPrimaryMain.Length-2]
                                                                                 |> Array.map (fun x -> w,wType, modifier)
                                                                           else [|w,wType,modifier|]
                                       )|> fun x -> Array.append x arr                                       
                               elif getBonusAttacksForPrimaryMain = [||] 
                                  then arr
                               else failwith "Unknown Problem related to Bonus Attacks from Modifications for PrimaryMain; pls contact support."
                          else failwith "Unknown Problem related to not having the right WeaponTypes"
            |> fun arr -> if getAttacksForPrimary <> [||]
                          then (Array.map (fun (w, wType, modifier) -> if wType = Primary 
                                                                         then getAttacksForPrimary |> Array.map (fun x -> w,wType, modifier-x)
                                                                         else [|w,wType,modifier|]
                                            ) arr 
                                 )|> Array.concat
                          elif getAttacksForPrimary = [||]  
                               && (Array.contains Natural (Array.map (fun (x,y) -> x.ManufacturedOrNatural) weapons)) = false
                          then arr |> Array.filter (fun (w,wType,modifier) -> wType <> Primary)
                          elif getAttacksForPrimary = [||]
                               && (Array.contains Natural (Array.map (fun (x,y) -> x.ManufacturedOrNatural) weapons)) = true
                          then arr
                          else failwith "Unknown Problem related to Primary Weapons (two-Weapon-Fighting); pls contact support.)"
            |> Array.sortByDescending (fun (w,wType,modi) -> modi )
    
        ///Get all AttackModifications constant for All weapons
        let getAttackModificationsForAll = 
            allModifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo = -20)
            |> fun x -> x

        ///Get all AttackModifications with limited numbers for All weapons
        let getAttackModificationsForAllLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getAttackArray.Length
                                           then (Array.create getAttackArray.Length arr),getAttackArray.Length
                                           elif int <= getAttackArray.Length
                                           then (Array.create int arr),int
                                           else failwith "Unknown Problem related to Limited Attack Modifiers added to All Attacks; pls contact support" 
                         )
            |> fun arr -> if arr <> [||]
                          then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getAttackArray.Length-int) ZeroMod 
                                                                                         )
                                          ) arr
                               )
                          elif arr = [||]
                          then [|Array.create getAttackArray.Length ZeroMod|]
                          else failwith "Unknown Problem related to limited attack modifiers for all weapons; pls contact support)"
    
        ///adds all modifications from "getAttackModificationsForAll" and "getAttackModificationsForAllLimited"
        let addAllAttackModificationsForAll =
            getAttackModificationsForAllLimited
            |> Array.map (fun x -> Array.mapi (fun i x -> i, x) x)
            |> Array.concat
            |> Array.groupBy (fun x -> fst x)
            |> Array.map (fun (header,tuple) -> tuple)
            |> Array.map (fun x -> Array.map (fun x -> snd x) x)
            |> Array.map (fun arr -> Array.append getAttackModificationsForAll arr)
    
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
            allModifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getSecondaryAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map ( fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfSecondaryAttacks 
                                           then (Array.create getNumberOfSecondaryAttacks arr),getNumberOfSecondaryAttacks
                                           elif int <= getNumberOfSecondaryAttacks
                                           then (Array.create int arr),int
                                           else failwith "Unknown Problem related to Limited Attack Modifiers added to Secondary Attacks; pls contact support" 
                         )
            |> fun arr -> if arr <> [||]
                          then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfSecondaryAttacks-int) ZeroMod 
                                                                                         )
                                          ) arr
                               ) 
                          elif arr = [||]
                          then [|Array.create getNumberOfSecondaryAttacks ZeroMod|]
                          else failwith "Unknown Problem related to limited attack modifiers for Secondary weapons; pls contact support"
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
            allModifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfPrimaryAttacks 
                                           then (Array.create getNumberOfPrimaryAttacks arr),getNumberOfPrimaryAttacks
                                           elif int <= getNumberOfPrimaryAttacks
                                           then (Array.create int arr),int
                                           else failwith "Unknown Problem related to Limited Attack Modifiers added to Primary Attacks; pls contact support" 
                         )
            |> fun arr -> if arr <> [||]
                          then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfPrimaryAttacks-int) ZeroMod 
                                                                                         )
                                          ) arr
                               ) |> fun x -> x
                          elif arr = [||]
                          then [|Array.create getNumberOfPrimaryAttacks ZeroMod|]
                          else failwith "Unknown Problem related to limited attack modifiers for Primary weapons; pls contact support"
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
            allModifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryMainAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> Array.map (fun x -> x, snd x.AppliedTo)
            |> Array.map (fun (arr,int) -> if int > getNumberOfPrimaryMainAttacks 
                                           then (Array.create getNumberOfPrimaryMainAttacks arr),getNumberOfPrimaryMainAttacks
                                           elif int <= getNumberOfPrimaryMainAttacks
                                           then (Array.create int arr),int
                                           else failwith "Unknown Problem related to Limited Attack Modifiers added to PrimaryMain Attacks; pls contact support" 
                         )
            |> fun arr -> if arr <> [||]
                          then (Array.map (fun (attackArr,int) -> Array.append attackArr (Array.create (getNumberOfPrimaryMainAttacks-int) ZeroMod 
                                                                                         )
                                          ) arr
                               ) |> fun x -> x
                          elif arr = [||]
                          then [|Array.create getNumberOfPrimaryMainAttacks ZeroMod|]
                          else failwith "Unknown Problem related to limited attack modifiers for PrimaryMain weapons; pls contact support"
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
                                             | PrimaryMain  -> arr 
                                                               |> Array.zip addAllPrimaryMainAttackModifications
                                                               |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | Primary      -> arr 
                                                               |> Array.zip addAllPrimaryAttackModifications
                                                               |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | Secondary    -> arr 
                                                               |> Array.zip addAllSecondaryAttackModifications
                                                               |> Array.map (fun (arr1,(w,wType,modi,modArr)) -> w,wType,modi, Array.append arr1 modArr)
                                             | _ -> failwith "Unknown Problem related to adding weaponType specific modifiers; pls contact support"
                         )
            |> Array.concat
    

        ///get One Attack per Attack Array, this is really similiar to the standard attack action!
        let getOneAttack (weapon: Weapon) (wType: WeaponType) (iterativeModifier: int) (modifications: AttackModification []) =
    
            /// calculates size changes due to modifications and applies them to the start size
            let calculatedSize =
                calculateSize size modifications

            /// calculates size bonus to attack rolls (eg. +1 for small)
            let sizeBonusToAttack =
                toHit.addSizeBonus calculatedSize
    
            /// calculates bonus on attack rolls due to the ability score used by the weapon
            let abilityModBoniToAttack =
                toHit.getUsedModifierToHit char weapon modifications
    
            /// calculates all boni to attack rolls from modifications and checks if they stack or not
            let modBoniToAttack = 
                toHit.addBoniToAttack modifications 
                
            /// Sums up all different boni to attack rolls
            let combinedAttackBoni =
                char.BAB + weapon.BonusAttackRolls + abilityModBoniToAttack + modBoniToAttack + sizeBonusToAttack + iterativeModifier
    
            /// rolls two dice; one for the regular hit and one for a possible crit confirmation roll
            let (attackRoll,critConfirmationRoll) = 
                let getAttackRolls =
                        rollDice 10000 20
                getRndArrElement getAttackRolls,getRndArrElement getAttackRolls
    
            /// complete bonus on attack = dice roll + Sum of all boni (getBonusToAttack)
            let totalAttackBonus =
                attackRoll + combinedAttackBoni
    
            /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
            let totalAttackCritBonus =
                toHit.getTotalAttackCritBonus modifications combinedAttackBoni
                |> (+) critConfirmationRoll

       //////////////// start with damage calculation ///////////////////////////////////////////////////
 
    
            /// rolls dice for weapon
            let getDamageRolls die =
                rollDice 100000 die
    
            /// calculates stat changes due to modifications
            let statChangesToDmg =
                toDmg.getStatChangesToDmg weapon modifications
    
            /// calculates size change and resizes weapon damage dice.
            let abilityModBoniToDmg =
                toDmg.addDamageMod char weapon statChangesToDmg
                |> fun modifier -> if Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) 
                                      && (wType = Primary || wType = Secondary)
                                   then (modifier * 0.5) |> floor |> int
                                   elif Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) 
                                        && wType = PrimaryMain
                                   then (modifier * weapon.Modifier.MultiplicatorOnDamage.Multiplicator) |> floor |> int
                                   elif Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons) = false 
                                        && wType = Primary
                                   then (modifier * weapon.Modifier.MultiplicatorOnDamage.Multiplicator) |> floor |> int
                                   else failwith "Unknown Weapon Combination to know if off-hand or not"

            /// calculates size change and resizes weapon damage dice.       
            let sizeAdjustedWeaponDamage =
                toDmg.adjustWeaponDamage size weapon.Damage.Die weapon.Damage.NumberOfDie modifications
                |> fun (n,die) -> createDamage n die weapon.Damage.DamageType
    
            /// Rolls dice for resized weapon damage dice
            let addWeaponDamage = 
                let rec getRandRoll listOfRolls=
                    (getRndArrElement (getDamageRolls sizeAdjustedWeaponDamage.Die) )::listOfRolls
                    |> fun rollList -> if rollList.Length >= (sizeAdjustedWeaponDamage.NumberOfDie)
                                       then rollList
                                       else getRandRoll rollList
                getRandRoll [] |> List.toArray |> Array.sum
                |> fun damageDice -> damageDice + weapon.DamageBonus
    
            /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
            let extraDamageOnHit = 
                toDmg.getExtraDamageOnHit weapon modifications sizeAdjustedWeaponDamage
    
            /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
            let extraDamageOnCrit = 
                toDmg.getExtraDamageOnCrit attackRoll weapon modifications sizeAdjustedWeaponDamage
            
            /// combines the extra damage and the extra damage on crit
            let extraDamageCombined =
                toDmg.combineExtraDamage extraDamageOnHit extraDamageOnCrit

            /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something
            let extraDamageToString extraDmgArr= 
                extraDmgArr
                |> Array.map (fun (value,dType,name) -> "+" + (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
                |> Array.fold (fun strArr x -> strArr + x) "" 
                |> fun x -> x.TrimEnd [|' ';','|]       
                
            /// calculates all boni to damage rolls from modifications and checks if they stack or not
            let modBoniToDmg =
                toDmg.addDamageBoni modifications
                |> fun bonus -> if (Array.contains (PowerAttack char.BAB) modifications) = true && 
                                        weapon.Modifier.MultiplicatorOnDamage.Hand = TwoHanded &&
                                        wType = PrimaryMain
                                then float bonus + ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                elif (Array.contains (PowerAttack char.BAB) modifications) = true 
                                     && Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons)
                                     && (wType = Primary || wType = Secondary)
                                then float bonus - ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                else bonus
            
            /// Sums up all different boni to damage
            let getDamage = 
                abilityModBoniToDmg + addWeaponDamage + modBoniToDmg
                |> fun x -> if x <= 0 then 1 else x
    
            if (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined = [||]
                then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage!" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType
            elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined = [||] 
                then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)!" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier
            elif (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined <> [||]
                then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage %s !" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType (extraDamageToString extraDamageCombined)
            elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined <> [||] 
                then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)(%s on a crit) / (%s when not confirmed) !" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier (extraDamageToString extraDamageCombined) (extraDamageToString extraDamageOnHit) 
                else printfn "You should not see this message, please open an issue with your input as a bug report"
     
        ///Maps through the created array of different attacks and produces one result each
        addAllWeaponTypeSpecificModifications
        |> Array.map (fun (w,wType,modi,modArr) -> getOneAttack w wType modi modArr)