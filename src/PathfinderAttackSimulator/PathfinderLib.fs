namespace PathfinderAttackSimulator

open System

module Library =

    type DamageTypes =
        | Feuer
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

    type BonusTypes =
        | Insight
        | Moral
        | Luck
        | Alchemical
        | Profane
        | Sacred
        | Circumstance
        | Flat
        | HasteLike
        | TWF_Like
        | Size

    type WeaponType =
        | PrimaryMain
        | Primary
        | Secondary
        | All

    type Damage = {
        NumberOfDie : int
        Die : int
        DamageType : DamageTypes
        }

    type UsedModifier = {
        ToHit : string
        ToDmg : string
        MultiplicatorOnDamage : float
        }

    type BonusAttacks = {
        NumberOfBonusAttacks : int
        TypeOfBonusAttacks : BonusTypes
        WeaponTypeWithBonusAttacks : WeaponType
        }

    type StatChange = {
        Attribute : string
        AttributeChange : int
        Bonustype : BonusTypes
        }

    type Bonus = {
        Value : int
        BonusType : BonusTypes
        }

    /// "0" 0 Flat if no StatChange
    let createStatChange att attChange bType = {
        Attribute = att
        AttributeChange = attChange
        Bonustype = bType
        }

    /// 0 Flat if no Bonus
    let createBonus value bType = {
        Value = value
        BonusType = bType
        }

    /// 0 Flat All if no BonusAttacks. num = number of bonus attacks; bonusAttackType = is meant for calculation of non-stacking effects like magus spell combat and twf
    ///in that case both are TWF_Like; appliedToWeaponType = the Weapons that get bonus attacks e.g. haste goes to primaryMain, twf goes to primary, multiattack goes to secondary
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

    /// hitting = Modifier used for hitting; damage = Modifier used for damage calculation; multiplicator = how often is damage modifier added; 1.5 means that power attack is also increased
    let createUsedModifier hitting damage multiplicator = {
        ToHit = hitting
        ToDmg = damage
        MultiplicatorOnDamage = multiplicator
        }

    /// Turns all letters in uppercase, which makes matchingfunctions more failproof.
    let createStringForLib inputString =
        inputString
        |> String.map (fun x -> Char.ToUpper x)

    type CharacterStats = 
        {
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
            ManufacturedOrNatural   : string
        }

    type AttackModification =
        {
            Name                : string
            BonusAttacks        : BonusAttacks
            BonusAttackRoll     : Bonus
            BonusDamage         : Bonus
            ExtraDamage         : Damage
            AppliedTo           : WeaponType [] * int
            StatChanges         : StatChange []
            Description         : string
        }

    module Characters =

        ///Stats for Character
        let myParrn =
            {
                CharacterName = createStringForLib "Parrn"
                BAB = 6
                Strength = 6
                Dexterity = 0
                Constitution = 0
                Intelligence = 0
                Wisdom = 0
                Charisma = 0
                CasterLevel1 = 0
                CasterLevel2 = 0
            }


        let myTumor =
            {
                CharacterName = createStringForLib "Stephano"
                BAB = 6
                Strength = -2
                Dexterity = 1
                Constitution = 0
                Intelligence = 0
                Wisdom = 0
                Charisma = 0
                CasterLevel1 = 0
                CasterLevel2 = 0
            }

        let myElemental =
            {
                CharacterName = createStringForLib "Michelangelo"
                BAB = 6
                Strength = 1
                Dexterity = 2
                Constitution = 1
                Intelligence = 0
                Wisdom = 0
                Charisma = 0
                CasterLevel1 = 0
                CasterLevel2 = 0
            }

    module Weapons =

        let glaiveGuisarmePlus1Flaming = 
            {
                Name                = "Glaive-Guisarme +1 flaming"
                Damage              = createDamage 1 10 Slashing
                DamageBonus         = 1
                ExtraDamage         = createDamage 1 6 Feuer
                BonusAttackRolls    = 1
                CriticalRange       = [|20|]
                CriticalModifier    = 3
                Modifier            = createUsedModifier "Str" "Str" 1.5
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let greatswordParrn =
            {
                Name                = "Large +1 Keen Greatsword"
                Damage              = createDamage 3 6 Slashing
                DamageBonus         = 1
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|17;18;19;20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.5
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let mwkSapLarge = 
            {
                Name                = "Masterwork Sap"
                Damage              = createDamage 1 8 Bludgeoning
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let mwkSapHuge = 
            {
                Name                = "Masterwork Sap, huge"
                Damage              = createDamage 2 6 Bludgeoning
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let butchersAxe =
            {
                Name                = "Butchers Axe"
                Damage              = createDamage 3 6 Slashing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 3
                Modifier            = createUsedModifier "Str" "Str" 1.5
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let mwkRapier =
            {
                Name                = "Mwk Rapier"
                Damage              = createDamage 1 6 Piercing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|18;19;20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Dex" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let enchantedLongswordElemental =
            {
                Name                = "+1 Longsword"
                Damage              = createDamage 1 6 Slashing
                DamageBonus         = 1
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|19;20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let talonsTumor =
            {
                Name                = "Talons"
                Damage              = createDamage 1 3 Piercing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Dex" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Natural"
            }

        let greatswordParrnHuge =
            {
                Name                = "Huge +1 Keen Greatsword"
                Damage              = createDamage 4 6 Slashing
                DamageBonus         = 1
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|17;18;19;20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.5
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let mwkLongbow =
            {
                Name                = "Mwk Longbow"
                Damage              = createDamage 1 8 Piercing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 1
                CriticalRange       = [|20|]
                CriticalModifier    = 3
                Modifier            = createUsedModifier "Dex" "0" 1.
                ManufacturedOrNatural = createStringForLib "Manufactured"
            }

        let bite =
            {
                Name                = "Bite"
                Damage              = createDamage 1 6 BludgeoningOrPiercingOrSlashing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Natural"
            }

        let slamElemental =
            {
                Name                = "Slam"
                Damage              = createDamage 1 4 Bludgeoning
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Natural"
            }

        let claw =
            {
                Name                = "Claw"
                Damage              = createDamage 1 6 Slashing
                DamageBonus         = 0
                ExtraDamage         = createDamage 0 0 Untyped
                BonusAttackRolls    = 0
                CriticalRange       = [|20|]
                CriticalModifier    = 2
                Modifier            = createUsedModifier "Str" "Str" 1.
                ManufacturedOrNatural = createStringForLib "Natural"
            }

    module Modifications =

        let Multiattack = 
            {
                Name = "Multiattack"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 3 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|Secondary|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let SneakAttackOnce rogueLevel =
            {
                Name = "Sneak Attack auf dem ersten Angriff"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage (int (ceil (float rogueLevel/2.))) 6 Precision
                AppliedTo = [|All|], 1        
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        ///mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
        let TwoWeaponFighting =
            {
                Name = "Two-Weapon-Fighting"
                BonusAttacks = createBonusAttacks 1 TWF_Like Primary
                BonusAttackRoll = createBonus -2 TWF_Like
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|Primary; PrimaryMain|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }    

        /// mit allen als Primary gelisteten Waffen; bisher nur mit -2 auf Treffen
        let ImprovedTwoWeaponFighting =
            {
                Name = "Improved-Two-Weapon-Fighting"
                BonusAttacks = createBonusAttacks 2 TWF_Like Primary
                BonusAttackRoll = createBonus -2 TWF_Like
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|Primary; PrimaryMain|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        let Haste =
            {
                Name = "Haste"
                BonusAttacks = createBonusAttacks 1 HasteLike PrimaryMain
                BonusAttackRoll = createBonus 1 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        let FlurryOfBlows =
            {
                Name = "Flurry Of Blows"
                BonusAttacks = createBonusAttacks 1 Flat PrimaryMain
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        let Charging =
            {
                Name = "Charge-Attack"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 2 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let Flanking = 
            {
                Name = "Flanking"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 2 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let PowerAttack bab =
            {
                Name = "Power Attack"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus (int( - (floor (float bab/4. + 1.)) )) Flat
                BonusDamage = createBonus (int( (floor (float bab/4.) * 2.) + 2. )) Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let FuriousFocus bab =
            {
                Name = "Furious Focus"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus (int(floor (float bab/4. + 1.))) Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|PrimaryMain|], 1
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let DivineFavor =
            {
                Name = "Divine Favor"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 1 Luck
                BonusDamage = createBonus 1 Luck
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let Wrath =
            {
                Name = "Wrath"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 1 Moral
                BonusDamage = createBonus 1 Moral
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let SneakAttack (rogueLevel:int) =
            {
                Name = "Sneak Attack"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage (int (ceil (float rogueLevel/2.))) 6 Precision
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let PlanarFocus =
            {
                Name = "Planar Focus"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 1 6 Feuer
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let Invisibility =
            {
                Name = "Invisibility"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 2 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let Small =
            {
                Name = "Small Size"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 1 Size
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
    
        let MutagenStrength =
            {
                Name = "Strength Mutagen"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|(createStatChange "Str" 2 Alchemical); (createStatChange "Int" -2 Alchemical)|]
                Description = ""
            }
    
        let EnlargePerson =
            {
                Name = "Enlarge Person"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus -1 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|(createStatChange "Str" 1 Size);(createStatChange "Dex" -1 Size)|]
                Description = ""
            }

        let WeaponFocus = 
            {
                Name = "Weapon Focus"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 1 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        let Shaken = 
            {
                Name = "Shaken"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus -2 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

        let WeaponSpecialization =
            {
                Name = "WeaponSpecialization"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 2 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }
        
        let Fatigued =
            {
                Name = "Fatigued"
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "Str" -1 Flat|]
                Description = ""
            }

        let AidAnother = {
               Name = "Aid Another"
               BonusAttacks = createBonusAttacks 0 Flat All
               BonusAttackRoll = createBonus 2 Flat
               BonusDamage = createBonus 0 Flat
               ExtraDamage = createDamage 0 0 Untyped
               AppliedTo = [|All|], -20
               StatChanges = [|createStatChange "0" 0 Flat|]
               Description = ""
            }
    
        /// Never delete this!! This is 100% necessary for FullRoundAttackAction to function, as it works as a filler for the modificationArrays
        let ZeroMod =
            {
                Name = ""
                BonusAttacks = createBonusAttacks 0 Flat All
                BonusAttackRoll = createBonus 0 Flat
                BonusDamage = createBonus 0 Flat
                ExtraDamage = createDamage 0 0 Untyped
                AppliedTo = [|All|], -20
                StatChanges = [|createStatChange "0" 0 Flat|]
                Description = ""
            }

    module Server = 

        let TestWeapon = {
                    Name                = "Test"
                    Damage              = createDamage 1 6 Slashing
                    DamageBonus         = 0
                    ExtraDamage         = createDamage 0 0 Untyped
                    BonusAttackRolls    = 0
                    CriticalRange       = [|20|]
                    CriticalModifier    = 2
                    Modifier            = createUsedModifier "Str" "Str" 1.
                    ManufacturedOrNatural = createStringForLib "Manufactured"
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

open Library.Modifications
open Library.Weapons
open Library.Characters

let showAll str =  
    let rdyStr = Library.createStringForLib str
    match rdyStr with
    | rdyStr when rdyStr = "MODIFICATIONS" -> [|
                                                Multiattack;SneakAttackOnce 0;TwoWeaponFighting;ImprovedTwoWeaponFighting;Haste;FlurryOfBlows;Shaken;WeaponFocus;EnlargePerson;MutagenStrength;
                                                Small;Invisibility;PlanarFocus;SneakAttack 0;Wrath;DivineFavor;FuriousFocus 0;PowerAttack 0;Flanking;Charging;WeaponSpecialization;Fatigued;
                                                AidAnother
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

    //
