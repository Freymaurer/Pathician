namespace PathfinderAttackSimulator

open System
open PathfinderAttackSimulator.Library
open PathfinderAttackSimulator.Library.AuxLibFunctions
open PathfinderAttackSimulator.Library.Modifications

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

    let myStandardAttackDPR (char: CharacterStats) (size: SizeType) (weapon: Weapon) (modifications: AttackModification []) (cr:int) (targetedArmorType:ArmorClassTypes) (statisticType: StatisticalAverages) =
        
    
        let crRelatedMonsterInfo = 
            let baseDirectory = __SOURCE_DIRECTORY__
            let baseDirectory' = Directory.GetParent(baseDirectory)
            let baseDirectory'' = Directory.GetParent(baseDirectory'.FullName)
            let filePath = "Pathfinder Bestiary with Statistics - Statistics.tsv"
            let fullPath = Path.Combine(baseDirectory''.FullName, filePath)
            let file = File.ReadAllLines(fullPath)
            let make2D = file 
                         |> Array.map (fun x -> x.Split('\t'))
            let armor = make2D.[5..34]
            let perceptionHitDieHitPoints = make2D.[104..133]
            //let attackCMDCMB = make2D.[38..67]
            //let saves = make2D.[71..100]
            getCrData armor perceptionHitDieHitPoints cr
    
        let hpInfo =
            crRelatedMonsterInfo.AverageHitPoints
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
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
        
        
        ///
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
        
        ///
        let addSizeBonus =
            calculatedSize
            |> fun x -> Map.find x findSizes
            |> fun x -> x.SizeModifier
        
        ///
        let getBonusToAttack =
            char.BAB + weapon.BonusAttackRolls + getUsedModifierToHit + addBoniToAttack + addSizeBonus
        
        let totalAttackBonus =
            float getBonusToAttack
        
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
            float (getBonusToAttack + critSpecificBonus)
        
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
            
        ///
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
        
        ///
        let addWeaponDamage = 
            float sizeAdjustedWeaponDamage.NumberOfDie * ((float sizeAdjustedWeaponDamage.Die+1.)/2.)
            |> fun damageDice -> damageDice + float weapon.DamageBonus
        
        ///getRandRoll is not so good; try find something better
        let extraDamageOnHit = 
            let getAvgDmg die number =
                float number * ((float die+1.)/2.)
            [|weapon.ExtraDamage,weapon.Name|]
            |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage,x.Name) )
            |> Array.map (fun (extraD,str) -> getAvgDmg extraD.OnHit.Die extraD.OnHit.NumberOfDie
                                              , extraD.OnHit.DamageType, str
                         )
            |> fun x -> x
            // Vital Strike hardcode
            |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                           || x = VitalStrikeImproved 
                                                                           || x = VitalStrikeGreater) modifications)
                               then Array.filter (fun x -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                    |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                    |> Array.head
                                    |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                        yield getAvgDmg sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                    |> fun x -> x
                                    |> fun (floatArr,str) -> Array.sum floatArr, str
                                    |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                               else extraDmg
    
        ///getRandRoll is not so good; try find something better
        let extraDamageOnCrit = 
            let getAvgDmg die number =
                float number * ((float die+1.)/2.)
            [|weapon.ExtraDamage,weapon.Name|]
            |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage,x.Name) )
            |> Array.map (fun (extraD,str) -> getAvgDmg extraD.OnCrit.Die extraD.OnCrit.NumberOfDie
                                              , extraD.OnCrit.DamageType, str
                         )
            |> fun x -> x
            // Vital Strike hardcode
            |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                           || x = VitalStrikeImproved 
                                                                           || x = VitalStrikeGreater) modifications)
                               then Array.filter (fun x -> x.ExtraDamage.OnCrit.DamageType = VitalStrikeDamage) modifications
                                    |> Array.sortByDescending (fun x -> x.ExtraDamage.OnCrit.NumberOfDie)
                                    |> Array.head
                                    |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnCrit.NumberOfDie do
                                                        yield getAvgDmg sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                    |> fun x -> x
                                    |> fun (floatArr,str) -> Array.sum floatArr, str
                                    |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                               else extraDmg
        
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
    
        ///
        let extraDamageToString = 
            extraDmgCombined
            |> Array.map (fun (value,dType,name) -> (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
            |> Array.fold (fun strArr x -> strArr + x) "" 
            |> fun x -> x.TrimEnd [|' ';','|]         
        
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
            float addDamageMod + addWeaponDamage + float addDamageBoni
            |> fun x -> if x <= 0. then 1. else x
        

        /// function to easily extract dmg value from extraDamage
        let extraDmgValue (extraDmg:(float*DamageTypes*string)[]) = 
            extraDmg
            |> Array.map (fun (x,_,_) -> x)
            |> Array.sum
    
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
    let myFullAttackDPR (char: CharacterStats) (size :SizeType) (weapons: (Weapon * WeaponType) []) (modifications: AttackModification []) (cr:int) (targetedArmorType:ArmorClassTypes) (statisticType: StatisticalAverages) =
    
        let crRelatedMonsterInfo = 
            let baseDirectory = __SOURCE_DIRECTORY__
            let baseDirectory' = Directory.GetParent(baseDirectory)
            let baseDirectory'' = Directory.GetParent(baseDirectory'.FullName)
            let filePath = "Pathfinder Bestiary with Statistics - Statistics.tsv"
            let fullPath = Path.Combine(baseDirectory''.FullName, filePath)
            let file = File.ReadAllLines(fullPath)
            let make2D = file 
                         |> Array.map (fun x -> x.Split('\t'))
            let armor = make2D.[5..34]
            let perceptionHitDieHitPoints = make2D.[104..133]
            //let attackCMDCMB = make2D.[38..67]
            //let saves = make2D.[71..100]
            getCrData armor perceptionHitDieHitPoints cr
    
        let hpInfo =
            crRelatedMonsterInfo.AverageHitPoints
            |> fun x -> match statisticType with 
                        | Mean      -> x.Mean
                        | Median    -> x.Median
                        | Mode      -> x.Mode
    
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
    
        // erzeugt extra Angriffe durch BAB und gibt Array mit extra attacks als boni von 0, 5, 10 wieder
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
            modifications
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
    
        // is absolutly necessary to produce any attacks with "Primary" weapon(?). gives back array similiar to extra BAB in the style of x+1 [|0; 5; 10|]
        let getAttacksForPrimary = 
            modifications
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
                          //// hier weitermachen
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
    
        ///get One Attack per Attack Array
        let getOneAttack (weapon: Weapon) (wType: WeaponType) (iterativeModifier: int) (modifications: AttackModification []) =
    
            ///
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
    
            ///
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
    
            ///
            let addSizeBonus =
                calculatedSize
                |> fun x -> Map.find x findSizes
                |> fun x -> x.SizeModifier
    
            ///
            let getBonusToAttack =
                char.BAB + weapon.BonusAttackRolls + getUsedModifierToHit + addBoniToAttack + addSizeBonus + iterativeModifier
    
            let totalAttackBonus =
                float getBonusToAttack
    
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
                float (getBonusToAttack + critSpecificBonus)
    
            ///
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
    
            //
            let addDamageMod =
                (match weapon.Modifier.ToDmg with
                | Strength     -> char.Strength
                | Dexterity    -> char.Dexterity
                | Constitution -> char.Constitution
                | Intelligence -> char.Intelligence
                | Wisdom       -> char.Wisdom
                | Charisma     -> char.Charisma
                | _            -> 10
                    )
                |> fun stat -> float stat + getStatChangesToDmg
                |> fun stat -> floor ((stat - 10.)/2.)
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
                                                               else failwith "Unrecognized Pattern of sizeChangeBoni." 
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
                                             | 1                                                        -> reCalcWeapon.Damage.NumberOfDie, reCalcWeapon.Damage.Die
                                             | odd when isOdd reCalcWeapon.Damage.NumberOfDie = true    -> int (ceil (float reCalcWeapon.Damage.NumberOfDie/2.)), 6
                                             | even when isEven reCalcWeapon.Damage.NumberOfDie = true  -> (reCalcWeapon.Damage.NumberOfDie/2), 8
                                             | _                                                        -> failwith "unknown combination for reCalcWeapon damage dice calculator accoringly to size; Error4"
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
    
            let addWeaponDamage = 
                float sizeAdjustedWeaponDamage.NumberOfDie * ((float sizeAdjustedWeaponDamage.Die+1.)/2.)
                |> fun damageDice -> damageDice + float weapon.DamageBonus
    
            ///getRandRoll is not so good; try find something better
            let extraDamageOnHit = 
                let getAvgDmg die number =
                    float number * ((float die+1.)/2.)
                [|weapon.ExtraDamage,weapon.Name|]
                |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage,x.Name) )
                |> Array.map (fun (extraD,str) -> getAvgDmg extraD.OnHit.Die extraD.OnHit.NumberOfDie
                                                  , extraD.OnHit.DamageType, str
                             )
                |> fun x -> x
                // Vital Strike hardcode
                |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                               || x = VitalStrikeImproved 
                                                                               || x = VitalStrikeGreater) modifications)
                                   then Array.filter (fun x -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                        |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                        |> Array.head
                                        |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                            yield getAvgDmg sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                        |> fun x -> x
                                        |> fun (floatArr,str) -> Array.sum floatArr, str
                                        |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                                   else extraDmg
    
        
            ///getRandRoll is not so good; try find something better
            let extraDamageOnCrit = 
                let getAvgDmg die number =
                    float number * ((float die+1.)/2.)
                [|weapon.ExtraDamage,weapon.Name|]
                |> Array.append (modifications |> Array.map (fun x -> x.ExtraDamage,x.Name) )
                |> Array.map (fun (extraD,str) -> getAvgDmg extraD.OnCrit.Die extraD.OnCrit.NumberOfDie
                                                  , extraD.OnCrit.DamageType, str
                             )
                |> fun x -> x
                // Vital Strike hardcode
                |> fun extraDmg -> if Array.contains true (Array.map (fun x -> x = VitalStrike 
                                                                               || x = VitalStrikeImproved 
                                                                               || x = VitalStrikeGreater) modifications)
                                   then Array.filter (fun x -> x.ExtraDamage.OnCrit.DamageType = VitalStrikeDamage) modifications
                                        |> Array.sortByDescending (fun x -> x.ExtraDamage.OnCrit.NumberOfDie)
                                        |> Array.head
                                        |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnCrit.NumberOfDie do
                                                            yield getAvgDmg sizeAdjustedWeaponDamage.Die sizeAdjustedWeaponDamage.NumberOfDie|], vitalS.Name
                                        |> fun x -> x
                                        |> fun (floatArr,str) -> Array.sum floatArr, str
                                        |> fun (bonus,str) -> Array.append [|bonus,sizeAdjustedWeaponDamage.DamageType,str|] extraDmg
                                   else extraDmg
            
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
    
            ///
            let extraDamageToString = 
                extraDmgCombined
                |> Array.map (fun (value,dType,name) -> (string value) + " " + (string dType) + " " + "damage" + " (" + name + ")" + ", ")
                |> Array.fold (fun strArr x -> strArr + x) "" 
                |> fun x -> x.TrimEnd [|' ';','|]       
                
            let addDamageBoni =
                modifications
                |> Array.map (fun x -> x.BonusDamage)
                |> Array.groupBy (fun x -> x.BonusType)
                |> Array.map (fun (x,bonusArr) -> if x <> BonusTypes.Flat 
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
                                        weapon.Modifier.MultiplicatorOnDamage.Hand = TwoHanded &&
                                        wType = PrimaryMain
                                then float bonus + ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                elif (Array.contains (PowerAttack char.BAB) modifications) = true 
                                     && Array.contains PrimaryMain (Array.map (fun x -> snd x) weapons)
                                     && (wType = Primary || wType = Secondary)
                                then float bonus - ((float (PowerAttack char.BAB).BonusDamage.Value) * 0.5) |> int
                                else bonus
            ///
            let getDamage = 
                float addDamageMod + addWeaponDamage + float addDamageBoni
                |> fun x -> if x <= 0. then 1. else x
    
            /// function to easily extract dmg value from extraDamage
            let extraDmgValue (extraDmg:(float*DamageTypes*string)[]) = 
                extraDmg
                |> Array.map (fun (x,_,_) -> x)
                |> Array.sum
        
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
            then (printfn "You hit the enemy for an average of %s damage (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits) !" (string avgDmg) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit))
                 avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
            elif extraDamageOnHit <> [||] || extraDamageOnCrit <> [||]
            then (printfn "You hit the enemy for an average of %s damage (%s = attack roll bonus; %s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits (%s)) !" (string avgDmg) (string totalAttackBonus) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit) extraDamageToString)
                 avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit
            else failwith "How did you even get this?"
                 
        addAllWeaponTypeSpecificModifications
        |> Array.map (fun (w,wType,modi,modArr) -> getOneAttack w wType modi modArr)
        |> fun quadruple -> Array.fold (fun acc (x,y,z,w) -> x + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> y + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> z + acc) 0. quadruple, Array.fold (fun acc (x,y,z,w) -> w + acc) 0. quadruple
        |> fun (avgDmg, dmgFromHit, dmgFromThreatenedCrit, dmgFromConfirmedCrit) -> printfn "Your combined damage per round is %s damage, the average enemy has %s hp (%s damage from normal hits; %s damage from threatened crits; %s damage from confirmed crits)" (string avgDmg) (string hpInfo) (string dmgFromHit) (string dmgFromThreatenedCrit) (string dmgFromConfirmedCrit)


