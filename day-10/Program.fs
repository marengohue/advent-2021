open Microsoft.FSharp.Core.LanguagePrimitives

type Token =
  | OpenParen = '('
  | CloseParen = ')'
  | OpenSquareBrace = '['
  | CloseSquareBrace = ']'
  | OpenCurlyBrace = '{'
  | CloseCurlyBrace = '}'
  | OpenTriangleBrace = '<'
  | CloseTriangleBrace = '>'

type ValidationResult =
  | Ok
  | Incomplete of (Token seq)
  | Error of Token

let tokenMap = Map.ofSeq (seq {
  Token.OpenParen, Token.CloseParen;
  Token.OpenSquareBrace, Token.CloseSquareBrace;
  Token.OpenCurlyBrace, Token.CloseCurlyBrace;
  Token.OpenTriangleBrace, Token.CloseTriangleBrace;
})

let errorScores = Map.ofSeq (seq {
  Token.CloseParen, 3UL;
  Token.CloseSquareBrace, 57UL;
  Token.CloseCurlyBrace, 1197UL;
  Token.CloseTriangleBrace, 25137UL;
})

let completionScores = Map.ofSeq (seq {
  Token.CloseParen, 1UL;
  Token.CloseSquareBrace, 2UL;
  Token.CloseCurlyBrace, 3UL;
  Token.CloseTriangleBrace, 4UL
})

let from = EnumOfValue<char, Token>
let isOpen = tokenMap.ContainsKey
let isClosing = errorScores.ContainsKey
let isClosingFor openBracket closeBracket = isOpen openBracket && ((tokenMap.Item openBracket).Equals(closeBracket))

let tokens _ (line: string) =
    line.ToCharArray()
    |> Seq.map from
    |> Option.Some

let median (lst: 'a list) = 
    let medianElement = List.length lst / 2
    List.sort lst |> List.item medianElement
 
let validate tokenStream =
    let completionForStack stack =
        stack |> Seq.map (fun opening -> tokenMap.Item(opening))

    let rec loop tokens stack =
        match tokens with
        // No tokens remaining in the stream. The result is either:
        // - OK if the closing stack is empty
        // - Needs completion otherwise
        | [] ->
            match stack with
            | [] -> Ok
            | _ -> Incomplete (completionForStack stack)

        // Process next token in stream:
        // - Opening tokens go on the stack
        // - Closing tokens validate against stack.
        | t::ts ->
            match t with
            | t when (isOpen t) -> loop ts (t :: stack)

            // Closing tokens are validated:
            // - If stack is empty, then this is an extraneous token (trying to close non-existing opening token)
            // - If top of stack is of different token type, this is a wrong token
            | t when (isClosing t) ->
                match stack with
                | [] -> Error t
                | s :: ss -> if (isClosingFor s t) then (loop ts ss) else Error t

            // This should not happen unless we get some weird config issue in the dictionaries above
            | _ -> failwith "unknown token"

    loop tokenStream []

let validationResults = 
  (Input.read tokens)
    |> Seq.map (Seq.toList >> validate)
    |> Seq.toList

let scoreOfErrors =
  validationResults
    |> Seq.choose (function
      | Error token -> Some token
      | _ -> None
    )
    |> Seq.choose (fun x -> Map.tryFind x errorScores)
    |> Seq.sum

let scoreOfCompletion = Seq.fold (fun acc tok -> 5UL * acc + (completionScores.Item tok)) (0UL)

let scoreOfCompletions =
    validationResults
    |> Seq.choose (function
      | Incomplete tokensToComplete -> Some tokensToComplete
      | _ -> None
    )
    |> Seq.map scoreOfCompletion
    |> Seq.toList
    |> median

printf "Error scores: %A\nCompletions median: %A" scoreOfErrors scoreOfCompletions
