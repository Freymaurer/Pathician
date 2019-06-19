namespace PathfinderAttackSimulator

open System

open PathfinderAttackSimulator.Library.AuxLibFunctions

/// Library for all pre-written modifications
module LibraryModifications =


    let AidAnother = {
        Name = "Aid Another"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], 1
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let BlessingOfFervorAttackBonus = {
        Name = "Blessing of Fervor"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 2 Flat 0 Flat
        BonusDamage = createBonus 2 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Blessing of Fervor with the +2 attack bonus as choice"
        }
    
    /// use this modification to add fast and easy flat boni to attack rolls or to damage.
    let BonusAttackDamage attack damage= {
        Name = "Blessing of Fervor"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit attack Flat 0 Flat
        BonusDamage = createBonus damage Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Use this modification to add fast and easy flat boni to attack rolls or to damage"
        }

    let Charging = {
        Name = "Charge-Attack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 2 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let CriticalFocus = {
        Name = "Critical Focus"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 4 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Applies +4 to all crits not, as its not able to separate weapons"
        }

    let DivineFavor = {
        Name = "Divine Favor"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 1 Luck 0 Flat
        BonusDamage = createBonus 1 Luck
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let EnlargePerson = {
        Name = "Enlarge Person"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|(createStatChange Strength 2 Size);(createStatChange Dexterity -2 Size)|]
        SizeChanges = createSizechange 1 Polymorph false
        Description = ""
        }

    let Fatigued = {
        Name = "Fatigued"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|(createStatChange Strength -2 Flat); (createStatChange Dexterity -2 Size)|]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let Flanking = {
        Name = "Flanking"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 2 Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let FlurryOfBlows = {
        Name = "Flurry Of Blows"
        BonusAttacks = createBonusAttacks 1 NoBA PrimaryMain
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let FuriousFocus bab = {
        Name = "Furious Focus"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit (int(floor (float bab/4. + 1.))) BonusTypes.Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|PrimaryMain|], 1
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let Haste = {
        Name = "Haste"
        BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
        BonusAttackRoll = createAttackBoniHitAndCrit 1 Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let InspireCourage bardLevel = 
        let (bonusValue:int) = match bardLevel with
                               | x when bardLevel >= 17 -> 4
                               | x when bardLevel >= 11 -> 3
                               | x when bardLevel >= 5 -> 2
                               | _ -> 1
        {
        Name = "Inspire Courage"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit bonusValue Competence 0 Flat
        BonusDamage = createBonus bonusValue Competence
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "For a set level, because of several IC increasing items"
        }

    let Invisibility = {
        Name = "Invisibility"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 2 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let Multiattack =  {
        Name = "Multiattack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 3 Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|Secondary|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let MutagenStrength = {
        Name = "Strength Mutagen"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [|(createStatChange Strength 4 Alchemical); (createStatChange Intelligence -2 Alchemical)|]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let PlanarFocusFire (lvl:int) = 
        let NumberOfExtraDie = int (lvl/4) + 1 
        {
        Name = "Planar Focus"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit NumberOfExtraDie 6 Fire 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let PowerAttack bab = {
        Name = "Power Attack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit (int( - (floor (float bab/4. + 1.)) )) Flat 0 Flat
        BonusDamage = createBonus (int( (floor (float bab/4.) * 2.) + 2. )) BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }
    
    let PowerAttackURL (handed:WeaponHanded) bab= {
        Name = "Power Attack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit (int( - (floor (float bab/4. + 1.)) )) Flat 0 Flat
        BonusDamage = (floor (float bab/4.) * 2.) + 2. 
                      |> fun x -> match handed with
                                  | TwoHanded -> createBonus (int (x * 1.5)) Flat
                                  | OneHanded -> createBonus (int x) Flat
                                  | OffHand -> createBonus (int (x * 0.5)) Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Use this only for the calculateURLAttack function"
        }        

    let Shaken = {
        Name = "Shaken"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit -2 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let ShockingGrasp casterLevel metalTF = {
        Name = "Intensified Empowered Shocking Grasp"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit (if metalTF = true then 3 else 0) Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit (if casterLevel > 5 then 5 else casterLevel) 6 Electricity (if casterLevel > 5 then 5 else casterLevel) 6 Electricity
        AppliedTo = [|All|], 1
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Shocking Grasp deals 1d6 / level electricity damage up to a maximum of 5d6."
        }

    let ShockingGraspIntensifiedEmpowered casterLevel metalTF = {
        Name = "Intensified Empowered Shocking Grasp"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit (if metalTF = true then 3 else 0) Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit ((if casterLevel > 10 then 10 else casterLevel) 
                                             |> fun x -> x + int (float x * 0.5) ) 6 Electricity
                                             ((if casterLevel > 10 then 10 else casterLevel) 
                                             |> fun x -> x + int (float x * 0.5) ) 6 Electricity
        AppliedTo = [|All|], 1
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Shocking Grasp deals 1d6 / level electricity damage up to a maximum of 10d6 for this intensified version. Empowered increases the number of all rolled dice by 50%"
        }

    let SneakAttack (rogueLevel:int) = {
        Name = "Sneak Attack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit (int (ceil (float rogueLevel/2.))) 6 Precision 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let SneakAttackOnce rogueLevel = {
        Name = "Sneak Attack"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit (int (ceil (float rogueLevel/2.))) 6 Precision 0 0 Untyped
        AppliedTo = [|All|], 1        
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Sneak Attack on first attack. This can happen due to a stealth attack or an full-round attack action from invisibility"
        }

    ///mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
    let TwoWeaponFighting = {
        Name = "Two-Weapon-Fighting"
        BonusAttacks = createBonusAttacks 1 TWFLike Primary
        BonusAttackRoll = createAttackBoniHitAndCrit -2 TwoWeaponFightingMalus 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|Primary; PrimaryMain|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }    

    /// mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
    let TwoWeaponFightingImproved = {
        Name = "Improved Two-Weapon-Fighting"
        BonusAttacks = createBonusAttacks 2 TWFLike Primary
        BonusAttackRoll = createAttackBoniHitAndCrit -2 TwoWeaponFightingMalus 0 Flat
        BonusDamage = createBonus 0 BonusTypes.Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|Primary; PrimaryMain|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    /// This modification is hardcoded, so it does not follow normal modification rules
    let VitalStrike = {
        Name = "Vital Strike"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 1 0 VitalStrikeDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
        }
        
    /// This modification is hardcoded, so it does not follow normal modification rules
    let VitalStrikeImproved = {
        Name = "Improved Vital Strike"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 2 0 VitalStrikeDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
        }

    /// This modification is hardcoded, so it does not follow normal modification rules
    let VitalStrikeGreater = {
        Name = "Vital Strike"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 3 0 VitalStrikeDamage 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
        }

    let WeaponFocus = {
        Name = "Weapon Focus"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 1 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = "Only use this if you only use one weapon."
        }

    let WeaponSpecialization ={
        Name = "WeaponSpecialization"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 2 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    let Wrath = {
        Name = "Wrath"
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 1 Moral 0 Flat
        BonusDamage = createBonus 1 Moral
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }

    /// Never delete this!! This is 100% necessary for FullRoundAttackAction to function, as it works as a filler for the modificationArrays.
    /// It also functions as a example for a completly empty modification, as this could be added multiple times without changing anything.
    let ZeroMod = {
        Name = ""
        BonusAttacks = createBonusAttacks 0 NoBA All
        BonusAttackRoll = createAttackBoniHitAndCrit 0 Flat 0 Flat
        BonusDamage = createBonus 0 Flat
        ExtraDamage = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
        AppliedTo = [|All|], -20
        StatChanges = [||]
        SizeChanges = createSizechange 0 Flat false
        Description = ""
        }


