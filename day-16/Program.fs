open System
open System.Globalization

type Op =
    | Sum
    | Prod
    | Min
    | Max
    | Gt
    | Lt
    | Eq

type Chunk =
    | ThreeBit of byte
    | FiveBit of byte
    | OneBit of byte
    | FifteenBit of uint16
    | ElevenBit of uint16

type BitsPacket =
    | Operator of uint * Op * BitsPacket seq
    | Literal of uint * uint

let parseBitstream _ (line: string) : (byte seq option) =
    line.ToCharArray()
    |> Seq.map (fun char -> UInt32.Parse(char.ToString(), NumberStyles.HexNumber) |> byte)
    |> Seq.collect (fun b ->
        seq {
           b &&& 8uy;
           b &&& 4uy;
           b &&& 2uy;
           b &&& 1uy;
        }
        |> Seq.map (fun bit -> if bit > 0uy then 1uy else 0uy)
    )
    |> Some

let take (x : int) (transform) (from : byte seq) : ('a option * byte seq) =
    let rest = from |> Seq.skip x
    let result =
        from
        |> Seq.truncate x
        |> Seq.toList
        |> (fun bits ->
            if List.length bits = x then
                seq { (x - 1) .. -1 .. 0 }
                |> Seq.sumBy (fun bitIdx -> (uint64 <| (List.item (x - bitIdx - 1) bits)) <<< bitIdx)
                |> transform
                |> Some
            else
                None
        )

    result, rest

let take11 = take 11 (uint16 >> ElevenBit)
let take15 = take 15 (uint16 >> FifteenBit)
let take5 = take 5 (byte >> FiveBit)
let take3 = take 3 (byte >> ThreeBit)
let take1 = take 1 (byte >> OneBit)

let rec versionSum =
    function
    | Literal(v, _) -> v
    | Operator(v, _, packs) -> v + (Seq.sumBy versionSum packs)

let rec eval =
    function
    | Literal(_, value) -> value |> uint64
    | Operator(_, op, packs) ->
        packs
        |> match op with
            | Sum -> Seq.sumBy eval
            | Prod -> Seq.fold (fun acc p -> acc * eval p) 1UL
            | Min -> Seq.map eval >> Seq.min
            | Max -> Seq.map eval >> Seq.max
            | Gt -> fun p ->
                if Seq.length p <> 2 then printf "DAMN!!!\n"
                if (p |> Seq.head |> eval) > (p |> Seq.skip 1 |> Seq.head |> eval) then 1UL else 0UL
            | Lt -> fun p ->
                if Seq.length p <> 2 then printf "DAMN!!!\n"
                if (p |> Seq.head |> eval) < (p |> Seq.skip 1 |> Seq.head |> eval) then 1UL else 0UL
            | Eq -> fun p ->
                if Seq.length p <> 2 then printf "DAMN!!!\n"
                if (p |> Seq.head |> eval) = (p |> Seq.skip 1 |> Seq.head |> eval) then 1UL else 0UL

let parseTypeId =
    function
    | 0uy -> Sum
    | 1uy -> Prod
    | 2uy -> Min
    | 3uy -> Max
    | 5uy -> Gt
    | 6uy -> Lt
    | 7uy -> Eq
    | _ -> failwith "invalid opcode"

let rec parsePacketStream (bits: byte seq) =
    let safeParsePacketStream (bitStream : byte seq) : (BitsPacket option * byte seq) =
        try
            bitStream
            |> parsePacketStream
            |> (fun (pack, rest) -> Some pack, rest)
        with
            | _ -> None, bits

    let packContents bits =
        Seq.initInfinite (fun it -> it)
        |> Seq.scan (fun (_, rest) _ -> take5 rest) (None, bits)
        |> Seq.skip 1
        |> Util.takeUntil (fun (result, _) ->
            printf "%A\n" result
            match result with
            | Some(FiveBit(b)) -> (b &&& 16uy) <> 0uy
            | _ -> false
        )
        |> Seq.map (fun (chunk, bits) -> chunk.Value, bits)
        |> Seq.fold (fun (acc, _) (chunk, rest) ->
            let fourBits =
                match chunk with
                | FiveBit(b) -> b &&& 15uy
                | _ -> failwith "Weird data"
                     
            acc * 16u + (fourBits |> uint), rest
        ) (0u, Seq.empty)

    let getSubPackets rest =
        let lengthIdBits, rest = take1 rest
        match lengthIdBits with
            | Some(OneBit(0uy)) -> 
                let bitsLen, rest = take15 rest
                let amountOfBits =
                    match bitsLen with
                    | Some(FifteenBit(bitsLen)) ->
                        bitsLen
                    | _ -> failwith "weird bit length"

                let remainingSeq = rest |> Seq.take (amountOfBits |> int)
                let rest = rest |> Seq.skip (amountOfBits |> int)
                
                let seqPackets =
                    Seq.initInfinite id
                    |> Seq.scan (fun (_, restOf) _ -> safeParsePacketStream restOf) (None, remainingSeq)
                    |> Seq.skip 1
                    |> Seq.takeWhile (fst >> Option.isSome)
                    |> Seq.choose fst
                seqPackets, rest
                
            | Some(OneBit(1uy)) ->
                let packetsLen, rest = take11 rest
                let amountOfPacks =
                    match packetsLen with
                    | Some(ElevenBit(plen)) -> plen
                    | _ -> failwith "weird packet len"
                let packsWithRest =
                    Seq.init (amountOfPacks |> int) id
                    |> Seq.scan (fun (_, restOf) _ -> safeParsePacketStream restOf) (None, rest)
                    |> Seq.skip 1
                let packs = packsWithRest |> Seq.choose fst
                let rest = packsWithRest |> Seq.last |> snd
                packs, rest
                
            | _ -> failwith "weird subpacket length"

    let versionBits, rest = take3 bits
    let typeIdBits, rest = take3 rest
    let version =
        match versionBits with
        | Some(ThreeBit(v)) -> v |> uint
        | _ -> failwith "invalid header"

    match typeIdBits with
    | Some(ThreeBit(4uy)) ->
        let content, rest = packContents rest
        let res = Literal(version, content)
        res, rest
    | Some(ThreeBit(typeId)) ->
        let subPackets, rest = getSubPackets rest
        let op = typeId |> parseTypeId
        let res = Operator(version, op, subPackets)
        res, rest
    | _ -> failwith "invalid pack type id"

let packetStream =
    Input.read Input.stdinStream parseBitstream
    |> Seq.head
    |> parsePacketStream

let result = packetStream |> fst
printf "%A" result
printf "%A" (packetStream |> snd)
// printf "Version sum %A\n" (packetStream |> fst |> versionSum)
printf "Eval result %A\n" (result |> eval)
