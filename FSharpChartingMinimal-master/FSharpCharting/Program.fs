open System
open FSharp.Charting

[<EntryPoint>]
let main argv =
    Chart.Line [ for x in 1.0 .. 100.0 -> (x, x ** 2.0) ]
    |> Chart.Show
    0