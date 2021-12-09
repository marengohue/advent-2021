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

let foldMap state _ value = state + value

let risk height = 1 + height

let sumLowPoints board =
  board
  |> Map.filter (fun setPoint setVal -> 
     (adjacent setPoint)
       |> Seq.filter board.ContainsKey
       |> Seq.forall (fun adjPoint -> (board.Item adjPoint) > setVal))
  |> Map.fold (fun acc _ height -> acc + (risk height)) 0



printf "%A" (sumLowPoints board)
