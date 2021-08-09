﻿namespace Pathician.Types

type Dice = {
    n:      int
    Dice:   int
} with
    member this.roll() =
        Pathician.DiceRoller.rollDice this.n this.Dice
    member this.rollSum() =
        Pathician.DiceRoller.rollDice this.n this.Dice
        |> Array.sum

type Bonus = {
    Bonus: int
    BonusType: UnionTypes.Bonus.Types
}

module ComplexTypes =
    
    /// Determines which ability score is used for attack rolls and/or damage rolls, also contains the dmg multiplier for the ability score.
    type AbilityScoreModifier = {
        ToHit:              AbilityScoreModifier
        ToDmg:              AbilityScoreModifier
        ToDmgMultiplier:    float
    }

    type DamageDice = {
        /// How many which sided dice need to be rolled for damage
        Damage:             Dice option
        /// Flat Boni to damage, stacking and non-stacking
        DamageBonus:        int option
        /// damage type (e.g. fire, slashing, force)
        DamageType:         UnionTypes.DamageTypes.DamageTypes
    } with
        member this.rollSum() =
            match this.Damage, this.DamageBonus with
            | Some dmg, Some flatBonus  -> dmg.rollSum() + flatBonus
            | Some dmg, None            -> dmg.rollSum()
            | None, Some flatBonus      -> flatBonus
            | None, None                -> 0
                
    type Damage = {
        DamageDice:         DamageDice
        // TODO: Might not be necessary
        /// if the Damage is multiplied on crit
        MultipliedOnCrit:   bool
        /// if damage crits, how often is it multiplied
        CritMultiplier:     int
        OnCritDamage:       DamageDice option
    }

    type BonusAttacks = {
        Type:           UnionTypes.BonusAttacks.Types
        /// This is an array of bonus attacks in relation to BAB. 
        /// Example: Haste: [0], Greater-Two-Weapon Fighting [0; -5; -10]
        AttackArray:    int []
    }

    type SizeChange = {
        ChangeBy:           int
        SizeIncreaseType:   UnionTypes.Size.IncreaseType
    }