module stockfighterDashboardApp

#if INTERACTIVE
#r "../build/Suave.dll"
#r "../build/stockfighterDashboard.dll"
#endif

open System
open System.IO

open Suave
open Suave.Operators
open Suave.Http
open Suave.Filters
open Suave.Files

let getMe request =
  System.IO.File.ReadAllLines("../quoteData.txt") |> String.concat "\n"

let app =
  choose
    [
      path "/quoteData" >=> choose [
        GET  >=> request(fun r -> Successful.OK <| getMe r)
        RequestErrors.NOT_FOUND "Found no handlers"]
      GET >=> browseHome
    ]

startWebServer defaultConfig app

[<EntryPoint>]
let main argv =
    printfn "%A" argv
    startWebServer defaultConfig app
    0 // return an integer exit code
