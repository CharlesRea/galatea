module NpData

open System
open HttpFs.Client
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open Hopac
open LiteDB
open LiteDB.FSharp
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open Shared

type LiteDbData = {
    Id: int
    Time: DateTimeOffset
    Data: NpTickData
}

//let mapper = FSharpBsonMapper()
//let liteDb = new LiteDatabase("galatea.db", mapper)
//let liteDbCollection = liteDb.GetCollection<LiteDbData>("api-data")

let mongoClient = MongoClient("mongodb://root:supersecure@localhost:27017")
let mongoDb = mongoClient.GetDatabase("galatea")
let collection = mongoDb.GetCollection<BsonDocument>("api-data")

let fetchCurrentData (logger: ILogger) =
    let response =
      Request.createUrl Get "http://nptriton.cqproject.net/game/5515700417069056/full"
      |> Request.responseAsString
      |> run

    let doc = BsonDocument.Parse(response)
    doc.Add("Time", (BsonValue.Create(DateTimeOffset.UtcNow.ToString("O")))) |> ignore
    collection.InsertOne(doc)

    logger.LogInformation("Got data from NP API")

//    let liteDbData = { Id = 0; Time = DateTimeOffset.UtcNow; Data = NpTickData response }
//    liteDbCollection.Insert(liteDbData) |> ignore

let rec fetchDataEverySecond (logger: ILogger) (stoppingToken: System.Threading.CancellationToken) = async {
  match stoppingToken.IsCancellationRequested with
  | true -> return ()
  | false ->
      fetchCurrentData logger
      do! Task.Delay(TimeSpan.FromHours(1.0), stoppingToken)
      return! fetchDataEverySecond logger stoppingToken }

type DataFetchingService(logger: ILogger<DataFetchingService>) =
    inherit BackgroundService()

    override this.ExecuteAsync(stoppingToken: System.Threading.CancellationToken): System.Threading.Tasks.Task =
        async {
            do! fetchDataEverySecond logger stoppingToken
        } |> Async.StartAsTask :> Task
