namespace PathfinderAttackSimulator

open System

module Library =

    /// This module contains all necessary types and create functions to operate the StandardAttack and Full-RoundAttack module
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

        /// NoAS 0 Flat if no StatChange, or leave array empty
        let createStatChange att attChange bType = {
            Attribute = att
            AttributeChange = attChange
            Bonustype = bType
            }

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

        let createSizechange value bonusType effectiveSizeChange = {
            SizeChangeValue = value
            SizeChangeBonustype = bonusType
            EffectiveSizeChange = effectiveSizeChange
            }

        /// Turns all letters in uppercase, which makes matchingfunctions more failproof.
        let createStringForLib inputString =
            inputString
            |> String.map (fun x -> Char.ToUpper x)


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
            ExtraDamage             : Damage
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
            BonusAttackRoll     : Bonus
            BonusDamage         : Bonus
            ExtraDamage         : Damage
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

    /// library for all pre-written weapons; this is mostly for personal use or meant as an example
    module Weapons =

        open AuxLibFunctions

        let glaiveGuisarmePlus1Flaming =  {
            Name                = "Glaive-Guisarme +1 flaming"
            Damage              = createDamage 1 10 Slashing
            DamageBonus         = 1
            ExtraDamage         = createDamage 1 6 Fire
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
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
            ExtraDamage         = createDamage 0 0 Untyped
            BonusAttackRolls    = 0
            CriticalRange       = [|20|]
            CriticalModifier    = 2
            Modifier            = createUsedModifier Strength Strength OneHanded 1.
            ManufacturedOrNatural = Natural
            Description         = ""
            }

    /// Library for all pre-written modifications
    module Modifications =

        open AuxLibFunctions

        let AidAnother = {
            Name = "Aid Another"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 2 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], 1
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let BlessingOfFervorAttackBonus = {
            Name = "Blessing of Fervor"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 2 Flat
            BonusDamage = createBonus 2 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Blessing of Fervor with the +2 attack bonus as choice"
            }
        
        /// use this modification to add fast and easy flat boni to attack rolls or to damage.
        let BonusAttackDamage attack damage= {
            Name = "Blessing of Fervor"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus attack Flat
            BonusDamage = createBonus damage Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Use this modification to add fast and easy flat boni to attack rolls or to damage"
            }

        let Charging = {
            Name = "Charge-Attack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 2 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let DivineFavor = {
            Name = "Divine Favor"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 1 Luck
            BonusDamage = createBonus 1 Luck
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let EnlargePerson = {
            Name = "Enlarge Person"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [|(createStatChange Strength 2 Size);(createStatChange Dexterity -2 Size)|]
            SizeChanges = createSizechange 1 Polymorph false
            Description = ""
            }

        let Fatigued = {
            Name = "Fatigued"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [|(createStatChange Strength -2 Flat); (createStatChange Dexterity -2 Size)|]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let Flanking = {
            Name = "Flanking"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 2 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let FlurryOfBlows = {
            Name = "Flurry Of Blows"
            BonusAttacks = createBonusAttacks 1 NoBA PrimaryMain
            BonusAttackRoll = createBonus 0 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let FuriousFocus bab = {
            Name = "Furious Focus"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus (int(floor (float bab/4. + 1.))) BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|PrimaryMain|], 1
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let Haste = {
            Name = "Haste"
            BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
            BonusAttackRoll = createBonus 1 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
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
            BonusAttackRoll = createBonus bonusValue Competence
            BonusDamage = createBonus bonusValue Competence
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "For a set level, because of several IC increasing items"
            }

        let Invisibility = {
            Name = "Invisibility"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 2 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let Multiattack =  {
            Name = "Multiattack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 3 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|Secondary|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let MutagenStrength = {
            Name = "Strength Mutagen"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
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
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage NumberOfExtraDie 6 Fire
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let PowerAttack bab = {
            Name = "Power Attack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus (int( - (floor (float bab/4. + 1.)) )) BonusTypes.Flat
            BonusDamage = createBonus (int( (floor (float bab/4.) * 2.) + 2. )) BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }
        
        let PowerAttackURL (handed:WeaponHanded) bab= {
            Name = "Power Attack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus (int( - (floor (float bab/4. + 1.)) )) Flat
            BonusDamage = (floor (float bab/4.) * 2.) + 2. 
                          |> fun x -> match handed with
                                      | TwoHanded -> createBonus (int (x * 1.5)) Flat
                                      | OneHanded -> createBonus (int x) Flat
                                      | OffHand -> createBonus (int (x * 0.5)) Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Use this only for the calculateURLAttack function"
            }        
    
        let Shaken = {
            Name = "Shaken"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus -2 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let ShockingGrasp casterLevel metalTF = {
            Name = "Intensified Empowered Shocking Grasp"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus (if metalTF = true then 3 else 0) Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage (if casterLevel > 5 then 5 else casterLevel) 6 Electricity
            AppliedTo = [|All|], 1
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Shocking Grasp deals 1d6 / level electricity damage up to a maximum of 5d6."
            }

        let ShockingGraspEmpowered casterLevel metalTF = {
            Name = "Intensified Empowered Shocking Grasp"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus (if metalTF = true then 3 else 0) Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage ((if casterLevel > 10 then 10 else casterLevel) 
                                       |> fun x -> x + int (float x * 0.5) ) 6 Electricity
            AppliedTo = [|All|], 1
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Shocking Grasp deals 1d6 / level electricity damage up to a maximum of 10d6 for this intensified version. Empowered increases the number of all rolled dice by 50%"
            }

        let SneakAttack (rogueLevel:int) = {
            Name = "Sneak Attack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage (int (ceil (float rogueLevel/2.))) 6 Precision
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let SneakAttackOnce rogueLevel = {
            Name = "Sneak Attack"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 BonusTypes.Flat
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage (int (ceil (float rogueLevel/2.))) 6 Precision
            AppliedTo = [|All|], 1        
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Sneak Attack on first attack. This can happen due to a stealth attack or an full-round attack action from invisibility"
            }

        ///mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
        let TwoWeaponFighting = {
            Name = "Two-Weapon-Fighting"
            BonusAttacks = createBonusAttacks 1 TWFLike Primary
            BonusAttackRoll = createBonus -2 TwoWeaponFightingMalus
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|Primary; PrimaryMain|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }    

        /// mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
        let TwoWeaponFightingImproved = {
            Name = "Improved Two-Weapon-Fighting"
            BonusAttacks = createBonusAttacks 2 TWFLike Primary
            BonusAttackRoll = createBonus -2 BonusTypes.TwoWeaponFightingMalus
            BonusDamage = createBonus 0 BonusTypes.Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|Primary; PrimaryMain|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let VitalStrike = {
            Name = "Vital Strike"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 1 0 VitalStrikeDamage
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
            }
            
        let VitalStrikeImproved = {
            Name = "Improved Vital Strike"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 2 0 VitalStrikeDamage
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
            }

        let VitalStrikeGreater = {
            Name = "Vital Strike"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 3 0 VitalStrikeDamage
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "These extra weapon damage dice are not multiplied on a critical hit, but are added to the total"
            }

        let WeaponFocus = {
            Name = "Weapon Focus"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 1 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = "Only use this if you only use one weapon."
            }

        let WeaponSpecialization ={
            Name = "WeaponSpecialization"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 2 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }

        let Wrath = {
            Name = "Wrath"
            BonusAttacks = createBonusAttacks 0 NoBA All
            BonusAttackRoll = createBonus 1 Moral
            BonusDamage = createBonus 1 Moral
            ExtraDamage = createDamage 0 0 Untyped
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
            BonusAttackRoll = createBonus 0 Flat
            BonusDamage = createBonus 0 Flat
            ExtraDamage = createDamage 0 0 Untyped
            AppliedTo = [|All|], -20
            StatChanges = [||]
            SizeChanges = createSizechange 0 Flat false
            Description = ""
            }
      
    module Server = 

        open Modifications
        open Weapons
        open Characters
        open AuxLibFunctions

        ///not updated
        let showAll str =  
            let rdyStr = createStringForLib str
            match rdyStr with
            | rdyStr when rdyStr = "MODIFICATIONS" -> [|
                                                        Multiattack;SneakAttackOnce 0;TwoWeaponFighting;TwoWeaponFightingImproved;Haste;FlurryOfBlows;Shaken;WeaponFocus;EnlargePerson;MutagenStrength;
                                                        Invisibility;PlanarFocusFire 0;SneakAttack 0;Wrath;DivineFavor;FuriousFocus 0;PowerAttack 0;Flanking;Charging;WeaponSpecialization;Fatigued;
                                                        AidAnother;VitalStrike;VitalStrikeImproved;VitalStrikeGreater;InspireCourage 0; ShockingGrasp 0 true; ShockingGraspEmpowered 0 true; PowerAttackURL OffHand 0;
                                                        BlessingOfFervorAttackBonus; BonusAttackDamage 0 0;
                                                      |]
                                                      |> Array.map (fun x -> x.Name)
                                                      |> Array.sortBy (fun x -> x)
            | rdyStr when rdyStr = "WEAPONS" -> [|
                                                    claw;slamElemental;bite;mwkLongbow;greatswordParrnHuge;talonsTumor;enchantedLongswordElemental;
                                                    mwkRapier;butchersAxe;mwkSapHuge;mwkSapLarge;greatswordParrn;glaiveGuisarmePlus1Flaming
                                                |]
                                                |> Array.map (fun x -> x.Name)
                                                |> Array.sortBy (fun x -> x)
            | rdyStr when rdyStr = "CHARACTERS" -> [|
                                                        myElemental;myTumor;myParrn
                                                   |]
                                                   |> Array.map (fun x -> x.CharacterName)
                                                   |> Array.sortBy (fun x -> x)
            | _ -> failwith "Unknown Input. Type *Modifications*, *Weapons* or *Characters* (without the *) to see all related objects in the library."


        let TestWeapon = {
                Name                = "Test"
                Damage              = createDamage 1 6 Slashing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier Strength Strength OneHanded 1.
                ManufacturedOrNatural = Manufactured
                Description         = ""
                }
    
        let TestCharacter = { 
                CharacterName = createStringForLib "TestChar"
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
