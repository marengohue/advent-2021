open System
open System.Text.RegularExpressions

let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some (Map.ofSeq [ for g in m.Groups -> g.Name, g.Value ]) else None

let targetArea =
    let matchRegex = "target area: x=((?<xmin>-?\d+)\.\.(?<xmax>-?\d+)), y=((?<ymin>-?\d+)\.\.(?<ymax>-?\d+))"
    
    match Console.ReadLine() with
    | Match matchRegex result ->
        (
            (Map.find "xmin" result |> int, Map.find "xmax" result |> int),
            (Map.find "ymin" result |> int, Map.find "ymax" result |> int)
        )
    | _ -> failwith "invalid input"

type SimulationResult =
| Hit of int * (int*int)
| NoHit

let sim ((xMin, xMax), (yMin, yMax)) (v0x, v0y) =
    let isHit (x, y) = (x >= xMin && x <= xMax && y >= yMin && y <= yMax)

    Seq.initInfinite id
    |> Seq.skip 1 // 0th second is not interesting
    |> Seq.scan (fun ((lastX, lastY), (lastVx, lastVy), _) time ->
       let newX = lastX + lastVx
       let newY = lastY + lastVy
       let newVx = Math.Max(lastVx - 1, 0)
       let newVy = lastVy - 1
       ((newX, newY), (newVx, newVy), time)
    ) ((0, 0), (v0x, v0y), 0)
    |> Seq.takeWhile (fun ((x, y), _, _) -> (y >= yMin && x <= xMax))
    |> Seq.map (fun ((x, y), _, time) -> if (isHit (x, y)) then Hit(time, (v0x, v0y)) else NoHit)
    |> Seq.last

let maxVelocityY (_, (yMin, _)) = -yMin - 1
let actualMaxVelocityY = maxVelocityY targetArea

let maxVelocityX ((xMin, xMax), _) = if abs xMax > abs xMin then xMax else xMin
let actualMaxVelocityX = maxVelocityX targetArea

seq { 0 .. (if actualMaxVelocityX < 0 then -1 else +1) .. actualMaxVelocityX }    
|> Seq.collect (fun x ->
    seq { -(abs actualMaxVelocityY) - 1 .. (abs actualMaxVelocityY) }
    |> Seq.map (fun y -> (x, y))
)
|> Seq.map (sim targetArea)
|> Seq.filter (fun x ->
    match x with
    | Hit(_) -> true
    | _ -> false
)
|> Seq.length
|> printf "%A"
