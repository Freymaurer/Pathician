namespace Pathician.Types

type Character = {
    Name:           string
    BaseAttackBonus:int
    Strength:       int
    Dexterity:      int
    Constitution:   int
    Intelligence:   int
    Wisdom:         int
    Charisma:       int
    CasterLevel1:   int
    CasterLevel2:   int
    Size:           UnionTypes.Size.SizeCategory
    Description:    string
    //TODO: PLACEHOLDER
    Feats:          unit
}

