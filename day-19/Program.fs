open Advent.Day19

open Advent.Day19.Point.Point3

let recenter pointSet newOrigin =
    pointSet |> Set.map ((@-) newOrigin)

let rotateSet pointSet (_, rotFn) =
    pointSet |> Set.map rotFn

let findCommonPointWithRot overlapCount set1 set2 =
    set1
    |> Seq.map (fun newOriginS1 ->
        let recentered = recenter set1 newOriginS1
        (newOriginS1, recentered)
    )
    |> Seq.map (fun (newOriginS1, recenteredS1) ->
        set2
        |> Seq.collect (fun newOriginS2 ->
            enumerateRotationFns
                |> Seq.map (fun rot -> (rot, rotateSet set2 rot))
                |> Seq.map (fun ((rotName, rotFn), rotatedSet) ->
                    let rotatedOrigin = newOriginS2 |> rotFn 
                    ((rotName, rotFn), newOriginS2, (recenter rotatedSet rotatedOrigin))
                )
        )
        |> Seq.tryFind (fun (_, _, recenteredS2) ->
            (Set.intersect recenteredS1 recenteredS2)
            |> Set.count
            |> ((=) overlapCount)
        )
        |> Option.map (fun (rot, originS2, _) -> (rot, newOriginS1, originS2))
    )
    |> Seq.choose id
    |> Seq.tryHead

let overlapInSets = 12

let remapWithOrigin = function
| (setToRemap, Some((_, rotFn), p1, p2)) ->
    let delta = p1 @- (rotFn p2)
    // Delta is scanner position relative to origin
    delta, Set.map (rotFn >> (@+) delta) setToRemap
| _ -> failwith "Invalid data"

let mapOut (sets : Set<Point3> list) =
    let rec loop matched refs toMatch scanners =
        match refs with
        | nextRef :: remainingRefs when not (List.isEmpty toMatch) ->
            printf "%A left to map...\n" (List.length toMatch)
            let (mapped, nonMapped) =
                toMatch
                |> Seq.map (fun s -> (s, findCommonPointWithRot overlapInSets nextRef s))
                |> Util.partition (snd >> Option.isSome)

            let nextMatched = nextRef :: matched

            let remappedSetsWithScannerPos = mapped |> List.map remapWithOrigin

            let nextRefs = remainingRefs @ (remappedSetsWithScannerPos |> List.map snd)
            let nextToMatch = nonMapped |> List.map fst
            let nextScanners = scanners @ (remappedSetsWithScannerPos |> List.map fst)
                
            loop nextMatched nextRefs nextToMatch nextScanners
            
        | _ when (List.isEmpty toMatch) -> matched @ refs, [], [], scanners
        | _ ->
            failwith "Can't map out the sets."

    let origin = List.head sets
    let remaining = List.tail sets

    let (matched, _, _, scanners) = loop [] [origin] remaining [Point3(0, 0, 0)]
    matched |> Set.ofList |> Set.unionMany, scanners
    
let scanners =
    Seq.initInfinite id
    |> Seq.map (fun _ -> Io.read Io.stdinStream Io.parsePoint3)
    |> Seq.takeWhile (List.isEmpty >> not)
    |> Seq.map (Seq.toArray >> Set.ofArray)
    //|> Seq.take 2
    |> Seq.toList

let beacons, scannerPos = mapOut scanners

printf "Total beacons: %A\n" (Set.count beacons)

Util.cartesian (scannerPos, scannerPos)
    |> Seq.map (fun (a, b) -> a @- b |> manhattan)
    |> Seq.max
    |> printf "Largest distance between two scanners: %A"

