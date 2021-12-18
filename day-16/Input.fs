module Input

open System

let untuple f (a1, a2) = f a1 a2

let stdinStream = Seq.initInfinite (fun _ -> Console.ReadLine())

let read inputSeq parser =
   inputSeq
   |> Seq.takeWhile (String.IsNullOrEmpty >> not)
   |> Seq.indexed
   |> Seq.map (untuple parser)
   |> Seq.choose id
   |> Seq.toList
