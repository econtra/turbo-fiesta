#I "...\packages\FSharp.Charting.2.1.0"
#load "FSharp.Charting.2.1.0\FSharp.Charting.fsx"

open FSharp.Charting

Chart.Line [ for x in 1.0 .. 100.0 -> (x, x ** 2.0) ]
Chart.Line [ for x in 1.0 .. 100.0 -> x * x * sin x ]

