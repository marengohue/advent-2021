namespace Advent.Day20

module Types =
    let lightPixel = '#'
    let darkPixel = '.'

    module EnhancementAlgorithm =

        type EnhancementAlgorithm = int Set

        let isWhite (alg : EnhancementAlgorithm) (code : int) : bool = Set.contains code alg

        let isFlasing (alg : EnhancementAlgorithm) : bool = Set.contains 0 alg && not (Set.contains 511 alg)
    
    module Point =
        type Point = (int*int)

        let adjacentBy (by : int) ((x, y) : Point) : Point seq =
            seq {
                for newY in (y - by) .. (y + by) do
                    for newX in (x - by) .. (x + by) do
                        yield (newX, newY)
            }

        let adjacent = adjacentBy 1
        
    module Bounds =
        open Point

        type Bounds = (Point*Point)

        let contains ((minX, minY), (maxX, maxY)) ((x, y) : Point) : bool =
            x <= maxX && x >= minX && y >= minY && y <= maxY

        let grow (s : int) (((minX, minY), (maxX, maxY)) : Bounds) =
            (minX - s, minY - s), (maxX + s, maxY + s)

        let points (((minX, minY), (maxX, maxY)) : Bounds) : Point seq =
            seq {
                for newY in (minY) .. (maxY) do
                    for newX in (minX) .. (maxX) do
                        yield (newX, newY)
            }


        let zero = Bounds ((0, 0), (0, 0))

    module Image =
        open Point
        open Bounds

        type Image = {
            isLit            : bool
            pointsOfInterest : Point Set
            boundsOfInterest : Bounds
        }

        let isWhite (img : Image) (p : Point) : bool = 
            if Bounds.contains img.boundsOfInterest p then
                Set.contains p img.pointsOfInterest
            else
                img.isLit

        let ofPoints (pts : Point Set) : Image =
            {
                isLit = false;
                pointsOfInterest = pts;
                boundsOfInterest = pts
                    |> Seq.fold (fun ((minX, minY), (maxX, maxY)) (x, y) ->
                        ((min minX x), (min minY y)), ((max maxX x), (max maxY y))
                    ) Bounds.zero
                    |> Bounds.grow 1
            }
