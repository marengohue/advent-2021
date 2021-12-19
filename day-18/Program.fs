open Types

let tryExplode (a : SnailNum) : (SnailNum*bool) =
    let rec loop num depth leftCarry rightCarry =
        // When a carryover is already spread, ignore node processing
        // as only one node can be exploded in an iteration
        match (num, leftCarry, rightCarry) with
        | _, Spread, Spread ->
            num, Spread, Spread

        // When reaching an explodable node
        | Pair(Single(left), Single(right)), None, None when depth = 4 ->
            let leftCarry = if left > 0 then Carry(left) else Spread
            let rightCarry = if right > 0 then Carry(right) else Spread
            Single(0), leftCarry, rightCarry

        // When entering a node from the top
        | Pair(left, right), None, None ->
            let leftResult, remainingLeftCarry, remainingRightCarry = loop left (depth + 1) None None
            match remainingRightCarry with
            | Carry(_) ->
                // Branching to the left gave us an explosion
                // - try and spread the explosion on the right
                let rightResultAfterSpread, rightCarryAfterSpread, _ = loop right (depth + 1) remainingRightCarry Spread
                // left carry traverses upwards unchanged. right carry - only if failed to spread
                Pair(leftResult, rightResultAfterSpread), remainingLeftCarry, rightCarryAfterSpread
                
            | None ->
                // No explosion on the left - traverse right normally
                let rightResult, remainingLeftCarry, remainingRightCarry = loop right (depth + 1) None None
                match remainingLeftCarry with
                | Carry(_) ->
                    // Branching to the right gave us an explosion
                    // - try and spread the explosion on the left
                    let leftResultAfterSpread, _, leftCarryAfterSpread = loop left (depth + 1) Spread remainingLeftCarry
                    // Right carry traverses upwards. Left carry - only if failed to spread
                    Pair(leftResultAfterSpread, rightResult), leftCarryAfterSpread, remainingRightCarry
                | None ->
                    Pair(leftResult, rightResult), None, remainingRightCarry
                | Spread ->
                    Pair(leftResult, rightResult), Spread, remainingRightCarry
                    
            | Spread ->
                Pair(leftResult, right), remainingLeftCarry, Spread

        // When attempting to carry some value over to the left or the right branch of the tree
        | Pair(left, right), _, _ ->
            let leftResult, remainingLeftCarry, _ = loop left (depth + 1) leftCarry Spread
            let rightResult, remainingRightCarry, _ = loop right (depth + 1) Spread rightCarry
            Pair(leftResult, rightResult), remainingLeftCarry, remainingRightCarry

        // Finally reached a leaf with some carry values
        | Single(x), Carry(c), right ->
            Single(x + c), Spread, right
        | Single(x), left, Carry(c) ->
            Single(x + c), left, Spread

        // All other cases are intermitent traversal
        | _, _, _ ->
            num, leftCarry, rightCarry

    let (result, leftC, rightC) = loop a 0 None None
    let needAnotherPass =
        match (leftC, rightC) with
        | (None, None) -> false
        | _ -> true

    result, needAnotherPass

let trySplit (a : SnailNum) : (SnailNum*bool) =
    let rec loop (num, needsAnother) =
        match num with
        | _ when needsAnother -> (num, true)
        | Single(x) when x >= 10 -> (Pair(Single(x / 2), Single(x / 2 + x % 2)), true)
        | Pair(left, right) ->
            let (leftResult, leftNeedsAnother) = loop (left, needsAnother)
            let (rightResult, rightNeedsAnother) = loop (right, needsAnother || leftNeedsAnother)
            Pair(leftResult, rightResult), leftNeedsAnother || rightNeedsAnother
        | _ -> num, needsAnother

    loop (a, false)

let (@+) (a : SnailNum) (b : SnailNum) : SnailNum =
    Seq.initInfinite id
    |> Seq.scan (fun (last, _) _ ->
        let explosionResult = tryExplode last
        match explosionResult with
        | _, true ->
            explosionResult
        | _, _ ->
            trySplit last
    ) ((Pair(a, b)), true)
    |> Seq.takeWhile snd
    |> Seq.last
    |> fst

let rec magnitude = function
    | Pair(a, b) -> 3 * magnitude a + 2 * magnitude b
    | Single(a) -> a

let rec cartesian = function
 | ([],[]) -> []
 | (__,[])  -> []
 | ([],__)  -> []
 | (x::xs, ys) -> (List.map(fun y -> x,y) ys) @ (cartesian (xs,ys))

let allNums =
    Io.read Io.stdinStream (fun _ line -> Io.parse line)
    |> Seq.toList
    
allNums
|> Seq.reduce (fun acc next -> acc @+ next)
|> magnitude
|> printf "%i\n"
 
cartesian (allNums, allNums)
|> Seq.map ((fun (a, b) -> a @+ b) >> magnitude)
|> Seq.max
|> printf "%i\n"
