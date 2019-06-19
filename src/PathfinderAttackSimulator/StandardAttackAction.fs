namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.LibraryModifications
open CoreFunctions.AuxCoreFunctions
open CoreFunctions.OneAttack.toHit
open CoreFunctions.OneAttack.toDmg

/// This module contains some smaller helper functions for all attack calculator functions and also the calculator for standard attack actions "myStandardAttack"
module StandardAttackAction =

    /// This function returns the output of a standard attack action based on the used character stats, size, weapons and modifications.
    let myStandardAttack (char: CharacterStats) (size: SizeType) (weapon: Weapon) (modifications: AttackModification []) =
    
        /// calculates size changes due to modifications and applies them to the start size
        let calculatedSize =
            calculateSize size modifications

        /// calculates size bonus to attack rolls (eg. +1 for small)
        let sizeBonusToAttack =    
            addSizeBonus calculatedSize

        /// calculates bonus on attack rolls due to the ability score used by the weapon
        let abilityModBoniToAttack =
            getUsedModifierToHit char weapon modifications
    
        /// calculates all boni to attack rolls from modifications and checks if they stack or not
        let modBoniToAttack = 
            addBoniToAttack modifications
        
        /// Sums up all different boni to attack rolls
        let combinedAttackBoni =
            char.BAB + weapon.BonusAttackRolls + abilityModBoniToAttack + modBoniToAttack + sizeBonusToAttack
   
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
            getTotalAttackCritBonus modifications combinedAttackBoni
            |> (+) critConfirmationRoll
    

    //////////////// start with damage calculation ///////////////////////////////////////////////////


        /// rolls dice for weapon
        let getDamageRolls die =
            rollDice 100000 die
    
        let statChangesToDmg =
            getStatChangesToDmg weapon modifications

        /// calculates stat changes due to modifications
        let abilityModBoniToDmg =
            addDamageMod char weapon statChangesToDmg
            |> fun x -> x * weapon.Modifier.MultiplicatorOnDamage.Multiplicator 
            |> floor 
            |> int
    
        /// calculates size change and resizes weapon damage dice.
        let sizeAdjustedWeaponDamage =            
            adjustWeaponDamage size weapon.Damage.Die weapon.Damage.NumberOfDie modifications
            |> fun (n,die) -> createDamage n die weapon.Damage.DamageType

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
        let extraDamageOnHit = 
            getExtraDamageOnHit weapon modifications sizeAdjustedWeaponDamage

        /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
        let extraDamageOnCrit = 
            getExtraDamageOnCrit attackRoll weapon modifications sizeAdjustedWeaponDamage


        /// combines the extra damage and the extra damage on crit
        let extraDamageCombined =
            combineExtraDamage extraDamageOnHit extraDamageOnCrit

        /// calculates all boni to damage rolls from modifications and checks if they stack or not
        let modBoniToDmg =
            addDamageBoni modifications
            |> fun bonus -> if (Array.contains (PowerAttack char.BAB) modifications) = true 
                                && weapon.Modifier.MultiplicatorOnDamage.Hand = TwoHanded
                            then float bonus + ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) 
                                 |> int
                            else bonus

        /// Sums up all different boni to damage
        let getDamage = 
            abilityModBoniToDmg + addWeaponDamage + modBoniToDmg
            |> fun x -> if x <= 0 then 1 else x

        /////
        if (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined = [||]
            then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage!" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType
        elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined = [||] 
            then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)!" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier
        elif (Array.contains attackRoll weapon.CriticalRange) = false && extraDamageCombined <> [||]
            then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i %A damage %s !" weapon.Name totalAttackBonus attackRoll getDamage weapon.Damage.DamageType (extraDamageToString extraDamageCombined)
        elif (Array.contains attackRoll weapon.CriticalRange) = true && extraDamageCombined <> [||] 
            then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i %A damage (x %i)(%s on a crit) / (%s when not confirmed) !" weapon.Name totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll getDamage weapon.Damage.DamageType weapon.CriticalModifier (extraDamageToString extraDamageCombined) (extraDamageToString extraDamageOnHit) 
            else printfn "You should not see this message, please open an issue with your input as a bug report"