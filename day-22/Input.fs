namespace Advent.Day22

module Input =
    open System
    open Cuboid

    let stdinStream = Seq.initInfinite (fun _ -> Console.ReadLine())
    
    let parseCuboidOp _ (line : string) = 
        match line.Split(" ") with
        | [|isOn;bounds|] -> 
            let isOnBool = (isOn = "on")
            let coordResult = 
                bounds.Split(",")
                |> Array.map(fun coordStr ->
                    let coords = coordStr.Split("=") |> Array.last |> (fun it -> it.Split(".."))
                    (coords |> Array.head |> int), (coords |> Array.skip 1 |> Array.head |> int)
                )
            match coordResult with
            | [| (xMin, xMax) ; (yMin, yMax); (zMin, zMax) |] ->
                (isOnBool, Bounds3(Vector3(xMin, yMin, zMin), Vector3(xMax + 1, yMax + 1, zMax + 1))) |> Some
            | _ -> None
        | _ -> None

    let read inputSeq parser =
       inputSeq
       |> Seq.takeWhile (String.IsNullOrEmpty >> not)
       |> Seq.indexed
       |> Seq.map (fun (lineNo, str) -> parser lineNo str)
       |> Seq.choose id
       |> Seq.toList
    

