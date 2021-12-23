namespace Advent.Day23

module Distances =

    open Data

    let moverCost = function
    | Amber -> 1
    | Bronze -> 10
    | Copper -> 100
    | Desert -> 1000

    let baseDistanceMatrix =
        [|
            [| 0;2;2;4;4;6;6;8;8 |]
            [| 2;0;2;4;4;6;6;8;8 |]
            [| 2;2;0;2;2;4;4;6;6 |]
            [| 4;4;2;0;2;4;4;6;6 |]
            [| 4;4;2;2;0;2;2;4;4 |]
            [| 6;6;4;4;2;0;2;4;4 |]
            [| 6;6;4;4;2;2;0;2;2 |]
            [| 8;8;6;6;4;4;2;0;2 |]
            [| 8;8;6;6;4;4;2;2;0 |]
        |]

    let baseDistance src dst =
        let srcId src =
            match src with
            | FromLeftHallway -> 0
            | FromRoom(Amber) -> 1
            | FromAbHallway -> 2
            | FromRoom(Bronze) -> 3
            | FromBcHallway -> 4
            | FromRoom(Copper) -> 5
            | FromCdHallway -> 6
            | FromRoom(Desert) -> 7
            | FromRightHallway -> 8

        let dstId dst =
            match dst with
            | ToLeftHallway -> 0
            | ToRoom(Amber) -> 1
            | ToAbHallway -> 2
            | ToRoom(Bronze) -> 3
            | ToBcHallway -> 4
            | ToRoom(Copper) -> 5
            | ToCdHallway -> 6
            | ToRoom(Desert) -> 7
            | ToRightHallway -> 8

        baseDistanceMatrix[srcId src][dstId dst]

    let extraEnergySrc state src mover =
        match src with
        | FromRoom(roomId) ->
            let room = Data.getRoom state roomId
            match room with
            | RoomStack.One(_) -> 3 * moverCost mover
            | RoomStack.Two(_) -> 2 * moverCost mover
            | RoomStack.Three(_) -> moverCost mover
            | _ -> 0
        | FromLeftHallway ->
            match state.hallwayLeft with
            | HallwayStack.OneInTheBack(_) -> moverCost mover
            | _ -> 0
        | FromRightHallway ->
            match state.hallwayRight with
            | HallwayStack.OneInTheBack(_) -> moverCost mover
            | _ -> 0
        | _ -> 0

    let extraEnergyDst state dst mover =
        match dst with
        | ToRoom(roomId) ->
            let room = Data.getRoom state roomId
            match room with
            | RoomStack.Empty -> 3 * moverCost mover
            | RoomStack.One(_) -> 2 * moverCost mover
            | RoomStack.Two(_) -> 1 * moverCost mover
            | _ -> 0
        | ToLeftHallway ->
            match state.hallwayLeft with
            | HallwayStack.One(present) -> moverCost present
            | _ -> 0
        | ToRightHallway ->
            match state.hallwayRight with
            | HallwayStack.One(present) -> moverCost present
            | _ -> 0
        | _ -> 0

    let moveEnergy state (src, dst) =
        let mover =
            match src with
            | FromRoom(r) -> r |> Data.getRoom state |> Data.getStackTop
            | FromLeftHallway -> state.hallwayLeft |> Data.getHallwayStackTop
            | FromRightHallway -> state.hallwayRight |> Data.getHallwayStackTop
            | FromAbHallway -> state.abConnector
            | FromBcHallway -> state.bcConnector
            | FromCdHallway -> state.cdConnector
            |> Option.get

        baseDistance src dst
        |> ((*) (moverCost mover))
        |> ((+) (extraEnergySrc state src mover))
        |> ((+) (extraEnergyDst state dst mover))
        |> uint64