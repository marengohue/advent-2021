let factors = [
    (11, 6, true) // We will pop stack to get rid of the initial 0
    (11, 12, false)
    (15, 8, false)
    (-11, 7, true)
    (15, 7, false)
    (15, 12, false)
    (14, 2, false)
    (-7, 15, true)
    (12, 4, false)
    (-6, 5, true)
    (-10, 12, true)
    (-15, 11, true)
    (-9, 13, true)
    (0, 7, true)
]

let validDigitFns = [
    fun _ -> [9 .. -1 .. 1]
    fun _ -> [9 .. -1 .. 1]
    fun _ -> [9 .. -1 .. 1]
    fun stackTop -> [stackTop - 11]
    fun _ -> [9 .. -1 .. 1]
    fun _ -> [9 .. -1 .. 1]
    fun _ -> [9 .. -1 .. 1]
    fun stackTop -> [stackTop - 7]
    fun _ -> [9 .. -1 .. 1]
    fun stackTop -> [stackTop - 6]
    fun stackTop -> [stackTop - 10]
    fun stackTop -> [stackTop - 15]
    fun stackTop -> [stackTop - 9]
    fun stackTop -> [stackTop]
 ]

let cartesian xs ys =
    [
        for x in xs do
        for y in ys do
            yield (x, y)
    ]

let stepWithStack (a, b, popStack) stack w =
    let top = List.head stack
    let stack = if popStack then List.tail stack else stack
    if top + a <> w then
        w + b :: stack
    else
        stack

let isValidEntry (stack, digits) =
    digits |> List.forall (fun d -> d > 0 && d < 10)
    &&
    stack |> List.isEmpty

let allMatches =
    List.zip factors validDigitFns
    |> List.fold (fun acc (factors, digitFn) ->
        acc
        |> List.collect (fun (stack, digits) -> 
            digitFn (List.head stack)
            |> List.map (fun digit -> (stepWithStack factors stack digit), digit::digits)
        )
    ) [([0], [])]
    |> List.filter isValidEntry

let getNumber (_, digits) = digits |> List.rev |> List.map string |> String.concat ""

let min = (List.head allMatches |> getNumber)
let max = (Seq.rev allMatches |> Seq.head |> getNumber)

printf "min=%A, max=%A\n" min max


