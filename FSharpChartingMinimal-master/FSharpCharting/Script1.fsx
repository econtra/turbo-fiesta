﻿#I "...\packages\FSharp.Charting.2.1.0"
#load "FSharp.Charting.2.1.0\FSharp.Charting.fsx"

open FSharp.Charting

let inline Ind t x =
    if t <= x then 1.0 else 0.0

type Results = {Y0 : double[]; Y1 : double[]; Ybar : double[]; a : double[] ; p00 : double[,] ; p01 : double[,] ; p02 : double[,] ; p10 : double[,] ; p11 : double[,] ; p12 : double[,]}

//Kontraktparametre
let t_0 = 25.0
let n = 100.0
let R = 65.0
let piSensi = 80000.0
let b_death = 1000000.0
let b_dis = 50000.0


//Tekniske parametre
let timePoints = 1001
let delta = (n-t_0)/((double timePoints)-1.0)
let time : double array = Array.init timePoints (fun i -> t_0 + (double i) * delta)
let Rindex = Array.findIndex (fun t -> t > R) time
let Lateindex = Array.findIndex (fun t -> t > 95.0) time
let Earlyindex = Array.findIndex (fun t -> t > 60.0) time
let SeventyIndex = Array.findIndex (fun t -> t > 70.0) time






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

let inline M_01STRESSED (t: double) =
    3.0 * (0.0004 + 10.0**(4.54+0.06*t-10.0)) * Ind t R

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

let Make pi (r: double[]) (m_01) (m_10) (m_02) (m_12) (r_bar: double[]) (mu_bar) =
    let p_00 : double[,] = Array2D.create timePoints timePoints 1.0
    let p_01 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_02 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_10 : double[,] = Array2D.create timePoints timePoints 0.0
    let p_11 : double[,] = Array2D.create timePoints timePoints 1.0
    let p_12 : double[,] = Array2D.create timePoints timePoints 0.0

    let Y_0 : double[] = Array.create timePoints 0.0
    let Y_1 : double[] = Array.create timePoints 0.0
    let Y_bar : double[] = Array.create timePoints 0.0
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
                    + ((r.[i-1] + (m_02 time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Y_0.[i-1]
                    + p_00.[0,i-1] * ((Ind time.[i-1] R) * pi + 0.0)
                    + (m_10 time.[i-1]) * Y_1.[i-1] - (m_01 time.[i-1]) * Y_0.[i-1] - (m_02 time.[i-1]) * Y_0.[i-1]) * delta

        Y_1.[i] <- Y_1.[i-1]
                    + ((r.[i-1] + (m_12 time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Y_1.[i-1]
                    + p_01.[0,i-1] * (0.0 + 0.0)
                    + (m_01 time.[i-1]) * Y_0.[i-1] - (m_10 time.[i-1]) * Y_1.[i-1] - (m_12 time.[i-1]) * Y_1.[i-1]) * delta


        Y_bar.[i] <- Y_bar.[i-1]
                    + ((r.[i-1] + (m_02 time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Y_bar.[i-1]
                    + ((Ind time.[i-1] R) * pi + 0.0)) * delta


    let Y = {Y0 = Y_0; Y1 = Y_1; Ybar = Y_bar; Results.a = a; p00 = p_00; p01 = p_01; p02 = p_02; p10 = p_10; p11 = p_11; p12 = p_12}
    Y




let rMV_low = Array.create timePoints 0.02
let rMV_high = Array.create timePoints 0.04


let YMV = Make piSensi rMV m_01MV m_10MV m_02MV m_12MV rMV m_02MV
let YDisStress = Make piSensi rMV M_01STRESSED m_10MV m_02MV m_02MV rMV m_02MV // BEMÆRK SAMME DØDELIGHED
let YLongStress = Make piSensi rMV m_01MV m_10MV (fun t -> (m_02MV t) * 0.9) (fun t -> (m_12MV t) * 0.9) rMV m_02MV
let YInterestStress = Make piSensi (Array.init timePoints (fun i -> 0.02 + (Ind time.[i] R) * 0.01)) m_01MV m_10MV m_02MV m_12MV rMV m_02MV


    // Sensi

let p_00 : double[,] = Array2D.create timePoints timePoints 1.0
let p_01 : double[,] = Array2D.create timePoints timePoints 0.0
let p_02 : double[,] = Array2D.create timePoints timePoints 0.0
let p_10 : double[,] = Array2D.create timePoints timePoints 0.0
let p_11 : double[,] = Array2D.create timePoints timePoints 1.0
let p_12 : double[,] = Array2D.create timePoints timePoints 0.0

let W : double[] = Array.create timePoints 0.0
let W_high : double[] = Array.create timePoints 0.0
let W_low : double[] = Array.create timePoints 0.0
let Walpha : double[] = Array.create timePoints 0.0
let Walpha_high : double[] = Array.create timePoints 0.0
let Walpha_low : double[] = Array.create timePoints 0.0
let a : double[] = Array.create timePoints 0.0
let a_high : double[] = Array.create timePoints 0.0
let a_low : double[] = Array.create timePoints 0.0
let faktor: double[] = Array.create timePoints 0.0
let faktor_high: double[] = Array.create timePoints 0.0
let faktor_low: double[] = Array.create timePoints 0.0


    // a
for i = (timePoints-1) downto 0 do
    if i = (timePoints-1) then a.[i] <- 0.0
    else a.[i] <- a.[i+1] + (1.0 - a.[i+1] * (rMV.[i+1] + (m_02MV time.[i+1]))) * delta

for i = (timePoints-1) downto 0 do
    if i = (timePoints-1) then a_high.[i] <- 0.0
    else a_high.[i] <- a_high.[i+1] + (1.0 - a_high.[i+1] * rMV_high.[i+1]) * delta

for i = (timePoints-1) downto 0 do
    if i = (timePoints-1) then a_low.[i] <- 0.0
    else a_low.[i] <- a_low.[i+1] + (1.0 - a_low.[i+1] * rMV_low.[i+1]) * delta



for i = 0 to (timePoints-1) do
    for j = (i+1) to (timePoints-1) do
        p_00.[i,j] <- p_00.[i,j-1] + (-p_00.[i,j-1] * ((m_01MV time.[j-1]) + (m_02MV time.[j-1])) + p_01.[i,j-1] * (m_10MV time.[j-1])) * delta
        p_11.[i,j] <- p_11.[i,j-1] + (-p_11.[i,j-1] * ((m_10MV time.[j-1]) + (m_12MV time.[j-1])) + p_10.[i,j-1] * (m_01MV time.[j-1])) * delta

        p_02.[i,j] <- p_02.[i,j-1] + (p_00.[i,j-1] * (m_02MV time.[j-1]) + p_01.[i,j-1] * (m_12MV time.[j-1])) * delta
        p_12.[i,j] <- p_12.[i,j-1] + (p_10.[i,j-1] * (m_02MV time.[j-1]) + p_11.[i,j-1] * (m_12MV time.[j-1])) * delta

        p_01.[i,j] <- p_01.[i,j-1] + (-p_01.[i,j-1] * ((m_10MV time.[j-1]) + (m_12MV time.[j-1])) + p_00.[i,j-1] * (m_01MV time.[j-1])) * delta
        p_10.[i,j] <- p_10.[i,j-1] + (-p_10.[i,j-1] * ((m_01MV time.[j-1]) + (m_02MV time.[j-1])) + p_11.[i,j-1] * (m_10MV time.[j-1])) * delta

    if i = 0 then W.[i] <- 0.0
    else W.[i] <- W.[i-1] + (W.[i-1] * (rMV.[i-1] + (m_02MV time.[i-1])) + piSensi) * delta

    if i = 0 then W_high.[i] <- 0.0
    else W_high.[i] <- W_high.[i-1] + (W_high.[i-1] * (rMV_high.[i-1] + (m_02MV time.[i-1])) + piSensi) * delta

    if i = 0 then W.[i] <- 0.0
    else W_low.[i] <- W_low.[i-1] + (W_low.[i-1] * (rMV_low.[i-1] + (m_02MV time.[i-1])) + piSensi) * delta

    if i = 0 then Walpha.[i] <- 0.0
    else Walpha.[i] <- Walpha.[i-1] + ((rMV.[i-1] + (m_02MV time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Walpha.[i-1] + ((Ind time.[i-1] R) * piSensi + 0.0)) * delta

    if i = 0 then Walpha_high.[i] <- 0.0
    else Walpha_high.[i] <- Walpha_high.[i-1] + ((rMV_high.[i-1] + (m_02MV time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Walpha_high.[i-1] + ((Ind time.[i-1] R) * piSensi + 0.0)) * delta

    if i = 0 then Walpha_low.[i] <- 0.0
    else Walpha_low.[i] <- Walpha_low.[i-1] + ((rMV_low.[i-1] + (m_02MV time.[i-1]) - (1.0 - (Ind time.[i-1] R)) / a.[i-1]) * Walpha_low.[i-1] + ((Ind time.[i-1] R) * piSensi + 0.0)) * delta


    // faktors
for i = 0 to (timePoints-1) do
    if time.[i] < R then faktor.[i] <- 1.0
    else faktor.[i] <- faktor.[i-1] + (faktor.[i-1] * ((rMV.[i-1] + (m_02MV time.[i-1])) - 1.0/a.[i-1])) * delta

for i = 0 to (timePoints-1) do
    if time.[i] < R then faktor_high.[i] <- 1.0
    else faktor_high.[i] <- faktor_high.[i-1] + (faktor_high.[i-1] * ((rMV_high.[i-1] + (m_02MV time.[i-1])) - 1.0/a.[i-1])) * delta

for i = 0 to (timePoints-1) do
    if time.[i] < R then faktor_low.[i] <- 1.0
    else faktor_low.[i] <- faktor_low.[i-1] + (faktor_low.[i-1] * ((rMV_low.[i-1] + (m_02MV time.[i-1])) - 1.0/a.[i-1])) * delta
    





//let ReservesMV = Chart.Combine([
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YMV.Y0.[i]/1000000.0)],Name="\u1EF8\u2080")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YMV.Y1.[i]/1000000.0)],Name="\u1EF8\u2081")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YMV.Ybar.[i]/1000000.0)],Name="Yz")
//                        ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Right)
//                |> Chart.WithXAxis(TitleFontSize=16.0, Min = t_0)
//                |> Chart.WithYAxis(Title="m.DKK", TitleFontSize=16.0, Min = 0.0, Max = 8.0, MajorTickMark = ChartTypes.TickMark(Interval = 2.0))
//                |> Chart.WithTitle(Text = "Best Estimate",InsideArea = false)


//let ReservesMV2 = Chart.Combine([
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YDisStress.Y0.[i]/1000000.0)],Name="\u1EF8\u2080 ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YDisStress.Y1.[i]/1000000.0)],Name="\u1EF8\u2081 ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YDisStress.Ybar.[i]/1000000.0)],Name="Yz ")
//                        ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Right)
//                |> Chart.WithXAxis(Title="Age", TitleFontSize=16.0, Min = t_0)
//                |> Chart.WithYAxis(Title="m.DKK", TitleFontSize=16.0, Min = 0.0, Max = 8.0, MajorTickMark = ChartTypes.TickMark(Interval = 2.0))
//                |> Chart.WithTitle(Text = "Disability Stress",InsideArea = false)

//let ReservesMV3 = Chart.Combine([
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YLongStress.Y0.[i]/1000000.0)],Name="\u1EF8\u2080  ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YLongStress.Y1.[i]/1000000.0)],Name="\u1EF8\u2081  ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YLongStress.Ybar.[i]/1000000.0)],Name="Yz  ")
//                        ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Right)
//                |> Chart.WithXAxis(TitleFontSize=16.0, Min = t_0)
//                |> Chart.WithYAxis(TitleFontSize=16.0, Min = 0.0, Max = 8.0, MajorTickMark = ChartTypes.TickMark(Interval = 2.0))
//                |> Chart.WithTitle(Text = "Longevity Stress",InsideArea = false)


//let ReservesMV4 = Chart.Combine([
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YInterestStress.Y0.[i]/1000000.0)],Name="\u1EF8\u2080   ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YInterestStress.Y1.[i]/1000000.0)],Name="\u1EF8\u2081   ")
//                        Chart.Line ([for i in 0 .. (timePoints-1) -> (time.[i], YInterestStress.Ybar.[i]/1000000.0)],Name="Yz   ")
//                        ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Right)
//                |> Chart.WithXAxis(Title="Age", TitleFontSize=16.0, Min = t_0)
//                |> Chart.WithYAxis(TitleFontSize=16.0, Min = 0.0, Max = 8.0, MajorTickMark = ChartTypes.TickMark(Interval = 2.0))
//                |> Chart.WithTitle(Text = "Interest Stress",InsideArea = false)


//let ReservesCombo1 = Chart.Rows (seq {ReservesMV; ReservesMV2})
//let ReservesCombo2 = Chart.Rows (seq {ReservesMV3; ReservesMV4})
//let ReservesCombo3 = Chart.Columns (seq {ReservesCombo1; ReservesCombo2}) |> Chart.Save "H:\SpecialyNY\Reserves.png"



//let Benefits = Chart.Combine([
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YMV.Y0.[i] + YMV.Y1.[i])/(YMV.p00.[0,i] + YMV.p01.[0,i])/(YMV.a.[i])/1000.0)],Name="Best Estimate Restricted")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YMV.Ybar.[i])/(YMV.a.[i])/1000.0)],Name="Best Estimate Fixed")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YDisStress.Y0.[i] + YDisStress.Y1.[i])/(YDisStress.p00.[0,i] + YDisStress.p01.[0,i])/(YDisStress.a.[i])/1000.0)],Name="Disability Stress Restriced")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YDisStress.Ybar.[i])/(YDisStress.a.[i])/1000.0)],Name="Disability Stress Fixed")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YLongStress.Y0.[i] + YLongStress.Y1.[i])/(YLongStress.p00.[0,i] + YLongStress.p01.[0,i])/(YLongStress.a.[i])/1000.0)],Name="Longevity Stress Restricted")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YLongStress.Ybar.[i])/(YLongStress.a.[i])/1000.0)],Name="Longevity Stress Fixed")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YInterestStress.Y0.[i] + YInterestStress.Y1.[i])/(YInterestStress.p00.[0,i] + YInterestStress.p01.[0,i])/(YInterestStress.a.[i])/1000.0)],Name="Interest Stress Restricted")
//                Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], (YInterestStress.Ybar.[i])/(YInterestStress.a.[i])/1000.0)],Name="Interest Stress Fixed")
//                ])          
//              |> Chart.WithLegend(Docking=ChartTypes.Docking.Left) |> Chart.WithLegend(Docking=ChartTypes.Docking.Bottom)
//              |> Chart.WithXAxis(Title="Age", TitleFontSize=16.0, Min = R)
//              |> Chart.WithYAxis(Title="Prognosticated annual benefits in t.DKK", TitleFontSize=16.0, Min = 400.0)
//              |> Chart.WithLegend(Title="")
//              |> Chart.Save "H:\SpecialyNY\BenefitsNy.png"

//let RSensi = Chart.Combine([
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], faktor_low.[i]*(piSensi + W_low.[Rindex]/a.[Rindex])/a.[i]/1000.0)],Name="Low")
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], faktor.[i]*(piSensi + W.[Rindex]/a.[Rindex])/a.[i]/1000.0)],Name="Mean")
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], faktor_high.[i]*(piSensi + W_high.[Rindex]/a.[Rindex])/a.[i]/1000.0)],Name="High")
//                    ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Left)
//                |> Chart.WithXAxis(Title="Age", TitleFontSize=16.0, Min = R)
//                |> Chart.WithYAxis(Title="Retirement sensitivity for annual benefits in t.DKK", TitleFontSize=16.0)
//                |> Chart.Save "H:\SpecialyNY\Rsensiny.png"

//let AlphaSensi = Chart.Combine([
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], Walpha_low.[i]/a.[i]/1000.0)],Name="Low")
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], Walpha.[i]/a.[i]/1000.0)],Name="Mean")
//                    Chart.Line ([for i in Rindex .. Lateindex -> (time.[i], Walpha_high.[i]/a.[i]/1000.0)],Name="High")
//                    ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Left)
//                |> Chart.WithXAxis(Title="Age", TitleFontSize=16.0, Min = R)
//                |> Chart.WithYAxis(Title="Premium level sensitivity for annual benefits in t.DKK", TitleFontSize=16.0)
//                |> Chart.Save "H:\SpecialyNY\AlphaSensi.png"



//let LumpSum = Chart.Combine([
//                    Chart.Line ([for i in Rindex .. SeventyIndex -> (time.[i], 1.0)],Name="Fixed Path Prognosis")
//                    Chart.Line ([for i in Rindex .. SeventyIndex -> (time.[i], YMV.p00.[0,i]/(YMV.p00.[0,i]+YMV.p01.[0,i]))],Name="Restricted Path Prognosis")
//                    ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Left)
//                |> Chart.WithXAxis(Title="Time of retirement R", TitleFontSize=16.0, Min = R)
//                |> Chart.WithYAxis(Title="", TitleFontSize=16.0)
//                |> Chart.Save "H:\SpecialyNY\LumpSumNy.png"

//let Term = Chart.Combine([
//                    Chart.Line ([for i in 0 .. Rindex - 1 -> (time.[i], 1.0)],Name="Fixed Path Prognosis")
//                    Chart.Line ([for i in 0 .. Rindex - 1 -> (time.[i], (YMV.p00.[0,i] * (m_02MV time.[i]))/(YMV.p00.[0,i] * (m_02MV time.[i]) + YMV.p01.[0,i] * (m_12MV time.[i])))],Name="Restricted Path Prognosis")
//                    ])                  
//                |> Chart.WithLegend(Docking=ChartTypes.Docking.Left)
//                |> Chart.WithXAxis(Title="Age at death", TitleFontSize=16.0, Min = t_0)
//                |> Chart.WithYAxis(Title="", TitleFontSize=16.0)
//                |> Chart.Save "H:\SpecialyNY\TermNy.png"

let Exchange = Chart.Combine([
                    Chart.Line ([for i in Earlyindex .. SeventyIndex - 1 -> (time.[i], (piSensi + 1.0 * W.[i]/a.[i])/W.[i])],Name="alpha = 1")
                    ])                  
                |> Chart.WithXAxis(Title="Time of retirement R", TitleFontSize=16.0, Min = 60.0)
                |> Chart.WithYAxis(Title="Exchange ratio", TitleFontSize=16.0, Min = 0.08)
                |> Chart.Save "H:\SpecialyNY\ExchangeNEW.png"



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