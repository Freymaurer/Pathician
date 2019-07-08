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
    open CoreFunctions.FullAttack
    
    ///This function returns the output of a full round attack action based on the used character stats, weapons and modifications.
    ///Weapons need an additional WeaponType: PrimaryMain for the weapon which should be used with things like haste, Primary for Primary natural attacks or two weapon fighting, and Secondary for secondary natural attacks.
    let myFullAttack (char: CharacterStats) (size :SizeType) (weapons: (Weapon * WeaponType) []) (allModifications: AttackModification []) =
        
        /// creates bonus attacks due to BAB and yields them in an array of boni of 0, 5, 10
        let babExtraAttacks =
            floor ( (float char.BAB - 1.)/5. )
            |> int
            |> fun x -> if x <= 0 then 0 else x
            |> fun x -> [|1 .. 1 .. (x+1)|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        // checks for additional attacks for primaryMain weapon, gives back an array of zero.
        // If there are no additional attacks due to modifications then  an empty array is given back
        // else its an array of x+1 additional attacks (e.g. haste gives [| 0 ; 0 |])
        let bonusAttacksForPrimaryMain = 
            getBonusAttacksFor PrimaryMain allModifications
            |> fun x -> if x = 0 then [||] else Array.create (x+1) 0
    
        // is absolutly necessary to produce any attacks with "Primary" weapon. gives back array similiar to extra BAB in the style of x+1 [|0; 5; 10|]
        let bonusAttacksForPrimary = 
            getBonusAttacksFor Primary allModifications
            |> fun x -> if x = 0 then [||] else [|1 .. 1 .. x|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        /// Base Attack Array of all attacks but without related modifications
        let baseAttackArray =
            getAttackArray weapons babExtraAttacks bonusAttacksForPrimaryMain bonusAttacksForPrimary
    
        // Get AttackModifications, that are active on ALL attacks, for All weapons.
        let getAttackModificationsForAll = 
            allModifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo = -20)
            |> fun x -> x

        // Get AttackModifications, that are active on a limited number of attacks, for All weapons.
        let getAttackModificationsForAllLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited baseAttackArray.Length
    
        // adds modifications from "getAttackModificationsForAll" and "getAttackModificationsForAllLimited"
        let addAllAttackModificationsForAll =
            appendModificationsForLimited getAttackModificationsForAllLimited getAttackModificationsForAll
    
        // adds all modifications for All weapons to the several weapon attacks from "getAttackArray"
        let addAllUnspecifcModificationsToAttackArray =
            addAllAttackModificationsForAll
            |> Array.zip baseAttackArray
            |> Array.map (fun ((x,y,z),arr) -> x,y,z,arr)
    
        // begin calculating Secondary specific modifications
        let getNumberOfSecondaryAttacks =
            baseAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = Secondary)
            |> fun x -> x.Length
        let getSecondaryAttackModificationsForAll =
            allModifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getSecondaryAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited getNumberOfSecondaryAttacks

        let combinedSecondaryAttackModifications =
            appendModificationsForLimited getSecondaryAttackModificationsLimited getSecondaryAttackModificationsForAll

        ///begin calculating Primary specific modifications
        let getNumberOfPrimaryAttacks =
            baseAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = Primary)
            |> fun x -> x.Length
        let getPrimaryAttackModificationsForAll =
            allModifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited getNumberOfPrimaryAttacks

        let combinedPrimaryAttackModifications =
            appendModificationsForLimited getPrimaryAttackModificationsLimited getPrimaryAttackModificationsForAll
    
        ///begin calculating PrimaryMain specific modifications
        let getNumberOfPrimaryMainAttacks =
            baseAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = PrimaryMain)
            |> fun x -> x.Length
        let getPrimaryMainAttackModificationsForAll =
            allModifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryMainAttackModificationsLimited =
            allModifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited getNumberOfPrimaryMainAttacks

        let combinedPrimaryMainAttackModifications =
            appendModificationsForLimited getPrimaryMainAttackModificationsLimited getPrimaryMainAttackModificationsForAll
    

        /// This is the final attack array with all modifications sorted by their number of applications and their related WeaponType(s)
        let finalAttackArr =
            appendModificationsForSpecific addAllUnspecifcModificationsToAttackArray combinedPrimaryMainAttackModifications combinedPrimaryAttackModifications combinedSecondaryAttackModifications


        //////////////////////////////////////// START ONE ATTACK ///////////////////////////////////////////////////////////////////////////
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
                toHit.addModBoniToAttack modifications 
                
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
                toDmg.getExtraDamageOnHit weapon modifications sizeAdjustedWeaponDamage getRandRolls
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
    
            /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
            let extraDamageOnCrit = 
                toDmg.getExtraDamageOnCrit attackRoll weapon modifications sizeAdjustedWeaponDamage getRandRolls
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
            
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
                toDmg.addModDamageBoni modifications
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
            let totalDamage = 
                abilityModBoniToDmg + addWeaponDamage + modBoniToDmg
                |> fun x -> if x <= 0 then 1 else x
    
            if (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined = [||]
                then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage!" weapon.Name totalAttackBonus attackRoll totalDamage weapon.Damage.DamageType
            elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined = [||] 
                then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)!" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage weapon.Damage.DamageType weapon.CriticalModifier
            elif (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined <> [||]
                then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage %s !" weapon.Name totalAttackBonus attackRoll totalDamage weapon.Damage.DamageType (extraDamageToString extraDamageCombined)
            elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined <> [||] 
                then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)(%s on a crit) / (%s when not confirmed) !" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage weapon.Damage.DamageType weapon.CriticalModifier (extraDamageToString extraDamageCombined) (extraDamageToString extraDamageOnHit) 
                else printfn "You should not see this message, please open an issue with your input as a bug report"
     
        ///Maps through the created array of different attacks and produces one result each
        finalAttackArr
        |> Array.map (fun (w,wType,modi,modArr) -> getOneAttack w wType modi modArr)