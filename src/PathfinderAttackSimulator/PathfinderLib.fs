namespace PathfinderAttackSimulator

open System

/// This module is made from several sub moduls containing necessary types to create characters, weapons and modifications, as well as librarys for each of those classes.
module Library =

    /// This module contains all necessary types and create functions to operate the StandardAttack and Full-RoundAttack module.
    module AuxLibFunctions =

        type SizeType =
            | Fine
            | Diminuitive
            | Tiny
            | Small
            | Medium
            | Large
            | Huge
            | Gargantuan
            | Colossal
        
        type AbilityScore =
            | Strength
            | Dexterity
            | Constitution
            | Intelligence
            | Wisdom
            | Charisma
            | NoAS

        type DamageTypes =
            | Fire
            | Slashing
            | Bludgeoning
            | Piercing
            | Cold
            | Acid
            | Electricity
            | Untyped
            | BludgeoningOrPiercing
            | BludgeoningOrPiercingOrSlashing
            | PiercingOrSlashing
            | Precision
            | VitalStrikeDamage

        type BonusTypes =
            | Insight
            | Moral
            | Luck
            | Alchemical
            | Profane
            | Sacred
            | Circumstance
            | Flat
            | Size
            | TwoWeaponFightingMalus
            | Polymorph
            | Competence

        type BonusAttacksType =
            | HasteLike
            | TWFLike
            | FlurryOfBlowsLike
            | FlatBA
            | NoBA

        type WeaponType =
            | PrimaryMain
            | Primary
            | Secondary
            | All

        type NaturalManufactured =
            | Natural
            | Manufactured

        type WeaponHanded =
            | OneHanded
            | TwoHanded
            | OffHand

        type SizeAttributes = {
            SizeModifier : int
            SizeId : int
            Size : SizeType
            }

        type Damage = {
            NumberOfDie : int
            Die : int
            DamageType : DamageTypes
            }

        type DamageHitAndCrit = {
            OnHit : Damage
            OnCrit : Damage
            }

        type WeaponDamageMultiplicator = {
            Hand : WeaponHanded
            Multiplicator : float     
            }

        type UsedModifier = {
            ToHit : AbilityScore
            ToDmg : AbilityScore
            MultiplicatorOnDamage : WeaponDamageMultiplicator
            }

        type BonusAttacks = {
            NumberOfBonusAttacks : int
            TypeOfBonusAttacks : BonusAttacksType
            WeaponTypeWithBonusAttacks : WeaponType
            }

        type StatChange = {
            Attribute : AbilityScore
            AttributeChange : int
            Bonustype : BonusTypes
            }

        type Bonus = {
            Value : int
            BonusType : BonusTypes
            }

        type SizeChange = {
            SizeChangeValue : int
            SizeChangeBonustype : BonusTypes
            EffectiveSizeChange : bool
            }

        type AttackBonusHitAndCrit = {
            OnHit   : Bonus
            OnCrit  : Bonus
            }

        //type Size(size: SizeType) =

        //    let sizeID = 
        //        match size with
        //        | Fine          -> 1
        //        | Diminuitive   -> 2
        //        | Tiny          -> 3
        //        | Small         -> 4
        //        | Medium        -> 5
        //        | Large         -> 6
        //        | Huge          -> 7
        //        | Gargantuan    -> 8
        //        | Colossal      -> 9

        //    member this.SizeID = sizeID

        //    member this.Modifier =
        //        match this.SizeID with
        //        | 1 -> 8
        //        | 2 -> 4
        //        | 3 -> 2
        //        | 4 -> 1
        //        | 5 -> 0
        //        | 6 -> -1
        //        | 7 -> -2
        //        | 8 -> -4
        //        | 9 -> -8
        //        | tooSmall when this.SizeID < 1 -> 8
        //        | tooBig   when this.SizeID > 9 -> -8

        //    member this.SizeIncrease(increase: int) =
        //        let newID = if sizeID + increase > 9 then 9
        //                    elif sizeID + increase < 1 then 1
        //                    else sizeID + increase
        //        let newSize =
        //            match newID with
        //            | 1 -> Fine
        //            | 2 -> Diminuitive
        //            | 3 -> Tiny
        //            | 4 -> Small
        //            | 5 -> Medium
        //            | 6 -> Large
        //            | 7 -> Huge
        //            | 8 -> Gargantuan
        //            | 9 -> Colossal
        //        new Size(newSize)
        /// Represents ability score changes; e.g. the alchemist's mutagen (createStatChange Strength 4 Alchemical).

        /// NoAS 0 Flat if no StatChange, or leave array empty
        let createStatChange att attChange bType = {
            Attribute = att
            AttributeChange = attChange
            Bonustype = bType
            }

        /// defines OneHanded/TwoHanded for power attack, but also the multiplicator for the bonus damage
        /// from the attribute (think dragon style).
        let createWeaponDamageMultiplicator handling multiplicator = {
            Hand = handling
            Multiplicator = multiplicator
            }

        /// 0 Flat if no Bonus
        let createBonus value bType = {
            Value = value
            BonusType = bType
            }

        /// 0 NoBA All if no BonusAttacks. num = number of bonus attacks; bonusAttackType = is meant for calculation of non-stacking effects like magus spell combat and twf
        /// in that case both are TWFLike; appliedToWeaponType = the Weapons that get bonus attacks e.g. haste goes to primaryMain, twf goes to primary, multiattack goes to secondary
        let createBonusAttacks num bonusAttackType appliedToWeaponType= {
            BonusAttacks.NumberOfBonusAttacks = num
            BonusAttacks.TypeOfBonusAttacks = bonusAttackType
            BonusAttacks.WeaponTypeWithBonusAttacks = appliedToWeaponType
            }

        /// 0 0 Untyped if no Damage. num = number of damage die; die is how many sides the damage die has; dType is e.g. Bludgeoning
        let createDamage num die dType = {
            Damage.NumberOfDie = num
            Damage.Die = die
            Damage.DamageType = dType
            }

        /// hitting = Modifier used for hitting; damage = Modifier used for damage calculation; handling referrs to onehanded twohanded etc. necessary to calculate power attack; multiplicator = how often is damage modifier added
        let createUsedModifier hitting damage handling multiplicator = {
            ToHit = hitting
            ToDmg = damage
            MultiplicatorOnDamage = createWeaponDamageMultiplicator handling multiplicator
            }

        ///The first value represents number of size changes and the direction, e.g. -1 = shrink by 1 size category. Next Value will be mostly "Polymorph" or Flat as the type of size change.
        ///Last is a false/true question, whether this size change is an actual size change or an effective size change: Write "true" if it is just an effective size change (Improved Natural Attack).
        let createSizechange value bonusType effectiveSizeChange = {
            SizeChangeValue = value
            SizeChangeBonustype = bonusType
            EffectiveSizeChange = effectiveSizeChange
            }

        /// Turns all letters in uppercase, which makes matchingfunctions more failproof.
        let createStringForLib inputString =
            inputString
            |> String.map (fun x -> Char.ToUpper x)

        /// Represents ability score changes; e.g. the alchemist's mutagen (createStatChange Strength 4 Alchemical).
        let createSizeAttributes modifier id sizeType = {
            SizeModifier = modifier
            SizeId = id
            Size = sizeType
            }

        let findSizes = [1,createSizeAttributes 8 1 Fine;
                        2,createSizeAttributes 4 2 Diminuitive;
                        3,createSizeAttributes 2 3 Tiny;
                        4,createSizeAttributes 1 4 Small;
                        5,createSizeAttributes 0 5 Medium;
                        6,createSizeAttributes -1 6 Large;
                        7,createSizeAttributes -2 7 Huge;
                        8,createSizeAttributes -4 8 Gargantuan;
                        9,createSizeAttributes -8 9 Colossal
                        ] |> Map.ofSeq


        // the OnHit bonus is applied to all attacks crit or not, whereas the OnCrit bonus is applied to crits IN ADDITION to the OnHit Bonus.
        let createAttackBoniHitAndCrit hitValue hitValueType critValue critValueType = {
            OnHit = createBonus hitValue hitValueType
            OnCrit = createBonus critValue critValueType
            }

        /// first 3 variables are number of for extra damage on normal hits, (1d6 Fire damage = 1 6 Fire).
        /// The latter 3 are used for dmg only(!) applied on crits! (Flaming burst = createDamageHitAndCrit 1 6 Fire 2 10 Fire)
        let createDamageHitAndCrit numberOfDieoOnHit dieOnHit dmgTypeOnHit numberOfDieOnCrit dieOnCrit dmgTypeOnCrit = {
            DamageHitAndCrit.OnHit = createDamage numberOfDieoOnHit dieOnHit dmgTypeOnHit
            DamageHitAndCrit.OnCrit = createDamage numberOfDieOnCrit dieOnCrit dmgTypeOnCrit
            }

        type CharacterStats = {
            CharacterName : string
            BAB : int
            Strength : int
            Dexterity : int
            Constitution: int
            Intelligence: int
            Wisdom: int
            Charisma: int
            CasterLevel1 : int
            CasterLevel2 : int
            }

        type Weapon = {
            Name                    : string
            Damage                  : Damage
            DamageBonus             : int
            ExtraDamage             : DamageHitAndCrit
            BonusAttackRolls        : int
            CriticalRange           : int []
            CriticalModifier        : int
            Modifier                : UsedModifier
            ManufacturedOrNatural   : NaturalManufactured
            Description             : string
            }

        type AttackModification = {
            Name                : string
            BonusAttacks        : BonusAttacks
            BonusAttackRoll     : AttackBonusHitAndCrit
            BonusDamage         : Bonus
            ExtraDamage         : DamageHitAndCrit
            AppliedTo           : WeaponType [] * int
            StatChanges         : StatChange []
            SizeChanges         : SizeChange
            Description         : string
            }

    /// library for all prebuild characters; this is mostly for personal use or functions as an example
    module Characters =

        open AuxLibFunctions

        ///Stats for Character
        let myParrn = {
            CharacterName = "Parrn"
            BAB = 6
            Strength = 22
            Dexterity = 10
            Constitution = 10
            Intelligence = 10
            Wisdom = 10
            Charisma = 10
            CasterLevel1 = 0
            CasterLevel2 = 0
            }


        let myTumor = {
            CharacterName = "Stephano"
            BAB = 6
            Strength = 6
            Dexterity = 12
            Constitution = 10
            Intelligence = 10
            Wisdom = 10
            Charisma = 10
            CasterLevel1 = 0
            CasterLevel2 = 0
            }

        let myElemental = {
            CharacterName = "Michelangelo"
            BAB = 6
            Strength = 12
            Dexterity = 14
            Constitution = 12
            Intelligence = 10
            Wisdom = 10
            Charisma = 10
            CasterLevel1 = 0
            CasterLevel2 = 0
            }

    /// Library for all pre-written weapons; this is mostly for personal use or meant as an example
    module Weapons =

        open AuxLibFunctions

        let glaiveGuisarmePlus1FlamingBurst =  {
            Name                = "Glaive-Guisarme +1 flaming"
            Damage              = createDamage 1 10 Slashing
            DamageBonus         = 1
            ExtraDamage         = createDamageHitAndCrit 1 6 Fire 2 10 Fire
            BonusAttackRolls    = 1
            CriticalRange       = [|20|]
            CriticalModifier    = 3
            Modifier            = createUsedModifier Strength Strength TwoHanded 1.5
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let greatswordParrn = {
            Name                = "Large +1 Keen Greatsword"
            Damage              = createDamage 3 6 Slashing
            DamageBonus         = 1
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|17;18;19;20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength TwoHanded 1.5
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let mwkSapLarge = {
            Name                = "Masterwork Sap"
            Damage              = createDamage 1 8 Bludgeoning
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let mwkSapHuge = {
            Name                = "Masterwork Sap, huge"
            Damage              = createDamage 2 6 Bludgeoning
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let butchersAxe = {
            Name                = "Butchers Axe"
            Damage              = createDamage 3 6 Slashing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 3
            Modifier            = createUsedModifier Strength Strength TwoHanded 1.5
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let mwkRapier = {
            Name                = "Mwk Rapier"
            Damage              = createDamage 1 6 Piercing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|18;19;20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Dexterity Strength OneHanded 1.
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let enchantedLongswordElemental = {
            Name                = "+1 Longsword"
            Damage              = createDamage 1 6 Slashing
            DamageBonus         = 1
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|19;20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let talonsTumor = {
            Name                = "Talons"
            Damage              = createDamage 1 3 Piercing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Dexterity Strength OneHanded 1.
            ManufacturedOrNatural = Natural
            Description         = ""
            }

        let greatswordParrnHuge = {
            Name                = "Huge +1 Keen Greatsword"
            Damage              = createDamage 4 6 Slashing
            DamageBonus         = 1
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|17;18;19;20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength TwoHanded 1.5
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let mwkLongbow = {
            Name                = "Mwk Longbow"
            Damage              = createDamage 1 8 Piercing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 1
            CriticalRange       = [|20|]
            CriticalModifier    = 3
            Modifier            = createUsedModifier Dexterity NoAS OneHanded 1.
            ManufacturedOrNatural = Manufactured
            Description         = ""
            }

        let bite = {
            Name                = "Bite"
            Damage              = createDamage 1 6 BludgeoningOrPiercingOrSlashing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Natural
            Description         = ""
            }

        let slamElemental = {
            Name                = "Slam"
            Damage              = createDamage 1 4 Bludgeoning
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Natural
            Description         = ""
            }

        let claw = {
            Name                = "Claw"
            Damage              = createDamage 1 6 Slashing
            DamageBonus         = 0
            ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Natural
            Description         = ""
            }

    /// This part is still under construction, come back later.
    module Server = 
   
        open AuxLibFunctions
    
        /////not updated
        //let showAll str =  
        //    let rdyStr = createStringForLib str
        //    match rdyStr with
        //    | rdyStr when rdyStr = "MODIFICATIONS" -> [|
        //                                                Multiattack;SneakAttackOnce 0;TwoWeaponFighting;TwoWeaponFightingImproved;Haste;FlurryOfBlows;Shaken;WeaponFocus;EnlargePerson;MutagenStrength;
        //                                                Invisibility;PlanarFocusFire 0;SneakAttack 0;Wrath;DivineFavor;FuriousFocus 0;PowerAttack 0;Flanking;Charging;WeaponSpecialization;Fatigued;
        //                                                AidAnother;VitalStrike;VitalStrikeImproved;VitalStrikeGreater;InspireCourage 0; ShockingGrasp 0 true; ShockingGraspIntensifiedEmpowered 0 true; PowerAttackURL OffHand 0;
        //                                                BlessingOfFervorAttackBonus; BonusAttackDamage 0 0;
        //                                              |]
        //                                              |> Array.map (fun x -> x.Name)
        //                                              |> Array.sortBy (fun x -> x)
        //    | rdyStr when rdyStr = "WEAPONS" -> [|
        //                                            claw;slamElemental;bite;mwkLongbow;greatswordParrnHuge;talonsTumor;enchantedLongswordElemental;
        //                                            mwkRapier;butchersAxe;mwkSapHuge;mwkSapLarge;greatswordParrn;glaiveGuisarmePlus1FlamingBurst
        //                                        |]
        //                                        |> Array.map (fun x -> x.Name)
        //                                        |> Array.sortBy (fun x -> x)
        //    | rdyStr when rdyStr = "CHARACTERS" -> [|
        //                                                myElemental;myTumor;myParrn
        //                                           |]
        //                                           |> Array.map (fun x -> x.CharacterName)
        //                                           |> Array.sortBy (fun x -> x)
        //    | _ -> failwith "Unknown Input. Type *Modifications*, *Weapons* or *Characters* (without the *) to see all related objects in the library."
    
    
        let TestWeapon = {
                Name                = "Test"
                Damage              = createDamage 1 6 Slashing
                DamageBonus         = 0
                ExtraDamage         = createDamageHitAndCrit 0 0 Untyped 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier Strength Strength OneHanded 1.
                ManufacturedOrNatural = Manufactured
                Description         = ""
                }
    
        let EmptyChar = { 
                CharacterName = createStringForLib ""
                BAB = 0
                Strength = 0
                Dexterity = 0
                Constitution = 0
                Intelligence = 0
                Wisdom = 0
                Charisma = 0
                CasterLevel1 = 0
                CasterLevel2 = 0
                }

    //
