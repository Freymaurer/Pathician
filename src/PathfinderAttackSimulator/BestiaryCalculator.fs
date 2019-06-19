namespace PathfinderAttackSimulator

open System
open Library
open Library.AuxLibFunctions


/// This module contains both bestiary calculator functions for standard attacks "calculateStandardAttack" and full-round attacks "calculateFullAttack"
module BestiaryCalculator =
    
    open BestiaryReader.AuxFunctions
    open CoreFunctions.AuxCoreFunctions
    open CoreFunctions.OneAttack

    let private testForNaturalAttack (str:string) =
        let regexNaturalAttack = System.Text.RegularExpressions.Regex("(claw|vine|tentacle|bite|gore|hoof|wing|pincers|tail\sslap|slam|sting|talon|tongue)")
        regexNaturalAttack.IsMatch(str)

    /// Folds the damage values to a string to print as result. This allows to separate different damage types should a creature be immune to something
    let private extraDamageToString extraDmgArr= 
        extraDmgArr
        |> Array.map (fun (value,dType) -> "+" + (string value) + " " + (string dType) + " " + "damage"  + ", ")
        |> Array.fold (fun strArr x -> strArr + x) "" 
        |> fun x -> x.TrimEnd [|' ';','|]    

    /// this adds information to the result: stuff like grab, poison, etc.
    let getAdditionalInfoString attackAdditionalInfo =
        if attackAdditionalInfo = ""
        then ""
        else "plus " + attackAdditionalInfo

    /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
    let private getURLExtraDamageOnHit (modifications:AttackModification []) (sizeAdjustedWeaponDamage:URLDamage)=
        let getDamageRolls numberOfDie die=
            let rolledDice = rollDice 1000 die
            [|for i=1 to numberOfDie do
                yield getRndArrElement rolledDice|]
            |> Array.sum
        let extraDamageModifications =
            modifications 
            |> Array.map (fun x -> x.ExtraDamage.OnHit,x.Name)
            |> Array.map (fun (extraDmg,str) -> getDamageRolls extraDmg.NumberOfDie extraDmg.Die
                                                , extraDmg.DamageType, str
                         )
            ///Vital Strike hardcode
            |> fun extraDmg -> if (Array.exists (fun modi -> modi = LibraryModifications.VitalStrike
                                                             || modi = LibraryModifications.VitalStrikeImproved
                                                             || modi = LibraryModifications.VitalStrikeGreater) modifications 
                                  ) = true
                               then Array.filter (fun (x:AttackModification) -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                    |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                    |> Array.head
                                    |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                        yield getDamageRolls sizeAdjustedWeaponDamage.NumberOfDie sizeAdjustedWeaponDamage.Die|], vitalS.Name
                                    |> fun x -> x
                                    |> fun (intList,str) -> Array.sum intList, str
                                    |> fun (bonus,str) -> Array.append [|bonus,VitalStrikeDamage,str|] extraDmg
                               else extraDmg
            |> Array.map (fun (bonusValue,dmgType,modificationName) -> bonusValue,(string dmgType))
        //add weapon extra dmg (e.g. shocking enchantment) to modification extra dmg
        extraDamageModifications
        |> Array.filter (fun (extraDmgValue,dType) -> extraDmgValue <> 0 )

    /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst)
    let private getURLExtraDamageOnCrit (modifications:AttackModification []) sizeAdjustedWeaponDamage attackRoll wantedAttack=
        let getDamageRolls numberOfDie die=
            let rolledDice = rollDice 1000 die
            [|for i=1 to numberOfDie do
                yield getRndArrElement rolledDice|]
            |> Array.sum
        let extraDamageModifications =
            if (Array.contains attackRoll wantedAttack.CriticalRange) = false
            then [||]
            else modifications 
                 |> Array.map (fun x -> x.ExtraDamage.OnCrit,x.Name)
                 |> Array.map (fun (extraDmg,str) -> getDamageRolls extraDmg.NumberOfDie extraDmg.Die
                                                     , extraDmg.DamageType, str
                              )
                 ///Vital Strike hardcode
                 |> fun extraDmg -> if (Array.exists (fun modi -> modi = LibraryModifications.VitalStrike
                                                                  || modi = LibraryModifications.VitalStrikeImproved
                                                                  || modi = LibraryModifications.VitalStrikeGreater) modifications 
                                       ) = true
                                    then Array.filter (fun (x:AttackModification) -> x.ExtraDamage.OnHit.DamageType = VitalStrikeDamage) modifications
                                         |> Array.sortByDescending (fun x -> x.ExtraDamage.OnHit.NumberOfDie)
                                         |> Array.head
                                         |> fun vitalS -> [|for i in 1 .. vitalS.ExtraDamage.OnHit.NumberOfDie do
                                                             yield getDamageRolls sizeAdjustedWeaponDamage.NumberOfDie sizeAdjustedWeaponDamage.Die|], vitalS.Name
                                         |> fun x -> x
                                         |> fun (intList,str) -> Array.sum intList, str
                                         |> fun (bonus,str) -> Array.append [|bonus,VitalStrikeDamage,str|] extraDmg
                                    else extraDmg
                 |> Array.map (fun (bonusValue,dmgType,modificationName) -> bonusValue,(string dmgType))
        //add weapon extra dmg (e.g. shocking enchantment) to modification extra dmg
        extraDamageModifications
        |> Array.filter (fun (extraDmgValue,dType) -> extraDmgValue <> 0 )


    /// combines the extra damage and the extra damage on crit
    let private getURLExtraDamageCombined extraDamageOnHit extraDamageOnCrit urlAttack=
        let getDamageRolls numberOfDie die=
            let rolledDice = rollDice 1000 die
            [|for i=1 to numberOfDie do
                yield getRndArrElement rolledDice|]
            |> Array.sum
        if extraDamageOnCrit = [||]
        then extraDamageOnHit
            |> Array.append [|(getDamageRolls urlAttack.ExtraDamage.NumberOfDie urlAttack.ExtraDamage.Die, urlAttack.ExtraDamage.DamageType)|]
            |> Array.filter (fun (extraDmgValue,dType) -> extraDmgValue <> 0 )
        else Array.map2 (fun (onHit:(int*string)) (onCrit:(int*string)) -> (fst onHit) + (fst onCrit), snd onHit) extraDamageOnHit extraDamageOnCrit
            |> Array.append [|(getDamageRolls urlAttack.ExtraDamage.NumberOfDie urlAttack.ExtraDamage.Die, urlAttack.ExtraDamage.DamageType)|]
            |> Array.filter (fun (extraDmgValue,dType) -> extraDmgValue <> 0 ) 
     
    /// This function returns the calculated attack rolls of a d20pfsrd/archives of nethys bestiary entry.
    /// attackinfo = the output of the "getMonsterInformation" function, attackVariant = Melee/Ranged,
    /// attackNumber = the exact attack variant that should be calculated, starting at 1
    /// modifications = array of attackmodifications (StatChanges will not work for this function)
    let calculateStandardAttack (attackInfo: URLMonsterAttacks []) (attackVariant:AttackVariant) (attackNumber:int) (modifications: AttackModification []) =
        
        //check if there is actually the attack wanted by the user
        if attackNumber > attackInfo.Length then failwith "The chosen url does not provide enough different attacks for the attackNumber given. Try giving a smaller number or 1."
        let monsterStats = attackInfo.[0].RelevantMonsterStats
    
    // get the relevant informations about which monsterstats should be used /////////////////////////////

        //filter for either melee or ranged
        let wantedMonsterAttack =
            attackInfo
            |> Array.filter (fun x -> x.AttackType = attackVariant)
            |> fun x -> if x.Length = 0 
                        then failwith "The chosen url does not provide any attacks for the chosen attack type (melee or ranged)."
                        else x.[attackNumber-1]

    // get the relevant informations about which attackstats should be used /////////////////////////////

        let wantedAttack =
            wantedMonsterAttack
            |> fun x -> x.AttackScheme.[0]

    // start with attack boni information //////////////////////////////////////////////////////////////

        /// rolls two dice; one for the regular hit and one for a possible crit confirmation roll
        let (attackRoll,critConfirmationRoll) = 
            let getAttackRolls =
                    rollDice 10000 20
            getRndArrElement getAttackRolls,getRndArrElement getAttackRolls

        let startSize =
                match monsterStats.Size with
                | Fine          -> 1
                | Diminuitive   -> 2
                | Tiny          -> 3
                | Small         -> 4
                | Medium        -> 5
                | Large         -> 6
                | Huge          -> 7
                | Gargantuan    -> 8
                | Colossal      -> 9

        let calculatedSize =
            calculateSize monsterStats.Size modifications
    
        /// calculates size bonus to attack rolls (eg. +1 for small)
        let sizeBonusToAttack =
            let sizeModifierNew =
                calculatedSize
                |> fun x -> Map.find x findSizes
                |> fun x -> x.SizeModifier
            let sizeModifierOld =
                startSize
                |> fun x -> Map.find x findSizes
                |> fun x -> x.SizeModifier
            sizeModifierNew - sizeModifierOld


        /// calculates all boni to attack rolls from modifications and checks if they stack or not
        let modBoniToAttack = 
            toHit.addBoniToAttack modifications

        //Start adding up attack boni
        let attackBonus =
            wantedAttack
            |> fun x -> Array.max x.AttackBonus
    
        /// tries to remove possible mali from feats that only apply to fullround attack actions
        let attackBoniWithoutFullRoundFeats =
            let regexTwoWeaponFighting = System.Text.RegularExpressions.Regex("Two\WWeapon\sFighting")
            let regexRapidShot = System.Text.RegularExpressions.Regex("Rapid\sShot")
            let (|RapidShot|NoRapidShot|) (strArr:string []) = 
                if (Array.contains true (Array.map (fun str -> regexRapidShot.IsMatch(str)
                                                   ) strArr                                        
                                        ) 
                   ) = true
                   && attackVariant = Ranged
                then RapidShot
                else NoRapidShot
            let (|TwoWeaponFighting|NoTwoWeaponFighting|) (strArr:string []) = 
                if (Array.contains true (Array.map (fun str -> regexTwoWeaponFighting.IsMatch(str)
                                                   ) strArr
                                        ) 
                   ) = true
                   && wantedMonsterAttack.AttackScheme.Length > 1
                   && (testForNaturalAttack wantedAttack.WeaponName) = false
                then TwoWeaponFighting
                else NoTwoWeaponFighting
            let balanceTwoWeaponMalus =
                match monsterStats.Feats with
                | TwoWeaponFighting -> 2
                | NoTwoWeaponFighting -> 0
            let balanceRapidShotMalus =
                match monsterStats.Feats with
                | RapidShot -> 2
                | NoRapidShot -> 0
            balanceTwoWeaponMalus + balanceRapidShotMalus

        /// Sums up all different boni to attack rolls
        let combinedAttackBoni =
            attackBonus + modBoniToAttack + sizeBonusToAttack + attackBoniWithoutFullRoundFeats
    
        /// complete bonus on attack = dice roll + Sum of all boni (getBonusToAttack) 
        let totalAttackBonus =
            attackRoll + combinedAttackBoni

        /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
        let totalAttackCritBonus =
            toHit.getTotalAttackCritBonus modifications combinedAttackBoni
            |> (+) critConfirmationRoll
    
        /////End attack boni/Start damage boni/////
    
        /// calculates size change and resizes weapon damage dice. Cannot be changed to standard function, as it uses different input. Maybe change later to fit! TODO
        let sizeAdjustedWeaponDamage =
            toDmg.adjustWeaponDamage monsterStats.Size wantedAttack.WeaponDamage.Die wantedAttack.WeaponDamage.NumberOfDie modifications
            |> fun (n,die) -> createURLDamage n die  wantedAttack.WeaponDamage.BonusDamage wantedAttack.WeaponDamage.DamageType

        /// rolls dice for weapon
        let damageRolls =
            let getDamageRolls =
                rollDice 1000 sizeAdjustedWeaponDamage.Die
            [|for i=1 to sizeAdjustedWeaponDamage.NumberOfDie do
                yield getRndArrElement getDamageRolls|]
            |> Array.sum

        /// calculates bonus on damage rolls due to the ability score used by the weapon and the related multiplied
        let modBoniToDmg =
            toDmg.addDamageBoni modifications
    
        /// Sums up all different boni to damage
        let totalDamage =
            damageRolls + sizeAdjustedWeaponDamage.BonusDamage + modBoniToDmg
            |> fun x -> if x <= 0 then 1 else x
    
        /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
        let extraDamageOnHit =
            getURLExtraDamageOnHit modifications sizeAdjustedWeaponDamage
    
        /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst)
        let extraDamageOnCrit =
            getURLExtraDamageOnCrit modifications sizeAdjustedWeaponDamage attackRoll wantedAttack
            
        /// combines the extra damage and the extra damage on crit
        let extraDamageCombined =
            getURLExtraDamageCombined extraDamageOnHit extraDamageOnCrit wantedAttack

        let additionalInfoString =
            getAdditionalInfoString wantedAttack.AdditionalEffects

        if (Array.contains attackRoll wantedAttack.CriticalRange) = false && extraDamageCombined = [||]
        then printfn "You attack with a %s and hit with a %i (rolled %i) for %i damage %s!" wantedAttack.WeaponName totalAttackBonus attackRoll totalDamage additionalInfoString
        elif (Array.contains attackRoll wantedAttack.CriticalRange) = true && extraDamageCombined = [||] 
        then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i Damage (x %i) %s!" wantedAttack.WeaponName totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage wantedAttack.CriticalModifier additionalInfoString
        elif (Array.contains attackRoll wantedAttack.CriticalRange) = false && extraDamageCombined <> [||]
        then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i damage %s %s!" wantedAttack.WeaponName totalAttackBonus attackRoll totalDamage (extraDamageToString extraDamageCombined) additionalInfoString
        elif (Array.contains attackRoll wantedAttack.CriticalRange) = true && extraDamageCombined <> [||] 
        then printfn ("You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i damage (x %i) %s (%s on a crit) / (%s when not confirmed) !") wantedAttack.WeaponName totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage wantedAttack.CriticalModifier additionalInfoString (extraDamageToString extraDamageCombined) (extraDamageToString extraDamageOnHit)  
        else printfn "You should not see this message, please open an issue with your input as a bug report"

    /// This function returns the calculated attack rolls of a d20pfsrd/archives of nethys bestiary entry.
    /// attackinfo = the output of the "getMonsterInformation" function, attackVariant = Melee/Ranged,
    /// attackNumber = the exact attack variant that should be calculated, starting at 1
    /// modifications = array of attackmodifications (StatChanges will not work for this function)
    let calculateFullAttack (attackInfo: URLMonsterAttacks []) (attackVariant:AttackVariant) (attackNumber:int) (modificationsAll: AttackModification []) =
        
        //check if there is actually the attack wanted by the user
        if attackNumber > attackInfo.Length then failwith "The chosen url does not provide enough different attacks for the attackNumber given. Try giving a smaller number or 1."
        let monsterStats = attackInfo.[0].RelevantMonsterStats
    
        //filter for either melee or ranged
        let wantedAttack =
            attackInfo
            |> Array.filter (fun x -> x.AttackType = attackVariant)
            |> fun x -> if x.Length = 0 
                        then failwith "The chosen url does not provide any attacks for the chosen attack type (melee or ranged)."
                        else x.[attackNumber-1]
    
        let attackArr =
            wantedAttack
            |> fun x -> x.AttackScheme
                        |> Array.collect (fun attack -> [|for i = 0 to (attack.AttackBonus.Length-1) do
                                                            yield attack.AttackBonus.[i], attack|] 
                                         )
    
        let modificationsForAllAttacks =
            modificationsAll
            |> Array.filter (fun x -> snd x.AppliedTo = -20)
    
        let modificationsForNotAllAttacks =
            let attackMaximumLength appliedTo = if appliedTo < attackArr.Length then appliedTo else attackArr.Length
            modificationsAll
            |> Array.filter (fun x -> snd x.AppliedTo <> -20)
            |> fun arr -> if Array.isEmpty arr
                          then Array.create attackArr.Length LibraryModifications.ZeroMod
                               |> Array.mapi (fun i x -> i,x)
                          else Array.collect (fun x -> (Array.create (snd x.AppliedTo) x) 
                                                       |> fun x -> Array.append x (Array.create (attackArr.Length - x.Length) LibraryModifications.ZeroMod)
                                                       |> Array.mapi (fun i x -> i,x)
                                              ) arr
            |> Array.groupBy (fun (x,y) -> x)
            |> Array.map (fun (x,y) -> (Array.map snd y))
            |> fun x -> x
        
        let modificationsCombined =
            modificationsForNotAllAttacks 
            |> Array.map (Array.append modificationsForAllAttacks)
    
        let calculateOneAttack attackBonus (urlAttack: URLAttack) (modifications: AttackModification []) =
            
            /// rolls two dice; one for the regular hit and one for a possible crit confirmation roll
            let (attackRoll,critConfirmationRoll) = 
                let getAttackRolls =
                        rollDice 10000 20
                getRndArrElement getAttackRolls,getRndArrElement getAttackRolls

            //
            let startSize =
                match monsterStats.Size with
                | Fine          -> 1
                | Diminuitive   -> 2
                | Tiny          -> 3
                | Small         -> 4
                | Medium        -> 5
                | Large         -> 6
                | Huge          -> 7
                | Gargantuan    -> 8
                | Colossal      -> 9

            /// calculates size changes due to modifications and applies them to the start size
            let calculatedSize =
                calculateSize monsterStats.Size modifications

            /// calculates size bonus to attack rolls (eg. +1 for small)
            let sizeBonusToAttack =
                let sizeModifierNew =
                    calculatedSize
                    |> fun x -> Map.find x findSizes
                    |> fun x -> x.SizeModifier
                let sizeModifierOld =
                    startSize
                    |> fun x -> Map.find x findSizes
                    |> fun x -> x.SizeModifier
                sizeModifierNew - sizeModifierOld
    
            //Start adding up attack boni
            let modBoniToAttack = 
                toHit.addBoniToAttack modifications
    
            /// Sums up all different boni to attack rolls
            let combinedAttackBoni =
                attackBonus + modBoniToAttack + sizeBonusToAttack
    
            /// complete bonus on attack = dice roll + Sum of all boni (getBonusToAttack)
            let totalAttackBonus =
                attackRoll + combinedAttackBoni

            /// complete bonus on crit confirmation attack roll = dice roll + Sum of all boni (getBonusToAttack) + critical hit confirmation roll specific boni
            let totalAttackCritBonus =
                toHit.getTotalAttackCritBonus modifications combinedAttackBoni
                |> (+) critConfirmationRoll
    
            /////End attack boni/Start damage boni/////
    
            /// calculates size change and resizes weapon damage dice. Cannot be changed to standard function, as it uses different input. Maybe change later to fit! TODO
            let sizeAdjustedWeaponDamage =
                toDmg.adjustWeaponDamage monsterStats.Size urlAttack.WeaponDamage.Die urlAttack.WeaponDamage.NumberOfDie modifications
                |> fun (n,die) -> createURLDamage n die  urlAttack.WeaponDamage.BonusDamage urlAttack.WeaponDamage.DamageType

            /// rolls dice for weapon
            let damageRolls =
                let getDamageRolls =
                    rollDice 1000 sizeAdjustedWeaponDamage.Die
                [|for i=1 to sizeAdjustedWeaponDamage.NumberOfDie do
                    yield getRndArrElement getDamageRolls|]
                |> Array.sum
    
            /// calculates all boni to damage rolls from modifications and checks if they stack or not
            let modBoniToDamage =
                toDmg.addDamageBoni modifications
    
            /// Sums up all different boni to damage
            let totalDamage =
                damageRolls + sizeAdjustedWeaponDamage.BonusDamage + modBoniToDamage
                //the next line sets a minimum dmg of 1 for all attacks. the additional "(urlAttack.WeaponDamage.NumberOfDie <> 0)" circumvents a 1 dmg attack, if the attack is not meant to deal any attack.
                |> fun x -> if (x <= 0) && (urlAttack.WeaponDamage.NumberOfDie <> 0) then 1 else x
    
            /// Calculates damage like Sneak Attack, Vital Strike or the weapon enhancement flaming
            let extraDamageOnHit =
                getURLExtraDamageOnHit modifications sizeAdjustedWeaponDamage
            
            /// Calculates extra damage which is multiplied or changed on crits (think Shocking Grasp or flaming burst)
            let extraDamageOnCrit =
                getURLExtraDamageOnCrit modifications sizeAdjustedWeaponDamage attackRoll urlAttack

            /// combines the extra damage and the extra damage on crit
            let extraDamageCombined =
                getURLExtraDamageCombined extraDamageOnHit extraDamageOnCrit urlAttack
    
            let additionalInfoString =
                getAdditionalInfoString urlAttack.AdditionalEffects
    
            ////
            if (Array.contains attackRoll urlAttack.CriticalRange) = false && extraDamageCombined = [||]
            then printfn "You attack with a %s and hit with a %i (rolled %i) for %i damage %s!" urlAttack.WeaponName totalAttackBonus attackRoll totalDamage additionalInfoString
            elif (Array.contains attackRoll urlAttack.CriticalRange) = true && extraDamageCombined = [||] 
            then printfn "You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i Damage (x %i) %s!" urlAttack.WeaponName totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage urlAttack.CriticalModifier additionalInfoString
            elif (Array.contains attackRoll urlAttack.CriticalRange) = false && extraDamageCombined <> [||]
            then printfn "You attack with a %s and hit the enemy with a %i (rolled %i) for %i damage %s %s!" urlAttack.WeaponName totalAttackBonus attackRoll totalDamage (extraDamageToString extraDamageCombined) additionalInfoString
            elif (Array.contains attackRoll urlAttack.CriticalRange) = true && extraDamageCombined <> [||] 
            then printfn ("You attack with a %s and (hopefully) critically hit the enemy with a %i (rolled %i) and confirm your crit with a %i (rolled %i) for %i damage (x %i) %s (%s on a crit) / (%s when not confirmed) !") urlAttack.WeaponName totalAttackBonus attackRoll totalAttackCritBonus critConfirmationRoll totalDamage urlAttack.CriticalModifier additionalInfoString (extraDamageToString extraDamageCombined) (extraDamageToString extraDamageOnHit)  
            else printfn "You should not see this message, please open an issue with your input as a bug report"
        attackArr
        |> Array.mapi (fun i (attackBonus,attack) -> calculateOneAttack attackBonus attack modificationsCombined.[i])
