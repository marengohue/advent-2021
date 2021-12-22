namespace Advent.Day22

module Cuboid =
   
    type Vector3 = (int*int*int)
    
    type Bounds3 = (Vector3*Vector3)
    
    let isValid (((minX, minY, minZ), (maxX, maxY, maxZ)) : Bounds3) : bool =
        minX < maxX && minY < maxY && minZ < maxZ
    
    let intersects (a : Bounds3) (b : Bounds3) : bool =
        let coordIntersects (aMin, aMax) (bMin, bMax) =
            (aMin >= bMin && aMin < bMax) || (bMin >= aMin && bMin < aMax)

        let ((aMinX, aMinY, aMinZ), (aMaxX, aMaxY, aMaxZ)) = a
        let ((bMinX, bMinY, bMinZ), (bMaxX, bMaxY, bMaxZ)) = b
        
        coordIntersects (aMinX, aMaxX) (bMinX, bMaxX)
        && coordIntersects (aMinY, aMaxY) (bMinY, bMaxY)
        && coordIntersects (aMinZ, aMaxZ) (bMinZ, bMaxZ)

    let coordChunk (limMin, limMax) (projMin, projMax) =
        let clampInLim = Util.clamp limMin limMax
        seq {
            limMin, clampInLim projMin
            clampInLim projMin, clampInLim projMax
            clampInLim projMax, limMax
        }
        |> Seq.filter (fun (a, b) -> a <> b)

    let volume (a : Bounds3) : uint64 =
        let ((aMinX, aMinY, aMinZ), (aMaxX, aMaxY, aMaxZ)) = a
        (aMaxX - aMinX) |> uint64
        |> (*) ((aMaxY - aMinY) |> uint64)
        |> (*) ((aMaxZ - aMinZ) |> uint64)

    let subdivide (a : Bounds3) (b : Bounds3) : Bounds3 Set =
        let ((aMinX, aMinY, aMinZ), (aMaxX, aMaxY, aMaxZ)) = a
        let ((bMinX, bMinY, bMinZ), (bMaxX, bMaxY, bMaxZ)) = b

        let xChunks = coordChunk (aMinX, aMaxX) (bMinX, bMaxX)
        let yChunks = coordChunk (aMinY, aMaxY) (bMinY, bMaxY)
        let zChunks = coordChunk (aMinZ, aMaxZ) (bMinZ, bMaxZ)

        Util.cartesian3 xChunks yChunks zChunks
        |> List.map (fun ((xMin, xMax), (yMin, yMax), (zMin, zMax)) -> (xMin, yMin, zMin), (xMax, yMax, zMax))
        |> List.filter isValid
        |> Set.ofList

    let cut (a : Bounds3) (b : Bounds3) : (Bounds3*Bounds3 Set) =
        let subdivisionAb = subdivide a b
        let subdivisionBa = subdivide b a
        
        // resulting set will contain only a single item
        let cutOut = Set.intersect subdivisionAb subdivisionBa

        (cutOut |> Set.minElement, (Set.difference subdivisionAb cutOut))
