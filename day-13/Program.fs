type Point = (uint*uint)
type Axis = X | Y
type Fold = (Axis*uint)

let pointParser _ (line: string) =
    let parseResult = (line.Trim().Split(',')
        |> Seq.map uint
        |> Seq.toList
    )
    match parseResult with 
    | x :: y :: _ -> Some (x, y)
    | _ -> None

let foldParser _ (line: string) =
    let split (where: char) (str: string) = str.Split(where)
    let parseResult = (line.Trim()
        |> split ' '
        |> Seq.skip 2 // skip "fold along"
        |> Seq.head
        |> split '='
        |> Seq.toList
    )
    match parseResult with
    | "x" :: coord :: _ -> Some(Axis.X, (coord |> uint))
    | "y" :: coord :: _ -> Some(Axis.Y, (coord |> uint))
    | _ -> None

let applyFolds (points: Point seq) (foldDef: Fold) =
    points
    |> Seq.groupBy (fun (x, y) ->
        match foldDef with
        | Axis.Y, foldCoord -> y > foldCoord
        | Axis.X, foldCoord -> x > foldCoord
    )
    |> Seq.map (fun (needsFolding, points) ->
        if needsFolding then
            points |> Seq.map (fun (x, y) ->
                match foldDef with
                | Axis.Y, foldCoord -> (x, 2u * foldCoord - y)
                | Axis.X, foldCoord -> (2u * foldCoord - x, y)
            )
        else
            points
    )
    |> Seq.collect id
    |> Seq.distinct
    |> Seq.toList

let display (points: Point seq) =
    let findMaxPoint (xState, yState) (x, y) = ((max xState x), (max yState y))
    let (xMax, yMax) = Seq.fold findMaxPoint (0u, 0u) points
    let pointList = points |> Seq.toList

    printf "\n"
    for y in 0u..yMax do
        for x in 0u..xMax do
            if List.contains (x, y) pointList then
                printf "#"
            else
                printf "."
        printf "\n"

let points = Input.read Input.stdinStream pointParser
let folds = Input.read Input.stdinStream foldParser

let allFolds =
    folds
    |> Seq.scan applyFolds points

allFolds |> Seq.iter (fun fold -> printf "%i " (Seq.length fold))
allFolds |> Seq.last |> display
