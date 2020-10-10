﻿#I "...\packages\FSharp.Charting.2.1.0"
#load "FSharp.Charting.2.1.0\FSharp.Charting.fsx"

open FSharp.Charting

Chart.Line [ for x in 1.0 .. 100.0 -> (x, x ** 2.0) ]
Chart.Line [ for x in 1.0 .. 100.0 -> x * x * sin x ]

let inline Ind t x =
    if t <= x then 1.0 else 0.0

//Kontraktparametre
let t_0 = 25.0
let n = 90.0
let R = 65.0
let pi = 80000.0
let b_death = 1000000.0
let b_dis = 50000.0


//Tekniske parametre
let timePoints = 1001
let delta = (n-t_0)/((double timePoints)-1.0)
let time : double array = Array.init timePoints (fun i -> t_0 + (double i) * delta)
time.[time.Length-1]





//Teknisk grundlag G82K 0%

let rTEK = 0.05

let inline m_01TEK (t: double) =
    (0.0004 + 10.0**(4.71609+0.06*t-10.0)) * Ind t R

let inline m_10TEK (t: double) =
    0.0

let inline m_02TEK (t: double)  =
    0.0005 + 10.0**(5.728+0.038*t-10.0)

let inline m_12TEK (t: double) =
    m_02TEK

//Teknisk reserve

let V_1TEK : double[] = Array.create timePoints 0.0
let V_0TEK : double[] = Array.create timePoints 0.0

for i = (timePoints-1) downto 0 do
    if time.[i] > R then V_1TEK.[i] <- 0.0
    else V_1TEK.[i] <- V_1TEK.[i+1] + (V_1TEK.[i+1] * rTEK + b_dis + (m_02TEK time.[i+1]) * b_death) * delta

for i = (timePoints-1) downto 0 do
    if time.[i] > R then V_0TEK.[i] <- 0.0
    else V_0TEK.[i] <- V_0TEK.[i+1] + (V_0TEK.[i+1] * rTEK + (m_01TEK time.[i+1]) * V_1TEK.[i+1]) * delta

//Y'er

let Y_1TEK : double[] = Array.create timePoints 0.0
let Y_0TEK : double[] = Array.create timePoints 0.0

for i = 0 to (timePoints-1) do
    if time.[i] > R then V_1TEK.[i] <- 0.0
    else V_1TEK.[i] <- V_1TEK.[i+1] + (V_1TEK.[i+1] * rTEK + b_dis + (m_02TEK time.[i+1]) * b_death) * delta



    // Andet grundlag

let r = 0.05

let inline m_01 (t: double) =
    (0.0004 + 10.0**(4.54+0.06*t-10.0)) * Ind t R

let inline m_10 (t: double) =
    (2.0058 * System.Math.Exp (-0.117*t)) * Ind t R

let inline m_02 (t: double) =
    0.0005 + 10.0**(5.88+0.038*t-10.0)

let inline m_12 (t: double) =
    m_02 t + (m_02 t) * Ind t R


Chart.Combine(
    [ Chart.Line ([for t in t_0 .. delta .. n -> (t, m_01 t)],Name="m_01")
      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_10 t)],Name="m_10")
      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_02 t)],Name="m_02")
      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_12 t)],Name="m_12")
    ])
    |> Chart.WithLegend(Title="Hej")



let p_00 : double[,] = Array2D.create timePoints timePoints 1.0
let p_01 : double[,] = Array2D.create timePoints timePoints 0.0
let p_02 : double[,] = Array2D.create timePoints timePoints 0.0
let p_10 : double[,] = Array2D.create timePoints timePoints 0.0
let p_11 : double[,] = Array2D.create timePoints timePoints 1.0
let p_12 : double[,] = Array2D.create timePoints timePoints 0.0

let W : double[] = Array.create timePoints 0.0
let a : double[] = Array.create timePoints 0.0
let faktor: double[] = Array.create timePoints 0.0



for i = 0 to (timePoints-1) do
    for j = (i+1) to (timePoints-1) do
        p_00.[i,j] <- p_00.[i,j-1] + (-p_00.[i,j-1] * ((m_01 time.[j-1]) + (m_02 time.[j-1])) + p_01.[i,j-1] * (m_10 time.[j-1])) * delta
        p_11.[i,j] <- p_11.[i,j-1] + (-p_11.[i,j-1] * ((m_10 time.[j-1]) + (m_12 time.[j-1])) + p_10.[i,j-1] * (m_01 time.[j-1])) * delta

        p_02.[i,j] <- p_02.[i,j-1] + (p_00.[i,j-1] * (m_02 time.[j-1]) + p_01.[i,j-1] * (m_12 time.[j-1])) * delta
        p_12.[i,j] <- p_12.[i,j-1] + (p_10.[i,j-1] * (m_02 time.[j-1]) + p_11.[i,j-1] * (m_12 time.[j-1])) * delta

        p_01.[i,j] <- p_01.[i,j-1] + (-p_01.[i,j-1] * ((m_10 time.[j-1]) + (m_12 time.[j-1])) + p_00.[i,j-1] * (m_01 time.[j-1])) * delta
        p_10.[i,j] <- p_10.[i,j-1] + (-p_10.[i,j-1] * ((m_01 time.[j-1]) + (m_02 time.[j-1])) + p_11.[i,j-1] * (m_10 time.[j-1])) * delta

    if i = 0 then W.[i] <- 0.0
    else W.[i] <- W.[i-1] + (W.[i-1] * r + pi) * delta

    // a
for i = (timePoints-1) downto 0 do
    if i = (timePoints-1) then a.[i] <- 0.0
    else a.[i] <- a.[i+1] + (1.0 - a.[i+1] * (r + m_02 time.[i+1])) * delta

    // faktor
for i = 0 to (timePoints-1) do
    if time.[i] < R then faktor.[i] <- 1.0
    else faktor.[i] <- faktor.[i-1] + (faktor.[i-1] * (r - 1.0/a.[i-1])) * delta


Chart.Combine([
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_00.[0,i])],Name="p_00")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_11.[0,i])],Name="p_11")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_02.[0,i])],Name="p_02")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_12.[0,i])],Name="p_12")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_01.[0,i])],Name="p_01")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_10.[0,i])],Name="p_10")
      //Chart.Line ([for i in 0 .. (Array.findIndex (fun t -> t > 65.0) time) -> (time.[i], W.[i])],Name="W")
      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], a.[i])],Name="a")
      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 60.0) time) .. (Array.findIndex (fun t -> >= 90.0) time) -> (time.[i], W.[i]/a.[i])],Name="W/a")
      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], W.[(Array.findIndex (fun t -> t > 65.0) time)])],Name="W")
      Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], faktor.[i])],Name="faktor")
      Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], a.[i])],Name="a")
      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], faktor.[i]*(pi + W.[(Array.findIndex (fun t -> t > 65.0) time)]/a.[(Array.findIndex (fun t -> t > 50.0) time)])/a.[i])],Name="Rsens")
    ])
    |> Chart.WithLegend(Title="Legend")
    |> Chart.WithXAxis(Min = 0.0)