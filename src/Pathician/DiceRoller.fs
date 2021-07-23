namespace Pathician

module DiceRoller =

    let rollDice count (diceSides:int) =
        let rnd = System.Random()
        if diceSides = 0 
        then [|0|]
        else Array.init count (fun _ -> rnd.Next (1, diceSides+1))