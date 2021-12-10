module Input
open System

let untuple f (a1, a2) = f a1 a2
let read parser = 
    Seq.initInfinite (fun _ -> Console.ReadLine())
    |> Seq.takeWhile (String.IsNullOrEmpty >> not)
    |> Seq.indexed
    |> Seq.map (untuple parser)
    |> Seq.choose id