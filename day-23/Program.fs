open Advent.Day23
open Data
open Distances
open System

let initialTest = {
    hallwayLeft = Empty
    hallwayRight = Empty
    abConnector = None
    bcConnector = None
    cdConnector = None

    roomA = RoomStack.Four(Bronze, Desert, Desert, Copper)
    roomB = RoomStack.Four(Desert, Bronze, Copper, Bronze)
    roomC = RoomStack.Four(Amber, Amber, Bronze, Desert)
    roomD = RoomStack.Four(Copper, Copper, Amber, Amber)
}

let isBlocked fromRoom toRoom state =
    match fromRoom with
    | Amber -> 
        match toRoom with
        | Amber -> false
        | Bronze -> state.abConnector <> None
        | Copper -> state.abConnector <> None || state.bcConnector <> None
        | Desert -> state.abConnector <> None || state.bcConnector <> None || state.cdConnector <> None
    | Bronze -> 
        match toRoom with
        | Amber -> state.abConnector <> None
        | Bronze -> false
        | Copper -> state.bcConnector <> None
        | Desert -> state.bcConnector <> None || state.cdConnector <> None
    | Copper -> 
        match toRoom with
        | Amber -> state.abConnector <> None || state.bcConnector <> None
        | Bronze -> state.bcConnector <> None
        | Copper -> false
        | Desert -> state.cdConnector <> None
    | Desert -> 
        match toRoom with
        | Amber -> state.abConnector <> None || state.bcConnector <> None || state.cdConnector <> None
        | Bronze -> state.bcConnector <> None || state.cdConnector <> None
        | Copper -> state.cdConnector <> None
        | Desert -> false

let possibleMovesIntoRoom from forAmphipod state =
    [| Amber; Bronze; Copper; Desert |]
    |> Array.filter (fun targetRoom ->
        forAmphipod = targetRoom
        |> (&&) (
            let room = getRoom state targetRoom
            match room with
            | RoomStack.Four(_) -> false
            | RoomStack.One(x) -> x = forAmphipod && targetRoom = forAmphipod
            | RoomStack.Two(x1, x2) -> x1 = forAmphipod && x2 = forAmphipod && targetRoom = forAmphipod
            | RoomStack.Three(x1, x2, x3) -> x1 = forAmphipod && x2 = forAmphipod && x3 = forAmphipod && targetRoom = forAmphipod
            | _ -> true
        )
        |> (&&) (not (isBlocked from targetRoom state)))
    |> Array.map (fun target -> ToRoom target)

let possibleMovesIntoConnectors from state =
    [
        if Option.isNone state.abConnector && not (isBlocked from Amber state) then
            yield ToAbHallway
    
        if Option.isNone state.bcConnector && not (isBlocked from Bronze state) then
            yield ToBcHallway
    
        if Option.isNone state.cdConnector && not (isBlocked from Copper state) then
            yield ToCdHallway
    ]

let possibleMovesIntoHallway from state =
    [
        yield
            match state.hallwayLeft with
            | HallwayStack.Two(_, _) -> None
            | _ -> if (isBlocked from Amber state) then None else Some(ToLeftHallway)

        yield
            match state.hallwayRight with
            | HallwayStack.Two(_, _) -> None
            | _ -> if (isBlocked from Desert state) then None else Some(ToRightHallway)
    ]
    |> List.choose id


let movesOutOfRoom roomId state =
    let roomStack = getRoom state roomId
    match roomStack with
    | RoomStack.Empty -> []
    | RoomStack.One(a) when a = roomId -> []
    | RoomStack.Two(a, b) when a = roomId && b = roomId -> []
    | RoomStack.Three(a, b, c) when a = roomId && b = roomId && c = roomId -> []
    | RoomStack.Four(a, b, c, d) when a = roomId && b = roomId && c = roomId && d = roomId -> []
    | _ -> 
        let mover = getStackTop roomStack |> Option.get
        let result = [
            let roomMoves = possibleMovesIntoRoom roomId mover state
            if Array.isEmpty roomMoves then
                yield! possibleMovesIntoConnectors roomId state
                yield! possibleMovesIntoHallway roomId state
            else
                yield! roomMoves
        ]
        result |> List.map (fun dst -> FromRoom roomId, dst)

let getHallwayNextTo state room =
    match room with
    | Amber -> state.hallwayLeft
    | Desert -> state.hallwayRight
    | _ -> failwith "no room there"

let movesOutOfHallway state roomPos moveSrc : Move list =
    let hallway = getHallwayNextTo state roomPos
    match hallway with
    | HallwayStack.Empty -> []
    | _ ->
        let topOfStack = getHallwayStackTop hallway |> Option.get
        let result = [
            yield! possibleMovesIntoRoom roomPos topOfStack state
        ]
        result |> List.map (fun dst -> moveSrc, dst)

let movesOutOfConnectors (state : BoardState) =
    seq {
        state.abConnector, Amber, Bronze, FromAbHallway
        state.bcConnector, Bronze, Copper, FromBcHallway
        state.cdConnector, Copper, Desert, FromCdHallway
    }
    |> Seq.choose (fun (maybeConnector, left, right, src) -> 
        match maybeConnector with
        | Some(connector) -> Some(connector, left, right, src)
        | None -> None
    )
    |> Seq.collect (fun (connector, left, right, src) ->
        [
            yield! possibleMovesIntoRoom left connector state
            yield! possibleMovesIntoRoom right connector state
        ]
        |> Seq.map(fun dst -> src, dst)
    )

let legalMoves (state : BoardState) : Move list =
    [
        yield! movesOutOfRoom Amber state
        yield! movesOutOfRoom Bronze state
        yield! movesOutOfRoom Copper state
        yield! movesOutOfRoom Desert state
        yield! movesOutOfHallway state Amber FromLeftHallway
        yield! movesOutOfHallway state Desert FromRightHallway
        yield! movesOutOfConnectors state
    ]

let isComplete state =
    state.roomA = RoomStack.Four(Amber, Amber, Amber, Amber)
    |> ((&&) (state.roomB = RoomStack.Four(Bronze, Bronze, Bronze, Bronze)))
    |> ((&&) (state.roomC = RoomStack.Four(Copper, Copper, Copper, Copper)))
    |> ((&&) (state.roomD = RoomStack.Four(Desert, Desert, Desert, Desert)))


let emptyRoom state roomId =
    let roomState = getRoom state roomId
    match roomState with
    | RoomStack.One(x) -> 
        match roomId with
        | Amber -> x, { state with roomA = RoomStack.Empty }
        | Bronze -> x, { state with roomB = RoomStack.Empty }
        | Copper -> x, { state with roomC = RoomStack.Empty }
        | Desert -> x, { state with roomD = RoomStack.Empty }
    | RoomStack.Two(remain, x) ->
        match roomId with
        | Amber -> x, { state with roomA = RoomStack.One(remain) }
        | Bronze -> x, { state with roomB = RoomStack.One(remain) }
        | Copper -> x, { state with roomC = RoomStack.One(remain) }
        | Desert -> x, { state with roomD = RoomStack.One(remain) }
    | RoomStack.Three(r1, r2, x) ->
        match roomId with
        | Amber -> x, { state with roomA = RoomStack.Two(r1, r2) }
        | Bronze -> x, { state with roomB = RoomStack.Two(r1, r2) }
        | Copper -> x, { state with roomC = RoomStack.Two(r1, r2) }
        | Desert -> x, { state with roomD = RoomStack.Two(r1, r2) }
    | RoomStack.Four(r1, r2, r3, x) ->
        match roomId with
        | Amber -> x, { state with roomA = RoomStack.Three(r1, r2, r3) }
        | Bronze -> x, { state with roomB = RoomStack.Three(r1, r2, r3) }
        | Copper -> x, { state with roomC = RoomStack.Three(r1, r2, r3) }
        | Desert -> x, { state with roomD = RoomStack.Three(r1, r2, r3) }
    | RoomStack.Empty -> failwith "cant empty room"

let emptyHallway state hallway left =
    match hallway with
    | HallwayStack.One(x) -> 
        x, if left then { state with hallwayLeft = HallwayStack.Empty } else { state with hallwayRight = HallwayStack.Empty }
    | HallwayStack.OneInTheBack(x) -> 
        x, if left then { state with hallwayLeft = HallwayStack.Empty } else { state with hallwayRight = HallwayStack.Empty }
    | HallwayStack.Two(remain, x) ->
        x, if left then { state with hallwayLeft = HallwayStack.OneInTheBack(remain) } else { state with hallwayRight = HallwayStack.OneInTheBack(remain) }
    | _ -> failwith "cant empty hallway"

let emptyConnector state where =
    match where with
    | FromAbHallway -> state.abConnector |> Option.get, { state with abConnector = None }
    | FromBcHallway -> state.bcConnector |> Option.get, { state with bcConnector = None }
    | FromCdHallway -> state.cdConnector |> Option.get, { state with cdConnector = None }
    | _ -> failwith "cant empty connector"

let moveIntoRoom state targetRoom mover =
    let room = getRoom state targetRoom
    match room with
    | RoomStack.Empty ->
        match targetRoom with
        | Amber -> { state with roomA = RoomStack.One(mover) }
        | Bronze -> { state with roomB = RoomStack.One(mover) }
        | Copper -> { state with roomC = RoomStack.One(mover) }
        | Desert -> { state with roomD = RoomStack.One(mover) }
    | RoomStack.One(x) -> 
        match targetRoom with
        | Amber -> { state with roomA = RoomStack.Two(x, mover) }
        | Bronze -> { state with roomB = RoomStack.Two(x, mover) }
        | Copper -> { state with roomC = RoomStack.Two(x, mover) }
        | Desert -> { state with roomD = RoomStack.Two(x, mover) }
    | RoomStack.Two(x1, x2) -> 
        match targetRoom with
        | Amber -> { state with roomA = RoomStack.Three(x1, x2, mover) }
        | Bronze -> { state with roomB = RoomStack.Three(x1, x2, mover) }
        | Copper -> { state with roomC = RoomStack.Three(x1, x2, mover) }
        | Desert -> { state with roomD = RoomStack.Three(x1, x2, mover) }
    | RoomStack.Three(x1, x2, x3) ->
        match targetRoom with
        | Amber -> { state with roomA = RoomStack.Four(x1, x2, x3, mover) }
        | Bronze -> { state with roomB = RoomStack.Four(x1, x2, x3, mover) }
        | Copper -> { state with roomC = RoomStack.Four(x1, x2, x3, mover) }
        | Desert -> { state with roomD = RoomStack.Four(x1, x2, x3, mover) }
    | RoomStack.Four(_) -> failwith "Cant move into full room"
    
let applyMove state (src, dst) =
    let mover, moveStartedState = 
        match src with
        | FromRoom(roomId) -> emptyRoom state roomId
        | FromLeftHallway -> emptyHallway state state.hallwayLeft true
        | FromRightHallway -> emptyHallway state state.hallwayRight false
        | _ -> emptyConnector state src

    match dst with
    | ToRoom(targetRoom) -> moveIntoRoom moveStartedState targetRoom mover
    | ToLeftHallway -> 
        match moveStartedState.hallwayLeft with
        | HallwayStack.Empty -> { moveStartedState with hallwayLeft = HallwayStack.One(mover) }
        | HallwayStack.One(x) -> { moveStartedState with hallwayLeft = HallwayStack.Two(x, mover) }
        | HallwayStack.OneInTheBack(x) -> { moveStartedState with hallwayLeft = HallwayStack.Two(x, mover) }
        | _ -> failwith "Couldn't move into left hallway"
    | ToRightHallway -> 
    match moveStartedState.hallwayRight with
        | HallwayStack.Empty -> { moveStartedState with hallwayRight = HallwayStack.One(mover) }
        | HallwayStack.One(x) -> { moveStartedState with hallwayRight = HallwayStack.Two(x, mover) }
        | HallwayStack.OneInTheBack(x) -> { moveStartedState with hallwayRight = HallwayStack.Two(x, mover) }
        | _ -> failwith "Couldn't move into hallway"
    | ToAbHallway -> if state.abConnector.IsNone then { moveStartedState with abConnector = Some(mover) } else failwith "ab occupied"
    | ToBcHallway -> if state.bcConnector.IsNone then { moveStartedState with bcConnector = Some(mover) } else failwith "bc occupied"
    | ToCdHallway -> if state.cdConnector.IsNone then { moveStartedState with cdConnector = Some(mover) } else failwith "cd occupied"

let solve initialState = 
    let mutable lowestEnergy = UInt64.MaxValue

    let rec loop currentPath currentState currentPathEnergy =
        if isComplete currentState then
            lowestEnergy <- min lowestEnergy currentPathEnergy
            Some(currentPath, currentPathEnergy)
        else
            let moves = legalMoves currentState 
            if List.isEmpty moves then
                None
            else 
                let result = 
                    moves
                    |> List.map(fun move ->
                        let moveEnergy = Distances.moveEnergy currentState move
                        if currentPathEnergy + moveEnergy > lowestEnergy then
                            None
                        else
                            loop (move :: currentPath) (applyMove currentState move) (currentPathEnergy + moveEnergy)
                    )
                    |> List.choose id

                if List.isEmpty result then
                    None
                else 
                    result |> List.minBy snd |> Some
                

    loop [] initialState 0UL

let draw state =
    let amphipod = function
    | Amber -> 'A'
    | Bronze -> 'B'
    | Copper -> 'C'
    | Desert -> 'D'

    let hallwayState hallway =
        match hallway with
        | OneInTheBack(a) -> amphipod a, '.'
        | One(a) -> '.', amphipod a
        | Two(a, b) -> amphipod a, amphipod b
        | Empty -> '.', '.'

    let roomState room =
        match room with
        | RoomStack.One(a) -> amphipod a, '.', '.', '.'
        | RoomStack.Two(a, b) -> amphipod a, amphipod b, '.', '.'
        | RoomStack.Three(a, b, c) -> amphipod a, amphipod b, amphipod c, '.'
        | RoomStack.Four(a, b, c, d) -> amphipod a, amphipod b, amphipod c, amphipod d
        | RoomStack.Empty -> '.', '.', '.', '.'

    let (farLeft, left) = hallwayState state.hallwayLeft
    let (farRight, right) = hallwayState state.hallwayRight
    let (fa, sa, ta, la) = roomState state.roomA
    let (fb, sb, tb, lb) = roomState state.roomB
    let (fc, sc, tc, lc) = roomState state.roomC
    let (fd, sd, td, ld) = roomState state.roomD

    let ab = state.abConnector |> Option.map (amphipod) |> Option.defaultValue '.'
    let bc = state.bcConnector |> Option.map (amphipod) |> Option.defaultValue '.'
    let cd = state.cdConnector |> Option.map (amphipod) |> Option.defaultValue '.'

    printf "%c%c.%c.%c.%c.%c%c\n" farLeft left ab bc cd right farRight
    printf "##%c#%c#%c#%c##\n" la lb lc ld
    printf "##%c#%c#%c#%c##\n" ta tb tc td
    printf "##%c#%c#%c#%c##\n" sa sb sc sd
    printf "##%c#%c#%c#%c##\n" fa fb fc fd
    printf "\n"

let path, score = solve initialTest

printf "score: %A\n" score

path
|> Seq.rev
|> Seq.fold (fun state move ->
    let moveEnergy = Distances.moveEnergy state move
    let nextState = applyMove state move
    printf "Move nrg: %A\n" moveEnergy
    draw nextState |> ignore

    nextState
) initialTest
|> ignore