module Snapshot

open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open System.Collections.Generic
open MongoDB.Driver

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Tech = {
    level: int
}

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Player = {
    Uid: int
    Huid: int
    Alias: string
    TotalEconomy: int
    TotalIndustry: int
    TotalScience: int
    TotalStars: int
    TotalFleets: int
    TotalStrength: int
    Tech: Dictionary<string, Tech>
}

[<CLIMutable>]
[<BsonIgnoreExtraElements>]
type Data = {
    Id: ObjectId
    Tick: int
    Productions: int
    Now: int64
    Players: Dictionary<string, Player>
}

let mongoClient = MongoClient("mongodb://root:supersecure@localhost:27017")
let mongoDb = mongoClient.GetDatabase("galatea")
let collection = mongoDb.GetCollection<Data>("api-data")

let getSnapshots (): Data seq =
    collection.Find(fun _ -> true).ToList() |> seq

type IGalateaApi =
    { snapshots: unit -> Async<Data seq> }

let galateaApi: IGalateaApi = {
    snapshots = fun () -> async { return getSnapshots () }
}
