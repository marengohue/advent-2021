namespace Advent.Day23

module Data =

    type Amphipod = Amber | Bronze | Copper | Desert

    type RoomStack =
    | One of Amphipod
    | Two of Amphipod*Amphipod
    | Three of Amphipod*Amphipod*Amphipod
    | Four of Amphipod*Amphipod*Amphipod*Amphipod
    | Empty

    type HallwayStack =
    | One of Amphipod
    | Two of Amphipod*Amphipod
    | OneInTheBack of Amphipod
    | Empty

    type BoardState = {
        hallwayLeft : HallwayStack
        hallwayRight : HallwayStack
    
        roomA : RoomStack
        roomB : RoomStack
        roomC : RoomStack
        roomD : RoomStack

        abConnector : Amphipod option
        bcConnector : Amphipod option
        cdConnector : Amphipod option
    }

    type MoveSource =
    | FromRoom of Amphipod
    | FromLeftHallway
    | FromRightHallway
    | FromAbHallway
    | FromBcHallway
    | FromCdHallway

    type MoveDestination =
    | ToRoom of Amphipod
    | ToLeftHallway
    | ToRightHallway
    | ToAbHallway
    | ToBcHallway
    | ToCdHallway

    type Move = MoveSource*MoveDestination

    let getRoom state room =
        match room with
        | Amber -> state.roomA
        | Bronze -> state.roomB
        | Copper -> state.roomC
        | Desert -> state.roomD
    
    let getStackTop = function
    | RoomStack.One(x) -> Some(x)
    | RoomStack.Two(_, x) -> Some(x)
    | RoomStack.Three(_, _, x) -> Some(x)
    | RoomStack.Four(_, _, _, x) -> Some(x)
    | _ -> None
    
    let getHallwayStackTop = function
    | HallwayStack.One(x) -> Some(x)
    | HallwayStack.OneInTheBack(x) -> Some(x)
    | HallwayStack.Two(_, x) -> Some(x)
    | _ -> None
    