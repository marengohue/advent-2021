namespace Advent.Day19

module Io =
    open System
    open Advent.Day19.Point.Point2
    open Advent.Day19.Point.Point3

    let stdinStream = Seq.initInfinite (fun _ -> Console.ReadLine())

    let read inputSeq parser =
        inputSeq
        |> Seq.takeWhile (String.IsNullOrEmpty >> not)
        |> Seq.indexed
        |> Seq.map (fun (a, b) -> parser a b)
        |> Seq.choose id
        |> Seq.toList

    let parsePoint2 _ (line: string) =
        let parts = line.Split(",")
        match parts with
        | [|x; y|] -> Some (Point2 (x |> int, y |> int))
        | _ -> None

    let parsePoint3 _ (line: string) =
        let parts = line.Split(",")
        match parts with
        | [|x; y; z|] -> Some (Point3 (x |> int, y |> int, z |> int))
        | _ -> None
