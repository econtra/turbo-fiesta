#I "packages/FSharp.Charting"
#load "FSharp.Charting.fsx"

open FSharp.Charting
open System

Chart.Line [for x in 0 .. 10 -> x, x*x]