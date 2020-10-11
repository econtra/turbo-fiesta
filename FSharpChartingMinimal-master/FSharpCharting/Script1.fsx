﻿#I "...\packages\FSharp.Charting.2.1.0"
#load "FSharp.Charting.2.1.0\FSharp.Charting.fsx"

open FSharp.Charting

let inline Ind t x =
    if t <= x then 1.0 else 0.0

type Results = {Y0 : double[]; Y1 : double[]; a : double[] ; p00 : double[,] ; p01 : double[,] ; p02 : double[,] ; p10 : double[,] ; p11 : double[,] ; p12 : double[,]}

//Kontraktparametre
let t_0 = 25.0
let n = 90.0
let R = 65.0
let piSensi = 80000.0
let b_death = 1000000.0
let b_dis = 50000.0


//Tekniske parametre
let timePoints = 1001
let delta = (n-t_0)/((double timePoints)-1.0)
let time : double array = Array.init timePoints (fun i -> t_0 + (double i) * delta)
let Rindex = Array.findIndex (fun t -> t > 65.0) time





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



    // Andet grundlag

let rMV = Array.create timePoints 0.03

let inline m_01MV (t: double) =
    (0.0004 + 10.0**(4.54+0.06*t-10.0)) * Ind t R

let inline m_10MV (t: double) =
    (2.0058 * System.Math.Exp (-0.117*t)) * Ind t R

let inline m_02MV (t: double) =
    0.0005 + 10.0**(5.88+0.038*t-10.0)

let inline m_12MV (t: double) =
    m_02MV t + (m_02MV t) * Ind t R


//Chart.Combine(
//    [ Chart.Line ([for t in t_0 .. delta .. n -> (t, m_01MV t)],Name="m_01")
//      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_10MV t)],Name="m_10")
//      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_02MV t)],Name="m_02")
//      Chart.Line ([for t in t_0 .. delta .. n -> (t, m_12MV t)],Name="m_12")
//    ])
//    |> Chart.WithLegend(Title="Hej")






// Fremskriv Y_tilder

let Make pi (r: double[]) (m_01: double -> double) (m_10) (m_02) (m_12) (r_bar: double[]) (mu_bar) =
    let p_00 : double[,] = Array2D.create timePoints timePoints 1.0
    let p_01 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_02 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_10 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_11 : double[,] = Array2D.create timePoints timePoints 1.0
    let p_12 : double[,] = Array2D.create timePoints timePoints 0.0

    let Y_0 : double[] = Array.create timePoints 0.0
    let Y_1 : double[] = Array.create timePoints 0.0
    let a : double[] = Array.create timePoints 0.0

    for i = 0 to (timePoints-1) do
        for j = (i+1) to (timePoints-1) do
            p_00.[i,j] <- p_00.[i,j-1] + (-p_00.[i,j-1] * ((m_01 time.[j-1]) + (m_02 time.[j-1])) + p_01.[i,j-1] * (m_10 time.[j-1])) * delta
            p_11.[i,j] <- p_11.[i,j-1] + (-p_11.[i,j-1] * ((m_10 time.[j-1]) + (m_12 time.[j-1])) + p_10.[i,j-1] * (m_01 time.[j-1])) * delta
    
            p_02.[i,j] <- p_02.[i,j-1] + (p_00.[i,j-1] * (m_02 time.[j-1]) + p_01.[i,j-1] * (m_12 time.[j-1])) * delta
            p_12.[i,j] <- p_12.[i,j-1] + (p_10.[i,j-1] * (m_02 time.[j-1]) + p_11.[i,j-1] * (m_12 time.[j-1])) * delta
    
            p_01.[i,j] <- p_01.[i,j-1] + (-p_01.[i,j-1] * ((m_10 time.[j-1]) + (m_12 time.[j-1])) + p_00.[i,j-1] * (m_01 time.[j-1])) * delta
            p_10.[i,j] <- p_10.[i,j-1] + (-p_10.[i,j-1] * ((m_01 time.[j-1]) + (m_02 time.[j-1])) + p_11.[i,j-1] * (m_10 time.[j-1])) * delta

    for i = (timePoints-1) downto 0 do
        if i = (timePoints-1) then a.[i] <- 0.0
        else a.[i] <- a.[i+1] + (1.0 - a.[i+1] * (r_bar.[i+1] + (mu_bar time.[i+1]))) * delta
    
    for i = 1 to (timePoints-1) do
        Y_0.[i] <- Y_0.[i-1]
                    + ((r.[i-1] + (m_02 time.[i-1]) * 0.0 - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Y_0.[i-1]
                    + p_00.[0,i-1] * ((Ind time.[i-1] R) * pi + 0.0)
                    + (m_10 time.[i-1]) * Y_1.[i-1] - (m_01 time.[i-1]) * Y_0.[i-1] - (m_02 time.[i-1]) * Y_0.[i-1]) * delta

        Y_1.[i] <- Y_1.[i-1]
                    + ((r.[i-1] + (m_12 time.[i-1]) * 0.0 - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Y_1.[i-1]
                    + p_01.[0,i-1] * (0.0 + 0.0)
                    + (m_01 time.[i-1]) * Y_0.[i-1] - (m_10 time.[i-1]) * Y_1.[i-1] - (m_12 time.[i-1]) * Y_1.[i-1]) * delta


    let Y = {Y0 = Y_0; Y1 = Y_1; Results.a = a; p00 = p_00; p01 = p_01; p02 = p_02; p10 = p_10; p11 = p_11; p12 = p_12}
    Y




let rMV_bar = Array.create timePoints 0.06

let YMV = Make piSensi rMV m_01MV m_10MV m_02MV m_12MV rMV_bar m_01MV
let YDisStress = Make piSensi rMV (fun t -> (m_01MV t) * 1.2) m_10MV m_02MV m_12MV rMV_bar m_01MV
let YLongStress = Make piSensi rMV m_01MV m_10MV (fun t -> (m_02MV t) * 0.8) m_12MV rMV_bar m_01MV
let YInterestStress = Make piSensi (Array.init timePoints (fun i -> (Ind time.[i] R) * 0.03)) m_01MV m_10MV m_02MV m_12MV rMV_bar m_01MV


    // Sensi

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
        p_00.[i,j] <- p_00.[i,j-1] + (-p_00.[i,j-1] * ((m_01MV time.[j-1]) + (m_02MV time.[j-1])) + p_01.[i,j-1] * (m_10MV time.[j-1])) * delta
        p_11.[i,j] <- p_11.[i,j-1] + (-p_11.[i,j-1] * ((m_10MV time.[j-1]) + (m_12MV time.[j-1])) + p_10.[i,j-1] * (m_01MV time.[j-1])) * delta

        p_02.[i,j] <- p_02.[i,j-1] + (p_00.[i,j-1] * (m_02MV time.[j-1]) + p_01.[i,j-1] * (m_12MV time.[j-1])) * delta
        p_12.[i,j] <- p_12.[i,j-1] + (p_10.[i,j-1] * (m_02MV time.[j-1]) + p_11.[i,j-1] * (m_12MV time.[j-1])) * delta

        p_01.[i,j] <- p_01.[i,j-1] + (-p_01.[i,j-1] * ((m_10MV time.[j-1]) + (m_12MV time.[j-1])) + p_00.[i,j-1] * (m_01MV time.[j-1])) * delta
        p_10.[i,j] <- p_10.[i,j-1] + (-p_10.[i,j-1] * ((m_01MV time.[j-1]) + (m_02MV time.[j-1])) + p_11.[i,j-1] * (m_10MV time.[j-1])) * delta

    if i = 0 then W.[i] <- 0.0
    else W.[i] <- W.[i-1] + (W.[i-1] * rMV.[i-1] + piSensi) * delta

    // a
for i = (timePoints-1) downto 0 do
    if i = (timePoints-1) then a.[i] <- 0.0
    else a.[i] <- a.[i+1] + (1.0 - a.[i+1] * rMV.[i+1]) * delta

    // faktor
for i = 0 to (timePoints-1) do
    if time.[i] < R then faktor.[i] <- 1.0
    else faktor.[i] <- faktor.[i-1] + (faktor.[i-1] * (rMV.[i-1] - 1.0/a.[i-1])) * delta
    





//let ChartY = Chart.Combine([
//                Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YInterestStress.Y0.[i])],Name="Stress")
//                Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YMV.Y0.[i])],Name="MV")
//                ])                  
//              |> Chart.WithLegend(Title="Legend")
//              |> Chart.WithXAxis(Min = 0.0)


let Benefits = Chart.Combine([
                Chart.Line ([for i in Rindex .. (timePoints-50) -> (time.[i], (YMV.Y0.[i] + YMV.Y1.[i])/(YMV.p00.[0,i] + YMV.p01.[0,i])/(YMV.a.[i]))],Name="MV")
                Chart.Line ([for i in Rindex .. (timePoints-50) -> (time.[i], (YInterestStress.Y0.[i] + YInterestStress.Y1.[i])/(YInterestStress.p00.[0,i] + YInterestStress.p01.[0,i])/(YInterestStress.a.[i]))],Name="InterestStress")
                ])                  
              |> Chart.WithLegend(Title="Legend")


//Chart.Combine([
//      Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YInterestStress.a.[i])],Name="1")
//      Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YMV.a.[i])],Name="2")
//      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_02.[0,i])],Name="p_02")
//      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_12.[0,i])],Name="p_12")
//      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_01.[0,i])],Name="p_01")
//      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], p_10.[0,i])],Name="p_10")
//      //Chart.Line ([for i in 0 .. (Array.findIndex (fun t -> t > 65.0) time) -> (time.[i], W.[i])],Name="W")
//      //Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], a.[i])],Name="a")
//      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 60.0) time) .. (Array.findIndex (fun t -> >= 90.0) time) -> (time.[i], W.[i]/a.[i])],Name="W/a")
//      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], W.[(Array.findIndex (fun t -> t > 65.0) time)])],Name="W")
//      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], faktor.[i])],Name="faktor")
//      //Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 90.0) time) -> (time.[i], a.[i])],Name="a")
//      Chart.Line ([for i in (Array.findIndex (fun t -> t > 65.0) time) .. (Array.findIndex (fun t -> t >= 85.0) time) -> (time.[i], faktor.[i]*(piSensi + W.[(Array.findIndex (fun t -> t > 65.0) time)]/a.[(Array.findIndex (fun t -> t > 65.0) time)])/a.[i])],Name="Rsens")
    //])                  
    //|> Chart.WithLegend(Title="Legend")
    //|> Chart.WithXAxis(Min = 0.0)