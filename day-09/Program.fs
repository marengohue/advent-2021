open System
open System.Collections.Generic

let untuple f (a1, a2) = f a1 a2

let read parser = 
    Seq.initInfinite (fun _ -> Console.ReadLine())
    |> Seq.takeWhile (String.IsNullOrEmpty >> not)
    |> Seq.indexed
    |> Seq.map (untuple parser)
    |> Seq.choose id

let intRowParser (lineNo: int) (line: string) =
    line.ToCharArray()
    |> Seq.indexed
    |> Seq.map (fun (idx, item) -> ((idx, lineNo), (string >> int) item))
    |> Option.Some

let board = (read intRowParser) |> Seq.collect id |> Map

let adjacent (x, y) = seq { 
    (x - 1, y)
    (x + 1, y)
    (x, y - 1)
    (x, y + 1)
}

let get (board: Map<(int*int), int>) (point: (int*int)) =
    if (board.ContainsKey point) then
        Some (board.Item point)
    else
        None

let lowPoints board =
    board
    |> Map.filter (fun setPoint setVal -> 
        (adjacent setPoint)
            |> Seq.choose (get board)
            |> Seq.forall (fun it -> it > setVal))
    |> Seq.map (fun kvp -> kvp.Key)

let basinFor board point =
    let toVisit point leftToVisit basin =
        point
            |> adjacent
            |> Seq.filter (fun it -> List.contains it basin |> not)
            |> Seq.append leftToVisit
            |> Seq.toList
            |> List.distinct
    
    let rec loop points basin =
        match points with
            | p::ps ->
                match get board p with
                    | Some(9) -> loop ps basin
                    | None -> loop ps basin
                    | Some(_) -> loop (toVisit p ps basin) (p :: basin)
            | [] -> basin
            
    loop [point] []
    

let sumLowPoints board =
    let risk = (+) 1
    (lowPoints board)
    |> Seq.fold (fun acc point -> acc + (risk (board.Item point))) 0

let basinsForLowPoints board =
    lowPoints board
    |> Seq.map (basinFor board)
    // Not a thing in the input data, but could happen that multiple
    // low points are in the same basin
    |> Seq.distinct
    |> Seq.map List.length
    |> Seq.sortDescending
    |> Seq.take 3
    |> Seq.reduce (*)

printf "%A " (sumLowPoints board)
printf "%A" (basinsForLowPoints board)
