namespace Pathician.Types

type Weapon = {
    Name:           string
    // Basic Weapon Damage
    Modifier:       ComplexTypes.AbilityScoreModifier
    Damage:         ComplexTypes.Damage
    AttackBonus:    Bonus
    CriticalRange:  int
    WeaponType:     UnionTypes.WeaponType
    // Bonus Damage from for example burning weapon enhancement
    BonusDamage:    ComplexTypes.Damage []
    Description:    string
}