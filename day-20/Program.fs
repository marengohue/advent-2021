open Advent.Day20

open Types
open Types.EnhancementAlgorithm
open Types.Image

let enhance algorithm (image : Image) : Image =
    let pointsToLookAt = image.boundsOfInterest |> Bounds.points |> Set.ofSeq

    let nextPointsOfInterest = 
        pointsToLookAt
        |> Set.filter (fun point ->
            let lookupCode =
                Point.adjacent point
                |> Seq.rev
                |> Seq.indexed
                |> Seq.sumBy (fun (idx, adj) ->
                    if Image.isWhite image adj then (1 <<< idx) else 0
                )
            EnhancementAlgorithm.isWhite algorithm lookupCode
        ) |> Set.ofSeq

    let nextBounds = Bounds.grow 1 image.boundsOfInterest       
    {
        // Grow bounds depending on if the algorithm is flashing or not
        boundsOfInterest = nextBounds
    
        // Account for flashing algorithms
        isLit =
            if EnhancementAlgorithm.isFlasing algorithm then
                (not image.isLit)
            else image.isLit

        pointsOfInterest = 
            match EnhancementAlgorithm.isFlasing algorithm with
            // When we are about to become lit in a flashing algorithm,
            // we need to add all of the light pixels outside of interest
            // into the new points of interest
            | true when (not image.isLit) ->
                let pointsWithinNewBounds = Bounds.points nextBounds |> Set.ofSeq
                let pointsToAdd = Set.difference pointsWithinNewBounds pointsToLookAt
                Set.union nextPointsOfInterest pointsToAdd

            | _ -> nextPointsOfInterest
    }


let algorithm : EnhancementAlgorithm =
    Io.readStdin Io.parseAlgorithm |> List.head

let image : Image =
    Io.readStdin Io.parseImageLine |> Set.unionMany |> Image.ofPoints

let iters = 50
seq { 1 .. iters }
|> Seq.fold (fun acc _ -> enhance algorithm acc) image
|> (fun img -> 
    // Zoom your terminal out before printing this image
    // Io.printImage img

    img.pointsOfInterest |> Set.count |> printf "\n\n Total light pixels: %A \n"
)
