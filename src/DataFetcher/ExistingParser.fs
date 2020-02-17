module DataFetcher.ExistingParser

open MongoDB.Bson
open MongoDB.Bson.IO
open Snapshot
open MongoDB.Driver
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

let importExistingSnapshots () =
    let mongoClient = MongoClient("mongodb://root:supersecure@localhost:27017")
    let mongoDb = mongoClient.GetDatabase("neptunespride")
    let collection = mongoDb.GetCollection<BsonDocument>("api_data")

    let contractResolver = DefaultContractResolver()
    contractResolver.NamingStrategy <- SnakeCaseNamingStrategy()
    let jsonOptions = JsonSerializerSettings()
    jsonOptions.ContractResolver <- contractResolver

    let items = collection.Find(fun  _ -> true).ToList()

    let itemStrings = items |> Seq.map (fun item -> item.ToString().Remove(3, 46) )

    let jsonWriterSettings = JsonWriterSettings()
    jsonWriterSettings.OutputMode <- JsonOutputMode.Strict
    let item = items |> Seq.head |> (fun item -> item.ToJson(jsonWriterSettings))
    printf "%s" (item)

    let parsed = items |> Seq.map (fun item -> item.ToJson(jsonWriterSettings)) |> Seq.map (fun item -> JsonConvert.DeserializeObject<Snapshot>(item, jsonOptions))

    let newDb = mongoClient.GetDatabase("galatea")
    let newCollection = newDb.GetCollection<Snapshot>("snapshots")

    newCollection.InsertMany parsed
