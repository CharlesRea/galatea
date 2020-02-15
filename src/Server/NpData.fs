module NpData

open HttpFs.Client
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System.Net.Http
open Hopac

let fetchCurrentData () =
    let response =
      Request.createUrl Get "http://nptriton.cqproject.net/game/5515700417069056/full"
      |> Request.responseAsString
      |> run

    printfn "Here's the body: %s" response

let rec fetchDataEverySecond (stoppingToken: System.Threading.CancellationToken) = async {
  match stoppingToken.IsCancellationRequested with
  | true -> return ()
  | false ->
      fetchCurrentData ()
      do! Task.Delay(1000, stoppingToken)
      return! fetchDataEverySecond (stoppingToken) }

type CustomHostedService() =
    inherit BackgroundService()

    override this.ExecuteAsync(stoppingToken: System.Threading.CancellationToken): System.Threading.Tasks.Task =
        async {
            do! fetchDataEverySecond (stoppingToken)
        } |> Async.StartAsTask :> Task
