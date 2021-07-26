namespace Pathician.Types

open System

/// This module is made from several sub moduls containing necessary types to create characters, weapons and modifications, as well as librarys for each of those classes.
module UnionTypes =
    
    module Size =

        type SizeCategory =
        | Fine
        | Diminuitive
        | Tiny
        | Small
        | Medium
        | Large
        | Huge
        | Gargantuan
        | Colossal 
            static member ofString (str:string) =
                match str with
                | "Fine"        -> Fine
                | "Diminuitive" -> Diminuitive
                | "Tiny"        -> Tiny
                | "Small"       -> Small
                | "Medium"      -> Medium
                | "Large"       -> Large
                | "Huge"        -> Huge
                | "Gargantuan"  -> Gargantuan
                | "Colossal"    -> Colossal
                | anythingElse  -> failwith (sprintf "Error. Could not parse SizeType of `%s`!" anythingElse)

            static member Order =
                [|  Fine
                    Diminuitive
                    Tiny
                    Small
                    Medium
                    Large
                    Huge
                    Gargantuan
                    Colossal    |]

            member this.toAttackModifier =
                match this with
                | Fine          -> 8
                | Diminuitive   -> 4
                | Tiny          -> 2
                | Small         -> 1
                | Medium        -> 0
                | Large         -> -1
                | Huge          -> -2
                | Gargantuan    -> -4
                | Colossal      -> -8

            member this.changeBy (sizeChanges:int) =
                let min = 1
                let max = SizeCategory.Order.Length
                let currentInd = SizeCategory.Order |> Array.findIndex (fun category -> category = this)
                let newInd = 
                    let tempNewInd = currentInd + sizeChanges
                    if tempNewInd < min then 
                        min
                    elif tempNewInd > max then 
                        max
                    else 
                        tempNewInd
                SizeCategory.Order.[newInd-1]

        type IncreaseType =
        | EffectiveSizeIncrease
        | SizeIncrease

    type AbilityScore =
    | Strength
    | Dexterity
    | Constitution
    | Intelligence
    | Wisdom
    | Charisma
        static member ofString (str:string) =
            match str with
            | "Strength"        -> Strength
            | "Dexterity"       -> Dexterity
            | "Constitution"    -> Constitution
            | "Intelligence"    -> Intelligence
            | "Wisdom"          -> Wisdom
            | "Charisma"        -> Charisma
            | anythingElse  -> failwith (sprintf "Error. Could not parse AbilityScore of `%s`!" anythingElse)

    type DamageTypes =
    | Fire
    | Cold
    | Acid
    | Electricity
    | Bludgeoning
    | Slashing
    | Piercing
    | BludgeoningOrPiercing
    | BludgeoningOrPiercingOrSlashing
    | PiercingOrSlashing
    | PositiveEnergy
    | NegativeEnergy
    | Force
    | Precision
    | Untyped

    type WeaponType =
    | Natural
    | Manufactured

    type WeaponWielding =
    | OneHanded
    | TwoHanded
    | OffHand
    | PrimaryNaturalAttack
    | SecondaryNaturalAttack

    module Bonus =

        /// StackingBonusTypes are Pathfinder Bonus types which stack. Not only the highest bonus counts.
        type StackingBonusTypes =
        | Circumstance
        | Flat
        
        /// UniqueBonusTypes are Pathfinder Bonus types which do not stack. Only the highest bonus counts.
        type UniqueBonusTypes =
        | Alchemical
        | Competence
        | Enhancement
        | Inherent
        | Insight
        | Luck
        | Morale
        | Racial
        | Profane
        | Sacred
        | Size
        | Trait

        type Types =
        | StackingBonus of StackingBonusTypes
        | UniqueBonus of UniqueBonusTypes 

    module BonusAttacks =

        type UniqueBonusAttacks =
        | HasteLike
        | TWFLike
        | FlurryOfBlowsLike

        type Types =
        | StackingBonusAttacks
        | UniqueBonus of UniqueBonusAttacks




