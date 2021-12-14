type Polymer = char
type Chain = Polymer list

let templateParser _ (line: string) : Chain option =
  line.ToCharArray()
  |> Seq.toList
  |> Some

let rulesParser _ (line: string) =
    match line.Split(" -> ") |> Seq.toList with
    | input :: insertion :: _ ->
        match input.ToCharArray() |> Seq.toList with
        | c1 :: c2 :: _ -> Some ((c1, c2), (insertion.Chars 0))
        | _ -> None
    | _ -> None

let template = Input.read Input.stdinStream templateParser |> List.head
let insertionRules = Input.read Input.stdinStream rulesParser |> Map.ofSeq

let countResultPairs (steps: int) (chain: Chain) =
    let rec loop step pairCounts =
        match step with
        | 0 -> pairCounts
        | _ -> 
            pairCounts
            |> Seq.collect (fun ((p1, p2), count) ->
                let mapping = Map.find (p1, p2) insertionRules
                seq {
                    ((p1, mapping), count)
                    ((mapping, p2), count)
                }
            )
            |> Seq.groupBy fst
            |> Seq.map (fun (pair, counts) -> (pair, counts |> Seq.sumBy snd))
            |> loop (step - 1)

    let distinctPairs = chain |> Seq.pairwise |> Seq.countBy id
    distinctPairs
        |> Seq.map (fun (p, count) -> (p, count |> bigint))
        |> loop steps

let totalPolyCount (template : Chain) (pairCounts : ((Polymer*Polymer)*bigint) seq) =
    pairCounts
    |> Seq.map (fun (pair, count) -> (fst pair, count))
    |> Seq.groupBy fst
    |> Seq.map (fun (char, counts) ->
        let totalSum = counts |> Seq.sumBy snd
        match char with
        | _ when char.Equals(template |> Seq.last) -> (char, totalSum + (1 |> bigint))
        | _ -> (char, totalSum)
    )

let stepCount = 40
let counts =
    countResultPairs stepCount template
    |> totalPolyCount template
    |> Seq.sortBy snd

printf "%A" ((counts |> Seq.last |> snd) - (counts |> Seq.head |> snd))
