namespace Advent.Day20

module Io =
    open System
    open Types
    open Types.Image

    let stdinStream = Seq.initInfinite (fun _ -> Console.ReadLine())

    let read inputSeq parser =
        inputSeq
        |> Seq.takeWhile (String.IsNullOrEmpty >> not)
        |> Seq.indexed
        |> Seq.map (fun (a, b) -> parser a b)
        |> Seq.choose id
        |> Seq.toList

    let readStdin<'a> = read stdinStream : (int -> string -> 'a option) -> 'a list

    let parseAlgorithm _ (line: string) =
        line.ToCharArray()
        |> Seq.indexed
        |> Seq.choose (fun (idx, pixel) -> if pixel = Types.lightPixel then Some idx else None)
        |> Set.ofSeq
        |> Some

    let parseImageLine lineNo (line : string) =
       line.ToCharArray()
       |> Seq.indexed
       |> Seq.choose (fun (idx, pixel) -> if pixel = Types.lightPixel then Some (idx, lineNo) else None)
       |> Set.ofSeq
       |> Some

    let printImage (img : Image) =
        let (minX, minY), (maxX, maxY) = img.boundsOfInterest
        for y in seq { minY - 5 .. maxY + 5 } do
            for x in seq { minX - 5 .. maxX + 5 } do
                if Image.isWhite img (x, y) then
                    printf "%c" lightPixel
                else
                    printf "%c" darkPixel
            printf "\n"

