namespace Pathician.Types

open System

/// This module is made from several sub moduls containing necessary types to create characters, weapons and modifications, as well as librarys for each of those classes.
module UnionTypes =
    
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

    module Bonus =

        /// StackingBonusTypes are Pathfinder Bonus types which stack. Not only the highest bonus counts.
        type StackingBonusTypes =
        | Circumstance
        
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




