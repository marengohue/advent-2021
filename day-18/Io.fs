module Io

open FSharp.Data
open System
open Types

let rec show (a : SnailNum) : string =
    match a with
    | Single(x) -> x.ToString()
    | Pair(x, y) -> $"[{show x},{show y}]"

let parse (s : string) : SnailNum option =
    let rec parseNode = function
    | JsonValue.Array [| left; right |] ->
        let leftNum = left |> parseNode
        let rightNum = right |> parseNode
        match (leftNum, rightNum) with
        | Some(l), Some(r) -> (Some <| Pair(l, r))
        | _ -> Option.None
        
    | JsonValue.Number num ->
        Single(num |> int)
        |> Option.Some
    
    | _ -> Option.None
    
    s |> JsonValue.Parse |> parseNode

let stdinStream = Seq.initInfinite (fun _ -> Console.ReadLine())

let read inputSeq parser =
   inputSeq
   |> Seq.takeWhile (String.IsNullOrEmpty >> not)
   |> Seq.indexed
   |> Seq.map (fun (a, b) -> parser a b)
   |> Seq.choose id
   |> Seq.toList
