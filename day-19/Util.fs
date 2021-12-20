namespace Advent.Day19

module Util =
    let partition<'a> (predicate : 'a -> bool) (s : 'a seq) =

        let groupingResult =
            s
            |> Seq.groupBy predicate
            |> Seq.toList
        match groupingResult with
        | (firstMatch, first) :: (_, second) :: _ when firstMatch -> (first |> Seq.toList, second |> Seq.toList)
        | (_, first) :: (secondMatch, second) :: _ when secondMatch -> (second |> Seq.toList, first |> Seq.toList)
        | (onlyMatch, matches) :: _ when onlyMatch -> (matches |> Seq.toList, List.empty)
        | _ -> (List.empty, s |> Seq.toList)

    let rec cartesian = function
    | ([],[]) -> []
    | (__,[])  -> []
    | ([],__)  -> []
    | (x::xs, ys) -> (List.map(fun y -> x,y) ys) @ (cartesian (xs,ys))
