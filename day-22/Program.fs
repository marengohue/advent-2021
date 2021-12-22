open Advent.Day22

let inputCuboids = Input.read Input.stdinStream Input.parseCuboidOp

let (small, large) =
    inputCuboids
    |> Util.partition (fun (_, ((xMin, yMin, zMin), (xMax, yMax, zMax))) -> 
        abs xMin <= 50 && abs xMax <= 51 && abs yMin <= 50 && abs yMax <= 51 && abs zMin <= 50 && abs zMax <= 51
    )

let applyCuboids =
    Seq.fold (fun allCuboids (isOn, nextCuboid) ->
        let (intersectedCuboids, restOfCuboids) = allCuboids |> Util.partition (Cuboid.intersects nextCuboid)
        let placeNewCuboid cuboid affected =
            let rec loop toPlace affected resolved =
                match toPlace with
                | tp :: restTp ->
                    match affected |> List.tryFind (Cuboid.intersects tp) with
                    | Some(intersected) ->
                        let (_, pieces) = Cuboid.cut tp intersected
                        loop (restTp @ (pieces |> Set.toList)) affected resolved
                    | None -> loop restTp affected (tp :: resolved)
                | [] -> resolved @ affected

            loop [cuboid] affected []

        if not isOn then
            intersectedCuboids
            |> Seq.collect (fun intersected ->
                let (_, remainder) = Cuboid.cut intersected nextCuboid
                remainder
            )
            |> Set.ofSeq
            |> Set.union (restOfCuboids |> Set.ofList)
        else
            placeNewCuboid nextCuboid intersectedCuboids
            |> Set.ofList
            |> Set.union (restOfCuboids |> Set.ofList)
    )

let smallApplicationResult =
    small
    |> applyCuboids Set.empty

smallApplicationResult |> Seq.sumBy Cuboid.volume |> printf "%A\n"

let fullApplicationResult =
    large
    |> applyCuboids smallApplicationResult

fullApplicationResult |> Seq.sumBy Cuboid.volume |> printf "%A\n"

