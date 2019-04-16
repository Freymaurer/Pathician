namespace PathfinderAttackSimulator

open System
open System.Net
open Library.AuxLibFunctions

module D20pfsrdReader =
    
    module AuxFunctions = 

        type AttackVariant = 
            | Melee
            | Ranged
        
        type URLDamage = {
            NumberOfDie : int
            Die : int
            BonusDamage : int
            DamageType : string
            }
        
        let createURLDamage nOfDie die bonusDmg damageType= {
            NumberOfDie = nOfDie
            Die         = die
            BonusDamage = bonusDmg
            DamageType = damageType
            } 
        
        type URLAttack = {
            AttackBonus : int []
            WeaponName : string
            WeaponDamage : URLDamage
            ExtraDamage : URLDamage
            CriticalRange : int []
            CriticalModifier : int
            AdditionalEffects : string
            }
        
        let createURLAttack attackBonus weaponName weaponDamage extraDamage criticalRange criticalModifer additionalEffects = {
            AttackBonus         = attackBonus
            WeaponName          = weaponName
            WeaponDamage        = weaponDamage
            ExtraDamage         = extraDamage
            CriticalRange       = criticalRange
            CriticalModifier    = criticalModifer
            AdditionalEffects   = additionalEffects
            }
        
        type URLMonsterStats = {
            BAB : int
            Str : int 
            Dex : int
            Feats : string []
            Size: SizeType
            }
        
        let createURLMonsterStats bab str dex feats size = {
            BAB = bab
            Str = str 
            Dex = dex
            Feats = feats
            Size = size
            }
        
        type URLMonsterAttacks = {
            AttackType : AttackVariant
            AttackScheme : URLAttack []
            RelevantMonsterStats : URLMonsterStats
            }
        
        let createURLMonsterAttacks attackVariant urlAttack bab str dex feats size = {
            AttackType = attackVariant
            AttackScheme = urlAttack
            RelevantMonsterStats = createURLMonsterStats bab str dex feats size
            }
        
        // Fetch the contents of a web page
        let fetchUrl callback url =        
            let req = WebRequest.Create(Uri(url)) 
            use resp = req.GetResponse() 
            use stream = resp.GetResponseStream() 
            use reader = new IO.StreamReader(stream) 
            callback reader url
        
        let myCallback (reader:IO.StreamReader) url = 
            let html = reader.ReadToEnd()
            //let html1000 = html.Substring(0,1000)
            //printfn "Downloaded %s. First 1000 is %s" url html1000
            html      // return all the html
        
        // build a function with the callback "baked in"
        let fetchUrl2 = fetchUrl myCallback 

    open AuxFunctions

    // after extracting the html styled string from the d20pfsrd bestiary the most complex part is the extraction of the attack information.
    // this is done in the following function.
    let private createAttackFromString (str:string) =
        
        //all necessary regex pattern matchings to get the information
        let regexIterativeAttackBoni = System.Text.RegularExpressions.Regex("\+\d+(\/\+\d+)*")
        let regexGetDamage = System.Text.RegularExpressions.Regex("\((.*?)\)")
        let regexMatchPlusOnwards = System.Text.RegularExpressions.Regex("\s*(?=plus\s)(.*)")
        let regexMatchDamage = System.Text.RegularExpressions.Regex("[0-9]+d[0-9]+((\+|\-)[0-9]+)?")
        let regexNumberOfDie = System.Text.RegularExpressions.Regex("[0-9]+(?=d[0-9]+((\+|\-)[0-9]+)?)")
        let regexDieSize = System.Text.RegularExpressions.Regex("(?<=[0-9]+d)[0-9]+(?=((\+|\-)[0-9]+)?)")
        let regexBonusDamage = System.Text.RegularExpressions.Regex("(?<=[0-9]+d[0-9]+)((\+|\-)[0-9]+)?")
        let regexCriticalRange = System.Text.RegularExpressions.Regex("(?<=/)[0-9]+(\W[0-9}]+)?")
        let regexCriticalModifier = System.Text.RegularExpressions.Regex("(?<=/x)\d+")
        let regexPlusBoniExtraDamage = System.Text.RegularExpressions.Regex("[0-9]+d[0-9]+((\+|\-)[0-9]+)?\s\w*")
        let regexPlusBoniExtraDamageType = System.Text.RegularExpressions.Regex("(?<=[0-9]+d[0-9]+((\+|\-)[0-9]+)?\s)\w*")
    
        // this function is used to finally check if the attack actually exists. if a empty string is detected a filler is inserted
        // otherwise the original value is given back
        let getInformation (intVal:int) (arr: string []) =
            if arr.[intVal] = ""
            then match intVal with
                 | 2 -> "+0"
                 | 3 -> "20-20"
                 | 4 -> "2"
                 | 5 -> "0"
                 | 6 -> "0"
                 | 7 -> "+0"
                 | 8 -> ""
                 | 9 -> ""
                 | 0 -> "0"
                 | 1 -> "0"
                 | _ -> failwith "Unknown Parameter; Error01.1"
            else arr.[intVal]
        
        // this function translates the possible 19-20 input to an array from the first to the latter number
        let getCriticalModifer (str:string) =
            let redexMatchSeparator = System.Text.RegularExpressions.Regex("(?<=[0-9]+)\W(?=[0-9}]+)?")
            redexMatchSeparator.Split(str)
            |> fun x -> [|int x.[0] .. int x.[1]|]
    
        //pattern matching for the damage given in brackets
        let damage = 
            let patternMatch = regexGetDamage.Matches(str)
            (patternMatch.Item (patternMatch.Count-1)).Value
            |> fun x -> x.Trim([|'(';')'|])

        //same pattern matching as befor, but this time the dmg is replaced with "", so we only get the rest
        let attack = regexGetDamage.Replace(str,"") 
                     |> fun x -> x.Trim()
    
        // the following resolves the non-damage part, splitting it into a weapon name and an attack arr.
        // weapon name is build by any substring not containing a number. Which also leads to "touch" also being part of the name, altough its meant to describe a touch attack.
        // number of attacks with that weapon is calculated taking multiples of the same weapon into account
        let (name,attackArr) =

            let splitAttackArr = attack.Split(' ')
            let checkMultipleAttacks =
                if String.IsNullOrEmpty attack = true
                then 0
                else attack.[0] 
                     |> fun x -> if Char.IsNumber(x) 
                                 then splitAttackArr
                                      |> Array.head
                                      |> int
                                 else 1
            let weaponName =
                splitAttackArr
                |> Array.filter (fun x -> if String.IsNullOrEmpty x = true
                                          then x = x
                                          else Char.IsLetter(x.[0]) = true
                                )
                |> Array.fold (fun elem arr -> elem + " " + arr) ""
                |> fun x -> x.Trim()
            let iterativeAttacks =
                Array.rev splitAttackArr
                |> Array.tryPick (fun x -> match regexIterativeAttackBoni.IsMatch(x) with
                                           | true -> Some x
                                           | false -> None
                                 )
                |> fun x -> if x.IsSome
                            then x.Value.Split('/')
                                 |> Array.map (fun x -> int x)
                                 |> fun x -> [|for i = 1 to checkMultipleAttacks do
                                                  yield x|]
                                 |> Array.concat
                            else [|0|]
            
            weaponName, iterativeAttacks

        // the following function resolves the damage extraction, more or less an huge stream of regex pattern matchings to get the needed values into the correct place.
        // this needs to be based on a string array, as there could be no damage for attacks only inflicting status effects. so an string array of length = 1 signals such a case and gives back only the whole
        // attack description.
        let weaponDmg =

            let removePlusBoni = regexMatchPlusOnwards.Replace(damage,"")
            let getPlusBoni = regexMatchPlusOnwards.Match(damage).Value
                              |> fun x -> x.Trim()
            let getBaseDamage = regexMatchDamage.Match(removePlusBoni).Value
            let plusBoniExtraDamage = regexPlusBoniExtraDamage.Match(getPlusBoni).Value
            let plusBoniExtraDamageValues = 
                (regexPlusBoniExtraDamageType.Replace(plusBoniExtraDamage,"") |> fun x -> x.Trim())
                |> fun x -> x
            let plusBoniWithoutDamage = 
                regexPlusBoniExtraDamage.Replace(getPlusBoni,"")
                |> fun x -> x.Replace("plus","")
                |> fun x -> x.Trim()
            let urlDmg = if String.IsNullOrEmpty getBaseDamage 
                         then [|damage|]
                         else [|regexNumberOfDie.Match(removePlusBoni).Value;
                                regexDieSize.Match(removePlusBoni).Value;
                                regexBonusDamage.Match(removePlusBoni).Value;
                                regexCriticalRange.Match(removePlusBoni).Value;
                                regexCriticalModifier.Match(removePlusBoni).Value
                                regexNumberOfDie.Match(plusBoniExtraDamageValues).Value;
                                regexDieSize.Match(plusBoniExtraDamageValues).Value;
                                regexBonusDamage.Match(plusBoniExtraDamageValues).Value;
                                regexPlusBoniExtraDamageType.Match(plusBoniExtraDamage).Value;
                                plusBoniWithoutDamage
                                |]
            urlDmg
        
        //here check for actual damage or just description
        if weaponDmg.Length = 1
        then weaponDmg 
             |> fun x -> createURLAttack    attackArr
                                            name
                                            (createURLDamage 0 0 0 "flat") 
                                            (createURLDamage 0 0 0 "flat")
                                            [|0|]
                                            0
                                            x.[0]
        else weaponDmg
             |> fun x -> createURLAttack    attackArr
                                            name
                                            (createURLDamage (int (getInformation 0 x)) (int (getInformation 1 x)) (int (getInformation 2 x)) "flat")
                                            (createURLDamage (int (getInformation 5 x)) (int (getInformation 6 x)) (int (getInformation 7 x)) (getInformation 8 x))
                                            (getCriticalModifer (getInformation 3 x))
                                            (int (getInformation 4 x))
                                            (getInformation 9 x)
    
    // TODO: give more information here
    /// This function returns all necessary information of a pathfinder bestiary monster/NPC by exctracting the information from the d20pfsrd entry via regex pattern matching.
    let getMonsterInformation url = 
    
        let regexFindMonsterStats = System.Text.RegularExpressions.Regex("(?s)(?=article-content)(.*?)((?=<div id=\"comments\" class=\"comments\">)|(?=section15)|(?=ECOLOGY))")
        let regexFindMeleeStats (meleeOrRanged:AttackVariant)= System.Text.RegularExpressions.Regex( sprintf "(?s)(?=%A)(.*?)((?=<br>)|(?=</p>)|(?=<br />))" meleeOrRanged)
        let regexFindHTMLTags = System.Text.RegularExpressions.Regex("\<(.*?)\>")
        let regexCommaOutsideBrackets = System.Text.RegularExpressions.Regex(",(?![^(]*\))")
        let regexMatchWithOr = System.Text.RegularExpressions.Regex("(?<=\s)or(?=\s)")
        let regexMatchScore (str:string) = System.Text.RegularExpressions.Regex(("(?s)(?<=STATISTICS.*"+str+"\s)\d+"))
        let regexBAB = System.Text.RegularExpressions.Regex("(?s)(?<=Base Atk.)(\+\d+)(?=.*Skills)")
        let regexGetSpecialFeats = System.Text.RegularExpressions.Regex("(?s)(?<=Feats.*)(\w+\s)?(Two\WWeapon\sFighting|Power\sAttack|Rapid\sShot|Deadly\sAim)(?=.*Skills)")
        let regexSize = System.Text.RegularExpressions.Regex("(?s)(?<=XP.*)(Fine|Diminuitive|Tiny|Small|Medium|Large|Huge|Gargantuan|Colossal)(?!=.*DEFENSE)")
    
        // writes html code of webpage to string
        let baseString = fetchUrl2 url
        // matches size string to give back size directly
        let matchSize (str:string) =
            match str with 
            | "Fine"           -> Fine 
            | "Diminuitive"    -> Diminuitive
            | "Tiny"           -> Tiny
            | "Small"          -> Small
            | "Medium"         -> Medium
            | "Large"          -> Large
            | "Huge"           -> Huge
            | "Gargantuan"     -> Gargantuan
            | "Colossal"       -> Colossal
            | _ -> failwith "found unknown size category in monsterstatblock; Error02.1"
    
        // delete all webpage information from the string; only leaves statblock
        let monsterInformation = regexFindMonsterStats.Match baseString     //.Match returns always only the first hit
                                 |> fun x -> x.Value
                                 |> fun x -> x.Replace('–','-')
        // deletes all html tags from the statblock, as the linebreak html tag is used to extract melee/ranged this can not be done from the beginning
        let monsterInformationWithoutHTML = regexFindHTMLTags.Replace(monsterInformation,"")
        // gets all attack combinations/schemes and writes them into the URLAttack type, that is easily accessible in later functions
        let getAttacks (meleeOrRanged:AttackVariant) = 
            let attackInfo = (regexFindMeleeStats meleeOrRanged).Match monsterInformation        //.Match returns always only the first hit
                             |> fun x -> regexFindHTMLTags.Replace(x.Value,"")
         
            attackInfo
            |> fun x -> x.Replace((sprintf "%A" meleeOrRanged),"")
            |> fun x -> regexMatchWithOr.Split(x)
            |> Array.map (fun x -> regexCommaOutsideBrackets.Split(x)
                                   |> Array.map (fun x -> x.Trim())
                                   |> Array.map (fun x -> createAttackFromString x)
                         )
            |> fun x -> x
        let attacksMelee = getAttacks Melee
                           |> Array.map (fun x -> Melee, x)
        let attacksRanged = getAttacks Ranged
                            |> Array.map (fun x -> Ranged, x)
        let attacks = Array.append attacksMelee attacksRanged

        // final part writes all information into the URLMonsterAttacks type
        [|for i = 0 to (attacks.Length-1) do
            yield createURLMonsterAttacks (fst attacks.[i])
                                          (snd attacks.[i]) 
                                          (regexBAB.Match(monsterInformationWithoutHTML).Value |> int)
                                          ((regexMatchScore "Str").Match(monsterInformationWithoutHTML).Value |> int) 
                                          ((regexMatchScore "Dex").Match(monsterInformationWithoutHTML).Value |> int)
                                          (regexGetSpecialFeats.Matches(monsterInformationWithoutHTML)
                                           |>  fun x -> [|for i = 0 to (x.Count-1) do 
                                                            yield (x.Item i).Value|]
                                          )
                                          (matchSize (regexSize.Match(monsterInformationWithoutHTML).Value))
                                          
        |]
        //filter out empty attackschemes (e.g. a non existing ranged attack scheme)
        |> Array.filter (fun x -> x.AttackScheme
                                  |> fun urlAttackArr -> Array.exists (fun urlAttack -> (urlAttack.WeaponName <> "") && urlAttack.CriticalModifier <> 0) urlAttackArr
                        )

