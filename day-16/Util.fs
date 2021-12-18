module Util

let takeUntil predicate s = 
    let rec inner ss = seq {
        if Seq.isEmpty ss = false then
            yield ss |> Seq.head
            if ss |> Seq.head |> predicate then
                yield! ss |> Seq.skip 1 |> inner
        }

    inner s
