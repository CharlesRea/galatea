module NpData

open System
open HttpFs.Client
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Hopac
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let contractResolver = DefaultContractResolver()
contractResolver.NamingStrategy <- SnakeCaseNamingStrategy()
let jsonOptions = JsonSerializerSettings()
jsonOptions.ContractResolver <- contractResolver

let fetchSnapshot (logger: ILogger) =
    let response =
        Request.createUrl Get "http://nptriton.cqproject.net/game/5515700417069056/full"
        |> Request.responseAsString
        |> run

    let parsedResponse = JsonConvert.DeserializeObject<Snapshot.Data>(response, jsonOptions)
    Snapshot.collection.InsertOne(parsedResponse) |> ignore
    logger.LogInformation("Got data from NP API")

let rec fetchSnapshotEveryHour (logger: ILogger) (stoppingToken: System.Threading.CancellationToken) =
    async {
        match stoppingToken.IsCancellationRequested with
        | true -> return ()
        | false ->
            try
                fetchSnapshot logger
            with
            | ex -> logger.LogError(ex, "Error polling for NP API data")

            do! Task.Delay(TimeSpan.FromHours(1.0), stoppingToken)
            return! fetchSnapshotEveryHour logger stoppingToken
    }

type SnapshotFetcher(logger: ILogger<SnapshotFetcher>) =
    inherit BackgroundService()

    override this.ExecuteAsync(stoppingToken: System.Threading.CancellationToken): System.Threading.Tasks.Task =
        async {
            do! fetchSnapshotEveryHour logger stoppingToken
        } |> Async.StartAsTask :> Task
