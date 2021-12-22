namespace Advent.Day22
    module Util =

        let clamp minA maxA = max minA >> min maxA

        let cartesian2 xs ys =
            [
                for x in xs do
                for y in ys do
                    yield x, y
            ]

        let cartesian3 xs ys zs =
            [
                for x in xs do
                for y in ys do
                for z in zs do
                    yield x, y, z
            ]

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