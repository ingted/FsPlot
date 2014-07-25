﻿module FsPlot.Google.Js

open FsPlot.Config
open FsPlot.Data
open FsPlot.Highcharts.Options
open FsPlot.Quote
open FunScript
open FunScript.Compiler
open FunScript.TypeScript
open System

[<ReflectedDefinition>]
module Chart =
    
    type pack = {packages : string []}

    let _type typeCode =
        match typeCode with
        | TypeCode.Boolean -> "boolean"
        | TypeCode.DateTime -> "datetime"
        | TypeCode.String -> "string"
        | _ -> "number"

    let columnLabel config idx series =
        let categories = config.Categories
        match Array.isEmpty categories with
        | false -> categories.[idx]
        | true -> series.Name

    let addColumns (config:ChartConfig) (dataTable:google.visualization.DataTable) =
        config.Data
        |> Array.iteri(fun idx series ->
            dataTable.addColumn(_type series.XType) |> ignore
            let label = columnLabel config idx series
            dataTable.addColumn(_type series.YType, label) |> ignore
            dataTable.addRows series.Values |> ignore          
        )

    let drawOnLoad (drawChart:unit -> unit) =
        google.Globals.load("visualization", "1", {packages = [|"corechart"|]})
        google.Globals.setOnLoadCallback drawChart

    let bar (config:ChartConfig) =
        let drawChart() =
            let options = createEmpty<google.visualization.BarChartOptions>()
            
            match config.Title with
            | None -> ()
            | Some x -> options.title <- x

            match config.XTitle with
            | None -> ()
            | Some x ->
                let xAxis = createEmpty<google.visualization.ChartAxis>()
                xAxis.title <- x
                options.hAxis <- xAxis 

            match config.YTitle with
            | None -> ()
            | Some x ->
                let yAxis = createEmpty<google.visualization.ChartAxis>()
                yAxis.title <- x
                options.vAxis <- yAxis 

            let dataTable = google.visualization.DataTable.Create()
            addColumns config dataTable

            let chart = google.visualization.BarChart.Create(Globals.document.getElementById("chart"))
            chart.draw(dataTable, options)

        drawOnLoad drawChart

let inline compile expr =
    Compiler.Compile(
        expr,
        noReturn = true,
        shouldCompress = true)

let bar config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.bar %%configExpr @>
