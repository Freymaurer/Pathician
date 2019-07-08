namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.LibraryModifications
open CoreFunctions.FullAttack

module DamagePerRound =

    module AuxDPRFunctions =

        type StatisticalAverages =
            | Mean
            | Mode
            | Median
        
        type StatisticalValues = {
            Mean    : float
            Median  : float
            Mode    : float
            }
        
        type ArmorClassTypes =
            | ArmorClass
            | FlatFootedArmor
            | TouchArmor
        
        let createStatisticalValues mean median mode = {
             Mean   = mean
             Median = median
             Mode   = mode
            }
        
        type CRData = {
            CR              : int
            AC              : StatisticalValues
            TouchAC         : StatisticalValues
            FlatFootedAC    : StatisticalValues
            AverageHitPoints: StatisticalValues
            NumberOfEntrys  : int
            }
        
        let createCRData cr acMean acMedian acMode flatMean flatMedian flatMode touchMean touchMedian touchMode hpMean hpMedian hpMode nOfEntrys= {
            CR                  = cr
            AC                  = createStatisticalValues acMean acMedian acMode
            TouchAC             = createStatisticalValues touchMean touchMedian touchMode
            FlatFootedAC        = createStatisticalValues flatMean flatMedian flatMode
            AverageHitPoints    = createStatisticalValues hpMean hpMedian hpMode
            NumberOfEntrys      = nOfEntrys
            }
        
        /// Gets information from .tsv file. Should be redone with deedle framework!
        let getCrData (armor: string [][]) (hitpoints: string [][]) (cr:int) = 
            let tryParseIntOrReplace (str:string)=
                let parsedString = System.Int32.TryParse str
                if fst parsedString = true
                then snd parsedString
                else 0   
            let tryParseFloatOrReplace (str:string)=
                let parsedString = System.Single.TryParse str
                if fst parsedString = true
                then float str
                else 0. 
            let crAvgArmor = armor.[cr-1]
            let crAvgHP = hitpoints.[cr-1]
            createCRData (tryParseIntOrReplace crAvgArmor.[0]) // Cr
                         (tryParseFloatOrReplace crAvgArmor.[6]) // acMean
                         (tryParseFloatOrReplace crAvgArmor.[7]) // acMedian
                         (tryParseFloatOrReplace crAvgArmor.[8]) // acMode 
                         (tryParseFloatOrReplace crAvgArmor.[14]) // flatMean
                         (tryParseFloatOrReplace crAvgArmor.[15]) // flatMedian
                         (tryParseFloatOrReplace crAvgArmor.[16]) // flatMode
                         (tryParseFloatOrReplace crAvgArmor.[22]) // touchMean
                         (tryParseFloatOrReplace crAvgArmor.[23]) // touchMedian
                         (tryParseFloatOrReplace crAvgArmor.[24]) // touchMode
                         (tryParseFloatOrReplace crAvgHP.[22]) // hpMean
                         (tryParseFloatOrReplace crAvgHP.[23]) // hpMedian
                         (tryParseFloatOrReplace crAvgHP.[24]) // hpMode
                         (tryParseIntOrReplace crAvgArmor.[1]) // number of entrys

        let getValue (triple:(float*DamageTypes*string)) = 
            triple |> fun (value,dType,string) -> value
        let getDmgType (triple:(float*DamageTypes*string)) = 
            triple |> fun (value,dType,string) -> dType
        let getName (triple:(float*DamageTypes*string)) = 
            triple |> fun (value,dType,string) -> string

    open AuxDPRFunctions
    open System.IO
    open CoreFunctions.AuxCoreFunctions
    open CoreFunctions.OneAttack

    let myStandardAttackDPR (char: CharacterStats) (size: SizeType) (weapon: Weapon) (modifications: AttackModification []) (cr:int) (targetedArmorType:ArmorClassTypes) (statisticType: StatisticalAverages) filePath=
        
        /// normally should use relative path directly injected in function below, but then QuickStart users would have problems
        let crRelatedMonsterInfo = 
            //let baseDirectory = __SOURCE_DIRECTORY__
            //let baseDirectory' = Directory.GetParent(baseDirectory)
            //let baseDirectory'' = Directory.GetParent(baseDirectory'.FullName)
            //let filePath = "Pathfinder Bestiary with Statistics - Statistics.tsv"
            //let fullPath = Path.Combine(baseDirectory''.FullName, filePath)
            let file = File.ReadAllLines(filePath)
            let make2D = file 
                         |> Array.map (fun x -> x.Split('\t'))
            let armor = make2D.[5..34]
            let perceptionHitDieHitPoints = make2D.[104..133]
            //let attackCMDCMB = make2D.[38..67]
            //let saves = make2D.[71..100]
            getCrData armor perceptionHitDieHitPoints cr
    
        /// gets information about monster hp
        let hpInfo =
            crRelatedMonsterInfo.AverageHitPoints
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
        /// gets information about monster ac
        let armorInfo =
            crRelatedMonsterInfo
            |> fun x -> if x.CR <> cr then failwith "Found cr does not match wanted cr, pls open an issue with your input and this error message."
                        match targetedArmorType with
                        | ArmorClass        -> x.AC
                        | FlatFootedArmor   -> x.FlatFootedAC
                        | TouchArmor        -> x.TouchAC
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
        /// calculates size changes due to modifications and applies them to the start size
        let calculatedSize =
            calculateSize size modifications
        
        /// calculates size bonus to attack rolls (eg. +1 for small)
        let sizeBonusToAttack =
            toHit.addSizeBonus calculatedSize

        /// calculates bonus on attack rolls due to the ability score used by the weapon
        let abilityModBonusToAttack =        
            toHit.getUsedModifierToHit char weapon modifications
        
        /// calculates all boni to attack rolls from modifications and checks if they stack or not
        let modBoniToAttack = 
            toHit.addModBoniToAttack modifications
             
        /// Sums up all different boni to attack rolls
        let combinedAttackBoni =
            char.BAB + weapon.BonusAttackRolls + abilityModBonusToAttack + modBoniToAttack + sizeBonusToAttack
        
        /// Not necessary but kept for simplicity and overview reasons
        let totalAttackBonus =
            float combinedAttackBoni
        
        /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
        let totalAttackCritBonus =
            toHit.getTotalAttackCritBonus modifications combinedAttackBoni
            |> float
        
        /// Calculation of all propabilites for hits/crits/threatened crits
        let (propabilityToHit,propabilitytoCrit,propabilityToConfirmCrit) =
            let critConfirmChance =
                // 1 always misses and 20 always hits, so for the missing 20 here, later there will be an unconditional +1 for hits
                [|2. .. 19.|]
                |> Array.map (fun x -> x + totalAttackCritBonus)
                |> Array.filter (fun x -> x >= armorInfo)
                // hits divided by number of possible rolls
                |> fun x -> (float x.Length+1.)/20.
            let critChance =
                weapon.CriticalRange
                |> Array.map (fun x -> float x + totalAttackBonus)
                |> Array.filter (fun x -> x >= armorInfo)
                |> fun x -> if Array.isEmpty x then (1./20.) else (float x.Length/20.)
            let hitChance = 
                // 1 always misses and 20 always hits, so for the missing 20 here, later there will be an unconditional +1 for hits
                let rollsWithoutCrit = [|2. .. 19.|] |> Array.filter (fun x -> (Array.contains (int x) weapon.CriticalRange) = false)
                rollsWithoutCrit
                |> Array.map (fun x -> x + totalAttackBonus)
                |> Array.filter (fun x -> x >= armorInfo)
                // hits divided by number of possible rolls
                |> fun x -> (float x.Length)/20.
            hitChance,critChance,critConfirmChance
    
        /// calculates stat changes due to modifications
        let statChangesToDmg =
            toDmg.getStatChangesToDmg weapon modifications
            
        /// calculates bonus on damage rolls due to the ability score used by the weapon and the related multiplied
        let abilityModBoniToDmg =
            toDmg.addDamageMod char weapon statChangesToDmg
            |> fun x -> x * weapon.Modifier.MultiplicatorOnDamage.Multiplicator |> floor |> int
        
        /// calculates size change and resizes weapon damage dice.
        let sizeAdjustedWeaponDamage =
            toDmg.adjustWeaponDamage size weapon.Damage.Die weapon.Damage.NumberOfDie modifications
            |> fun (n,die) -> createDamage n die weapon.Damage.DamageType

        /// Rolls dice for resized weapon damage dice
        let addWeaponDamage = 
            float sizeAdjustedWeaponDamage.NumberOfDie * ((float sizeAdjustedWeaponDamage.Die+1.)/2.)
            |> fun damageDice -> damageDice + float weapon.DamageBonus
        
        /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
        let extraDamageOnHit = 
            CoreFunctions.OneAttack.toDmg.getExtraDamageOnHit weapon modifications sizeAdjustedWeaponDamage getAvgDmg
    
        /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
        let extraDamageOnCrit = 
            CoreFunctions.OneAttack.toDmg.getExtraDamageOnCrit -20 weapon modifications sizeAdjustedWeaponDamage getAvgDmg
        
        /// calculates the extra dmg values for hits,critical threats and critical hits
        let (avgExtraDmgOnHit, avgExtraDmgOnThreatenedCrit, avgExtraDmgOnConfirmedCrit) =
            let sumOfHitAndCrit = 
                Array.map2 (fun onHit onCrit -> getValue onHit + getValue onCrit, getDmgType onHit, getName onCrit) extraDamageOnHit extraDamageOnCrit
            let onHit =
                extraDamageOnHit
                |> Array.map (fun (value,dmgType,name) -> value * propabilityToHit ,dmgType,name)
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
            let dmgFromThreatenedCrit =
                extraDamageOnHit
                |> Array.map (fun (value,dmgType,name) -> value * (propabilitytoCrit * (1.-propabilityToConfirmCrit)) ,dmgType,name )
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
            let dmgFromConfirmedCrit =
                sumOfHitAndCrit
                |> Array.map (fun (value,dmgType,name) -> value * (propabilitytoCrit * propabilityToConfirmCrit) ,dmgType,name )
                |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
            onHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
        
        /// combines all values from function above for "extraDamageToString" function
        let extraDmgCombined =
            Array.map3 (fun x y z -> getValue x + getValue y + getValue z, getDmgType x, getName x) avgExtraDmgOnHit avgExtraDmgOnThreatenedCrit avgExtraDmgOnConfirmedCrit
    
        /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something 
        let extraDamageToString = 
            extraDmgCombined
            |> Array.map (fun (value,dType,name) -> (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
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
            float abilityModBoniToDmg + addWeaponDamage + float addDamageBoni
            |> fun x -> if x <= 0. then 1. else x
        

        /// function to easily extract dmg value from extraDamage
        let extraDmgValue (extraDmg:(float*DamageTypes*string)[]) = 
            extraDmg
            |> Array.map (fun (x,_,_) -> x)
            |> Array.sum
    
        /// adds up damage values with propabilites to hit, separated in on hit, on crit, on threatened but not confirmed crit
        let (dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit) = 
            let dmgFromHit = (propabilityToHit * getDamage) 
                             + extraDmgValue avgExtraDmgOnHit
            let dmgFromThreatenedCrit = ((propabilitytoCrit*(1.-propabilityToConfirmCrit)) * getDamage) 
                                        + extraDmgValue avgExtraDmgOnThreatenedCrit
            let dmgFromConfirmedCrit = ((propabilitytoCrit*propabilityToConfirmCrit) * (getDamage*float weapon.CriticalModifier)) 
                                       + extraDmgValue avgExtraDmgOnConfirmedCrit
            dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
    
        let avgDmg =
            dmgFromHit + dmgFromThreatenedCrit + dmgFromConfirmedCrit
        
        ///
        if extraDamageOnHit = [||] && extraDamageOnCrit = [||]
            then printfn "You hit the enemy for an average of %s damage, the average enemy has %s hp (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits) !" (string avgDmg) (string hpInfo) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit)
        elif extraDamageOnHit <> [||] || extraDamageOnCrit <> [||]
            then printfn "You hit the enemy for an average of %s damage, the average enemy has %s hp (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits (%s)) !" (string avgDmg) (string hpInfo) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit) extraDamageToString

    ///This function returns the output of a full round attack action based on the used character stats, weapons and modifications.
    ///Weapons need an additional WeaponType: PrimaryMain for the weapon which should be used with things like haste, Primary for Primary natural attacks or two weapon fighting, and Secondary for secondary natural attacks.
    let myFullAttackDPR (char: CharacterStats) (size :SizeType) (weapons: (Weapon * WeaponType) []) (modifications: AttackModification []) (cr:int) (targetedArmorType:ArmorClassTypes) (statisticType: StatisticalAverages) filePath=
    
        /// normally should use relative path directly injected in function below, but then QuickStart users would have problems
        let crRelatedMonsterInfo = 
            //let baseDirectory = __SOURCE_DIRECTORY__
            //let baseDirectory' = Directory.GetParent(baseDirectory)
            //let baseDirectory'' = Directory.GetParent(baseDirectory'.FullName)
            //let filePath = "Pathfinder Bestiary with Statistics - Statistics.tsv"
            //let fullPath = Path.Combine(baseDirectory''.FullName, filePath)
            let file = File.ReadAllLines(filePath)
            let make2D = file 
                         |> Array.map (fun x -> x.Split('\t'))
            let armor = make2D.[5..34]
            let perceptionHitDieHitPoints = make2D.[104..133]
            //let attackCMDCMB = make2D.[38..67]
            //let saves = make2D.[71..100]
            getCrData armor perceptionHitDieHitPoints cr
    
        /// gets information about monster hp
        let hpInfo =
            crRelatedMonsterInfo.AverageHitPoints
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
        /// gets information about monster ac
        let armorInfo =
            crRelatedMonsterInfo
            |> fun x -> if x.CR <> cr then failwith "Found cr does not match wanted cr, pls open an issue with your input and this error message."
                        match targetedArmorType with
                        | ArmorClass        -> x.AC
                        | FlatFootedArmor   -> x.FlatFootedAC
                        | TouchArmor        -> x.TouchAC
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
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
            getBonusAttacksFor PrimaryMain modifications
            |> fun x -> if x = 0 then [||] else Array.create (x+1) 0
    
        // is absolutly necessary to produce any attacks with "Primary" weapon(?). gives back array similiar to extra BAB in the style of x+1 [|0; 5; 10|]
        let bonusAttacksForPrimary = 
            getBonusAttacksFor Primary modifications
            |> fun x -> if x = 0 then [||] else [|1 .. 1 .. x|]
            |> Array.map (fun x -> int ( (float x-1.) * 5.) )
    
        // Base Attack Array of all attacks but without related modifications
        let baseAttackArray =
            getAttackArray weapons babExtraAttacks bonusAttacksForPrimaryMain bonusAttacksForPrimary
    
        // Get AttackModifications, that are active on ALL attacks, for All weapons.
        let getAttackModificationsForAll = 
            modifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo = -20)
            |> fun x -> x
        // Get AttackModifications, that are active on a limited number of attacks, for All weapons.
        let getAttackModificationsForAllLimited =
            modifications
            |> Array.filter (fun x -> Array.contains All (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited baseAttackArray.Length
    
        // adds all modifications from "getAttackModificationsForAll" and "getAttackModificationsForAllLimited"
        let addAllAttackModificationsForAll = 
            appendModificationsForLimited getAttackModificationsForAllLimited getAttackModificationsForAll
    
        // adds all modifications for All weapons to the several weapon attacks from "getAttackArray"
        let addAllUnspecifcModificationsToAttackArray =
            addAllAttackModificationsForAll
            |> Array.zip baseAttackArray
            |> Array.map (fun ((x,y,z),arr) -> x,y,z,arr)
    
        ///begin calculating Secondary specific modifications
        let getNumberOfSecondaryAttacks =
            baseAttackArray
            |> Array.filter (fun (w,wType,modi) -> wType = Secondary)
            |> fun x -> x.Length
        let getSecondaryAttackModificationsForAll =
            modifications
            |> Array.filter (fun x -> Array.contains Secondary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getSecondaryAttackModificationsLimited =
            modifications
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
            modifications
            |> Array.filter (fun x -> Array.contains Primary (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryAttackModificationsLimited =
            modifications
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
            modifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo = -20)
        let getPrimaryMainAttackModificationsLimited =
            modifications
            |> Array.filter (fun x -> Array.contains PrimaryMain (fst x.AppliedTo) && snd x.AppliedTo <> -20)
            |> filterForLimited getNumberOfPrimaryMainAttacks

        let combinedPrimaryMainAttackModifications =
            appendModificationsForLimited getPrimaryMainAttackModificationsLimited getPrimaryMainAttackModificationsForAll
    


        /// This is the final attack array with all modifications sorted by their number of applications and their related WeaponType(s)
        let finalAttackArr =
            appendModificationsForSpecific addAllUnspecifcModificationsToAttackArray combinedPrimaryMainAttackModifications combinedPrimaryAttackModifications combinedSecondaryAttackModifications
    
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
   
            let totalAttackBonus =
                float combinedAttackBoni
    
            /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
            let totalAttackCritBonus =
                toHit.getTotalAttackCritBonus modifications combinedAttackBoni
                |> float
    
            /// Calculation of all propabilites for hits/crits/threatened crits
            let (propabilityToHit,propabilitytoCrit,propabilityToConfirmCrit) =
                let critConfirmChance =
                    // 1 always misses and 20 always hits, so for the missing 20 here, later there will be an unconditional +1 for hits
                    [|2. .. 19.|]
                    |> Array.map (fun x -> x + totalAttackCritBonus)
                    |> Array.filter (fun x -> x >= armorInfo)
                    // hits divided by number of possible rolls
                    |> fun x -> (float x.Length+1.)/20.
                let critChance =
                    weapon.CriticalRange
                    |> Array.map (fun x -> float x + totalAttackBonus)
                    |> Array.filter (fun x -> x >= armorInfo)
                    |> fun x -> if Array.isEmpty x then (1./20.) else (float x.Length/20.)
                let hitChance = 
                    // 1 always misses and 20 always hits, so for the missing 20 here, later there will be an unconditional +1 for hits
                    let rollsWithoutCrit = [|2. .. 19.|] |> Array.filter (fun x -> (Array.contains (int x) weapon.CriticalRange) = false)
                    rollsWithoutCrit
                    |> Array.map (fun x -> x + totalAttackBonus)
                    |> Array.filter (fun x -> x >= armorInfo)
                    // hits divided by number of possible rolls
                    |> fun x -> (float x.Length)/20.
                hitChance,critChance,critConfirmChance
    
    
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
    
            /// takes average for weapon dice rolls for resized weapon damage dice
            let addWeaponDamage = 
                float sizeAdjustedWeaponDamage.NumberOfDie * ((float sizeAdjustedWeaponDamage.Die+1.)/2.)
                |> fun damageDice -> damageDice + float weapon.DamageBonus
    
            /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
            let extraDamageOnHit = 
                CoreFunctions.OneAttack.toDmg.getExtraDamageOnHit weapon modifications sizeAdjustedWeaponDamage getAvgDmg
    
        
            /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst) 
            let extraDamageOnCrit = 
                CoreFunctions.OneAttack.toDmg.getExtraDamageOnCrit -20 weapon modifications sizeAdjustedWeaponDamage getAvgDmg
            
            /// calculates the extra dmg values for hits,critical threats and critical hits
            let (avgExtraDmgOnHit, avgExtraDmgOnThreatenedCrit, avgExtraDmgOnConfirmedCrit) =
                let sumOfHitAndCrit = 
                    Array.map2 (fun onHit onCrit -> getValue onHit + getValue onCrit, getDmgType onHit, getName onCrit) extraDamageOnHit extraDamageOnCrit
                let onHit =
                    extraDamageOnHit
                    |> Array.map (fun (value,dmgType,name) -> value * propabilityToHit ,dmgType,name)
                    |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
                let dmgFromThreatenedCrit =
                    extraDamageOnHit
                    |> Array.map (fun (value,dmgType,name) -> value * (propabilitytoCrit * (1.-propabilityToConfirmCrit)) ,dmgType,name )
                    |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
                let dmgFromConfirmedCrit =
                    sumOfHitAndCrit
                    |> Array.map (fun (value,dmgType,name) -> value * (propabilitytoCrit * propabilityToConfirmCrit) ,dmgType,name )
                    |> Array.filter (fun (bonus,dType,str) -> (bonus,dType) <> (0.,Untyped) && (bonus,dType) <> (0.,VitalStrikeDamage) )
                onHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
            
            /// combines all values from function above for "extraDamageToString" function
            let extraDmgCombined =
                Array.map3 (fun x y z -> getValue x + getValue y + getValue z, getDmgType x, getName x) avgExtraDmgOnHit avgExtraDmgOnThreatenedCrit avgExtraDmgOnConfirmedCrit
    
            /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something
            let extraDamageToString = 
                extraDmgCombined
                |> Array.map (fun (value,dType,name) -> (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
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
                float abilityModBoniToDmg + addWeaponDamage + float modBoniToDmg
                |> fun x -> if x <= 0. then 1. else x
    
            /// function to easily extract dmg value from extraDamage
            let extraDmgValue (extraDmg:(float*DamageTypes*string)[]) = 
                extraDmg
                |> Array.map (fun (x,_,_) -> x)
                |> Array.sum
        
            /// adds up damage values with propabilites to hit, separated in on hit, on crit, on threatened but not confirmed crit
            let (dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit) = 
                let dmgFromHit = (propabilityToHit * totalDamage) 
                                 + extraDmgValue avgExtraDmgOnHit
                let dmgFromThreatenedCrit = ((propabilitytoCrit*(1.-propabilityToConfirmCrit)) * totalDamage) 
                                            + extraDmgValue avgExtraDmgOnThreatenedCrit
                let dmgFromConfirmedCrit = ((propabilitytoCrit*propabilityToConfirmCrit) * (totalDamage*float weapon.CriticalModifier)) 
                                           + extraDmgValue avgExtraDmgOnConfirmedCrit
                dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
        
            let avgDmg =
                dmgFromHit + dmgFromThreatenedCrit + dmgFromConfirmedCrit
    
            ///
            if extraDamageOnHit = [||] && extraDamageOnCrit = [||]
            then (printfn "You hit the enemy for an average of %s damage (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits) !" (string avgDmg) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit))
                 avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
            elif extraDamageOnHit <> [||] || extraDamageOnCrit <> [||]
            then (printfn "You hit the enemy for an average of %s damage (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits (%s)) !" (string avgDmg) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit) extraDamageToString)
                 avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
            else failwith "How did you even get this?"
                 
        finalAttackArr
        |> Array.map (fun (w,wType,modi,modArr) -> getOneAttack w wType modi modArr)
        |> fun quadruple -> Array.fold (fun acc (x,y,z,w) -> x + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> y + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> z + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> w + acc) 0. quadruple
        |> fun (avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit) -> printfn "Your combined damage per round is %s damage, the average enemy has %s hp (%s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits)" (string avgDmg) (string hpInfo) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit)


